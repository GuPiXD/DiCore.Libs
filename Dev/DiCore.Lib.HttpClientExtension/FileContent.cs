using System;
using System.IO;
using System.Text;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Описание файла
    /// </summary>
    public class FileContent:IDisposable
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Поток данных
        /// </summary>
        public Stream Stream { get; }
        /// <summary>
        /// Тип содержимого
        /// </summary>
        public string ConetentType { get; set; }
        /// <summary>
        /// Кодировка
        /// </summary>
        public Encoding Ecoding { get; set; }
        /// <summary>
        /// Создание
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="stream">Поток</param>
        public FileContent(string fileName, Stream stream)
        {
            FileName = fileName;
            Stream = stream;
        }
        /// <summary>
        /// Удаление
        /// </summary>
        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}