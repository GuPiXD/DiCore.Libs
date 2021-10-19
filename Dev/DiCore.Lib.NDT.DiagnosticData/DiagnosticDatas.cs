using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.CoordinateProvider;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.MFL;
using DiCore.Lib.NDT.DataProviders.WM;
using DiCore.Lib.NDT.DataProviders.MPM;
using DiCore.Lib.NDT.Types;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.CDPA;
using DiCore.Lib.NDT.DataProviders.EMA;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.DataProviders.NAV.NavSetup;
using DiCore.Lib.NDT.DataProviders.WM.WM32;

namespace DiCore.Lib.NDT.DiagnosticData
{
    public partial class DiagnosticData
    {
        /// <summary>
        /// Получение данных WM
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WMSensor Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных WM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="applySoCalibration">Применять калибровку SO данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа DiCore.Lib.NDT.DataProviders.WM.WMSensor (требуется вызов Dispose() после использования</returns>

        public DataHandleWm GetWmData(int scanStart, int countScan, bool applySoCalibration = false, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = (WmDataProvider)GetDataProvider(DataType.Wm);
            wmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;

            var calibration = new float[wmDataProvider.SensorCount];
            if (applySoCalibration)
            {
                calibration = wmDataProvider.GetCorrection(coordinateDataProvider.DataTypeScan2Dist(scanStart + countScan / 2, wmDataProvider.SectionOffset));
            }

            return wmDataProvider.GetWmData(scanStart, countScan, calibration, compressStep);
        }

        /// <summary>
        /// Получение данных WM
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WMSensor Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="applySoCalibration">Применять калибровку SO данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа DiCore.Lib.NDT.DataProviders.WM.WMSensor (требуется вызов Dispose() после использования</returns>
        public DataHandleWm GetWmData(double distanceStart, double distanceStop, bool applySoCalibration = false, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = GetDataProvider(DataType.Wm);
            
            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, wmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, wmDataProvider.SectionOffset);

            return GetWmData(scanStart, scanStop - scanStart, applySoCalibration, compressStep);
        }

        /// <summary>
        /// Получить тип файлов WM
        /// </summary>
        /// <returns>Тип файлов</returns>
        public ushort GetWmFileType()
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return ushort.MaxValue;

            var wmDataProvider = (WmDataProvider)GetDataProvider(DataType.Wm);
            wmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return wmDataProvider.FileType;
        }

        /// <summary>
        /// Получение данных WM WT
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных WM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetWmWtData(int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = (WmDataProvider)GetDataProvider(DataType.Wm);
            wmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return wmDataProvider.GetWtData(scanStart, countScan, compressStep);
        }

        /// <summary>
        /// Получение данных WM WT
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetWmWtData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = GetDataProvider(DataType.Wm);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, wmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, wmDataProvider.SectionOffset);

