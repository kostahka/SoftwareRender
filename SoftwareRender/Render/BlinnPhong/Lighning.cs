using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SoftwareRender.Render.BlinnPhong
{
    internal class Lighning
    {
        static float ambient = 0.1f;
        static float diffuse = 1.0f;
        static float specullar = 0.5f;
        public static (float, float, float) DotLight(Vector3 L, Vector3 N, Vector3 V, Vector3 R, float specNs)
        {
            float dif = MathF.Max(Vector3.Dot(L, N), 0);

            float reflectDot = MathF.Max(Vector3.Dot(V, R), 0);
            float spec = MathF.Pow(reflectDot, specNs);

            return (ambient, diffuse * dif, specullar * spec);
        }
        public static Vector3 fragment(Vector3 pos, 
                                        Vector3 normal, 
                                        Vector3 ambientColor, 
                                        Vector3 diffuseColor, 
                                        Vector3 specullarColor,
                                        Vector3 lighPos,
                                        Vector3 lightColor,
                                        Vector3 vPos,
                                        float specNs)
        {
            Vector3 L = lighPos - pos;
            float distance = L.Length();
            L /= distance;
            Vector3 V = Vector3.Normalize(vPos - pos);
            Vector3 R = -Vector3.Reflect(L, normal);

            (float ambient, float diffuse, float specullar) = BlinnPhong.Lighning.DotLight(L, normal, V, R, specNs);

            float attenuation = 1.0f / (distance * distance);
            diffuse *= attenuation;
            specullar *= attenuation;

            Vector3 resColor = ambient * ambientColor
                + diffuse * diffuseColor * lightColor
                + specullar * specullarColor * lightColor;

            return resColor;
        }
    }
}
