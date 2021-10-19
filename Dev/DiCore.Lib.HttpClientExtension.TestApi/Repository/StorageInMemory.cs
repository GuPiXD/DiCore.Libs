using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class StorageInMemory : IStorage
    {
        private readonly ConcurrentDictionary<Guid, Person> persons = new ConcurrentDictionary<Guid, Person>();
        private readonly ConcurrentDictionary<Guid, FileData> fileDatas = new ConcurrentDictionary<Guid, FileData>();


        public StorageInMemory()
        {
        }

        public void InitializePersons(Dictionary<Guid, Person> data)
        {
            persons.Clear();
            foreach (var (key, person) in data)
            {
                persons.TryAdd(key, person);
            }
        }

        public IEnumerable<PersonItem> GetPersons()
        {
            return persons.Select(p => new PersonItem
                {Id = p.Key, Name = p.Value.Name, Age = p.Value.Age, Active = p.Value.Active});
        }

        public PersonItem GetPerson(Guid id)
        {
            if (!persons.TryGetValue(id, out var person) || person == null)
            {
                return null;
            }

            return new PersonItem {Id = id, Name = person.Name, Age = person.Age, Active = person.Active};
        }

        public Guid InsertPerson(Person person)
        {
            var id = Guid.NewGuid();
            persons.TryAdd(id, person);
            return id;
        }

        public bool UpdatePerson(Guid id, Person person)
        {
            return persons.TryGetValue(id, out var existedPerson) &&
                   persons.TryUpdate(id, person, existedPerson);
        }

        public bool DeletePerson(Guid id)
        {
            return persons.TryRemove(id, out _);
        }

        public FileDataItem GetFileData(Guid id)
        {
            if (!fileDatas.TryGetValue(id, out var fileData) || fileData == null)
            {
                return null;
            }

            return new FileDataItem()
            {
                Id = id,
                Data = fileData.Data,
                FileName = fileData.FileName,
                ContentType = fileData.ContentType
            };
        }

        public Guid InsertFileData(FileData data)
        {
            var id = Guid.NewGuid();
            fileDatas.TryAdd(id, data);
            return id;
        }

        public bool UpdateFileData(Guid id, FileData data)
        {
            return fileDatas.TryGetValue(id, out var existedData) && fileDatas.TryUpdate(id, data, existedData);
        }

        public bool DeleteFileData(Guid id)
        {
            return fileDatas.TryRemove(id, out _);
        }
    }
}