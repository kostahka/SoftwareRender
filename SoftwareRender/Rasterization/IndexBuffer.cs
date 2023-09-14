using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SoftwareRender.Rasterization
{
    internal unsafe class IndexBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        int[] buffer;
        public IndexBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            buffer = new int[Width * Height];
        }

        public int GetIndex(int x, int y)
        {
            return buffer[x + y * Width];
        }
        public int GetIndex(int i)
        {
            return buffer[i];
        }
        public void SetIndex(int x, int y, int index)
        {
            buffer[x + y * Width] = index;
        }

        public void ClearBuffer()
        {
            for (int i = 0; i < Width * Height; i++)
            {
                buffer[i] = 0;
            }
        }
    }
}
