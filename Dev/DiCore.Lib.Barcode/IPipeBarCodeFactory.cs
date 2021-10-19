namespace DiCore.Lib.Barcode
{
    public interface IPipeBarCodeFactory
    {
        IBarCode Create(PipeBarCodeData pipeBarCodeData);
    }
}
