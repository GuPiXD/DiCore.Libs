using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DiCore.Lib.Barcode
{
    public class PipeBarCodeDrawer:IBarCodeDrawer
    {
        private class BarCodeRectangle
        {
            public BarCodeRectangle(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }
            public int Height { get; }

            private static readonly Dictionary<char, BarCodeRectangle> RectanglesDict =
                new Dictionary<char, BarCodeRectangle>()
                {
                    {'0', new BarCodeRectangle(5, 5)},
                    {'1', new BarCodeRectangle(2, 2)},
                    {'2', new BarCodeRectangle(2, 3)},
                    {'3', new BarCodeRectangle(2, 4)},
                    {'4', new BarCodeRectangle(2, 5)},
                    {'5', new BarCodeRectangle(3, 2)},
                    {'6', new BarCodeRectangle(3, 3)},
                    {'7', new BarCodeRectangle(3, 4)},
                    {'8', new BarCodeRectangle(3, 5)},
                    {'9', new BarCodeRectangle(4, 2)},
                    {'A', new BarCodeRectangle(4, 3)},
                    {'a', new BarCodeRectangle(4, 3)},
                    {'B', new BarCodeRectangle(4, 4)},
                    {'b', new BarCodeRectangle(4, 4)},
                    {'C', new BarCodeRectangle(4, 5)},
                    {'c', new BarCodeRectangle(4, 5)},
                    {'D', new BarCodeRectangle(5, 2)},
                    {'d', new BarCodeRectangle(5, 2)},
                    {'E', new BarCodeRectangle(5, 3)},
                    {'e', new BarCodeRectangle(5, 3)},
                    {'F', new BarCodeRectangle(5, 4)},
                    {'f', new BarCodeRectangle(5, 4)},
                    {' ', new BarCodeRectangle(1, 6)},
                    {'-', new BarCodeRectangle(1, 6)}
                };

            public static BarCodeRectangle Create(char x)
            {
                return RectanglesDict.ContainsKey(x) ? RectanglesDict[x] : new BarCodeRectangle(0, 0);
            }
        }

        private IEnumerable<BarCodeRectangle> GetBarcodeRectangles(string data)
        {
            yield return BarCodeRectangle.Create(' ');
            foreach (var c in data)
            {
                yield return BarCodeRectangle.Create(c);
            }
            yield return BarCodeRectangle.Create(' ');
        }

        public Image Draw(IBarCode barCode, int width, int height)
        {

            var image = new Bitmap(width, height);
            using (var gfx = Graphics.FromImage(image))
            using (var brush = new SolidBrush(Color.Black))
            {

                var rectangles = GetBarcodeRectangles(barCode.Text).ToArray();
                var heigthInRecs = rectangles.Max(r => r.Height);
                var widthInRecs = rectangles.Sum(r => r.Width) + rectangles.Length - 1;
                var rectangleWidth = width/widthInRecs;
                var rectangleHeight = height/heigthInRecs;
                var positonX = 0;
                foreach (var barCodeRectangle in rectangles)
                {

                    gfx.FillRectangle(brush, positonX, 0, barCodeRectangle.Width*rectangleWidth,
                        barCodeRectangle.Height*rectangleHeight);
                    positonX += (barCodeRectangle.Width + 1)*rectangleWidth;
                }
                
            }
            return image;
        }
    }
}
