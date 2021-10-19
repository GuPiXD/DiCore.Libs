using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Файловые данные
    /// </summary>
    public class FileData : IMultipartData
    {
        /// <summary>
        /// Тип контента
        /// </summary>
        public string ContentType { get; }
        /// <summary>
        /// Кодировка
        /// </summary>
        public Encoding Encoding { get; }
        /// <summary>
        /// Поток с данными
        /// </summary>
        public Stream Stream { get; }
        /// <summary>
        /// Закрывать поток при удалении клиента
        /// </summary>
        private bool disposeStream = false;

        private FileData(string name, string fileName, string contentType, Encoding encoding)
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
            Encoding = encoding;
        }
        /// <summary>
        /// Создание данных из массива байт
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="name">Название поля</param>
        /// <param name="fileName">Название файла</param>
        /// <param name="contentType">Тип содержимого</param>
        /// <param name="encoding">Кодировка</param>
        public FileData(byte[] data, string name, string fileName, string contentType = null,
            Encoding encoding = null) : this(name, fileName, contentType, encoding)
        {
            Stream = new MemoryStream();
            Stream.Write(data, 0, data.Length);
            Stream.Position = 0;
            disposeStream = true;
        }
        /// <summary>
        /// Создание данных из потока
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <param name="name">Название поля</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="contentType">Тип содержимого</param>
        /// <param name="encoding">Кодировка</param>
        public FileData(Stream stream, string name, string fileName, string contentType = null,
            Encoding encoding = null) : this(name, fileName, contentType, encoding)
        {
            Stream = stream;
        }
        /// <summary>
        /// Создание данных из FileContent
        /// </summary>
        /// <param name="fileContent">Данные файла</param>
        /// <param name="name">Название поля</param>
        public FileData(FileContent fileContent, string name) : this(fileContent.Stream, name,
            fileContent.FileName, fileContent.ConetentType, fileContent.Ecoding)
        {
        }
        /// <summary>
        /// Название поля
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Названени файла
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Контент HTTP
        /// </summary>
        public HttpContent Content
        {
            get
            {
                Stream.Position = 0;
                var content = new StreamContent(Stream);
                if (!string.IsNullOrWhiteSpace(ContentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                }

                if (Encoding != null)
                {
                    content.Headers.ContentEncoding.Add(Encoding.HeaderName);
                }

                return content;
            }
        }

        public void Dispose()
        {
            if (disposeStream)
            {
                Stream.Dispose();
            }
        }
    }
}