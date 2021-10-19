using System.Drawing;

namespace DiCore.Lib.Barcode
{
    public interface IBarCodeDrawer
    {
        Image Draw(IBarCode barCode, int width, int height);
    }

}
