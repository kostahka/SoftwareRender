using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render.PBR
{
    internal class Lighning
    {
        public static Vector3 fragment(Vector3 pos,
                                        Vector3 N,
                                        Vector3 ambientColor,
                                        Vector3 diffuseColor,
                                        Vector3 specullarColor,
                                        Vector3 lighPos,
                                        Vector3 lightColor,
                                        Vector3 vPos,
                                        float perceptualRoughness,
                                        float metallic,
                                        float ao)
        {
            Vector3 L = lighPos - pos;
            float distance = L.Length();
            L /= distance;
            Vector3 V = Vector3.Normalize(vPos - pos);
            Vector3 H = Vector3.Normalize(V + L);

            float attenuation = 1.0f / (distance * distance);
            Vector3 radiance = lightColor * attenuation;

            float NoV = MathF.Max(Vector3.Dot(N, V), PBR.Utils.minValue);
            float NoL = MathF.Max(Vector3.Dot(N, L), PBR.Utils.minValue);
            float NoH = MathF.Max(Vector3.Dot(N, H), PBR.Utils.minValue);
            float LoH = MathF.Max(Vector3.Dot(L, H), PBR.Utils.minValue);

            Vector3 F0 = new Vector3(0.04f);
            F0 = Vector3.Lerp(F0, diffuseColor, metallic);

            perceptualRoughness = MathF.Max(perceptualRoughness, 0.045f);
            float roughness = perceptualRoughness * perceptualRoughness;

            (Vector3 Fd, Vector3 Fs) = PBR.Utils.BRDF(NoH, NoV, LoH, NoL, roughness, F0);

            Fd *= (1.0f - metallic) * diffuseColor;
            Fs *= specullarColor;

            Vector3 L0 = (Fs + Fd) * radiance * NoL;

            Vector3 ambient = ambientColor * ao;

            return L0 + ambient;
        }
    }
}
