namespace DiCore.Lib.Barcode
{
    public class PipeBarCodeHammingFactory:IPipeBarCodeFactory
    {
        

        public IBarCode Create(PipeBarCodeData pipeBarCodeData)
        {
            return new PipeBarCodeHamming(pipeBarCodeData);
        }
    }
}