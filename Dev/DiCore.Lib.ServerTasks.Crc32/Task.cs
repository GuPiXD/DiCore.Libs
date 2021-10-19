using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiCore.Lib.ServerTasks.Crc32
{
    public static class Task
    {
        private const uint polynomial = 0xEDB88320;
        private static readonly uint[] Table = new uint[256];
        private const int bufferSize = 262144;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>  
        static Task()
        {

            for (var i = 0u; i < 256u; i++)
            {
                var value = i;

                for (var j = 8; j > 0; j--)
                {
                    if ((value & 1) == 1)
                        value = (value >> 1) ^ polynomial;
                    else
                        value >>= 1;
                }

                Table[i] = value;
            }
        }

        /// <summary>
        /// Создание файла-журнала с рекурсивным списком файлов и их CRC32
        /// </summary>
        /// <param name="rootPath">Имя корневой директории, файл list.crc будет находиться в ней</param>
        public static void CalculateCrc32Folder(string rootPath)
        {
            var result = new List<Tuple<string, uint>>();

            var resultFileName = Path.Combine(rootPath, "list.crc");
            if (File.Exists(resultFileName))
                File.Delete(resultFileName);

            var rootPathLength = (Path.GetDirectoryName(resultFileName)?.Length) ?? 0;

            InnerCalculateCrc32(rootPath, result, rootPathLength);

            using (
                var resultFile = new FileStream(resultFileName, FileMode.CreateNew, FileAccess.ReadWrite,
                    FileShare.ReadWrite))
            {
                var writer = new StreamWriter(resultFile, Encoding.UTF8);

                writer.WriteLine($"File;CRC32;{DateTime.Now.ToString("G")}");

                foreach (var item in result)
                {

                    writer.WriteLine($"{item.Item1};{item.Item2};");
                }
                writer.Flush();
            }
        }

        private static void InnerCalculateCrc32(string path, List<Tuple<string, uint>> result, int rootPathLength)
        {
            var files = Directory.GetFiles(path);

            result.AddRange(from file in files let crc = CalculateCrc32File(file) select new Tuple<string, uint>(file.Substring(rootPathLength), crc));

            var dirs = Directory.GetDirectories(path);

            foreach (var dirPath in dirs)
            {
                InnerCalculateCrc32(dirPath, result, rootPathLength);
            }
        }

        /// <summary>
        /// Получение контрольной суммы по алгоритму CRC32 для файла
        /// </summary>
        /// <param name="filePath">Полное имя файла</param>
        /// <returns></returns>
        public static uint CalculateCrc32File(string filePath)
        {
            var result = 0xFFFFFFFF;

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = CalculateCrc32File(file);
            }

            return ~result;
        }

        /// <summary>
        /// Получение контрольй суммы по алгоритму CRC32 для файла
        /// </summary>
        /// <param name="stream">Поток данных файла</param>
        /// <returns></returns>
        public static uint CalculateCrc32File(Stream stream)
        {
            uint result = 0xFFFFFFFF;
            var buffer = new byte[bufferSize];

            stream.Position = 0;

            var count = stream.Read(buffer, 0, bufferSize);
            while (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    result = ((result >> 8) & 0x00FFFFFF) ^ Table[(result ^ buffer[i]) & 0xFF];

//                    result = ((result) >> 8) ^ Table[(buffer[i]) ^ ((result) & 0x000000FF)];
                }

                count = stream.Read(buffer, 0, bufferSize);
            }

            return ~result;
        }

        public static uint CalculateCrc32Array(int[] targetArray)
        {
            uint result = 0xFFFFFFFF;
            var buffer = new byte[bufferSize];
            var elementSize = sizeof(int);

            var length = targetArray.Length;
            var steps = (length * elementSize) /bufferSize + 1;
            var stepLength = bufferSize/elementSize;

            for (var i = 0; i < steps; i++)
            {
                var elementCount = Math.Min(length, (i+1)*stepLength);
                for (var j = i*stepLength; j < elementCount; j++)
                {
                    var element = targetArray[j];

                    buffer[j*elementSize] = (byte) (element & 0x000000FF);
                    buffer[j*elementSize + 1] = (byte) ((element & 0x0000FF00)>>8);
                    buffer[j*elementSize + 2] = (byte) ((element & 0x00FF0000)>>16);
                    buffer[j*elementSize + 3] = (byte) ((element & 0xFF000000)>>24);
                }

                for (var k = 0; k < elementCount*4; k++)
                {
                    result = ((result >> 8) & 0x00FFFFFF) ^ Table[(result ^ buffer[k]) & 0xFF];
                    
                }
            }

            return ~result;
        }
    }
}
