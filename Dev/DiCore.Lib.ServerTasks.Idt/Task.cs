using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DiCore.Lib.ServerTasks.Idt
{
    public static class Task
    {
        /// <summary>
        /// Проверка и в случае необходимости создание файла индексов idt рекурсивно для данной директории
        /// </summary>
        /// <param name="rootPath">Имя корневой директории</param>
        public static void CreateIdtFiles(string rootPath)
        {
            var ccdFiles = Directory.GetFiles(rootPath, "*.ccd", SearchOption.TopDirectoryOnly);

            var validCcdFile =
                ccdFiles.FirstOrDefault(
                    ccdFile =>
                    {
                        var pathRoot = Path.GetDirectoryName(ccdFile)??"";
                        var targetDirname = pathRoot.Split(Path.DirectorySeparatorChar).LastOrDefault()??"_1";
                        targetDirname = targetDirname.Substring(0, targetDirname.Length - 2).ToUpper();
                        return Path.GetFileNameWithoutExtension(ccdFile)?.ToUpper()== targetDirname;
                    });

            if (validCcdFile == null)
            {
                foreach (var directory in Directory.GetDirectories(rootPath))
                    CreateIdtFiles(directory);
            }
            else
            {
                try
                {
                    CreateIdt(validCcdFile);
                }
                catch (Exception e)
                {
                    var logPath =
                        $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}{Path.DirectorySeparatorChar}CDDCheckersLogs";
                    if (!Directory.Exists(logPath))
                        Directory.CreateDirectory(logPath);

                    using (var log = File.AppendText($"{logPath}{Path.DirectorySeparatorChar}{DateTime.Now.ToString("D")}.txt"))
                    {
                        log.WriteLine(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Создание индексного файла idt
        /// </summary>
        /// <param name="ccdPath">Целевой файл координатной информации ccd</param>
        public static void CreateIdt(string ccdPath)
        {
            var hashLow = File.GetCreationTime(ccdPath).ToBinary();
            var hashHigh = File.GetLastWriteTime(ccdPath).ToBinary();

            var idtPath =
                $"{Path.GetDirectoryName(ccdPath)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(ccdPath)}.idt";
            if (File.Exists(idtPath))
            {
                var idtFile = new IDTFile();
                if  (idtFile.Open(idtPath))
                {
                    var valid = idtFile.Check(hashLow, hashHigh);
                    idtFile.Dispose();
                    if (valid)
                        return;

                    File.Delete(idtPath);
                }
            }

            FillIndexFile(ccdPath, idtPath, hashLow, hashHigh);
        }

        private static void FillIndexFile(string ccdPath, string idtPath, long hashLow, long hashHigh)
        {
            const int iodRate = 7;
            const int iotRate = 9;

            using (var ccdFileStream = File.Open(ccdPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var ccdFile = new BinaryReader(ccdFileStream))
                unsafe
                {
                    var recordCount = ccdFileStream.Length/sizeof (CoordinateItem);

                    var firstRecordInFile = GetNextRecord(ccdFile);
                    ccdFileStream.Seek(-14, SeekOrigin.End);
                    var lastRecordInFile = GetNextRecord(ccdFile);

                    var odometerRecordCount = ((lastRecordInFile.Odometer - firstRecordInFile.Odometer) >> iodRate) + 1;
                    var timeRecordCount = ((lastRecordInFile.Time - firstRecordInFile.Time) >> iotRate) + 1;

                    var odometers = new uint[odometerRecordCount];
                    var times = new uint[timeRecordCount];

                    var firstOdometer = firstRecordInFile.Odometer >> iodRate;
                    var firstTime = firstRecordInFile.Time >> iotRate;
                    var lastWritedRecordNumberOdometer = 0u;
                    var lastWritedRecordNumberTime = 0u;
                    uint lastIndexOdometer = 0;
                    uint lastIndexTime = 0;

                    WriteValue(odometers, lastIndexOdometer, 0);
                    WriteValue(times, lastIndexTime, 0);

                    ccdFileStream.Seek(0, SeekOrigin.Begin);

                    for (var recordNumber = 0u; recordNumber < recordCount; recordNumber++)
                    {
                        var record = GetNextRecord(ccdFile);

                        var curOdometer = record.Odometer >> iodRate;
                        var curTime = record.Time >> iotRate;

                        if (curOdometer < lastIndexOdometer)
                        {
                            throw new Exception(
                                $"Ошибка создания idt, неверное показание одометра. Файл: {ccdPath}. Номер записи: {recordNumber}");
                        }

                        var difOdometer = curOdometer - firstOdometer - lastIndexOdometer;
                        var difTime = curTime - firstTime - lastIndexTime;

                        switch (difOdometer)
                        {
                            case 0:
                                break;

                            case 1:
                            {
                                lastIndexOdometer++;
                                WriteValue(odometers, lastIndexOdometer, recordNumber);
                                lastWritedRecordNumberOdometer = recordNumber;
                                break;
                            }

                            default:
                            {
                                if (difOdometer > UInt32.MaxValue/10000)
                                    throw new InvalidDataException(
                                        $"Ошибка создания idt, переполнение показания дистанции. Файл: {ccdPath}. Номер записи: {recordNumber}");

                                for (var j = 1; j < difOdometer; j++)
                                {
                                    lastIndexOdometer++;
                                    WriteValue(odometers, lastIndexOdometer, lastWritedRecordNumberOdometer);
                                }
                                lastIndexOdometer++;
                                WriteValue(odometers, lastIndexOdometer, recordNumber);
                                lastWritedRecordNumberOdometer = recordNumber;
                            }
                                break;
                        }

                        switch (difTime)
                        {
                            case 0:
                                break;

                            case 1:
                            {
                                lastIndexTime++;
                                WriteValue(times, lastIndexTime, recordNumber);
                                lastWritedRecordNumberTime = recordNumber;
                                break;
                            }

                            default:
                            {
                                if (difTime > UInt32.MaxValue/2)
                                    throw new InvalidDataException(
                                        $"Ошибка создания idt, переполнение показания времени. Файл: {ccdPath}. Номер записи: {recordNumber}");

                                for (var j = 1; j < difTime; j++)
                                {
                                    lastIndexTime++;
                                    WriteValue(times, lastIndexTime, lastWritedRecordNumberTime);
                                }
                                lastIndexTime++;
                                WriteValue(times, lastIndexTime, recordNumber);
                                lastWritedRecordNumberTime = recordNumber;
                            }
                                break;
                        }
                    }

                    var idtFile = new IDTFile();
                    if (!idtFile.Create(idtPath, firstOdometer, firstTime, odometerRecordCount, timeRecordCount, hashLow, hashHigh))
                        throw new IOException($"Ошибка при создании файла {idtPath}");

                    idtFile.WriteData(odometers, true);
                    idtFile.WriteData(times, false);
                    idtFile.Dispose();
                }
        }

        private static void WriteValue(uint[] dest, uint index, uint value)
        {
            if (index >= dest.Length)
                return;
            dest[index] = value;
        }


        private static CoordinateItem GetNextRecord(BinaryReader ccdFile)
        {
            return new CoordinateItem(ccdFile.ReadUInt32(), ccdFile.ReadUInt32(), ccdFile.ReadUInt32(), ccdFile.ReadUInt16());
        }
    }
}
