using SoftwareRender.RenderConveyor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRender.Render
{
    class ModelVertexInput : IVertexInputInfo<ModelVertexInput>
    {
        public ModelVertexInput() { }
        public List<Type> getInParametrs()
        {
            return new()
            {
                typeof(Vector4)
            };
        }
    }
    struct ModelFragmentData : IFragmentData<ModelFragmentData>
    {
        Vector4 vertexPos;
        public ModelFragmentData(Vector4 pos)
        {
            vertexPos = pos;
        }
        public Vector4 GetVertexPos()
        {
            return vertexPos;
        }

        public ModelFragmentData Interpolate(ModelFragmentData other, float t)
        {
            return new ModelFragmentData(new Vector4(0));
        }
    }
    class ModelShader : ShaderProgram<ModelFragmentData>
    {
        public Matrix4x4 model { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 proj { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 view { get; set; } = Matrix4x4.Identity;
        public override Vector4 fragment(ModelFragmentData data)
        {
            return new(0);
        }

        public override ModelFragmentData vertex(ref List<GCHandle?> dataPtrs)
        {
            Vector4 pos = (Vector4)dataPtrs[0].Value.Target;

            Matrix4x4 mvpMatrix = model * view * proj;
            Vector4 endPos = Vector4.Transform(pos, mvpMatrix);

            return new(endPos);
        }
    }
}
