using SoftwareRender.RenderConveyor;
using SoftwareRender.Render.MaterialSupport;
using System;
using System.Numerics;

namespace SoftwareRender.Render.ModelSupport
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public Vector3 lightPos { get; set; } = new();
        public float lightIntensity { get; set; } = 26f;
        public Vector3 eyePos { get; set; } = new();
        float ambient = 0.1f;
        float diffuse = 0.5f;
        float specullar = 0.3f;

        public Vector3 fragment(Vector3 pos, Vector3 normal, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specullarColor, float specNs)
        {
            Vector3 lightDir = lightPos - pos;
            float lightDist = lightDir.Length();
            lightDir /= lightDist;

            float dif = Vector3.Dot(lightDir, normal) / (lightDist * lightDist) * lightIntensity;
            if (dif < 0)
                dif = 0;

            Vector3 eyeDir = Vector3.Normalize(eyePos - pos);
            Vector3 reflectDir = -Vector3.Reflect(lightDir, normal);
            float reflectDot = Vector3.Dot(eyeDir, reflectDir);
            float spec;
            if (reflectDot < 0)
                spec = 0;
            else
                spec = MathF.Pow(reflectDot, specNs);
            Vector3 resColor = ambient * ambientColor
                + dif * diffuse * diffuseColor
                + spec * specullar * specullarColor;

            return resColor;
        }

        public Vector3 fragmentP(Material material, Vector4 pos)
        {
            return material.AmbientColor.getValue();
        }

        public Vector3 fragmentPT(Material material, Vector4 pos, Vector3 textUV)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            return fragment(p,
                    material.NormalText == null ? material.normal : material.NormalText.Value.GetNormal(textUV),
                    material.AmbientColor.getValue(textUV),
                    material.DiffuseColor.getValue(textUV),
                    material.SpecullarColor.getValue(textUV),
                    material.specNs
                );
        }
        public Vector3 fragmentPN(Material material, Vector4 pos, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            return fragment(p,
                    normal,
                    material.AmbientColor.getValue(),
                    material.DiffuseColor.getValue(),
                    material.SpecullarColor.getValue(),
                    material.specNs
                );
        }

        public Vector3 fragmentPTN(Material material, Vector4 pos, Vector3 textUV, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            return fragment(p,
                    normal,
                    material.AmbientColor.getValue(textUV),
                    material.DiffuseColor.getValue(textUV),
                    material.SpecullarColor.getValue(textUV),
                    material.specNs
                );
        }
        public Vector3 normal(Vector3 norm)
        {
            return norm;
        }
        public Vector4 vertexNormilized(Vector4 pos)
        {
            return Vector4.Transform(pos, view * proj);
        }

        public Vector4 vertexToWorld(Vector4 pos)
        {
            return Vector4.Transform(pos, model);
        }
    }
}
