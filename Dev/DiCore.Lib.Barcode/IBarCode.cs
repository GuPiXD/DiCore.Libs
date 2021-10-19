namespace DiCore.Lib.Barcode
{
    public interface IBarCode
    {
        string Text { get; }
        string[] Parts { get; }
    }
}
