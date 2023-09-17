using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoftwareRender.Rasterization
{
    public class RenderCanvas : Pbgra32Bitmap
    {
        private Int32Rect drawArea;
        public RenderCanvas(int pixelWidth, int pixelHeight) : base(pixelWidth, pixelHeight)
        {
            drawArea = new(0, 0, PixelWidth, PixelHeight);
        }

        public void DrawLineBresenhem(int x0, int y0, int x1, int y1, Vector3 color)
        {

            int deltaX = Math.Abs(x0 - x1);
            int deltaY = Math.Abs(y0 - y1);
            int error = 0;
            int deltaErr = deltaY + 1;
            int dY = y1 - y0 > 0 ? 1 : -1;
            int dX = x1 - x0 > 0 ? 1 : -1;

            int y = y0;
            int x = x0;

            while(x != x1)
            {
                if(x >= 0 &&  y >= 0 && x < PixelWidth && y < PixelHeight)
                    SetPixel(x, y, color);
                error += deltaErr;
                while (error >= deltaX + 1)
                {
                    y += dY;
                    error -= deltaX + 1;
                }
                x += dX;
            }
        }
        
        public void SwapBuffers()
        {
            try
            {
                Source.Lock();
                Source.AddDirtyRect(drawArea);
            }
            finally
            {
                Source.Unlock();
            }
        }
    }
}
