using SoftwareRender.Rasterization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SoftwareRender.RenderConveyor
{
    public enum PrimitiveType
    {
        triangles,
        quads
    }
    internal class RenderConv
    {

        Matrix4x4 viewport;
        int viewportX;
        int viewportY;

        IShaderProgram? program = null;
        RenderCanvas canvas;
        public RenderConv(RenderCanvas canv) 
        {
            canvas = canv;
        }
    
        public void SetShaderProgram(IShaderProgram shaderProgram)
        {
            program = shaderProgram;
        }

        private bool triangleInOfCulling(Vector4 v1, Vector4 v2, Vector4 v3)
        {
            return (v1.X > -1 && v1.X < 1 && v1.Y > -1 && v1.Z < 1)
                || (v2.X > -1 && v2.X < 1 && v2.Y > -1 && v2.Z < 1)
                || (v3.X > -1 && v3.X < 1 && v3.Y > -1 && v3.Z < 1);
        }
        public static Matrix4x4 CreateViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            result.M11 = width * 0.5f;
            result.M22 = -height * 0.5f;
            result.M33 = maxDepth - minDepth;
            result.M41 = x + result.M11;
            result.M42 = y - result.M22;
            result.M43 = minDepth;

            return result;
        }
        private Vector3 Cross(Vector4 v1, Vector4 v2) 
        {
            return Vector3.Cross(new(v1.X, v1.Y, v1.Z), new(v2.X, v2.Y, v2.Z));
        }
        public void DrawData<T1, T2>(IVertexArrayObject<T1> vao, int vertCount, PrimitiveType primitiveType)
            where T1 : IVertexInputInfo<T1>, new() where T2 : IFragmentData<T2>
        {
            if(program != null)
            {
                
                if (primitiveType == PrimitiveType.triangles)
                {
                    int triangleCountPerThread = (vertCount / 3) / 4;
                    Parallel.For(0, 4,
                        (int j) =>
                        {
                            ShaderProgram<T2> shaderProgram = (ShaderProgram<T2>)program;
                            T1 vInInfo = new();
                            List<GCHandle?> vertexInPtrs = new();
                            foreach (var type in vInInfo.getInParametrs())
                            {
                                vertexInPtrs.Add(null);
                            }

                            int startI = triangleCountPerThread * j;
                            int endI = j == 3 ? vertCount / 3 : startI + triangleCountPerThread;

                            for (int i = startI; i < endI; i++)
                            {
                                vao.GetInParametrPtrs(i * 3 + 0, ref vertexInPtrs);
                                T2 v1 = shaderProgram.vertex(ref vertexInPtrs);
                                vao.GetInParametrPtrs(i * 3 + 1, ref vertexInPtrs);
                                T2 v2 = shaderProgram.vertex(ref vertexInPtrs);
                                vao.GetInParametrPtrs(i * 3 + 2, ref vertexInPtrs);
                                T2 v3 = shaderProgram.vertex(ref vertexInPtrs);

                                Vector4 vp1 = v1.GetVertexPos();
                                Vector4 vp2 = v2.GetVertexPos();
                                Vector4 vp3 = v3.GetVertexPos();

                                vp1 /= vp1.W;
                                vp2 /= vp2.W;
                                vp3 /= vp3.W;

                                if (triangleInOfCulling(vp1, vp2, vp3) && Cross((vp2 - vp1), (vp2 - vp3)).Z <= 0)
                                {
                                    Vector4 uv1 = Vector4.Transform(vp1, viewport);
                                    Vector4 uv2 = Vector4.Transform(vp2, viewport);
                                    Vector4 uv3 = Vector4.Transform(vp3, viewport);

                                    canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv2.X, (int)uv2.Y, new Vector3(0, 0, 0));
                                    canvas.DrawLineBresenhem((int)uv2.X, (int)uv2.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                    canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));

                                    // TODO: Fragment process
                                }
                            }
                        }
                    );
                    
                }
                else if(primitiveType == PrimitiveType.quads)
                {
                    ShaderProgram<T2> shaderProgram = (ShaderProgram<T2>)program;
                    T1 vInInfo = new();
                    List<GCHandle?> vertexInPtrs = new();
                    foreach (var type in vInInfo.getInParametrs())
                    {
                        vertexInPtrs.Add(null);
                    }
                    for (int i = 0; i < vertCount; i += 4)
                    {
                        vao.GetInParametrPtrs(i + 0, ref vertexInPtrs);
                        T2 v1 = shaderProgram.vertex(ref vertexInPtrs);
                        vao.GetInParametrPtrs(i + 1, ref vertexInPtrs);
                        T2 v2 = shaderProgram.vertex(ref vertexInPtrs);
                        vao.GetInParametrPtrs(i + 2, ref vertexInPtrs);
                        T2 v3 = shaderProgram.vertex(ref vertexInPtrs);
                        vao.GetInParametrPtrs(i + 3, ref vertexInPtrs);
                        T2 v4 = shaderProgram.vertex(ref vertexInPtrs);

                        Vector4 vp1 = v1.GetVertexPos();
                        Vector4 vp2 = v2.GetVertexPos();
                        Vector4 vp3 = v3.GetVertexPos();
                        Vector4 vp4 = v4.GetVertexPos();

                        vp1 /= vp1.W;
                        vp2 /= vp2.W;
                        vp3 /= vp3.W;
                        vp4 /= vp4.W;

                        if (Cross((vp2 - vp1), (vp2 - vp3)).Z <= 0 && (triangleInOfCulling(vp1, vp2, vp3) || triangleInOfCulling(vp4, vp2, vp3)))
                        {
                            Vector4 uv1 = Vector4.Transform(vp1, viewport);
                            Vector4 uv2 = Vector4.Transform(vp2, viewport);
                            Vector4 uv3 = Vector4.Transform(vp3, viewport);
                            Vector4 uv4 = Vector4.Transform(vp4, viewport);

                            canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv2.X, (int)uv2.Y, new Vector3(0, 0, 0));
                            canvas.DrawLineBresenhem((int)uv2.X, (int)uv2.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                            canvas.DrawLineBresenhem((int)uv4.X, (int)uv4.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                            canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv4.X, (int)uv4.Y, new Vector3(0, 0, 0));

                            // TODO: Fragment process
                        }
                    }
                }
            }
        }

        public void SetViewport(int w, int h)
        {
            viewport = CreateViewport(0, 0, w, h, 0, 1);
            viewportX = w;
            viewportY = h;
        }
    }
}
