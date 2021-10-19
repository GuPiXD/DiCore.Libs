using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;
using Directory = Diascan.Utils.IO.Directory;
using File = Diascan.Utils.IO.File;

namespace DiCore.Lib.NDT.DataProviders
{
    public static class TestDataHelper
    {
        private static bool TestData(DataLocation location, DiagdataDescription description)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = location.DataBasePath;
            var fullPath = partDataTypePath + description.DataDirSuffix + location.BaseName;

            var searchExt = description.PointerFileExt.FirstOrDefault(q => File.Exists($"{fullPath}{q}"));
            if (String.IsNullOrEmpty(searchExt))
                return false;

            return CheckThisType(fullPath, searchExt, description.IndexFileExt, description.DataFileExt);
        }

        public static bool TestCoordinateData(DataLocation location)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(location.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 location.BaseName);
            //Проверка базовой директории
            return File.Exists(String.Concat(partDataTypePath, ".ccd"));
        }

        public static bool CheckThisType(string fullPath, string pointerFileExt, string indexFileExt, string dataFileExt)
        {
            if (!CheckPointerFile(fullPath, pointerFileExt)) return false;

             var directoryName = Diascan.Utils.IO.Path.GetDirectoryName(fullPath);

            if (directoryName == null)
                return false;
            var files = Directory.EnumerateFiles(directoryName, $"{Path.GetFileName(fullPath)}_??????{indexFileExt}");
            var countIndexFile = files.Count(file => Diascan.Utils.IO.Path.GetExtension(file).ToLower() == indexFileExt);
            if (countIndexFile == 0)
                return false;

            files = Directory.EnumerateFiles(directoryName, $"{Path.GetFileName(fullPath)}_??????{dataFileExt}");
            var countDataFile = files.Count(file => Diascan.Utils.IO.Path.GetExtension(file).ToLower() == dataFileExt);

            if (countDataFile == 0)
                return false;

            return true;
        }

        public static bool TestDataCDL(DataLocation location)
        {
            return TestData(location, Constants.CDLDataDescription);
        }

        public static bool TestDataCDC(DataLocation location)
        {
            return TestData(location, Constants.CDCDataDescription);
        }

        public static bool TestDataCD360(DataLocation location)
        {
            if (!Directory.Exists(location.InspectionFullPath))
                return false;

            string[] dirs = Directory.GetDirectories(location.InspectionFullPath, null);

            var pattern = @"(DATACD){1}";
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            var cdDirs = dirs.Where(dir => rgx.IsMatch(dir)).ToList();

            if (cdDirs.Count < 2)
                return false;

            var fullPaths = cdDirs.Select(cdDir => Path.Combine(cdDir, location.BaseName)).ToList();
            return fullPaths.Any(fullPath=>CheckThisType(fullPath, Constants.CD360DataDescription.PointerFileExt.FirstOrDefault(), Constants.CD360DataDescription.IndexFileExt, Constants.CD360DataDescription.DataFileExt));
        }

        public static bool TestDataCDPA(DataLocation location)
        {
            if (!Directory.Exists(location.InspectionFullPath))
                return false;

            string[] dirs = Directory.GetDirectories(location.InspectionFullPath, null);

            var pattern = @"(DATACD){1}";
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            var cdDirs = dirs.Where(dir => rgx.IsMatch(dir)).ToList();

            if (cdDirs.Count < 2)
                return false;

            var fullPaths = cdDirs.Select(cdDir => Path.Combine(cdDir, location.BaseName)).ToList();
            return fullPaths.Any(fullPath => CheckThisType(fullPath, Constants.CDPADataDescription.PointerFileExt.FirstOrDefault(), Constants.CDPADataDescription.IndexFileExt, Constants.CDPADataDescription.DataFileExt));
        }

        public static bool TestDataWM(DataLocation location)
        {
            return TestData(location, Constants.WMDataDescription);
        }

        public static bool TestDataMS(DataLocation location)
        {
            return TestData(location, Constants.MSDataDescription);
        }

        public static bool TestDataMFL(DataLocation location)
        {
            return TestData(location, Constants.MFLT1DataDescription) ||
                   TestData(location, Constants.MFLT2DataDescription) ||
                   TestData(location, Constants.MFLT3DataDescription);
        }

        public static bool TestDataNewMFL(DataLocation location)
        {
            return TestData(location, Constants.MFLT11DataDescription) ||
                   TestData(location, Constants.MFLT22DataDescription) ||
                   TestData(location, Constants.MFLT31DataDescription) ||
                   TestData(location, Constants.MFLT32DataDescription) ||
                   TestData(location, Constants.MFLT33DataDescription) ||
                   TestData(location, Constants.MFLT34DataDescription);
        }

        public static bool TestDataTFI(DataLocation location)
        {
            return TestData(location, Constants.TFIDataDescription) ||
                   TestData(location, Constants.MFLT41DataDescription);
        }

        public static bool TestDataPF(DataLocation location)
        {
            return TestData(location, Constants.PFDataDescription);
        }

        public static bool TestDataNV(DataLocation location)
        {
            return TestData(location, Constants.NVDataDescription);
        }

        public static bool TestDataNVSetup(DataLocation location)
        {
            var desc = Constants.NVSetupDataDescription;
            var path = location.DataBasePath + desc.DataDirSuffix + location.BaseName;
            var fullPath = string.Concat(path, $"_{0:d6}", desc.DataFileExt);

            return File.Exists(fullPath);
        }

        public static bool TestDataSPF(DataLocation location)
        {
            return TestData(location, Constants.SPFDataDescription);
        }

        public static bool TestDataEMA(DataLocation location)
        {
            return TestData(location, Constants.EMADataDescription);
        }

        public static bool TestDataTELE(DataLocation location)
        {
            return TestTELEData(location);
        }

        public static bool TestDataTELEbkp(DataLocation location)
        {
            return TestTELEbkpData(location);
        }

        public static bool TestDataTELEcdwm(DataLocation location)
        {
            return TestTELEcdwmData(location);
        }

        public static bool TestDataTELEmfl(DataLocation location)
        {
            return TestTELEmflData(location);
        }

        public static bool TestDataTELEebs(DataLocation location)
        {
            return TestTELEebsData(location);
        }

        public static bool TestDataTELEevn(DataLocation location)
        {
            return TestTELEevnData(location);
        }

        public static bool TestDataTELE(DataLocation location, string extension)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = Path.Combine(location.InspectionFullPath, location.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + location.BaseName;

            return CheckPointerFile(fullPath, extension);
        }

        public static DataType TestDataType(DataLocation location)
        {
            var result = DataType.None;

            if (TestDataSPF(location)) result |= DataType.Spm;
            if (TestDataPF(location)) result |= DataType.Mpm;
            if (TestDataWM(location)) result |= DataType.Wm;
            if (TestData(location, Constants.MFLT1DataDescription)) result |= DataType.MflT1;
            if (TestData(location, Constants.MFLT11DataDescription)) result |= DataType.MflT11;
            if (TestData(location, Constants.MFLT2DataDescription)) result |= DataType.MflT2;
            if (TestData(location, Constants.MFLT22DataDescription)) result |= DataType.MflT22;
            if (TestData(location, Constants.MFLT3DataDescription)) result |= DataType.MflT3;
            if (TestData(location, Constants.MFLT31DataDescription)) result |= DataType.MflT31;
            if (TestData(location, Constants.MFLT32DataDescription)) result |= DataType.MflT32;
            if (TestData(location, Constants.MFLT33DataDescription)) result |= DataType.MflT33;
            if (TestData(location, Constants.MFLT34DataDescription)) result |= DataType.MflT34;
            if (TestData(location, Constants.TFIDataDescription)) result |= DataType.TfiT4;
            if (TestData(location, Constants.MFLT41DataDescription)) result |= DataType.TfiT41;
            if (TestDataCDL(location)) result |= DataType.Cdl;
            if (TestDataCDC(location)) result |= DataType.Cdc;
            if (TestDataCD360(location)) result |= DataType.Cd360;
            if (TestDataCDPA(location)) result |= DataType.CDPA;
            if (TestDataEMA(location)) result |= DataType.Ema;
            if (TestDataNV(location)) result |= DataType.Nav;
            if (TestDataNVSetup(location)) result |= DataType.NavSetup;

            return result;
        }

        private static bool TestTELEData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".bkp") || CheckPointerFile(fullPath, ".cdwm") || CheckPointerFile(fullPath, ".mfl") || CheckPointerFile(fullPath, ".ebs") || CheckPointerFile(fullPath, ".evn");
        }

        private static bool TestTELEbkpData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".bkp");
        }

        private static bool TestTELEcdwmData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".cdwm");
        }

        private static bool TestTELEmflData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".mfl");
        }

        private static bool TestTELEebsData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".ebs");
        }

        private static bool TestTELEevnData(DataLocation session)
        {
            //Частичный путь к папкам данных
            var partDataTypePath = String.Concat(session.InspectionFullPath, Path.DirectorySeparatorChar,
                                                 session.BaseName);

            var fullPath = partDataTypePath + @"_TELE" + @"\" + session.BaseName;

            return CheckPointerFile(fullPath, ".evn");
        }

        private static bool CheckPointerFile(string fullPath, string extension)
        {            
            var pointerFilePath = fullPath + extension;

            if (!File.Exists(pointerFilePath))
                return false;

            const int recordSize = sizeof(uint) * 2;                        
            var pointerLength = File.GetLength(pointerFilePath);
            return (pointerLength >= recordSize) && ((pointerLength % recordSize) == 0);
        }
    }
}
