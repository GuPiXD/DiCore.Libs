using DiCore.Lib.CoordinateProvider;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    static class IdtFileHelper
    {
        public static bool CheckIndexFile(this IdtFile file, string path, long hashLow, long hashHigh)
        {
            if (file.Open(path))
            {
                if (file.CheckHash(hashLow, hashHigh)) return true;
                file.Close();
            }
            return false;
        }
    }
}
