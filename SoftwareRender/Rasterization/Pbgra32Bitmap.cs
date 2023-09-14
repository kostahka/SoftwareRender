using System;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftwareRender.Rasterization
{
    public unsafe class Pbgra32Bitmap
    {
        private byte* BackBuffer { get; set; }
        private int BackBufferStride { get; set; }
        private int BytesPerPixel { get; set; }

        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public WriteableBitmap Source { get; private set; }

        public Pbgra32Bitmap(int pixelWidth, int pixelHeight)
        {
            Source = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32, null);
            InitializeProperties();
        }

        public Pbgra32Bitmap(BitmapSource source)
        {
            Source = new WriteableBitmap(source.Format != PixelFormats.Pbgra32 ? new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0) : source);
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            PixelWidth = Source.PixelWidth;
            PixelHeight = Source.PixelHeight;
            BackBuffer = (byte*)Source.BackBuffer;
            BackBufferStride = Source.BackBufferStride;
            BytesPerPixel = Source.Format.BitsPerPixel / 8;
        }

        private byte* GetPixelAddress(int x, int y)
        {
            return BackBuffer + y * BackBufferStride + x * BytesPerPixel;
        }

        public Vector3 GetPixel(int x, int y)
        {
            byte* pixel = GetPixelAddress(x, y);
            float b = pixel[0] / 255f;
            float g = pixel[1] / 255f;
            float r = pixel[2] / 255f;
            return new Vector3(r, g, b);
        }

        public void SetPixel(int x, int y, Vector3 color)
        {
            byte* pixel = GetPixelAddress(x, y);
            pixel[0] = (byte)(255 * color.Z);
            pixel[1] = (byte)(255 * color.Y);
            pixel[2] = (byte)(255 * color.X);
            pixel[3] = 255;
        }

        public void SetPixel(int index, Vector3 color)
        {
            byte* pixel = BackBuffer + index * BytesPerPixel;
            pixel[0] = (byte)(255 * color.Z);
            pixel[1] = (byte)(255 * color.Y);
            pixel[2] = (byte)(255 * color.X);
            pixel[3] = 255;
        }

        public void ClearPixel(int x, int y)
        {
            *(int*)GetPixelAddress(x, y) = 0;
        }
        public void ClearColor(Vector3 color)
        {
            UInt32 col = BitOperations.RotateLeft((UInt32)(255 * color.X), 0) 
                + BitOperations.RotateLeft((UInt32)(255 * color.Y), 8) 
                + BitOperations.RotateLeft((UInt32)(255 * color.Z), 16)
                + BitOperations.RotateLeft((UInt32)(255), 24);
            for(int i = 0; i < PixelWidth * PixelHeight; i++)
            {
                Buffer.MemoryCopy(&col, BackBuffer + i * BytesPerPixel, BytesPerPixel, 4);
            }
            /*
            for(int y = 0; y < PixelHeight; y++)
            {
                for (int x = 0; x < PixelWidth; x++)
                {
                    byte* pixel = GetPixelAddress(x, y);
                    pixel[0] = (byte)(255 * color.Z);
                    pixel[1] = (byte)(255 * color.Y);
                    pixel[2] = (byte)(255 * color.X);
                    pixel[3] = 255;
                }
            }
            */
        }
    }
}
