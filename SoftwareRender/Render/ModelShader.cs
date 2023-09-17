using SoftwareRender.RenderConveyor;
using System;
using System.Numerics;

namespace SoftwareRender.Render
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public Vector3 lightPos { get; set; } = new();
        public Vector3 eyePos { get; set; } = new();
        float ambient = 0.2f;
        float diffuse = 0.5f;
        float specullar = 0.3f;

        public Vector3 fragmentP(Material material, Vector4 pos)
        {
            return material.AmbientColor.getValue();
        }

        public Vector3 fragmentPT(Material material, Vector4 pos, Vector3 textUV)
        {
            Vector2 textCoords = new Vector2(textUV.X, textUV.Y);

            Vector3 p = new(pos.X, pos.Y, pos.Z);
            Vector3 lightDir = Vector3.Normalize(lightPos - p);
            Vector3 normal = Vector3.Normalize(material.Normal.getValue(textCoords));
            
            float dif = Vector3.Dot(lightDir, normal);
            if (dif < 0)
                dif = 0;

            Vector3 eyeDir = Vector3.Normalize(eyePos - p);
            Vector3 reflectDir = (lightDir - 2 * Vector3.Dot(lightDir, normal) * normal);

            float spec = MathF.Pow(Vector3.Dot(eyeDir, reflectDir), material.specNs);

            Vector3 ambientColor = material.AmbientColor.getValue(textCoords);
            Vector3 diffuseColor = material.DiffuseColor.getValue(textCoords);
            Vector3 specullarColor = material.SpecullarColor.getValue(textCoords);

            Vector3 resColor = ambient * ambientColor
                + dif * diffuse * diffuseColor
                + spec * specullar * specullarColor;

            return resColor;
        }
        public Vector3 fragmentPN(Material material, Vector4 pos, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            Vector3 lightDir = Vector3.Normalize(lightPos - p);
            normal = Vector3.Normalize(normal);

            float dif = Vector3.Dot(lightDir, normal);
            if (dif < 0)
                dif = 0;

            Vector3 eyeDir = Vector3.Normalize(eyePos - p);
            Vector3 reflectDir = (lightDir - 2 * Vector3.Dot(lightDir, normal) * normal);

            float spec = MathF.Pow(Vector3.Dot(eyeDir, reflectDir), material.specNs);

            Vector3 ambientColor = material.AmbientColor.getValue();
            Vector3 diffuseColor = material.DiffuseColor.getValue();
            Vector3 specullarColor = material.SpecullarColor.getValue();

            Vector3 resColor = ambient * ambientColor
                + dif * diffuse * diffuseColor
                + spec * specullar * specullarColor;

            return resColor;
        }

        public Vector3 fragmentPTN(Material material, Vector4 pos, Vector3 textUV, Vector3 normal)
        {
            Vector2 textCoords = new Vector2(textUV.X, textUV.Y);

            Vector3 p = new(pos.X, pos.Y, pos.Z);
            Vector3 lightDir = Vector3.Normalize(lightPos - p);
            normal = Vector3.Normalize(normal);

            float dif = Vector3.Dot(lightDir, normal);
            if (dif < 0)
                dif = 0;

            Vector3 eyeDir = Vector3.Normalize(eyePos - p);
            Vector3 reflectDir = (lightDir - 2 * Vector3.Dot(lightDir, normal) * normal);

            float spec = MathF.Pow(Vector3.Dot(eyeDir, reflectDir), material.specNs);

            Vector3 ambientColor = material.AmbientColor.getValue(textCoords);
            Vector3 diffuseColor = material.DiffuseColor.getValue(textCoords);
            Vector3 specullarColor = material.SpecullarColor.getValue(textCoords);

            Vector3 resColor = ambient * ambientColor
                + dif * diffuse * diffuseColor
                + spec * specullar * specullarColor;

            return resColor;
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
