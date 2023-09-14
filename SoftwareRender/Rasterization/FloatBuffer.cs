using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Rasterization
{
    internal class FloatBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        float[] buffer;
        public FloatBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            buffer = new float[Width * Height];
        }

        public float GetValue(int x, int y)
        {
            return buffer[x + y * Width];
        }
        public float GetValue(int i)
        {
            return buffer[i];
        }
        public void SetValue(int x, int y, float val)
        {
            buffer[x + y * Width] = val;
        }

        public void ClearBuffer(float val)
        {
            for (int i = 0; i < Width * Height; i++)
            {
                buffer[i] = val;
            }
        }
    }
}
