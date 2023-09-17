using SoftwareRender.Rasterization;
using System;
using System.Numerics;
using System.Threading;
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
        IndexBuffer indexBuffer;
        FloatBuffer zBuffer;
        FloatBuffer[] interpolationBuffers;
        SpinLock[] spinlocks;
        int procCount;
        public RenderConv(RenderCanvas canv) 
        {
            canvas = canv;
            viewportX = canvas.PixelWidth;
            viewportY = canvas.PixelHeight;
            viewport = CreateViewport(0, 0, viewportX, viewportY, 1, 2);
            indexBuffer = new IndexBuffer(viewportX, canvas.PixelHeight);
            zBuffer = new FloatBuffer(viewportX, viewportY);
            interpolationBuffers = new FloatBuffer[3];
            for(int i = 0; i < 3; i++)
            {
                interpolationBuffers[i] = new FloatBuffer(viewportX, viewportY);
            }
            spinlocks = new SpinLock[viewportX * viewportY];
            procCount = Environment.ProcessorCount;
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
        private bool vertexOutOfCulling(Vector4 v)
        {
            return v.X < -1 || v.X > 1 || v.Y < -1 || v.Y > 1 || v.Z < -1 || v.Z > 1;
        }
        private float InterpolateZ(float z1, float z2, float t)
        {
            return 1 / (1 / z1 * t + 1 / z2 * (1 - t));
        }
        private void DrawTriangleIndex(Vector4 uv1, Vector4 uv2, Vector4 uv3, int index)
        {
            Vector4 up;
            Vector4 middle;
            Vector4 down;

            if (uv1.Y > uv2.Y)
            {
                if (uv1.Y > uv3.Y)
                {
                    up = uv1;
                    if (uv2.Y > uv3.Y)
                    {
                        middle = uv2;
                        down = uv3;
                    }
                    else
                    {
                        middle = uv3;
                        down = uv2;
                    }
                }
                else
                {
                    up = uv3;
                    middle = uv1;
                    down = uv2;
                }
            }
            else
            {
                if (uv2.Y > uv3.Y)
                {
                    up = uv2;
                    if (uv1.Y > uv3.Y)
                    {
                        middle = uv1;
                        down = uv3;
                    }
                    else
                    {
                        middle = uv3;
                        down = uv1;
                    }
                }
                else
                {
                    up = uv3;
                    middle = uv2;
                    down = uv1;
                }
            }

            int midY = (int)MathF.Floor(middle.Y);
            int upY = (int)MathF.Floor(up.Y);
            int downY = (int)MathF.Floor(down.Y);

            float deltaY21 = uv2.Y - uv1.Y;
            float deltaX21 = uv2.X - uv1.X;
            float deltaY13 = uv1.Y - uv3.Y;
            float deltaX13 = uv1.X - uv3.X;

            for (int y = midY; y <= upY; y++)
            {
                float t1;
                float t2;
                if (y == upY)
                {
                    t1 = 0;
                    t2 = 0;
                }
                else
                {
                    float deltaUp = y - upY;
                    t1 = (deltaUp) / (downY - upY);
                    t2 = (deltaUp) / (midY - upY);
                }
                
                int x0 = (int)(up.X + (down.X - up.X) * t1);
                int x1 = (int)(up.X + (middle.X - up.X) * t2);

                int deltaX = x0 < x1 ? 1 : -1;

                for (int x = x0; x != x1 + deltaX; x += deltaX)
                {
                    float deltaXS1 = x - uv1.X;
                    float deltaYS1 = y - uv1.Y;

                    float k3 = (deltaXS1 * deltaY21 - deltaX21 * deltaYS1) / (deltaY13 * deltaX21 - deltaX13 * deltaY21);
                    float k2 = (deltaXS1 + k3 * deltaX13) / deltaX21;
                    float k1 = 1 - k2 - k3;

                    float z = 1 / (k1 / uv1.Z + k2 / uv2.Z + k3 / uv3.Z);
                    //float z = (k1 * uv1.Z + k2 * uv2.Z + k3 * uv3.Z);
                    
                    bool isLocked = false;
                    try
                    {
                        spinlocks[x + y * viewportY].Enter(ref isLocked);
                        if (zBuffer.GetValue(x, y) > z)
                        {
                            indexBuffer.SetIndex(x, y, index);
                            zBuffer.SetValue(x, y, z);
                            interpolationBuffers[0].SetValue(x, y, k1);
                            interpolationBuffers[1].SetValue(x, y, k2);
                            interpolationBuffers[2].SetValue(x, y, k3);
                        }
                    }
                    finally
                    {
                        if(isLocked) spinlocks[x + y * viewportY].Exit();
                    }
                    
                }
            }

            for (int y = downY; y < midY; y++)
            {
                float t1;
                float t2;
                if (y == downY)
                {
                    t1 = 0;
                    t2 = 0;
                }
                else
                {
                    float deltaDown = y - downY;
                    t1 = (deltaDown) / (upY - downY);
                    t2 = (deltaDown) / (midY - downY);
                }

                int x0 = (int)(down.X + (up.X - down.X) * t1);
                int x1 = (int)(down.X + (middle.X - down.X) * t2);

                int deltaX = x0 < x1 ? 1 : -1;

                for (int x = x0; x != x1 + deltaX; x += deltaX)
                {
                    float deltaXS1 = x - uv1.X;
                    float deltaYS1 = y - uv1.Y;

                    float k3 = (deltaXS1 * deltaY21 - deltaX21 * deltaYS1) / (deltaY13 * deltaX21 - deltaX13 * deltaY21);
                    float k2 = (deltaXS1 + k3 * deltaX13) / deltaX21;
                    float k1 = 1 - k2 - k3;

                    float z = 1 / (k1 / uv1.Z + k2 / uv2.Z + k3 / uv3.Z);
                    //float z = (k1 * uv1.Z + k2 * uv2.Z + k3 * uv3.Z);

                    bool isLocked = false;
                    try
                    {
                        spinlocks[x + y * viewportY].Enter(ref isLocked);
                        if (zBuffer.GetValue(x, y) > z)
                        {
                            indexBuffer.SetIndex(x, y, index);
                            zBuffer.SetValue(x, y, z);
                            interpolationBuffers[0].SetValue(x, y, k1);
                            interpolationBuffers[1].SetValue(x, y, k2);
                            interpolationBuffers[2].SetValue(x, y, k3);
                        }
                    }
                    finally
                    {
                        if (isLocked) spinlocks[x + y * viewportY].Exit();
                    }
                }
            }

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
        private void DrawDDALine(float x0, float y0, float x1, float y1, Vector3 color)
        {

            float deltaX = x1 - x0;
            float deltaY = y1 - y0;
            float maxDelta = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
            deltaX /= maxDelta;
            deltaY /= maxDelta;
            int L = (int)maxDelta;

            float x = x0;
            float y = y0;
            
            if(x < 0)
            {
                L -= (int)((0 - x) / deltaX);
                y += deltaY * (0 - x);
                x = 0;
            }
            if (x >= canvas.PixelWidth)
            {
                L -= (int)((canvas.PixelWidth - 1 - x) / deltaX);
                y += deltaY * (canvas.PixelWidth - 1 - x);
                x = canvas.PixelWidth - 1;
            }
            if (y < 0)
            {
                L -= (int)((0 - y) / deltaY);
                x += deltaX * (0 - y);
                y = 0;
            }
            if (y >= canvas.PixelHeight)
            {
                L -= (int)((canvas.PixelHeight - 1 - y) / deltaY);
                x += deltaX * (canvas.PixelHeight - 1 - y);
                y = canvas.PixelHeight - 1;
            }

            for (int i = 0; i <= L && x > 0 && y > 0 && x < canvas.PixelWidth && y < canvas.PixelHeight; i++)
            {
                canvas.SetPixel((int)x, (int)y, color);
                x += deltaX;
                y += deltaY;
            }
        }
        public void DrawData(Model model)
        {
            if(program != null)
            {
                // Vertices
                int verticesCount = model.Vertices.Count;
                int verticesCountPerThread = verticesCount / procCount;
                
                Parallel.For(0, procCount,
                    (int j) =>
                    {
                        int startI = verticesCountPerThread * j;
                        int endI = j == procCount - 1 ? verticesCount : startI + verticesCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            model.OutVertices[i] = program.vertexToWorld(model.Vertices[i]);

                            model.OutNormalizedVertices[i] = program.vertexNormilized(model.OutVertices[i]);
                            model.OutNormalizedVertices[i] /= model.OutNormalizedVertices[i].W;
                        }
                    }
                );

                // Normals
                int normalsCount = model.Normals.Count;
                int normalsCountPerThread =  normalsCount / procCount;

                Parallel.For(0, procCount,
                    (int j) =>
                    {
                        int startI = normalsCountPerThread * j;
                        int endI = j == procCount - 1 ? normalsCount : startI + normalsCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            model.OutNormals[i] = program.normal(model.Normals[i]);
                        }
                    }
                );

                indexBuffer.ClearBuffer();
                zBuffer.ClearBuffer(2);

                // Triangle indexes
                int triangleCount = model.Triangles.Count;
                int triangleCountPerThread = triangleCount / procCount;
                
                Parallel.For(0, procCount,
                    (int j) =>
                    {
                        int startI = triangleCountPerThread * j;
                        int endI = j == procCount - 1 ? triangleCount : startI + triangleCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            Triangle tr = model.Triangles[i];

                            Vector4 vw1 = model.OutVertices[tr.vertexIndexes[0].v_i - 1];
                            Vector4 vw2 = model.OutVertices[tr.vertexIndexes[1].v_i - 1];
                            Vector4 vw3 = model.OutVertices[tr.vertexIndexes[2].v_i - 1];

                            Vector4 v1 = model.OutNormalizedVertices[tr.vertexIndexes[0].v_i-1];
                            Vector4 v2 = model.OutNormalizedVertices[tr.vertexIndexes[1].v_i-1];
                            Vector4 v3 = model.OutNormalizedVertices[tr.vertexIndexes[2].v_i-1];

                            //if (triangleInOfCulling(v1, v2, v3) && Cross((v2 - v1), (v2 - v3)).Z <= 0)
                            if (!(vertexOutOfCulling(v1) || vertexOutOfCulling(v2) || vertexOutOfCulling(v3)))
                            {
                                Vector3 normal = Cross((v2 - v1), (v2 - v3));
                                if(normal.Z < 0)
                                {
                                    model.Triangles[i].primitiveMaterial.Normal.setValue(Cross(vw2 - vw1, vw2 - vw3));

                                    Vector4 uv1 = Vector4.Transform(v1, viewport);
                                    Vector4 uv2 = Vector4.Transform(v2, viewport);
                                    Vector4 uv3 = Vector4.Transform(v3, viewport);

                                    /*
                                    canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv2.X, (int)uv2.Y, new Vector3(0, 0, 0));
                                    canvas.DrawLineBresenhem((int)uv2.X, (int)uv2.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                    canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                    */

                                    //DrawDDALine(uv1.X, uv1.Y, uv2.X, uv2.Y, new Vector3(0, 0, 0));
                                    //DrawDDALine(uv3.X, uv3.Y, uv2.X, uv2.Y, new Vector3(0, 0, 0));
                                    //DrawDDALine(uv1.X, uv1.Y, uv3.X, uv3.Y, new Vector3(0, 0, 0));

                                    DrawTriangleIndex(uv1, uv2, uv3, i + 1);
                                }
                            }
                        }
                    }
                );

                // Pixels
                int pixelsPerThread = viewportX * viewportY / procCount;
                
                Parallel.For(0, procCount,
                    (int j) =>
                    {
                        int startI = pixelsPerThread * j;
                        int endI = j == procCount - 1 ? viewportX * viewportY : startI + pixelsPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            int triangleIndex = indexBuffer.GetIndex(i);
                            if(triangleIndex != 0)
                            {
                                Triangle tr = model.Triangles[triangleIndex - 1];

                                float k1 = interpolationBuffers[0].GetValue(i);
                                float k2 = interpolationBuffers[1].GetValue(i);
                                float k3 = interpolationBuffers[2].GetValue(i);

                                Vector4 v1 = model.OutVertices[tr.vertexIndexes[0].v_i - 1];
                                Vector4 v2 = model.OutVertices[tr.vertexIndexes[1].v_i - 1];
                                Vector4 v3 = model.OutVertices[tr.vertexIndexes[2].v_i - 1];

                                Vector4 v = v1 * k1 + v2 * k2 + v3 * k3;

                                Vector4 vn1 = model.OutNormalizedVertices[tr.vertexIndexes[0].v_i - 1];
                                Vector4 vn2 = model.OutNormalizedVertices[tr.vertexIndexes[1].v_i - 1];
                                Vector4 vn3 = model.OutNormalizedVertices[tr.vertexIndexes[2].v_i - 1];

                                Vector3 color;

                                if (tr.vertexIndexes[0].t_i != 0 && tr.vertexIndexes[0].n_i != 0)
                                {
                                    Vector3 t1 = model.TextureUVs[tr.vertexIndexes[0].t_i - 1];
                                    Vector3 t2 = model.TextureUVs[tr.vertexIndexes[1].t_i - 1];
                                    Vector3 t3 = model.TextureUVs[tr.vertexIndexes[2].t_i - 1];

                                    Vector3 t = t1 * k1 + t2 * k2 + t3 * k3;

                                    Vector3 n1 = model.OutNormals[tr.vertexIndexes[0].n_i - 1];
                                    Vector3 n2 = model.OutNormals[tr.vertexIndexes[1].n_i - 1];
                                    Vector3 n3 = model.OutNormals[tr.vertexIndexes[2].n_i - 1];

                                    Vector3 n = n1 * k1 + n2 * k2 + n3 * k3;
                                    color = program.fragmentPTN(tr.primitiveMaterial, v, t, n);
                                }
                                else if(tr.vertexIndexes[0].t_i != 0)
                                {
                                    Vector3 t1 = model.TextureUVs[tr.vertexIndexes[0].t_i - 1];
                                    Vector3 t2 = model.TextureUVs[tr.vertexIndexes[1].t_i - 1];
                                    Vector3 t3 = model.TextureUVs[tr.vertexIndexes[2].t_i - 1];

                                    Vector3 t = t1 * k1 + t2 * k2 + t3 * k3;

                                    color = program.fragmentPT(tr.primitiveMaterial, v, t);
                                }
                                else if (tr.vertexIndexes[0].n_i != 0)
                                {
                                    Vector3 n1 = model.OutNormals[tr.vertexIndexes[0].n_i - 1];
                                    Vector3 n2 = model.OutNormals[tr.vertexIndexes[1].n_i - 1];
                                    Vector3 n3 = model.OutNormals[tr.vertexIndexes[2].n_i - 1];

                                    Vector3 n = n1 * k1 + n2 * k2 + n3 * k3;
                                    color = program.fragmentPN(tr.primitiveMaterial, v, n);
                                }
                                else
                                {
                                    color = program.fragmentP(tr.primitiveMaterial, v);
                                }

                                canvas.SetPixel(i, color);
                            }
                        }
                    }
                );
            }
        }
    }
}
