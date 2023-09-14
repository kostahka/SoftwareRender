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
        IndexBuffer indexBuffer;
        FloatBuffer zBuffer;
        public RenderConv(RenderCanvas canv) 
        {
            canvas = canv;
            int viewportX = canvas.PixelWidth;
            int viewportY = canvas.PixelHeight;
            viewport = CreateViewport(0, 0, viewportX, viewportY, 0, 1);
            indexBuffer = new IndexBuffer(canvas.PixelWidth, canvas.PixelHeight);
            zBuffer = new FloatBuffer(canvas.PixelWidth, canvas.PixelHeight);
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

            for(int y = Math.Max((int)middle.Y, 0); y <= Math.Min((int)up.Y, indexBuffer.Height - 1); y++)
            {
                float deltaUp = y - up.Y;
                float t1 = (down.Y - up.Y) / (deltaUp);
                float t2 = (middle.Y - up.Y) / (deltaUp);
                int x0 = (int)(up.X + (down.X - up.X) * t1);
                int x1 = (int)(up.X + (middle.X - up.X) * t2);

                float z0 = InterpolateZ(up.Z, down.Z, t1);
                float z1 = InterpolateZ(up.Z, middle.Z, t2);

                int startX;
                int endX;
                float startZ;
                float endZ;
                if (x0 > x1)
                {
                    startX = x1;
                    endX = x0;
                    startZ = z1;
                    endZ = z0;
                }
                else
                {
                    startX = x0;
                    endX = x1;
                    startZ = z0;
                    endZ = z1;
                }

                for (int x = Math.Max(startX, 0); x <= Math.Min(endX, indexBuffer.Width - 1); x++)
                {
                    float t = (float)(x - startX) / (endX - startX);
                    float z = InterpolateZ(startZ, endZ, t);
                    if(zBuffer.GetValue(x, y) > z)
                    {
                        indexBuffer.SetIndex(x, y, index);
                        zBuffer.SetValue(x, y, z);
                    }
                }
            }
            for (int y = Math.Max((int)down.Y, 0); y < Math.Min((int)middle.Y, indexBuffer.Height); y++)
            {
                float deltaDown = y - down.Y;
                float t1 = (up.Y - down.Y) / (deltaDown);
                float t2 = (middle.Y - down.Y) / (deltaDown);
                int x0 = (int)(down.X + (up.X - down.X) * t1);
                int x1 = (int)(down.X + (middle.X - down.X) * t2);

                float z0 = InterpolateZ(down.Z, up.Z, t1);
                float z1 = InterpolateZ(down.Z, middle.Z, t2);

                int startX;
                int endX;
                float startZ;
                float endZ;
                if (x0 > x1)
                {
                    startX = x1;
                    endX = x0;
                    startZ = z1;
                    endZ = z0;
                }
                else
                {
                    startX = x0;
                    endX = x1;
                    startZ = z0;
                    endZ = z1;
                }

                for (int x = Math.Max(startX, 0); x <= Math.Min(endX, indexBuffer.Width - 1); x++)
                {
                    float t = (float)(x - startX) / (endX - startX);
                    float z = InterpolateZ(startZ, endZ, t);
                    if (zBuffer.GetValue(x, y) > z)
                    {
                        indexBuffer.SetIndex(x, y, index);
                        zBuffer.SetValue(x, y, z);
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

                int verticesCountPerThread = verticesCount / 4;
                
                Parallel.For(0, 4,
                    (int j) =>
                    {
                        IShaderProgram shaderProgram = program;

                        int startI = verticesCountPerThread * j;
                        int endI = j == 3 ? verticesCount : startI + verticesCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            model.OutVertices[i] = program.vertex(model.Vertices[i]);

                            model.OutVertices[i] /= model.OutVertices[i].W;
                        }
                    }
                );

                //indexBuffer.ClearBuffer();
                //zBuffer.ClearBuffer(1);

                int triangleCountPerThread = (indexesCount / 3) / 4;
                Parallel.For(0, 4,
                    (int j) =>
                    {
                        int startI = triangleCountPerThread * j;
                        int endI = j == 3 ? indexesCount / 3 : startI + triangleCountPerThread;

                        for (int i = startI; i < endI; i++)
                        {
                            Vector4 v1 = model.OutVertices[model.VerticesIndexes[i*3+0]-1];
                            Vector4 v2 = model.OutVertices[model.VerticesIndexes[i*3+1]-1];
                            Vector4 v3 = model.OutVertices[model.VerticesIndexes[i*3+2]-1];

                            if (triangleInOfCulling(v1, v2, v3) && Cross((v2 - v1), (v2 - v3)).Z <= 0)
                            {
                                Vector4 uv1 = Vector4.Transform(v1, viewport);
                                Vector4 uv2 = Vector4.Transform(v2, viewport);
                                Vector4 uv3 = Vector4.Transform(v3, viewport);

                                /*
                                canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv2.X, (int)uv2.Y, new Vector3(0, 0, 0));
                                canvas.DrawLineBresenhem((int)uv2.X, (int)uv2.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                canvas.DrawLineBresenhem((int)uv1.X, (int)uv1.Y, (int)uv3.X, (int)uv3.Y, new Vector3(0, 0, 0));
                                */

                                DrawDDALine(uv1.X, uv1.Y, uv2.X, uv2.Y, new Vector3(0, 0, 0));
                                DrawDDALine(uv3.X, uv3.Y, uv2.X, uv2.Y, new Vector3(0, 0, 0));
                                DrawDDALine(uv1.X, uv1.Y, uv3.X, uv3.Y, new Vector3(0, 0, 0));
                                
                                //DrawTriangleIndex(uv1, uv2, uv3, i + 1);
                                // TODO: Fragment process
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
