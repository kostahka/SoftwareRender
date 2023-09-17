using Pfim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftwareRender.Render
{
    internal class Texture
    {
        private Vector3[] colors;
        private int bufferWidth;
        private int bufferHeight;
        private static PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }
        public Texture(string path) 
        {
            var ext = System.IO.Path.GetExtension(path).Trim();
            BitmapSource Bitmap;
            BitmapPalette bPalette = new(new List<Color>() { Colors.Blue, Colors.Green, Colors.Blue });

            if(ext == ".tga")
            {
                using(var image = Pfimage.FromFile(path))
                {
                    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    var addr = handle.AddrOfPinnedObject();
                    Bitmap = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
                        PixelFormat(image), null, addr, image.DataLen, image.Stride);
                    handle.Free();
                }
            }
            else
            {
                var p = new Uri(path, UriKind.RelativeOrAbsolute);
                Bitmap = new BitmapImage(p);
            }
            Bitmap = new FormatConvertedBitmap(Bitmap, PixelFormats.Pbgra32, bPalette, 0.0);

            bufferWidth = Bitmap.PixelWidth;
            bufferHeight = Bitmap.PixelHeight;
            colors = new Vector3[bufferWidth * bufferHeight];

            int stride = (int)Bitmap.PixelWidth * (Bitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[(int)Bitmap.PixelHeight * stride];

            Bitmap.CopyPixels(pixels, stride, 0);
            for(int y = 0; y < bufferHeight; y++)
            {
                for(int x = 0; x < bufferWidth; x++)
                {
                    int startIndex = (stride * (bufferHeight - 1 - y)) + x * Bitmap.Format.BitsPerPixel / 8;
                    float b = pixels[startIndex + 0] / 255f;
                    float g = pixels[startIndex + 1] / 255f;
                    float r = pixels[startIndex + 2] / 255f;
                    colors[x + y * bufferWidth] = new(r, g, b);
                }
            }
        }
        public Vector3 GetPixel(float x, float y)
        {
            x = x - MathF.Floor(x);
            y = y - MathF.Floor(y);
            int tx = ((int)(bufferWidth * x));
            int ty = ((int)(bufferHeight * y));

            int index = tx + ty * bufferWidth;

            return colors[index];
        }
    }
}
