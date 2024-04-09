using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render.PBR
{
    internal class Utils
    {
        public static readonly float minValue = 0.0001f;
        public static float D_GGX(float NoH, float roughness)
        {
            float a = NoH * roughness;
            float k = roughness / (1.0f - NoH * NoH + a * a);
            return k * k * (1.0f / MathF.PI);
        }
        public static float V_SmithGGXCorrelated(float NoV, float NoL, float roughness)
        {
            float a2 = roughness * roughness;
            float GGXV = NoL * MathF.Sqrt(NoV * NoV * (1.0f - a2) + a2);
            float GGXL = NoV * MathF.Sqrt(NoL * NoL * (1.0f - a2) + a2);
            return 0.5f / (GGXV + GGXL);
        }
        public static Vector3 F_Shlick(float U, Vector3 F0)
        {
            float f = MathF.Pow(1.0f - U, 5.0f);
            return new Vector3(f) + F0 * new Vector3(1.0f - f);
        }
        public static (Vector3, Vector3) BRDF(float NoH, float NoV, float LoH, float NoL, float roughness, Vector3 F0)
        {
            float D = D_GGX(NoH, roughness);
            Vector3 F = F_Shlick(LoH, F0);
            float V_GGX = V_SmithGGXCorrelated(NoV, NoL, roughness);

            Vector3 Fs = (D * V_GGX) * F;
            Vector3 Fd = (Vector3.One - F) / MathF.PI;

            return (Fd, Fs);
        }
    }
}
