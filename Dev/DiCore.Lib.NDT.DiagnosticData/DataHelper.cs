using System;
using System.Collections.Generic;
using System.Linq;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.CoordinateProvider;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.CDPA;
using DiCore.Lib.NDT.DataProviders.EMA;
using DiCore.Lib.NDT.DataProviders.MFL;
using DiCore.Lib.NDT.DataProviders.MPM;
using DiCore.Lib.NDT.DataProviders.WM;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DiagnosticData
{
    public static class DataHelper
    {
        /// <summary>
        /// Обязательная настройка провайдеров перед их использованием
        /// </summary>
        /// <param name="adjustmentProviders">Настраиваемые провайдеры</param>
        public static void AdjustmentOffsetProviders(
            Dictionary<DataType, IDataProvider>.ValueCollection adjustmentProviders)
        {
            var dataProvider = adjustmentProviders.FirstOrDefault(item => item is WmDataProvider);
            if (dataProvider == null) return;

            var wmDataProvider = (WmDataProvider) dataProvider;

            var adjustmentValue = -wmDataProvider.AdjustmentSectionOffsetValue ??
                                  wmDataProvider.SectionOffset;

            dataProvider = adjustmentProviders.FirstOrDefault(item => item is CoordinateDataProviderCrop);
            if (dataProvider == null) return;

            var coordinateDataProvider = (CoordinateDataProviderCrop) dataProvider;
            coordinateDataProvider.CalcParameters.AdjustmentSectionOffsetValue = adjustmentValue;
            coordinateDataProvider.SectionOffset = wmDataProvider.SectionOffset;

            foreach (var provider in adjustmentProviders.Where(item => item is IOffsetAdjustProvider).Cast<IOffsetAdjustProvider>())
                provider.AdjustmentSectionOffsetValue = -adjustmentValue;
        }


        /// <summary>
        /// Тип массива данных принадлежит магнитным приборам
        /// </summary>
        /// <param name="dataType">Проверяемый тип</param>
        /// <returns>Да/нет</returns>
        public static bool IsMflTfiDataType(this DataType dataType)
        {
            switch (dataType)
            {
                default:
                    return false;
                case DataType.MflT1:
                case DataType.MflT11:
                case DataType.MflT2:
                case DataType.MflT22:
                case DataType.MflT3:
                case DataType.MflT31:
                case DataType.MflT32:
                case DataType.MflT33:
                case DataType.MflT34:
                case DataType.TfiT4:
                case DataType.TfiT41:
                    return true;
            }
        }

        /// <summary>
        /// Есть ли данные для которых нужен анализ просечек
        /// </summary>
        /// <param name="dataType">Проверяемый тип</param>
        /// <returns>Да/нет</returns>
        public static bool IsNotchDataType(this DataType dataType)
        {
            switch (dataType)
            {
                default: return false;
                //case DataType.MflT1:
                //case DataType.MflT11:
                //case DataType.TfiT4:
                //case DataType.TfiT41:
                case DataType.Mpm:
                    return true;
            }
        }

        internal static Type DataTypeToProviderType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.None:
                    return null;
                case DataType.Spm:
                    return null;
                case DataType.Mpm:
                    return typeof(MpmDataProvider);
                case DataType.Wm:
                    return typeof(WmDataProvider);
                case DataType.MflT1:
                    return typeof(MflT1DataProvider);
                case DataType.MflT11:
                    return typeof(MflT11DataProvider);
                case DataType.MflT2:
                    return typeof(MflT2DataProvider);
                case DataType.MflT22:
                    return typeof(MflT22DataProvider);
                case DataType.MflT3:
                    return typeof(MflT3DataProvider);
                case DataType.MflT31:
                    return typeof(MflT31DataProvider);
                case DataType.MflT32:
                    return typeof(MflT32DataProvider);
                case DataType.MflT33:
                    return typeof(MflT33DataProvider);
                case DataType.MflT34:
                    return typeof(MflT34DataProvider);
                case DataType.TfiT4:
                    return typeof(TfiT4DataProvider);
                case DataType.TfiT41:
                    return typeof(TfiT41DataProvider);
                case DataType.Cdl:
                    return typeof(CdlDataProvider);
                case DataType.Cdc:
                    return null;
                case DataType.Cd360:
                    return typeof(CdmDataProvider);
                case DataType.CDPA:
                    return typeof(CDpaDataProvider);
                case DataType.Ema:
                    return typeof(EmaDataProvider);
                default:
                    return null;
            }
        }
    }
}
