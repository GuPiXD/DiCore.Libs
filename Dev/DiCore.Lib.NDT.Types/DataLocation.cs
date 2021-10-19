using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiascanIO = Diascan.Utils.IO;

namespace DiCore.Lib.NDT.Types
{
    public class DataLocation
    {  /// <summary>
        /// Код прогона
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Корневой каталог
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Имя базового файла
        /// </summary>
        public string BaseFile { get; set; }

        /// <summary>
        /// Имя инспекционного каталога
        /// </summary>
        public string InspectionDirName { get; set; }

        /// <summary>
        /// Полный путь к инспекционному каталогу
        /// </summary>        
        public string InspectionFullPath { get; set; }

        /// <summary>
        /// Частичный путь к папкам данных
        /// </summary>
        public string DataBasePath => String.IsNullOrEmpty(BaseFile) ? InspectionFullPath : Path.Combine(InspectionFullPath, BaseName);



        public string BaseName => Path.GetFileNameWithoutExtension(BaseFile);

        /// <summary>
        /// Полный путь к файлу описания прогона
        /// </summary>
        public string FullPath => String.IsNullOrEmpty(Path.GetExtension(BaseFile))
            ? String.Concat(BaseDirectory, Path.DirectorySeparatorChar, BaseFile, ".omni")
            : String.Concat(BaseDirectory, Path.DirectorySeparatorChar, BaseFile);
    }

    public static class DataLocationHelper
    {
        internal static bool TestAvailable(this DataLocation location)
        {
            return Diascan.Utils.IO.Directory.Exists(location.InspectionFullPath);
        }

        public static DataLocation CreateDiagDataLocation(string omniFileFullName)
        {
            var inspectionsDirs = SearchInspectionsDir(omniFileFullName);

            return new DataLocation()
            {
                BaseFile = Path.GetFileName(omniFileFullName),
                BaseDirectory = Path.GetDirectoryName(omniFileFullName),
                InspectionDirName = new DirectoryInfo(inspectionsDirs[0]).Name,
                InspectionFullPath = inspectionsDirs[0]
            };
        }

        /// <summary>
        /// Поиск имен инспекционных каталогов
        /// </summary>
        /// <param name="omniFilePath">Полный путь к файлу описания прогона "*.omni"</param>
        private static string[] SearchInspectionsDir(string omniFilePath)
        {
            var inspectionsDir = DiascanIO.Directory.EnumerateDirectories(DiascanIO.Path.GetDirectoryName(omniFilePath))
                .Where(s => s.Contains($"{Path.GetFileNameWithoutExtension(DiascanIO.Path.GetFileName(omniFilePath)) }_"))
                .ToArray();

            for (var i = 0; i < inspectionsDir.Length; i++)
            {
                var dirInfo = new DirectoryInfo(inspectionsDir[i]);
                if (dirInfo.Exists)
                {
                    var strArry = dirInfo.Name.Split(new string[] { $"{Path.GetFileNameWithoutExtension(DiascanIO.Path.GetFileName(omniFilePath))}_" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArry.Length == 1)
                        if (new Regex("[0-9]{1}").IsMatch(strArry[0]) && !new Regex("[A-Za-z]{1}").IsMatch(strArry[0]))
                            continue;
                        else
                            inspectionsDir = inspectionsDir.Remove(i);
                    else
                        inspectionsDir = inspectionsDir.Remove(i);
                }
                else
                    inspectionsDir = inspectionsDir.Remove(i);
            }

            return inspectionsDir;
        }

        private static string[] Remove(this string[] oldStrArrey, int removeItemOfIndex)
        {
            var newStrArrey = new string[oldStrArrey.Length - 1];

            for (int i = 0, j = 0; i < newStrArrey.Length; ++i, ++j)
            {
                if (j == removeItemOfIndex)
                    ++j;

                newStrArrey[i] = oldStrArrey[j];
            }

            return newStrArrey;
        }
    }


}

