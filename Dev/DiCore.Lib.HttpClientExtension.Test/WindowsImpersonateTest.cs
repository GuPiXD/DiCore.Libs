using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DiCore.Lib.RestClient.TestCore.Api.Models;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DiCore.Lib.HttpClientExtension;
using FileData = DiCore.Lib.HttpClientExtension.FileData;


namespace DiCore.Lib.RestClient.TestCore
{
    public class WindowsImpersonateTest : IClassFixture<WindowsImpersonateTestFixture>
    {
        private readonly HttpClient client;
        private readonly IStorage storage;
        private const string PersonsRoute = "personsmiddle";
        private const string FilesRoute = "files";
        private const string FileName = "TestFile.txt";
        private const string PutFileName = "TestFilePut.txt";
        private const string ManyPath = "many";
        private const string ObjectPath = "object";

        private readonly Person personForPost = new Person()
        {
            Name = "Clare",
            Age = 58,
            Active = true
        };

        private const string PersonNamePut = "Mike";

        public WindowsImpersonateTest(WindowsImpersonateTestFixture windowsImpersonateTestFixture)
        {
            storage = windowsImpersonateTestFixture.ServiceProvider.GetRequiredService<IStorage>();
            var clientFactory = windowsImpersonateTestFixture.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            client = clientFactory.CreateClient("defaultCredentials");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public async Task GetManyAsync()
        {
            var storageItems = storage.GetPersons().ToArray();
            var apiItems = await client.GetAsync<PersonItem[]>(PersonsRoute);
            Assert.True(apiItems.Data.ItemEquals(storageItems), "Get many async");
        }

        [Fact]
        public void GetManySync()
        {
            var storageItems = storage.GetPersons().ToArray();
            var apiItems = client.Get<PersonItem[]>(PersonsRoute);
            Assert.True(apiItems.Data.ItemEquals(storageItems), "Get many sync");
        }

        [Fact]
        public async Task GetOneAsync()
        {
            var storageItem = storage.GetPersons().First();
            var apiItem = await client.GetAsync<PersonItem>(PersonsRoute, storageItem.Id);
            Assert.True(storageItem.Equals(apiItem.Data), "Get one async");
        }

        [Fact]
        public void GetOneSync()
        {
            var storageItem = storage.GetPersons().First();
            var apiItem = client.Get<PersonItem>(PersonsRoute, storageItem.Id);
            Assert.True(storageItem.Equals(apiItem.Data), "Get one sync");
        }

        [Fact]
        public async Task PostPutDeleteAsync()
        {
            var id = (await client.PostAsync<Guid>(personForPost, PersonsRoute)).Data;
            Assert.True(id != Guid.Empty, "Post async");
            var storageItemAfterPost = storage.GetPerson(id) as Person;
            Assert.True(personForPost.Equals(storageItemAfterPost), "Post async");
            var updatePerson = new Person()
            {
                Name = PersonNamePut,
                Age = personForPost.Age,
                Active = personForPost.Active
            };
            await client.PutAsync(updatePerson, PersonsRoute, id);
            var storageItemAfterPut = storage.GetPerson(id) as Person;
            Assert.True(updatePerson.Equals(storageItemAfterPut), "Put async");
            await client.DeleteAsync(PersonsRoute, id);
            var storageItemAfterDelete = storage.GetPerson(id);
            Assert.True(storageItemAfterDelete == null, "Delete async");
        }

        [Fact]
        public void PostPutDeleteSync()
        {
            var id = (client.Post<Guid>(personForPost, PersonsRoute)).Data;
            Assert.True(id != Guid.Empty, "Post sync");
            var storageItemAfterPost = storage.GetPerson(id) as Person;
            Assert.True(personForPost.Equals(storageItemAfterPost), "Post sync");
            var updatePerson = new Person()
            {
                Name = PersonNamePut,
                Age = personForPost.Age,
                Active = personForPost.Active
            };
            client.Put(updatePerson, PersonsRoute, id);
            var storageItemAfterPut = storage.GetPerson(id) as Person;
            Assert.True(updatePerson.Equals(storageItemAfterPut), "Put sync");
            client.Delete(PersonsRoute, id);
            var storageItemAfterDelete = storage.GetPerson(id);
            Assert.True(storageItemAfterDelete == null, "Delete sync");
        }

        [Fact]
        public async Task PostPutGetFileAsync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }

