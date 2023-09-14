using SoftwareRender.Rasterization;
using System;
using System.Numerics;
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
        public RenderConv(RenderCanvas canv) 
        {
            canvas = canv;
            viewportX = canvas.PixelWidth;
            viewportY = canvas.PixelHeight;
            viewport = CreateViewport(0, 0, viewportX, viewportY, 0, 1);
            indexBuffer = new IndexBuffer(canvas.PixelWidth, canvas.PixelHeight);
            zBuffer = new FloatBuffer(canvas.PixelWidth, canvas.PixelHeight);
            interpolationBuffers = new FloatBuffer[3];
            for(int i = 0; i < 3; i++)
            {
                interpolationBuffers[i] = new FloatBuffer(canvas.PixelWidth, canvas.PixelHeight);
            }
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

            for (int y = Math.Max(midY, 0); y <= Math.Min(upY, indexBuffer.Height - 1); y++)
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
                
                int x0 = (int)MathF.Floor(up.X + (down.X - up.X) * t1);
                int x1 = (int)MathF.Floor(up.X + (middle.X - up.X) * t2);


                int startX;
                int endX;

                if (x0 > x1)
                {
                    startX = x1;
                    endX = x0;
                }
                else
                {
                    startX = x0;
                    endX = x1;
                }

                for (int x = Math.Max(startX, 0); x <= Math.Min(endX, indexBuffer.Width - 1); x++)
                {
                    float deltaXS1 = x - uv1.X;
                    float deltaYS1 = y - uv1.Y;

                    float k3 = (deltaXS1 * deltaY21 - deltaX21 * deltaYS1) / (deltaY13 * deltaX21 - deltaX13 * deltaY21);
                    float k2 = (deltaXS1 + k3 * deltaX13) / deltaX21;
                    float k1 = 1 - k2 - k3;

                    float z = 1 / (k1 / uv1.Z + k2 / uv2.Z + k3 / uv3.Z);
                    if(zBuffer.GetValue(x, y) > z)
                    {
                        indexBuffer.SetIndex(x, y, index);
                        zBuffer.SetValue(x, y, z);
                        interpolationBuffers[0].SetValue(x, y, k1);
                        interpolationBuffers[1].SetValue(x, y, k2);
                        interpolationBuffers[2].SetValue(x, y, k3);
                    }
                }
            }

            for (int y = Math.Max(downY, 0); y < Math.Min(midY, indexBuffer.Height); y++)
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

                int x0 = (int)MathF.Floor(down.X + (up.X - down.X) * t1);
                int x1 = (int)MathF.Floor(down.X + (middle.X - down.X) * t2);

                int startX;
                int endX;

                if (x0 > x1)
                {
                    startX = x1;
                    endX = x0;
                }
                else
                {
                    startX = x0;
                    endX = x1;
                }

                for (int x = Math.Max(startX, 0); x <= Math.Min(endX, indexBuffer.Width - 1); x++)
                {
                    float deltaXS1 = x - uv1.X;
                    float deltaYS1 = y - uv1.Y;

                    float k3 = (deltaXS1 * deltaY21 - deltaX21 * deltaYS1) / (deltaY13 * deltaX21 - deltaX13 * deltaY21);
                    float k2 = (deltaXS1 + k3 * deltaX13) / deltaX21;
                    float k1 = 1 - k2 - k3;

                    float z = 1 / (k1 / uv1.Z + k2 / uv2.Z + k3 / uv3.Z);
                    if (zBuffer.GetValue(x, y) > z)
                    {
                        indexBuffer.SetIndex(x, y, index);
                        zBuffer.SetValue(x, y, z);
                        interpolationBuffers[0].SetValue(x, y, k1);
                        interpolationBuffers[1].SetValue(x, y, k2);
                        interpolationBuffers[2].SetValue(x, y, k3);
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
                int verticesCount = model.Vertices.Count;
                int indexesCount = model.VerticesIndexes.Count;

                // Vertices
                int verticesCountPerThread = verticesCount / 4;
                
                Parallel.For(0, 4,
                    (int j) =>
                    {
                        int startI = verticesCountPerThread * j;
                        int endI = j == 3 ? verticesCount : startI + verticesCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            model.OutVertices[i] = program.vertexToWorld(model.Vertices[i]);
                            model.OutVertices[i] /= model.OutVertices[i].W;

                            model.OutNormalizedVertices[i] = program.vertexNormilized(model.OutVertices[i]);
                            model.OutNormalizedVertices[i] /= model.OutNormalizedVertices[i].W;
                        }
                    }
                );

                // Normals
                int normalsCount = model.Normals.Count;
                int normalsCountPerThread =  normalsCount / 4;

                Parallel.For(0, 4,
                    (int j) =>
                    {
                        int startI = normalsCountPerThread * j;
                        int endI = j == 3 ? normalsCount : startI + normalsCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            model.OutNormals[i] = program.normal(model.Normals[i]);
                        }
                    }
                );

                indexBuffer.ClearBuffer();
                zBuffer.ClearBuffer(1);

                int triangleCountPerThread = (indexesCount / 3) / 4;
                Parallel.For(0, 4,
                    (int j) =>
                    {
                        int startI = triangleCountPerThread * j;
                        int endI = j == 3 ? indexesCount / 3 : startI + triangleCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            Vector4 v1 = model.OutNormalizedVertices[model.VerticesIndexes[i*3+0]-1];
                            Vector4 v2 = model.OutNormalizedVertices[model.VerticesIndexes[i*3+1]-1];
                            Vector4 v3 = model.OutNormalizedVertices[model.VerticesIndexes[i*3+2]-1];

                            //if (triangleInOfCulling(v1, v2, v3) && Cross((v2 - v1), (v2 - v3)).Z <= 0)
                            if (!(vertexOutOfCulling(v1) || vertexOutOfCulling(v2) || vertexOutOfCulling(v3)) && Cross((v2 - v1), (v2 - v3)).Z <= 0)
                            {
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
                                // TODO: Fragment process
                            }
                        }
                    }
                );

                int pixelsPerThread = viewportX * viewportY / 4;
                Parallel.For(0, 4,
                    (int j) =>
                    {
                        int startI = pixelsPerThread * j;
                        int endI = j == 3 ? viewportX * viewportY : startI + pixelsPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            int triangleIndex = indexBuffer.GetIndex(i);
                            if(triangleIndex != 0)
                            {
                                int index = (triangleIndex-1) * 3;

                                float k1 = interpolationBuffers[0].GetValue(i);
                                float k2 = interpolationBuffers[1].GetValue(i);
                                float k3 = interpolationBuffers[2].GetValue(i);

                                Vector4 v1 = model.OutVertices[model.VerticesIndexes[index + 0] - 1];
                                Vector4 v2 = model.OutVertices[model.VerticesIndexes[index + 1] - 1];
                                Vector4 v3 = model.OutVertices[model.VerticesIndexes[index + 2] - 1];

                                Vector4 v = v1 * k1 + v2 * k2 + v3 * k3;

                                Vector3 n1 = model.OutNormals[model.NormalIndexes[index + 0] - 1];
                                Vector3 n2 = model.OutNormals[model.NormalIndexes[index + 1] - 1];
                                Vector3 n3 = model.OutNormals[model.NormalIndexes[index + 2] - 1];

                                Vector3 n = n1 * k1 + n2 * k2 + n3 * k3;

                                Vector3 color = program.fragment(v, n);

                                canvas.SetPixel(i, color);
                            }
                        }
                    }
                );
            }
        }

        /*
        public void SetViewport(int w, int h)
        {
            viewport = CreateViewport(0, 0, w, h, 0, 1);
            viewportX = w;
            viewportY = h;
        }
        */
    }
}
