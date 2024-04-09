using SoftwareRender.Rasterization;
using SoftwareRender.Render.MaterialSupport;
using SoftwareRender.Render.ModelSupport;
using System;
using System.Collections.Concurrent;
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
        int[] indexBuffer;
        float[] zBuffer;
        SpinLock[] spinlocks;
        float minDepth = 1;
        float maxDepth = 2;
        public RenderConv(RenderCanvas canv) 
        {
            canvas = canv;
            viewportX = canvas.PixelWidth;
            viewportY = canvas.PixelHeight;
            viewport = CreateViewport(0, 0, viewportX, viewportY, minDepth, maxDepth);
            indexBuffer = new int[viewportX * viewportY];
            zBuffer = new float[viewportX * viewportY];
            spinlocks = new SpinLock[viewportX * viewportY];
        }
    
        public void SetShaderProgram(IShaderProgram shaderProgram)
        {
            program = shaderProgram;
        }
        private bool vertexOutScreen(Vector4 uv1)
        {
            return uv1.X < 0 || uv1.X >= viewportX || uv1.Y < 0 || uv1.Y >= viewportY;
        }
        private void DrawTriangleIndex(Vector4 uv1, Vector4 uv2, Vector4 uv3, int index)
        {
            Vector4 up = uv1;
            Vector4 down = uv2;
            Vector4 mid = uv3;

            if (down.Y > mid.Y)
                (down, mid) = (mid, down);
            if (down.Y > up.Y)
                (down, up) = (up, down);
            if (mid.Y > up.Y)
                (up, mid) = (mid, up);

            int upY = (int)up.Y;
            int midY = (int)mid.Y;
            int downY = (int)down.Y;

            up.Z = 1 / up.Z;
            down.Z = 1 / down.Z;
            mid.Z = 1 / mid.Z;

            int firstSegmentHeight = (int)(midY - downY);
            int secondSegmentHeight = (int)(upY - midY);

            int totalHeight = (int)(upY - downY);
            for(int i = 0; i < totalHeight; i++)
            {
                int y = i + downY;

                bool secondHalf = i > firstSegmentHeight || midY == downY;
                int segmentHeight = secondHalf ? secondSegmentHeight : firstSegmentHeight;
                float alpha = (float)i / totalHeight;
                float beta =(float)(i - (secondHalf ? firstSegmentHeight : 0)) / segmentHeight;

                Vector4 a = (down + (up - down) * alpha);
                Vector4 b = secondHalf ? (mid + (up - mid) * beta) : (down + (mid - down) * beta);

                if (a.X > b.X) (a, b) = (b, a);

                float deltaX = b.X - a.X + 1;

                for(int x = (int)a.X; x <= (int)b.X; x++)
                {
                    float P = (x - a.X) / deltaX;

                    float z = 1 / (a.Z + P * (b.Z - a.Z));

                    if (z > minDepth && z < maxDepth)
                    {
                        bool isLocked = false;
                        try
                        {
                            spinlocks[x + y * viewportX].Enter(ref isLocked);
                            if (z < zBuffer[x + y * viewportX])
                            {
                                indexBuffer[x + y * viewportX] = index;
                                zBuffer[x + y * viewportX] = z;
                            }
                        }
                        finally
                        {
                            if (isLocked) spinlocks[x + y * viewportX].Exit();
                        }
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
        public void BeginDraw()
        {
            Parallel.ForEach(Partitioner.Create(0, viewportX * viewportY),
                    range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            zBuffer[i] = maxDepth;
                        }
                    }
                );
        }
        public void DrawData(Model model)
        {
            if(program != null)
            {
                // Vertices
                int verticesCount = model.Vertices.Count;
                Parallel.ForEach(Partitioner.Create(0, verticesCount),
                    range =>
                    {
                        for(int i = range.Item1; i < range.Item2; i++)
                        {
                            model.OutVertices[i] = program.vertexToWorld(model.Vertices[i]);

                            Vector4 normalizedV = program.vertexNormilized(model.OutVertices[i]);
                            float normalizedW = normalizedV.W;
                            normalizedV /= normalizedV.W;

                            model.OutUVVertices[i] = Vector4.Transform(normalizedV, viewport);
                            model.OutUVVertices[i].W = normalizedW;
                        }
                    });

                /*
                // Normals
                int normalsCount = model.Normals.Count;

                Parallel.ForEach(Partitioner.Create(0, normalsCount),
                    range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            model.OutNormals[i] = program.normal(model.Normals[i]);
                        }
                    }
                );
                */

                Parallel.ForEach(Partitioner.Create(0, viewportX * viewportY),
                    range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            indexBuffer[i] = 0;
                        }
                    }
                );

                // Triangle indexes
                int triangleCount = model.Triangles.Count;
                
                Parallel.ForEach(Partitioner.Create(0, triangleCount),
                    range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            Triangle tr = model.Triangles[i];

                            

                            Vector4 uv1 = model.OutUVVertices[tr.vertexIndexes[0].v_i - 1];
                            Vector4 uv2 = model.OutUVVertices[tr.vertexIndexes[1].v_i - 1];
                            Vector4 uv3 = model.OutUVVertices[tr.vertexIndexes[2].v_i - 1];

                            if (!vertexOutScreen(uv1) && !vertexOutScreen(uv2) && !vertexOutScreen(uv3))
                            {
                                Vector3 normalNorm = Cross((uv2 - uv1), (uv2 - uv3));
                                if(normalNorm.Z > 0)
                                {
                                    if (tr.vertexIndexes[0].n_i == 0 && tr.primitiveMaterial.NormalText == null)
                                    {
                                        Vector4 vw1 = model.OutVertices[tr.vertexIndexes[0].v_i - 1];
                                        Vector4 vw2 = model.OutVertices[tr.vertexIndexes[1].v_i - 1];
                                        Vector4 vw3 = model.OutVertices[tr.vertexIndexes[2].v_i - 1];

                                        Vector3 normal = Vector3.Normalize(-Cross(vw2 - vw1, vw2 - vw3));
                                        tr.primitiveMaterial.normal = normal;
                                        model.Triangles[i] = tr; 
                                    }

                                    //canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv2.X, (int)uv2.Y, new Vector3(0, 0, 0));
                                    //canvas.DrawLineBresenhem((int)uv2.X, (int)uv2.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                    //canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                    

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
                Parallel.ForEach(Partitioner.Create(0, viewportX * viewportY),
                    range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            int triangleIndex = indexBuffer[i];
                            if(triangleIndex != 0)
                            {
                                int x = i % viewportX;
                                int y = i / viewportX;

                                Triangle tr = model.Triangles[triangleIndex - 1];

                                Vector4 uv1 = model.OutUVVertices[tr.vertexIndexes[0].v_i - 1];
                                Vector4 uv2 = model.OutUVVertices[tr.vertexIndexes[1].v_i - 1];
                                Vector4 uv3 = model.OutUVVertices[tr.vertexIndexes[2].v_i - 1];

                                Vector3 fVX = new(uv3.X - uv1.X, uv2.X - uv1.X, 0);
                                Vector3 fVY = new(uv3.Y - uv1.Y, uv2.Y - uv1.Y, 0);

                                fVY.Z = uv1.Y - y;
                                fVX.Z = uv1.X - x;

                                Vector3 k = Vector3.Cross(fVX, fVY);
                                float k1 = (1 - (k.X + k.Y) / k.Z) / uv1.W;
                                float k2 = k.Y / k.Z / uv2.W;
                                float k3 = k.X / k.Z / uv3.W;

                                float z0 = 1 / (k1 + k2 + k3);

                                float kp1 = k1 * z0;
                                float kp2 = k2 * z0;
                                float kp3 = k3 * z0;

                                Vector4 v1 = model.OutVertices[tr.vertexIndexes[0].v_i - 1];
                                Vector4 v2 = model.OutVertices[tr.vertexIndexes[1].v_i - 1];
                                Vector4 v3 = model.OutVertices[tr.vertexIndexes[2].v_i - 1];

                                Vector4 v = v1 * kp1 + v2 * kp2 + v3 * kp3;

                                Vector3 color;

                                if (tr.vertexIndexes[0].t_i != 0 && tr.vertexIndexes[0].n_i != 0)
                                {
                                    Vector3 t1 = model.TextureUVs[tr.vertexIndexes[0].t_i - 1];
                                    Vector3 t2 = model.TextureUVs[tr.vertexIndexes[1].t_i - 1];
                                    Vector3 t3 = model.TextureUVs[tr.vertexIndexes[2].t_i - 1];

                                    Vector3 t = t1 * kp1 + t2 * kp2 + t3 * kp3;

                                    Vector3 n1 = model.Normals[tr.vertexIndexes[0].n_i - 1];
                                    Vector3 n2 = model.Normals[tr.vertexIndexes[1].n_i - 1];
                                    Vector3 n3 = model.Normals[tr.vertexIndexes[2].n_i - 1];

                                    Vector3 n = n1 * kp1 + n2 * kp2 + n3 * kp3;
                                    color = program.fragmentPTN(tr.primitiveMaterial, v, t, n);
                                }
                                else if(tr.vertexIndexes[0].t_i != 0)
                                {
                                    Vector3 t1 = model.TextureUVs[tr.vertexIndexes[0].t_i - 1];
                                    Vector3 t2 = model.TextureUVs[tr.vertexIndexes[1].t_i - 1];
                                    Vector3 t3 = model.TextureUVs[tr.vertexIndexes[2].t_i - 1];

                                    Vector3 t = t1 * kp1 + t2 * kp2 + t3 * kp3;

                                    color = program.fragmentPT(tr.primitiveMaterial, v, t);
                                }
                                else if (tr.vertexIndexes[0].n_i != 0)
                                {
                                    Vector3 n1 = model.Normals[tr.vertexIndexes[0].n_i - 1];
                                    Vector3 n2 = model.Normals[tr.vertexIndexes[1].n_i - 1];
                                    Vector3 n3 = model.Normals[tr.vertexIndexes[2].n_i - 1];

                                    Vector3 n = n1 * kp1 + n2 * kp2 + n3 * kp3;
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
