using SoftwareRender.RenderConveyor;
using SoftwareRender.Render.MaterialSupport;
using System;
using System.Numerics;

namespace SoftwareRender.Render.ModelSupport
{
    class ModelShader : IShaderProgram
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Camera Camera { get; set; }
        public DotLight Light { get; set; }
        
        public ModelShader(Camera camera, DotLight light)
        {
            Camera = camera;
            Light = light;
        }

        public Vector3 fragment(Vector3 pos,
                                Vector3 normal,
                                Vector3 ambientColor,
                                Vector3 diffuseColor,
                                Vector3 specullarColor,
                                Vector3 lighPos,
                                Vector3 lightColor,
                                Vector3 vPos,
                                float specNs,
                                Vector3 MRAO)
        {
            /*Vector3 resColor = BlinnPhong.Lighning.fragment(pos,
                                normal,
                                ambientColor,
                                diffuseColor,
                                specullarColor,
                                lighPos,
                                lightColor,
                                vPos,
                                specNs);*/

            Vector3 resColor = PBR.Lighning.fragment(pos,
                                normal,
                                ambientColor,
                                diffuseColor,
                                specullarColor,
                                lighPos,
                                lightColor,
                                vPos, 
                                MRAO.Y,
                                MRAO.X,
                                MRAO.Z);

            resColor = ACES.ACESFitted(resColor);
            resColor = GammaCorrection.LinearTosRGB(resColor * 1.8f);

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
                    Light.Pos,
                    Light.LightColor,
                    Camera.Eye,
                    material.specNs,
                    material.MRAO.getValue(textUV)
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
                    Light.Pos,
                    Light.LightColor,
                    Camera.Eye,
                    material.specNs,
                    material.MRAO.getValue()
                );
        }

        public Vector3 fragmentPTN(Material material, Vector4 pos, Vector3 textUV, Vector3 normal)
        {
            Vector3 p = new(pos.X, pos.Y, pos.Z);
            return fragment(p,
                    material.NormalText == null ? normal : material.NormalText.Value.GetNormal(textUV),
                    material.AmbientColor.getValue(textUV),
                    material.DiffuseColor.getValue(textUV),
                    material.SpecullarColor.getValue(textUV),
                    Light.Pos,
                    Light.LightColor,
                    Camera.Eye,
                    material.specNs,
                    material.MRAO.getValue(textUV)
                );
        }
        public Vector3 normal(Vector3 norm)
        {
            return norm;
        }
        public Vector4 vertexNormilized(Vector4 pos)
        {
            return Vector4.Transform(pos, Camera.View * Camera.Projection);
        }

        public Vector4 vertexToWorld(Vector4 pos)
        {
            return Vector4.Transform(pos, model);
        }
    }
}