            return GetWmWtData(scanStart, scanStop-scanStart, compressStep);
        }

        /// <summary>
        /// Получение полных данных WM32
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WM32SensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных WM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WM32SensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandle<WM32SensorDataEx> GetWm32RawData(int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = (WmDataProvider)GetDataProvider(DataType.Wm);
            wmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return wmDataProvider.GetWm32Data(scanStart, countScan, compressStep);
        }

        /// <summary>
        /// Получение полных данных WM32
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WM32SensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.WM.WM32SensorDataEx Требуется вызов Dispose() после использования</returns>
        
        public DataHandle<WM32SensorDataEx> GetWm32RawData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Wm)) return null;

            var wmDataProvider = GetDataProvider(DataType.Wm);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, wmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, wmDataProvider.SectionOffset);

            return GetWm32RawData(scanStart, scanStop-scanStart, compressStep);
        }

        /// <summary>
        /// Получение провайдер  WM 
        /// </summary>
        /// <returns></returns>
        public WmDataProvider GetWMDataProvider() => (WmDataProvider)GetDataProvider(DataType.Wm);

        public DataHandleWm ConvertWm32RawDataToWmData(DataHandle<WM32Echo> sourceWm32RawData)
        {
            throw new NotImplementedException("Please email to SharovVY@ctd.transneft.ru if You have a need for this Method");
        }

        public int? DistToDataTypeScan(double distance, DataType dataType)
        {
            if (!IsOpen) return null;
            var offset = GetDataTypeOffset(dataType);
            if (!offset.HasValue) return null;
            return coordinateDataProvider.Dist2DataTypeScan(distance, offset.Value);
        }

        public double? DataTypeScanToDist(int scan, DataType dataType)
        {
            if (!IsOpen) return null;
            var offset = GetDataTypeOffset(dataType);
            if (!offset.HasValue) return null;
            return coordinateDataProvider.Scan2DataTypeDist(scan, offset.Value);
        }

        private double? GetDataTypeOffset(DataType dataType)
        {
            return GetDataProvider(dataType)?.SectionOffset;
        }

        /// <summary>
        /// Получение данных MFL/TFI
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных MFL/TFI)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="mflDataType">Тип массива магнитных данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetMflData(int scanStart, int countScan, DataType mflDataType, int compressStep = 1)
        {
            if (!mflDataType.IsMflTfiDataType()) return null;

            if (!IsOpen || !availableDataTypes.HasFlag(mflDataType)) return null;

            var mflDataProvider = (MflDataProvider)GetDataProvider(mflDataType);
            mflDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return mflDataProvider.GetData(scanStart, countScan, compressStep);
        }

        /// <summary>
        /// Получение данных MFL/TFI
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="mflDataType">Тип массива магнитных данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetMflData(double distanceStart, double distanceStop, DataType mflDataType, int compressStep = 1)
        {
            if (!mflDataType.IsMflTfiDataType()) return null;

            if (!IsOpen || !availableDataTypes.HasFlag(mflDataType)) return null;

            var mflDataProvider = GetDataProvider(mflDataType);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, mflDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, mflDataProvider.SectionOffset);

            return GetMflData(scanStart, scanStop - scanStart, mflDataType, compressStep);
        }

        /// <summary>
        /// Получение данных MPM
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных MFL/TFI)</param>
        /// <param name="scanCount">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetMpmData(int scanStart, int scanCount, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Mpm)) return null;

            var mpmDataProvider = (MpmDataProvider)GetDataProvider(DataType.Mpm);
            mpmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return mpmDataProvider.GetData(scanStart, scanCount, compressStep);
        }

        /// <summary>
        /// Получение данных MPM
        /// Объект памяти Ptr - float Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти типа float (требуется вызов Dispose() после использования</returns>
        public DataHandle<float> GetMpmData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Mpm)) return null;

            var mpmDataProvider = GetDataProvider(DataType.Mpm);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, mpmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, mpmDataProvider.SectionOffset);

            return GetMpmData(scanStart, scanStop - scanStart, compressStep);
        }

       
        public IDataProvider GetDataProvider(DataType type)
        {
            if (!DataProviders.ContainsKey(type))
                if (!CreateDataProvider(type))
                    return null;

            var dataProvider = DataProviders[type];

            if (!dataProvider.IsOpened)
            {
                dataProvider.Open(dataLocation);
                DataHelper.AdjustmentOffsetProviders(DataProviders.Values);
            }

            return dataProvider;
        }

        /// <summary>
        /// Получение полных данных CDL
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CDSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных CD)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CD.CDSensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandleCdl GetCdlData(int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cdl)) return null;

            var cdlDataProvider = (CdlDataProvider)GetDataProvider( DataType.Cdl);
            cdlDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return cdlDataProvider.GetCdlData(scanStart, countScan, compressStep, false);
        }

        /// <summary>
        /// Получение полных данных CDL
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CDSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CD.CDSensorDataEx Требуется вызов Dispose() после использования</returns>>
        public DataHandleCdl GetCdlData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cdl)) return null;

            var cdlDataProvider = GetDataProvider(DataType.Cdl);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, cdlDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, cdlDataProvider.SectionOffset);

            return GetCdlData(scanStart, scanStop - scanStart, compressStep);
        }
        
        /// <summary>
        /// Получение полных данных CD360
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CdSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="directionName">Направление</param>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных CDM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDM.CdSensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandleCdm[] GetCdmData(enDirectionName directionName, int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360)) return null;

            var cdmDataProvider = (CdmDataProvider) GetDataProvider(DataType.Cd360);
            cdmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return cdmDataProvider.GetCdmData(directionName, scanStart, countScan, compressStep).ToArray();
        }

        /// <summary>
        /// Получение полных данных CD360
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CdSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="directionName">Направление</param>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDM.CdSensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandleCdm[] GetCdmData(enDirectionName directionName, double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360)) return null;

            var cdmDataProvider = GetDataProvider(DataType.Cd360);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, cdmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, cdmDataProvider.SectionOffset);

            return GetCdmData(directionName, scanStart, scanStop - scanStart, compressStep);
        }

        /// <summary>
        /// Получение полных данных CD360
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CdSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="direction">Направление</param>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных CDM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDM.CdSensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandleCdm GetCdmData(CdmDirection direction, int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360)) return null;

            var cdmDataProvider = (CdmDataProvider)GetDataProvider(DataType.Cd360);
            cdmDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return cdmDataProvider.GetDirectionData(direction, scanStart, countScan, compressStep);
        }

        /// <summary>
        /// Получение полных данных CD360
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDL.CdSensorDataEx Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="direction">Направление</param>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDM.CdSensorDataEx Требуется вызов Dispose() после использования</returns>
        public DataHandleCdm GetCdmData(CdmDirection direction, double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360)) return null;

            var cdmDataProvider = GetDataProvider(DataType.Cd360);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, cdmDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, cdmDataProvider.SectionOffset);

            return GetCdmData(direction, scanStart, scanStop - scanStart, compressStep);
        }

        /// <summary>
        /// Получение сырых данных NavScanData
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavScanData Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavScanData Требуется вызов Dispose() после использования</returns>
        public DataHandle<NavScanData> GetNavData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Nav)) return null;

            var navDataProvider = GetDataProvider(DataType.Nav);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, navDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, navDataProvider.SectionOffset);

            return GetNavData( scanStart, scanStop - scanStart, compressStep);
        }

        /// <summary>
        /// Получение сырых данных NavScanData
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavScanData Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных Nav)</param>
        /// <param name="scanCount">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavScanData Требуется вызов Dispose() после использования</returns>
        public DataHandle<NavScanData> GetNavData(int scanStart, int scanCount, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Nav)) return null;

            var navDataProvider = (NavDataProvider)GetDataProvider(DataType.Nav);
            return navDataProvider.GetNavData(scanStart, scanCount, compressStep);
        }

        /// <summary>
        /// Получение кадра данных NavScanData
        /// </summary>
        /// <param name="scan">Скан данных (в пространстве сканов данных Nav)</param>
        /// <returns>Объект памяти NavScanData</returns>
        public NavScanData GetNavScanData(int scan)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Nav)) return NavScanData.Empty;

            var navDataProvider = (NavDataProvider)GetDataProvider(DataType.Nav);
            return navDataProvider.GetNavScanData(scan);
        }

        /// <summary>
        /// Получение сырых данных NavSetupScanData на этапе настройки снаряда
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavSetupScanData Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных Nav)</param>
        /// <param name="scanCount">Количество читаемых сканов</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.NAV.NavSetupScanData Требуется вызов Dispose() после использования</returns>
        public DataHandle<NavSetupScanData> GetNavSetupData(int scanStart, int scanCount)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.NavSetup)) return null;

            var navSetupDataProvider = (NavSetupDataProvider)GetDataProvider(DataType.NavSetup);
            return navSetupDataProvider.GetNavSetupData(scanStart, scanCount);
        }

        /// <summary>
        /// Получение полных данных EMA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.EMA.EmaSensorItem Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных EMA)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.EMA.EmaSensorItem Требуется вызов Dispose() после использования</returns>
        public DataHandleEma GetEmaData(int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Ema)) return null;

            var emaDataProvider = (EmaDataProvider)GetDataProvider(DataType.Ema);
            emaDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;

            return emaDataProvider.GetEmaData(scanStart, countScan, compressStep, false);
        }

        /// <summary>
        /// Получение полных данных EMA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.EMA.EmaSensorItem Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.EMA.EmaSensorItem Требуется вызов Dispose() после использования</returns>>
        public DataHandleEma GetEmaData(double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Ema)) return null;

            var emaDataProvider = GetDataProvider(DataType.Ema);
            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, emaDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, emaDataProvider.SectionOffset);

            return GetEmaData(scanStart, scanStop - scanStart, compressStep);
        }

        /// <summary>
        /// Получение полных данных CDPA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="directionName">Направление</param>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных CDPA)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования</returns>
        public DataHandleCDpa[] GetCDpaData(enDirectionName directionName, int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA)) return null;

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);
            cdpaDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return cdpaDataProvider.GetCDpaData(directionName, scanStart, countScan, compressStep).ToArray();
        }

        /// <summary>
        /// Получение полных данных CDPA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="directionName">Направление</param>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования</returns>
        public DataHandleCDpa[] GetCDpaData(enDirectionName directionName, double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA)) return null;

            var cdpaDataProvider = GetDataProvider(DataType.CDPA);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, cdpaDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, cdpaDataProvider.SectionOffset);

            return GetCDpaData(directionName, scanStart, scanStop - scanStart, compressStep);
        }

        /// <summary>
        /// Получение полных данных CDPA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="direction">Направление</param>
        /// <param name="scanStart">Скан начала получения данных (в пространстве сканов данных CDM)</param>
        /// <param name="countScan">Количество читаемых сканов</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования</returns>
        public DataHandleCDpa GetCDpaData(CDpaDirection direction, int scanStart, int countScan, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA)) return null;

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);
            cdpaDataProvider.CalcAligningSensors = coordinateDataProvider.FillSensorAligningMap;
            return cdpaDataProvider.GetDirectionData(direction, scanStart, countScan, compressStep);
        }

        /// <summary>
        /// Получение полных данных CDPA
        /// Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования
        /// </summary>
        /// <param name="direction">Направление</param>
        /// <param name="distanceStart">Дистанция начала чтения данных</param>
        /// <param name="distanceStop">Дистанция окончания чтения данных</param>
        /// <param name="compressStep">Шаг чтения</param>
        /// <returns>Объект памяти Ptr - DiCore.Lib.NDT.DataProviders.CDPA.CDRay Требуется вызов Dispose() после использования</returns>
        public DataHandleCDpa GetCDpaData(CDpaDirection direction, double distanceStart, double distanceStop, int compressStep = 1)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA)) return null;

            var cdpaDataProvider = GetDataProvider(DataType.CDPA);

            var scanStart = coordinateDataProvider.Dist2DataTypeScan(distanceStart, cdpaDataProvider.SectionOffset);
            var scanStop = coordinateDataProvider.Dist2DataTypeScan(distanceStop, cdpaDataProvider.SectionOffset);

            return GetCDpaData(direction, scanStart, scanStop - scanStart, compressStep);
        }

        public int? GetCarrierId(DataType dataType)
        {
            if (!DataProviders.ContainsKey(dataType)) return null;

            var dp = DataProviders[dataType] as BaseDataProvider;
            return dp?.CarrierId;
        }

        public int? GetMflId(DataType dataType)
        {
            if (!DataProviders.ContainsKey(dataType)) return null;

            var dp = DataProviders[dataType] as MflDataProvider;
            return dp?.MflId;
        }
    }
}