            Guid id;
            using (var streamFile = File.OpenRead(FileName))
            using (var fileParameter = new FileData(streamFile, "file", FileName))
            {
                id = (await client.PostAsync<Guid>(fileParameter, FilesRoute)).Data;
            }

            using (var memoryStream = new MemoryStream())
            {
                var result = await client.GetAsync(FilesRoute, memoryStream, id);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var receivedData = streamReader.ReadToEnd();
                    Assert.Equal(FileName, result.FileName);
                    Assert.Equal(data, receivedData);
                }
            }

            using (var streamFile = File.OpenRead(PutFileName))
            using (var fileParameter = new FileData(streamFile, "file", PutFileName))
            {
                await client.PutAsync(fileParameter, FilesRoute, id);
            }

            using (var memoryStream = new MemoryStream())
            {
                var result = await client.GetAsync(FilesRoute, memoryStream, id);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var receivedData = streamReader.ReadToEnd();
                    Assert.Equal(PutFileName, result.FileName);
                    Assert.Equal(putData, receivedData);
                }
            }
        }

        [Fact]
        public void PostPutGetFileSync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }

            Guid id;
            using (var streamFile = File.OpenRead(FileName))
            using (var fileParameter = new FileData(streamFile, "file", FileName))
            {
                id = client.Post<Guid>(fileParameter, FilesRoute).Data;
            }

            using (var memoryStream = new MemoryStream())
            {
                var result = client.Get(FilesRoute, memoryStream, id);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var receivedData = streamReader.ReadToEnd();
                    Assert.Equal(FileName, result.FileName);
                    Assert.Equal(data, receivedData);
                }
            }

            using (var streamFile = File.OpenRead(PutFileName))
            using (var fileParameter = new FileData(streamFile, "file", PutFileName))
            {
                client.Put(fileParameter, FilesRoute, id);
            }

            using (var memoryStream = new MemoryStream())
            {
                var result = client.Get(FilesRoute, memoryStream, id);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var receivedData = streamReader.ReadToEnd();
                    Assert.Equal(PutFileName, result.FileName);
                    Assert.Equal(putData, receivedData);
                }
            }
        }

        [Fact]
        public async void PostManyFilesAsync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }

            const string name = "Тестовое имя";
            const int len = 15;
            var uid = Guid.NewGuid();
            const double angle = -3.45d;

            using (var streamFile = File.OpenRead(FileName))
            using (var streamFilePut = File.OpenRead(PutFileName))
            using (var collection = new MultipartCollection(
                new StringData("name", name),
                new StringData("len", len),
                new StringData("angle", angle, CultureInfo.InvariantCulture),
                new StringData("uid", uid),
                new FileData(streamFile, "file", FileName),
                new FileData(streamFilePut, "file", PutFileName)))
            {
                var result = (await client.PostAsync<TestDataMany>(collection, FilesRoute, ManyPath)).Data;
                Assert.True(result.Name == name && result.Len == len && result.Files[FileName] == data &&
                            result.Files[PutFileName] == putData);
            }

            {
            }
        }

        [Fact]
        public async void PostManyFilesObjectAsync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }

            var name = "Тестовое имя";
            var len = new Random(DateTime.Now.Millisecond).Next();
            var uid = Guid.NewGuid();
            var angle = new Random(DateTime.Now.Millisecond).NextDouble();
            var flag = true;
            var timespan = DateTime.Now;
            using (var streamFile = File.OpenRead(FileName))
            using (var streamFilePut = File.OpenRead(PutFileName))
            using (var collection = new MultipartCollection(
                new StringData("name", name),
                new StringData("len", len),
                new StringData("angle", angle),
                new StringData("uid", uid),
                new StringData("flag", flag),
                new StringData("timespan", timespan),
                new FileData(streamFile, "file", FileName),
                new FileData(streamFilePut, "file", PutFileName)))
            {
                var result = (await client.PostAsync<TestDataMany>(collection, FilesRoute, ObjectPath)).Data;
                Assert.True(result.Name == name && result.Len == len && result.Files[FileName] == data &&
                            result.Files[PutFileName] == putData);
            }
        }

        [Fact]
        public async void PostManyFilesObjectReflectionAsync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }


            var innerFileName = $"copy_{FileName}";
            using (var streamFile = File.OpenRead(FileName))
            using (var streamFilePut = File.OpenRead(PutFileName))
            using (var streamFileInner = File.OpenRead(FileName))

            {
                var testData = new ObjectFilesClientModel
                {
                    Name = "Тестовое имя",
                    Len = new Random(DateTime.Now.Millisecond).Next(),
                    Uid = Guid.NewGuid(),
                    Angle = new Random(DateTime.Now.Millisecond).NextDouble(),
                    Flag = true,
                    Timespan = DateTime.Now,
                    File = new[]
                    {
                        new FileContent(FileName, streamFile),
                        new FileContent(PutFileName, streamFilePut),
                    },
                    StringArray = new[] { FileName, PutFileName },
                    InnerModel = new ObjectFilesInnerClientModel()
                    {
                        Name = "Тестовое имя 2",
                        Angle = new Random(DateTime.Now.Millisecond).NextDouble(),
                        Len = new Random(DateTime.Now.Millisecond).Next(),
                        Uid = Guid.NewGuid(),
                        Flag = false,
                        Timespan = DateTime.Now.AddHours(1),
                        StringArray = new[] { FileName, PutFileName, FileName },
                        File = new[] { new FileContent(innerFileName, streamFileInner) }
                    }
                };
                using (var collection = new MultipartCollection(testData))
                {
                    var result = (await client.PostAsync<TestDataMany>(collection, FilesRoute, ObjectPath)).Data;
                    Assert.True(testData.Equals(result));
                    Assert.True(result.Files.ContainsKey(FileName) && result.Files.ContainsKey(PutFileName) &&
                                result.Files[FileName] == data && result.Files[PutFileName] == putData);
                    Assert.True(result.InnerModel.Files.ContainsKey(innerFileName) &&
                                result.InnerModel.Files[innerFileName] == data);
                }
            }
        }

        [Fact]
        public async void PutManyFilesObjectReflectionAsync()
        {
            string data;
            using (var streamReader = new StreamReader(FileName))
            {
                data = streamReader.ReadToEnd();
            }

            string putData;
            using (var streamReader = new StreamReader(PutFileName))
            {
                putData = streamReader.ReadToEnd();
            }


            var innerFileName = $"copy_{FileName}";
            var id = Guid.NewGuid();
            using (var streamFile = File.OpenRead(FileName))
            using (var streamFilePut = File.OpenRead(PutFileName))
            using (var streamFileInner = File.OpenRead(FileName))

            {
                var testData = new ObjectFilesClientModel
                {
                    Name = "Тестовое имя",
                    Len = new Random(DateTime.Now.Millisecond).Next(),
                    Uid = Guid.NewGuid(),
                    Angle = new Random(DateTime.Now.Millisecond).NextDouble(),
                    Flag = true,
                    Timespan = DateTime.Now,
                    File = new FileContent[]
                    {
                        new FileContent(FileName, streamFile),
                        new FileContent(PutFileName, streamFilePut),
                    },
                    StringArray = new[] { FileName, PutFileName },
                    InnerModel = new ObjectFilesInnerClientModel()
                    {
                        Name = "Тестовое имя 2",
                        Angle = new Random(DateTime.Now.Millisecond).NextDouble(),
                        Len = new Random(DateTime.Now.Millisecond).Next(),
                        Uid = Guid.NewGuid(),
                        Flag = false,
                        Timespan = DateTime.Now.AddHours(1),
                        StringArray = new[] { FileName, PutFileName, FileName },
                        File = new[] { new FileContent(innerFileName, streamFileInner) }
                    }
                };
                using (var collection = new MultipartCollection(testData))
                {
                    var result = (await client.PutAsync<TestDataManyId>(collection, FilesRoute, ObjectPath, id)).Data;
                    Assert.Equal(result.Id, id);
                    Assert.True(testData.Equals(result));
                    Assert.True(result.Files.ContainsKey(FileName) && result.Files.ContainsKey(PutFileName) &&
                                result.Files[FileName] == data && result.Files[PutFileName] == putData);
                    Assert.True(result.InnerModel.Files.ContainsKey(innerFileName) &&
                                result.InnerModel.Files[innerFileName] == data);
                }
            }
        }
    }
}