namespace DiCore.Lib.NDT.DataProviders.WM
{
    public class Wm32CalcParameters
    {
        public void Setup(int thso, int tanalysis, int blwt, int timeStep, int amplStep, int thw1, int thw2, int thw3, int thw4, int tth2, int tth3, int tth4, int win, float deltaAmplWin)
        {
            ThSo = thso;
            Tanalysis = tanalysis;
            BlWt = blwt;
            TimeStep = timeStep;
            AmplStep = amplStep;
            ThWt1 = thw1;
            ThWt2 = thw2;
            ThWt3 = thw3;
            ThWt4 = thw4;
            TTh2 = tth2;
            TTh3 = tth3;
            TTh4 = tth4;
            Win = win;
            DeltaAmplWin = deltaAmplWin;
        }
        
        /// <summary>
        /// Порог поиска SO
        /// </summary>
        public int ThSo { get; set; } = 60;

        /// <summary>
        /// Время анализа сигнала поверхности
        /// </summary>
        public int Tanalysis { get; set; } = 20;

        /// <summary>
        /// Зона блокировки WT
        /// </summary>
        public int BlWt { get; set; } = 40;

        /// <summary>
        /// Длительность шага наклонного порога
        /// </summary>
        public int TimeStep { get; set; } = 1;

        /// <summary>
        /// Шаг изменения наклонного порога по амплитуде
        /// </summary>
        public int AmplStep { get; set; } = 6;

        /// <summary>
        /// Первый порог WT
        /// </summary>
        public int ThWt1 { get; set; } = 200;

        /// <summary>
        /// Второй порог WT
        /// </summary>
        public int ThWt2 { get; set; } = 20;

        /// <summary>
        /// Третий порог WT
        /// </summary>
        public int ThWt3 { get; set; } = 18;

        /// <summary>
        /// Четвертый порог WT
        /// </summary>
        public int ThWt4 { get; set; } = 16;

        /// <summary>
        /// Время действия второго порога WT
        /// </summary>
        public int TTh2 { get; set; } = 150;

        /// <summary>
        /// Время действия третьего порога WT
        /// </summary>
        public int TTh3 { get; set; } = 200;

        /// <summary>
        /// Время действия четвертого порога WT
        /// </summary>
        public int TTh4 { get; set; } = 250;

        /// <summary>
        /// Время действия окна
        /// </summary>
        public int Win { get; set; } = 250;


        /// <summary>
        /// Отношение амплитуд в окне
        /// </summary>
        public float DeltaAmplWin { get; set; } = 2;


        /// <summary>
        /// Медианный фильтр 
        /// </summary>
        public bool MedianFilterEnabled { get; set; } = false;
    }
}
