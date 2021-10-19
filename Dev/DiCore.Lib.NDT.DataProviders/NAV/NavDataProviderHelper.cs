using System;
using DiCore.Lib.NDT.DataProviders.NAV.NavSetup;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    public static class NavDataProviderHelper
    {
        private const float AngleFactor = 0.01f;
        private const float ArchFactor = 0.01f;
        private const float GeoFactor = 0.1f;
        private const float AccelFactor = 0.00001f;
        
        private static unsafe void ConvertRaw2DataRdc(NavScanData* scanData, NavScanDataRaw* scanDataRaw, double coordFactor)
        {
            scanData->LinearSpeed = scanDataRaw->LinearSpeed * 0.01f; // SpeedFactor
            scanData->RollAngle = scanDataRaw->RollAngle * AngleFactor;
            scanData->ElevationAngle = scanDataRaw->ElevationAngle * AngleFactor;
            scanData->AzimuthAngle = scanDataRaw->AzimuthAngle * AngleFactor;
            scanData->VerticalArch = scanDataRaw->VerticalArch * ArchFactor;
            scanData->HorizontalArch = scanDataRaw->HorizontalArch * ArchFactor;
            scanData->AbsoluteArch = scanDataRaw->AbsoluteArch * ArchFactor;
            scanData->ArchDirection = scanDataRaw->ArchDirection * AngleFactor;

            scanData->X = scanDataRaw->X * coordFactor;
            scanData->Y = scanDataRaw->Y * coordFactor;
            scanData->Z = scanDataRaw->Z * coordFactor;

            scanData->GeodeticLatitude = scanDataRaw->GeodeticLatitude * GeoFactor;
            scanData->GeodeticLongitude = scanDataRaw->GeodeticLongitude * GeoFactor;
        }

        private static unsafe void ConvertRaw2Data(NavScanData* scanData, NavScanDataRaw* scanDataRaw)
        {
            scanData->VerticalArchSmall = scanDataRaw->VerticalArchSmall * ArchFactor;
            scanData->VerticalArchBig = scanDataRaw->VerticalArchBig * ArchFactor;
            scanData->HorizontalArchSmall = scanDataRaw->HorizontalArchSmall * ArchFactor;
            scanData->HorizontalArchBig = scanDataRaw->HorizontalArchBig * ArchFactor;

            scanData->AbsoluteArchSmall = (float)Math.Sqrt(Math.Pow(scanData->VerticalArchSmall, 2) + Math.Pow(scanData->HorizontalArchSmall, 2));
            scanData->AbsoluteArchBig = (float)Math.Sqrt(Math.Pow(scanData->VerticalArchBig, 2) + Math.Pow(scanData->HorizontalArchBig, 2));
            scanData->ArchSmallDirection = (float)(Math.Abs(scanData->VerticalArchSmall) < 0.000001d ? // Epsilon
                0.0f : (((Math.Atan(scanData->HorizontalArchSmall / scanData->VerticalArchSmall)) * 180 / Math.PI) + 360) % 360);
        }

        private static unsafe void ConverRaw2SetupData(NavScanData* scanData, NavScanDataRaw* scanDataRaw)
        {
            scanData->LinearAccelProjX = scanDataRaw->LinearAccelProjX * AccelFactor;
            scanData->LinearAccelProjY = scanDataRaw->LinearAccelProjY * AccelFactor;
            scanData->LinearAccelProjZ = scanDataRaw->LinearAccelProjZ * AccelFactor;

            scanData->AngularSpeedX = scanDataRaw->AngularSpeedX * AccelFactor;
            scanData->AngularSpeedY = scanDataRaw->AngularSpeedY * AccelFactor;
            scanData->AngularSpeedZ = scanDataRaw->AngularSpeedZ * AccelFactor;

            scanData->RollAngleAccel = scanDataRaw->RollAngleAccel * AngleFactor;
            scanData->RollAngleGyro = scanDataRaw->RollAngleGyro * AngleFactor;
            scanData->PitchAngleAccel = scanDataRaw->ElevationAngleAccel * AngleFactor;
            scanData->PitchAngleGyro = scanDataRaw->ElevationAngleGyro * AngleFactor;

            scanData->PacketArrivalTime = scanDataRaw->PacketArrivalTime * 0.001f;
        }

        private static unsafe void ConverRaw2SetupData(NavSetupScanData* scanData, NavScanDataRaw* scanDataRaw)
        {
            scanData->LinearAccelProjX = scanDataRaw->LinearAccelProjX * AccelFactor;
            scanData->LinearAccelProjY = scanDataRaw->LinearAccelProjY * AccelFactor;
            scanData->LinearAccelProjZ = scanDataRaw->LinearAccelProjZ * AccelFactor;

            scanData->AngularSpeedX = scanDataRaw->AngularSpeedX * AccelFactor;
            scanData->AngularSpeedY = scanDataRaw->AngularSpeedY * AccelFactor;
            scanData->AngularSpeedZ = scanDataRaw->AngularSpeedZ * AccelFactor;

            scanData->RollAngleAccel = scanDataRaw->RollAngleAccel * AngleFactor;
            scanData->RollAngleGyro = scanDataRaw->RollAngleGyro * AngleFactor;
            scanData->PitchAngleAccel = scanDataRaw->ElevationAngleAccel * AngleFactor;
            scanData->PitchAngleGyro = scanDataRaw->ElevationAngleGyro * AngleFactor;

            scanData->PacketArrivalTime = scanDataRaw->PacketArrivalTime * 0.001f;
        }

        public static unsafe void ConvertRawToData(NavScanData* scanData, NavScanDataRaw* scanDataRaw, double coordFactor)
        {
            ConvertRaw2DataRdc(scanData, scanDataRaw, coordFactor);
            ConvertRaw2Data(scanData, scanDataRaw);
        }

        public static unsafe void ConverRawToDataRdc(NavScanData* scanData, NavScanDataRaw* scanDataRaw, double coordFactor)
        {
            ConvertRaw2DataRdc(scanData, scanDataRaw, coordFactor);
        }

        public static unsafe void ConverRawToDataExt(NavScanData* scanData, NavScanDataRaw* scanDataRaw, double coordFactor)
        {
            ConvertRaw2DataRdc(scanData, scanDataRaw, coordFactor);
            ConvertRaw2Data(scanData, scanDataRaw);
            ConverRaw2SetupData(scanData, scanDataRaw);
        }

        public static unsafe void ConverRawToSetupData(NavSetupScanData* scanData, NavScanDataRaw* scanDataRaw)
        {
            ConverRaw2SetupData(scanData, scanDataRaw);
        }

        public static enNavType CheckNavType(ushort typeCode)
        {
            switch (typeCode)
            {
                case 0x304E: return enNavType.Adis;
                case 0x314E: return enNavType.Bins;
                case 0x324E: return enNavType.Bep;
                default: return enNavType.None;
            }
        }
    }
}
