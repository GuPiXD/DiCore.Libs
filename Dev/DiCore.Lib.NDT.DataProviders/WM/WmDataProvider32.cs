using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.WM.WM32;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public partial class WmDataProvider
    {
        private float speedMetalFactor;
        private float speedOilFactor;

        public Wm32CalcParameters Wm32CalcParameters { get; private set; } = new Wm32CalcParameters();

        private float dAWT = 2;               // Отношение амплитуд в окне
        private float THSO = 18.3f;         // Порог поиска SO
        private float Ta = 0.4f;            // Время анализа сигнала поверхности
        private float BLWT = 8f;          // Зона блокировки WT
        private float TimeStep = 0.20f;    // Длительность шага наклонного порога
        private float AmplStep = 1.83f;     // Шаг изменения наклонного порога по амплитуде
        private float THWT1 = 125f;         // Первый порог WT
        private float THWT2 = 6.1f;         // Второй порог WT
        private float THWT3 = 5.49f;        // Третий порог WT
        private float THWT4 = 4.88f;        // Четвертый порог WT
        private float TTH2 = 30f;            // Время действия второго порога WT
        private float TTH3 = 40f;            // Время действия третьего порога WT
        private float TTH4 = 50f;            // Время действия четвертого порога WT

        private float WIN = 50f;             // Время действия окна

        public void SetCustomWm32CalcParameters(Wm32CalcParameters newParams)
        {
            Wm32CalcParameters = newParams;

            THSO = GetDb(Wm32CalcParameters.ThSo);
            Ta = GetTime(Wm32CalcParameters.Tanalysis);
            BLWT = GetTime(Wm32CalcParameters.BlWt);
            TimeStep = GetTime(Wm32CalcParameters.TimeStep);
            AmplStep = GetDb(Wm32CalcParameters.AmplStep);
            THWT1 = GetDb(Wm32CalcParameters.ThWt1);
            THWT2 = GetDb(Wm32CalcParameters.ThWt2);
            THWT3 = GetDb(Wm32CalcParameters.ThWt3);
            THWT4 = GetDb(Wm32CalcParameters.ThWt4);
            TTH2 = GetTime(Wm32CalcParameters.TTh2);
            TTH3 = GetTime(Wm32CalcParameters.TTh3);
            TTH4 = GetTime(Wm32CalcParameters.TTh4);
            WIN = GetTime(Wm32CalcParameters.Win);
            dAWT = Wm32CalcParameters.DeltaAmplWin;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetDb(int value)
        {
            return (float)Math.Sqrt(value) * 4f;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetTime(decimal value)
        {
            return (float)value * wmParameters.TimeDiscretNext / 10;
        }

        private bool CheckWM32()
        {
            return IndexFile?.FileType == 0x4443;
        }

        private unsafe void WM32FillDataBuffer(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var arg = (Tuple<DataHandleWm, float[]>)args;

            var result = arg.Item1;
            var calibrationSo = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var dataDescriptionPrt = (byte*)(packetHeaderPtr + packetHeaderSize);
                var baseData = (WMEchoRaw*)((uint)dataDescriptionPrt + SensorCount);

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var echoCount = dataDescriptionPrt[dataSensorIndex];
                    if (echoCount == 0) continue;

                    var sensorDataEx = new WM32SafeSensorDataEx();
                    var targetData = baseData;

                    sensorDataEx.Echos = new WM32Echo[echoCount];

                    for (var i = 0; i < dataSensorIndex; i++)
                        targetData += dataDescriptionPrt[i];
                    
                    FillDataBufferItem(sensorDataEx, targetData);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);

                    GetSoWtByFilter(sensorData, sensorDataEx, calibrationSo[dataSensorIndex]);
                }
            }
        }

        private unsafe void WM32FillDataBufferWt(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var result = (DataHandle<float>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var dataDescriptionPrt = (byte*)(packetHeaderPtr + packetHeaderSize);
                var baseData = (WMEcho32Raw*)((IntPtr)dataDescriptionPrt + SensorCount);

                Parallel.ForEach(item.SensorNumbers, sensorNumber =>
                    //foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short) sensorNumber);

                    var echoCount = dataDescriptionPrt[dataSensorIndex];
                    if (echoCount == 0) return;

                    var sensorDataEx = new WM32SafeSensorDataEx();
                    var targetData = baseData;

                    sensorDataEx.Echos = new WM32Echo[echoCount];

                    for (var i = 0; i < dataSensorIndex; i++)
                        targetData += dataDescriptionPrt[i];

                    FillDataBufferItem(sensorDataEx, targetData);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);

                    GetSoWtByFilterWt(sensorData, sensorDataEx);
                });
            }
        }

        private unsafe void GetSoWtByFilter(WMSensorData* sensorData, WM32SafeSensorDataEx sensorDataEx, float correctionSo)
        {
            if (sensorDataEx.Echos.Length <= 0)
                return;

            //var coef = (ThWt1 - ThWt2)*TimeStep;
            var coef = AmplStep / TimeStep;

            float wtAmpl0 = 0;
            var time = sensorDataEx.Echos[0].Time;
            var amplitude = sensorDataEx.Echos[0].Amplitude;
            float timeSo = 0;
            float timeWt = 0;
                float aw2 = 0;
                float aw = 0;
            for (byte i = 1; i < sensorDataEx.Echos.Length; i++)
            {
                FilterByValue(coef, time, amplitude,
                    ref timeSo, ref aw2,
                    ref timeWt, ref aw, ref wtAmpl0);
                
                time = sensorDataEx.Echos[i].Time;
                amplitude = sensorDataEx.Echos[i].Amplitude;
            }

            sensorData->SO = timeSo * speedOilFactor;

            if (sensorData->SO > 0)
                sensorData->SO += correctionSo;

            if (timeWt > timeSo)
                sensorData->WT = (float) ((timeWt - timeSo) * speedMetalFactor + wtCorrectionByCarrier);
        }

        private unsafe void GetSoWtByFilterWt(float* sensorData, WM32SafeSensorDataEx sensorDataEx)
        {
            if (sensorDataEx.Echos.Length <= 0)
                return;

            //var coef = (ThWt1 - ThWt2)*TimeStep;
            var coef = AmplStep / TimeStep;

            float wtAmpl0 = 0;
            var time = sensorDataEx.Echos[0].Time;
            var amplitude = sensorDataEx.Echos[0].Amplitude;
            float timeSo = 0;
            float timeWt = 0;
                float aw2 = 0;
                float aw = 0;
            for (byte i = 1; i < sensorDataEx.Echos.Length; i++)
            {
                FilterByValue(coef, time, amplitude,
                    ref timeSo, ref aw2,
                    ref timeWt, ref aw, ref wtAmpl0);
                
                time = sensorDataEx.Echos[i].Time;
                amplitude = sensorDataEx.Echos[i].Amplitude;
            }
            
            if (timeWt > timeSo)
                *sensorData = (float) ((timeWt - timeSo) * speedMetalFactor + wtCorrectionByCarrier);
        }

        private void FilterByValue(float coef, float time, float amplitude, ref float soTime, ref float soAmpl, ref float wtTime, ref float wtAmpl, ref float wtAmpl0)
        {
            var amplAbs = Math.Abs(amplitude);
            var amplSign = amplitude > 0;
            if (MathHelper.TestFloatEquals(soTime, 0) || (time - soTime) < Ta)
            {
                if (amplSign && amplAbs > THSO && amplAbs > soAmpl)
                {
                    soTime = time;
                    soAmpl = amplitude;
                    wtTime = 0;
                    return;
                }
            }
            if (MathHelper.TestFloatEquals(soTime, 0)) return;
            if (amplSign) return;
            var ttwt = soTime + BLWT;
            if (!(time > ttwt)) return;
            var tth1 = (THWT1 - THWT2) / coef;
            float wta = 0;
            float wtt = 0;
            // 1-й наклонный порог
            if (time < ttwt + tth1)
            {
                var ampl = THWT1 - coef * (time - ttwt); // текущий наклонный порог
                if (amplAbs > ampl)
                {
                    wtt = time;
                    wta = amplAbs;
                }
            }
            // 2-й порог
            else if (time < (ttwt + tth1 + TTH2))
            {
                if (amplAbs > THWT2)
                {
                    wtt = time;
                    wta = amplAbs;
                }
            }
            // 3-й порог
            else if (time < (ttwt + tth1 + TTH2 + TTH3))
            {
                if (amplAbs > THWT3)
                {
                    wtt = time;
                    wta = amplAbs;
                }
            }
            // 4-й порог
            else if (time < (ttwt + tth1 + TTH2 + TTH3 + TTH4))
            {
                if (amplAbs > THWT4)
                {
                    wtt = time;
                    wta = amplAbs;
                }
            }

            if (wta > 0)
            {
                if (MathHelper.TestFloatEquals(wtAmpl0, 0))
                {
                    wtAmpl = wtAmpl0 = wta;
                    wtTime = wtt;
                }
                // в зоне действия окна (время а нализа WT) Отсчет времени анализа WT от последнего сигнала, признанным WT
                if (time < (wtTime + WIN))
                {
                    if (wtAmpl0 + dAWT < wta)
                    {
                        wtAmpl = wtAmpl0 = wta;
                        wtTime = wtt;
                    }
                }
            }

            //if (time < (soTime + WIN))
            //{
            //    if (wtAmpl0 + dAWT < wta)
            //    {
            //        wtAmpl = wtAmpl0 = wta;
            //        wtTime = wtt;
            //    }
            //}
        }


        private unsafe void FillDataBufferItem(WM32SafeSensorDataEx sensorData, WMEchoRaw* dataItem)
        {
            if (sensorData.Echos.Length <= 0) return;

            var time = timeFirstMapPtr[dataItem->TimeCode];

            sensorData.Echos[0].Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];
            sensorData.Echos[0].Time = time;

            for (var j = 1; j < sensorData.Echos.Length; j++)
            {
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeCode];
                sensorData.Echos[j].Amplitude =
                    amplitudeMapPtr[dataItem->AmplitudeCode]; 
                sensorData.Echos[j].Time = time;
            }
        }

        private unsafe void FillDataBufferItem(WM32SafeSensorDataEx sensorData, WMEcho32Raw* dataItem)
        {
            if (sensorData.Echos.Length <= 0) return;

            var time = timeFirstMapPtr[dataItem->TimeCode];

            sensorData.Echos[0].Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];
            sensorData.Echos[0].Time = time;

            for (var j = 1; j < sensorData.Echos.Length; j++)
            {
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeCode];
                sensorData.Echos[j].Amplitude =
                    amplitudeMapPtr[dataItem->AmplitudeCode];
                sensorData.Echos[j].Time = time;
            }
        }

        private unsafe void WM32Fill32DataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;
            var result = (DataHandle<WM32SensorDataEx>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;
#if DEBUG
                var packetHeader = (DataPacketHeader*)packetHeaderPtr;
                if (packetHeader->ScanNumber != targetScan)
                    throw new Exception($"Failed scan number. Request scan - {targetScan}; response scan - {packetHeader->ScanNumber}");
#endif
                var dataDescriptionPrt = (byte*)(packetHeaderPtr + DataPacketHeader.RawSize);
                var baseData = (WM32EchoRaw*)((IntPtr)dataDescriptionPrt + SensorCount);

                //Parallel.ForEach(item.SensorNumbers, sensorNumber =>
                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var echoCount = dataDescriptionPrt[sensorNumber];
                    if (echoCount == 0) return;

                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var targetData = baseData;

                    sensorData->Count = echoCount;

                    for (var i = 0; i < dataSensorIndex; i++)
                        targetData += dataDescriptionPrt[i];

                    sensorData->Echos = result.Allocate<WM32Echo>(sensorData->Count);

                    FillDataBufferItem(sensorData, targetData);
                }/*);*/
            }
        }


        private unsafe void WM32Fill32DataBufferColUshort(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;
            var result = (DataHandle<WM32SensorDataEx>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

#if DEBUG
                var packetHeader = (DataPacketHeader*)packetHeaderPtr;
                if (packetHeader->ScanNumber != targetScan)
                    throw new Exception($"Failed scan number. Request scan - {targetScan}; response scan - {packetHeader->ScanNumber}");
#endif

                var dataDescriptionPrt = (byte*)(packetHeaderPtr + DataPacketHeader.RawSize);
                var baseData = (WMEcho32Raw*)((IntPtr)dataDescriptionPrt + SensorCount);

                //Parallel.ForEach(item.SensorNumbers, sensorNumber =>
                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var echoCount = dataDescriptionPrt[sensorNumber];
                    if (echoCount == 0) continue;

                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var targetData = baseData;

                    sensorData->Count = echoCount;

                    for (var i = 0; i < dataSensorIndex; i++)
                        targetData += dataDescriptionPrt[i];

                    sensorData->Echos = result.Allocate<WM32Echo>(sensorData->Count);

                    FillDataBufferItem(sensorData, targetData);
                }/*);*/
            }
        }


        private unsafe void FillDataBufferItem(WM32SensorDataEx* sensorData, WM32EchoRaw* dataItem)
        {
            var echos = sensorData->Echos;
            var time = timeFirstMapPtr[dataItem->TimeIndex];

            echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex];
            echos->Time = time;

            for (var j = 1; j<sensorData->Count; j++)
            {
                echos++;
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeIndex];
                echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex];
                echos->Time = time;
            }
        }

        private unsafe void FillDataBufferItem(WM32SensorDataEx* sensorData, WMEcho32Raw* dataItem)
        {
            var echos = sensorData->Echos;
            var time = timeFirstMapPtr[dataItem->TimeCode];

            echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];
            echos->Time = time;

            for (var j = 1; j < sensorData->Count; j++)
            {
                echos++;
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeCode];
                echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];
                echos->Time = time;
            }
        }
    }
}
