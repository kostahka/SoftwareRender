using SoftwareRender.Render.PBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    internal class ACES
    {
        static Matrix4x4 ACESInputMat =
        Matrix4x4.Transpose(new (
            0.59719f, 0.35458f, 0.04823f, 0.0f,
            0.07600f, 0.90834f, 0.01566f, 0.0f,
            0.02840f, 0.13383f, 0.83777f, 0.0f,
            0.0f,     0.0f,     0.0f,     0.0f
        ));

        static Matrix4x4 ACESOutputMat =
        Matrix4x4.Transpose(new (
             1.60475f, -0.53108f, -0.07367f, 0.0f,
            -0.10208f,  1.10813f, -0.00605f, 0.0f,
            -0.00327f, -0.07276f,  1.07602f, 0.0f,
             0.0f,      0.0f,      0.0f,     0.0f
        ));

        static Vector3 RRTAndODTFit(Vector3 v)
        {
            Vector3 a = v * (v + new Vector3(0.0245786f)) - new Vector3(0.000090537f);
            Vector3 b = v * (0.983729f * v + new Vector3(0.4329510f)) + new Vector3(0.238081f);
            return a / b;
        }

        public static Vector3 ACESFitted(Vector3 color)
        {
            color = Vector3.Transform(color, ACESInputMat);

            color = RRTAndODTFit(color);

            color = Vector3.Transform(color, ACESOutputMat);

            color = RenderUtils.Saturate(color);

            return color;
        }
    }
}
