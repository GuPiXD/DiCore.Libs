using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders
{
    public static class Constants
    {
        public static readonly DiagdataDescription CDPADataDescription = new DiagdataDescription(DataType.CDPA, ".cdp", ".cdi", ".cdf", 0x4D45, 0x4D46, @"_DATACDF000\");
        public static readonly DiagdataDescription CDLDataDescription = 
            new DiagdataDescription(DataType.Cdl, ".cdp", ".cdi", ".cdd", 0x4443, 0x4444, @"_DATACD\");
        
        public static readonly DiagdataDescription CDCDataDescription =
            new DiagdataDescription(DataType.Cdc, ".cdp", ".cdi", ".cdd", 0x4443, 0x4444, @"_DATACDC\");

        public static readonly DiagdataDescription CD360DataDescription =
            new DiagdataDescription(DataType.Cd360, ".cdp", ".cdi", ".cdd",  0x4443, 0x4444, @"_DATACD000\");

        public static readonly DiagdataDescription WMDataDescription =
            new DiagdataDescription(DataType.Wm, ".wmp", ".wmi", ".wmd", 0x4D57, 0x4D58, @"_DATAWM\");

        public static readonly DiagdataDescription MFLT1DataDescription  = new DiagdataDescription(DataType.MflT1, new[] { ".mdp", ".mdp1" }, ".mi1", ".md1", 0x314D, 0x314E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT11DataDescription = new DiagdataDescription(DataType.MflT11,new[] { ".mdp", ".mdp11" }, ".mi11", ".md11", 0x0B4D, 0x0B4E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT2DataDescription  = new DiagdataDescription(DataType.MflT2, new[] { ".mdp", ".mdp2" }, ".mi2", ".md2", 0x324D, 0x324E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT22DataDescription = new DiagdataDescription(DataType.MflT22, new[] {".mdp", ".mdp22"}, ".mi22", ".md22", 0x324D, 0x324E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT3DataDescription  = new DiagdataDescription(DataType.MflT3,  new[] {".mdp", ".mdp3"}, ".mi3", ".md3", 0x334D, 0x334E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT31DataDescription = new DiagdataDescription(DataType.MflT31, new[] {".mdp", ".mdp31"}, ".mi31", ".md31", 0x1F4D, 0x1F4E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT32DataDescription = new DiagdataDescription(DataType.MflT32, new[] {".mdp", ".mdp32"}, ".mi32", ".md32", 0x204D, 0x204E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT33DataDescription = new DiagdataDescription(DataType.MflT33, new[] {".mdp", ".mdp33"}, ".mi33", ".md33", 0x214D, 0x214E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT34DataDescription = new DiagdataDescription(DataType.MflT34, new[] {".mdp", ".mdp34"}, ".mi34", ".md34", 0x224D, 0x224E, @"_DATAMFL\");
        public static readonly DiagdataDescription TFIDataDescription    = new DiagdataDescription(DataType.TfiT4,  new[] {".mdp", ".mdp4"}, ".mi4", ".md4", 0x344D, 0x344E, @"_DATAMFL\");
        public static readonly DiagdataDescription MFLT41DataDescription = new DiagdataDescription(DataType.TfiT41, new[] {".mdp", ".mdp41"}, ".mi41", ".md41", 0x294D, 0x294E, @"_DATAMFL\");

        public const string TfiCalibrationFileExtention = ".tmb";

        public static readonly DiagdataDescription PFDataDescription =
            new DiagdataDescription(DataType.Mpm, ".pfp", ".pfi", ".pfd", 0x4650, 0x4651, @"_DATAPF\");

        public static readonly DiagdataDescription NVDataDescription =
            new DiagdataDescription(DataType.Nav, ".nvp", ".nvi", ".nvd", 0x564E, 0x564F, @"_DATANV\");
        public static readonly DiagdataDescription NVSetupDataDescription =
            new DiagdataDescription(DataType.NavSetup, string.Empty, string.Empty, ".nvd0", 0x564E, 0x564F, @"_DATANV\");

        public static readonly DiagdataDescription MSDataDescription =
            new DiagdataDescription(DataType.None, ".msp", ".msi", ".msd", 0x534D, 0x534E, @"_DATAMS\");

        public static readonly DiagdataDescription SPFDataDescription =
            new DiagdataDescription(DataType.Spm, ".spfp", ".spfi", ".spfd", 0x5350, 0x4651, @"_DATASPF\");

        public static readonly DiagdataDescription EMADataDescription =
            new DiagdataDescription(DataType.Ema, ".emp", ".emi", ".ema", 0x4D45, 0x4D46, @"_DATAEMA\");

        public const float CapacityCachingFactor = 1.03f;
        public const float CapacityCachingFactorProgressive = 1.2f;

        public const int BottomCommandsStartPosition = 99999;

        public static DiagdataDescription[] GetAllDescriptions()
        {
            return new[]
            {
                CDLDataDescription,
                CDPADataDescription,
                CDCDataDescription,
                CD360DataDescription,
                WMDataDescription,
                MFLT1DataDescription,
                MFLT11DataDescription,
                MFLT2DataDescription,
                MFLT22DataDescription,
                MFLT3DataDescription,
                MFLT31DataDescription,
                MFLT32DataDescription,
                MFLT33DataDescription,
                MFLT34DataDescription,
                TFIDataDescription,
                MFLT41DataDescription,
                PFDataDescription,
                NVDataDescription,
                SPFDataDescription,
                EMADataDescription,
            };
        }
    }
}