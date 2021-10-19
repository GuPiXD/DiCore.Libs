using System;
using System.Runtime.CompilerServices;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public partial class WmDataProvider
    {
        private float tMax;
        private float valueK;
        private float valueT;
        private float amplSOMinCode;
        private float amplSOMaxCode;
        private int[] deltaA;
        private float[] deltaT;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem3(WMSensorData* sensorData, WMEchoRaw* dataItem, float correctionSo)
        {
            if (IsInvalidCode(dataItem->TimeCode)) return;

            ReadEchos3(dataItem);

            int soEchoIndex, wtEchoIndex;
            AnalizeEchos(out soEchoIndex, out wtEchoIndex, dataItem);

            sensorData->SO = 0;
            sensorData->WT = 0;

            if (soEchoIndex == -1) return;
            var so = IsInvalidCode(dataItem[soEchoIndex].TimeCode) ? 0 : GetSO(dataItem[soEchoIndex].TimeCode) + correctionSo;
            sensorData->SO = so < 0 ? 0 : so;
            //  sensorData->AW2 = GetAW(dataItem[soEchoIndex].AmplitudeCode);

            if (wtEchoIndex == -1) return;
            sensorData->WT = IsInvalidCode(dataItem[soEchoIndex].TimeCode) ? 0 : GetWT(dataItem[wtEchoIndex].TimeCode);
            //   sensorData->AW = GetAW(dataItem[wtEchoIndex].AmplitudeCode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem3Wt(float* sensorData, WMEchoRaw* dataItem)
        {
            if (IsInvalidCode(dataItem->TimeCode)) return;

            ReadEchos3(dataItem);

            int soEchoIndex, wtEchoIndex;
            AnalizeEchos(out soEchoIndex, out wtEchoIndex, dataItem);

            *sensorData = 0;

            if (soEchoIndex == -1) return;
            if (wtEchoIndex == -1) return;

            *sensorData = IsInvalidCode(dataItem[soEchoIndex].TimeCode) ? 0 : GetWT(dataItem[wtEchoIndex].TimeCode);
        }

        private unsafe void ReadEchos3(WMEchoRaw* dataItem)
        {
            var curPtr = sensorEchos3Ptr;

            var time = timeFirstMapPtr[dataItem->TimeCode];

            curPtr->Time = time;
            curPtr->Time100 = (int)(time * 100 + 0.5);
            curPtr->AmplitudeCode = dataItem->AmplitudeCode;
            curPtr->Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];

            dataItem++;
            curPtr++;
            time += timeNextMapPtr[dataItem->TimeCode];

            curPtr->Time = time;
            curPtr->Time100 = (int)(time * 100 + 0.5);
            curPtr->AmplitudeCode = dataItem->AmplitudeCode;
            curPtr->Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];

            dataItem++;
            curPtr++;
            time += timeNextMapPtr[dataItem->TimeCode];

            curPtr->Time = time;
            curPtr->Time100 = (int)(time * 100 + 0.5);
            curPtr->AmplitudeCode = dataItem->AmplitudeCode;
            curPtr->Amplitude = amplitudeMapPtr[dataItem->AmplitudeCode];
        }

        private unsafe void AnalizeEchos(out int soEchoIndex, out int wtEchoIndex, WMEchoRaw* dataItem)
        {
            if (IsInvalidCode(dataItem[1].TimeCode))
            {
                // Второе время битое, значит этот и все остальные сигналы - ложные
                // Необходимо проверить только первое эхо (индекс = 0)
                // на принадлежность сигналу отступа
                // Сигнал толщины считаем потерянным
                wtEchoIndex = -1;
                soEchoIndex = FindSOEcho(0, 1);
                return;
            }

            if (IsInvalidCode(dataItem[2].TimeCode))
            {
                // Третье время битое, значит выбираем только из первых двух
                soEchoIndex = FindSOEcho(0, 2);
                switch (soEchoIndex)
                {
                    case -1:
                    case 1:
                        wtEchoIndex = -1;
                        break;
                    default:
                        wtEchoIndex = FindWTEcho(1, 1, soEchoIndex);
                        break;
                }
                return;
            }

            //Все времена действительны
            soEchoIndex = FindSOEcho(0, 3);

            if (soEchoIndex == -1)
            {
                wtEchoIndex = -1;
                return;
            }

            // Если отступ - третье событие, то толщина потеряна
            switch (soEchoIndex)
            {
                case 2:
                    wtEchoIndex = -1;
                    break;
                case 1:
                    wtEchoIndex = FindWTEcho(2, 1, soEchoIndex);
                    break;
                default:
                    wtEchoIndex = FindWTEcho(1, 2, soEchoIndex);
                    break;
            }
        }

        private unsafe int FindWTEcho(int indexEchoBegin, int countTrueEchoesBegin, int soIndex)
        {
            var indexEcho = indexEchoBegin;
            var countTrueEchoes = countTrueEchoesBegin;
            var timeSO = sensorEchos3Ptr[soIndex].Time;

            // Определяем количество событий уложившихся во временной диапазон
            for (var i = 0; i < countTrueEchoesBegin; i++)
            {
                if (indexEcho + i >= sensorEchos3.Count)
                    break;
                // Если время события ближе 1 мкс к сигналу отступа то переходим к следующему индексу
                var checkWTTiming = CheckWTTiming(sensorEchos3Ptr[indexEcho + i].Time, timeSO);

                if (checkWTTiming < 0)
                {
                    indexEcho++;
                    countTrueEchoes--;
                    continue;
                }
                // Если время события больше максимально допустимого 
                if (checkWTTiming > 0)
                    countTrueEchoes--;
            }

            // Если ни одно событие не уложилось во временной диапазон
            if (indexEcho >= 3)
                return -1;

            // Проверяем события по наклонному порогу
            for (var i = 0; i < countTrueEchoes; i++)
            {
                if (CheckEchoInterlace(sensorEchos3Ptr[indexEcho + i], timeSO))
                    return indexEcho + i;
            }

            // Проверяем события по ступечатому порогу
            //var l_deltaA = new int[countTrueEchoes]; // Массив разностей амплитуд со значениями амплитуд порогов
            //var l_deltaT = new float[countTrueEchoes];// Массив времен относительно времени отступа
            var countHigh = 0; // Количество превышений порогов
            var indexHighForOneEcho = 0;
            for (var i = 0; i < countTrueEchoes; i++)
            {
                deltaT[i] = sensorEchos3Ptr[indexEcho + i].Time - timeSO;
                deltaA[i] = sensorEchos3Ptr[indexEcho + i].AmplitudeCode - GetAmplFromStep(deltaT[i]);

                if (deltaA[i] > 0)
                {
                    countHigh++;
                    indexHighForOneEcho = indexEcho + i;
                }
            }

            // Если превышений не было, то толщина не меряется (пока)
            if (countHigh == 0)
                return -1;
            // Если только одно событие превысило ступенчатый порог, то оно признается событием толщины
            if (countHigh == 1)
                return indexHighForOneEcho;
            // Если 2 события превысили ступенчатый порог, то производится вычисление
            // (deltaT = abs(t2/2 - t1))
            float dt = Math.Abs(deltaT[1] / 2 - deltaT[0]);
            // Если дельта Т меньше или равно заданной величины то первое из двух событий признается 
            // эхосигналом толщны
            if (dt <= wmParameters.ThresholdsParameters.Delta_T_Max)
                return 1;

            return (deltaA[1] > deltaA[0]) ? 2 : 1;
        }

        #region Получение величины амплитуды ступенчатого фильтра по заданному времени GetAmplFromStep()

        private int GetAmplFromStep(float dt)
        {
            //var thresholds = WMParameters.CalcParameters.ThresholdsParameters.Thresholds;
            //for (var i = 0; i < thresholds.Count; i++)
            //{
            //    if (dt <= thresholds[i].Time)
            //        return thresholds[i].Ampl;
            //}
            //return thresholds[thresholds.Count - 1].Ampl; 

            //var thresholds = WMParameters.CalcParameters.ThresholdsParameters.Thresholds;
            //var count = thresholds.Count;
            //for (var i = 0; i < count; i++)
            //{
            //    if (dt <= thresholds[i].Time)
            //        return thresholds[i].Ampl;
            //}
            //return thresholds[count - 1].Ampl;

            if (thresholdsFastArray == null)
                return 0;

            var value = (int)(dt * 100);

            var length = thresholdsFastArray.Length;

            return value < length ? thresholdsFastArray[value] : thresholdsFastArray[length - 1];
        }

        #endregion

        private unsafe bool CheckEchoInterlace(WMEcho3 echo3, float timeSO)
        {
            // Проверяем событие на превышение установленного наклонного порога
            // Aпор = К(1-dT/T)
            // где dT = Tc-Tпов
            // Tc - время регистрации события (echo.Time) 
            // К - максимальное значение порога при Тс = Тпов
            // Т - точка на временной оси, в которой порог равен нулю
            //float Tc = p_echoes[p_IndexEcho].cr_time/10f - p_timeSO;
            var dT = echo3.Time - timeSO;

            //     var apor = valueK * (1 - dT / valueT);

            var testdT = (int)(Math.Round(dT * 100, MidpointRounding.AwayFromZero));

            var echoInterlaceMapPtr = (float*)echoInterlaceMap.Data;

            var apor = echoInterlaceMapPtr[testdT];

            return echo3.AmplitudeCode > apor;
        }

        private unsafe int FindSOEcho(int indexEcho, int countTrueEchoes)
        {
            for (var i = 0; i < countTrueEchoes; i++)
            {
                var index = indexEcho + i;

                if (index > 1)
                    break;

                // Если амплитуда события больше порога SOmin, то сравниваем
                // амплитуду с порогом SOmax

                if (sensorEchos3Ptr[index].AmplitudeCode <= amplSOMinCode)
                    continue;

                // Если амплитуда события больше SOmax, то оно признается эхосигналом от поверхности
                // (сигнал отступа)

                if (sensorEchos3Ptr[index].AmplitudeCode > amplSOMaxCode)
                    return index;

                // Если нет, то в течении 1 мкс после этого события отыскиваем 
                // событие с большей амплитудой

                var indexFindMore = FindEchoFromCurrent1Mks(index, countTrueEchoes);

                // Если такого события нет (indexFindMore == 0)
                // То предыдущее событие признается событием от поверхности
                return indexFindMore == 0 ? index : indexFindMore;
            }

            return -1;
        }
        private int CheckWTTiming(float echoTime, float timeSO)
        {
            //Вычисляем время с которого необходимо начинать искать сигнал толщины (1 мкс от времени отступа)
            var lTimeSO = timeSO + 1;

            //Если время регистрации события меньше Тпов+1мкс возвращаем -1
            if (echoTime <= lTimeSO)
                return -1;

            // Если время регистрации события превышает Tmax возвращаем 1
            // Если время регистрации события попадает в заданный диапазон возвращаем 0

            return echoTime > (tMax + timeSO) ? 1 : 0;
        }


        #region Поиск события по п.3 алгоритма Шерашова (в течении 1 мкс после этого события отыскиваем событие с большей амплитудой) FindEchoFromCurrent1mks()

        private unsafe int FindEchoFromCurrent1Mks(int indexEchoCurrent, int countTrueEchoes)
        {
            // Если нашли эхо в течении 1 мкс от текущего
            // И его амплитуда больше амплитуды текущего то возвращаем индекс найденного события

            for (var i = 0; i < countTrueEchoes; i++)
            {
                if (indexEchoCurrent + i + 1 >= sensorEchos3.Count)
                    break;

                if (sensorEchos3Ptr[indexEchoCurrent].Time + 1 >= sensorEchos3Ptr[indexEchoCurrent + i + 1].Time)
                {
                    if (sensorEchos3Ptr[indexEchoCurrent].AmplitudeCode < sensorEchos3Ptr[indexEchoCurrent + i + 1].AmplitudeCode)
                    {
                        return indexEchoCurrent + i + 1;
                    }
                }
                else
                {
                    break;
                }
            }

            return 0;
        }
        #endregion

        private void PrepareOldFormatSupporting()
        {
            tMax =wmParameters.ThresholdsParameters.T_Max;
            valueK = wmParameters.ThresholdsParameters.Value_K;
            valueT = wmParameters.ThresholdsParameters.Value_T;
            amplSOMinCode = wmParameters.ThresholdsParameters.AmplSOMinCode;
            amplSOMaxCode = wmParameters.ThresholdsParameters.AmplSOMaxCode;

            deltaA = new int[3]; // Массив разностей амплитуд со значениями амплитуд порогов
            deltaT = new float[3];// Массив времен относительно времени отступа
        }
    }
}
