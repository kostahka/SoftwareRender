using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    internal class GammaCorrection
    {
        public static float LinearTosRGB(float a)
        {
            return a <= 0.0031308f ? 12.92f * a : 1.055f * MathF.Pow(a, (1 / 2.4f));
        }
        public static Vector3 LinearTosRGB(Vector3 linearColor)
        {
            linearColor = RenderUtils.Saturate(linearColor);
            Vector3 y = 1.055f * RenderUtils.Pow(linearColor, 1.0f / 2.4f) - new Vector3(0.055f);
            Vector3 x = linearColor * 12.92f;

            Vector3 clr = linearColor;
            clr.X = clr.X < 0.0031308f ? x.X : y.X;
            clr.Y = clr.Y < 0.0031308f ? x.Y : y.Y;
            clr.Z = clr.Z < 0.0031308f ? x.Z : y.Z;

            return clr;
        }
        public static Vector3 sRGBToLinear(Vector3 sRgb)
        {
            Vector3 x = sRgb / 12.92f;
            Vector3 y = RenderUtils.Pow(Vector3.Max((sRgb + new Vector3(0.055f)) / 1.055f, Vector3.Zero), 2.4f);

            Vector3 clr = sRgb;
            clr.X = sRgb.X <= 0.04045f ? x.X : y.X;
            clr.Y = sRgb.Y <= 0.04045f ? x.Y : y.Y;
            clr.Z = sRgb.Z <= 0.04045f ? x.Z : y.Z;

            return clr;
        }
    }
}
