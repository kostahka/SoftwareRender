﻿using SoftwareRender.Rasterization;
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
            int viewportX = canvas.PixelWidth;
            int viewportY = canvas.PixelHeight;
            viewport = CreateViewport(0, 0, viewportX, viewportY, 0, 1);
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
