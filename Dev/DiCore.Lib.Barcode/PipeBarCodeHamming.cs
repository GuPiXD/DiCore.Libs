namespace DiCore.Lib.Barcode
{
    public class PipeBarCodeHamming:IBarCode
    {
        private readonly PipeBarCodeData pipeBarCodeData;

        private readonly ICheckSumCalculator checkSumCalculator;
        public PipeBarCodeHamming(PipeBarCodeData pipeBarCodeData)
        {
            this.pipeBarCodeData = pipeBarCodeData;
            checkSumCalculator = new HammingCheckSumCalculator();
        }
        
        public string Text => string.Join("-", Parts);

        public string[] Parts => new[]
        {
            pipeBarCodeData.FactoryCode,
            pipeBarCodeData.PartNumber,
            pipeBarCodeData.PipeNumber,
            checkSumCalculator.Calculate(pipeBarCodeData.ToString())
        };
    }
}
