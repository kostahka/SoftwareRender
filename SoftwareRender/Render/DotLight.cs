using SoftwareRender.Render.ModelSupport;
using SoftwareRender.Render.MaterialSupport;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRender.Render
{
    internal class DotLight
    {
        float distanceToTarget = 8;
        float angleX = 0;
        float angleY = 0;
        Vector3 target = new Vector3(0, 3, 0);

        public Vector3 Pos { get; private set; }
        public float Scale { get; private set; }
        public Model Model { get; private set; }
        public float Intensity { get; private set; }
        
        public DotLight(float intensity) 
        {
            Intensity = intensity;
            List<Vector4> vertices = new List<Vector4>() 
            { 
                new(-0.5f, -0.5f, -0.5f, 1), new(0.5f, -0.5f, -0.5f, 1), new(0.5f, -0.5f, 0.5f, 1), new(-0.5f, -0.5f, 0.5f, 1),
                new(-0.5f, 0.5f, -0.5f, 1), new(0.5f, 0.5f, -0.5f, 1), new(0.5f, 0.5f, 0.5f, 1), new(-0.5f, 0.5f, 0.5f, 1),
            };

            ConstMaterialProperty ambientCol = new(new(0.6f, 0.7f, 0.7f));
            ConstMaterialProperty diffuseCol = new(new(0));
            ConstMaterialProperty specCol = new(new(0));

            Material material = new Material(ambientCol, diffuseCol, specCol, null, 0);

            VertexIndexes[][] triangleIndexes = new VertexIndexes[][] 
            {
                new VertexIndexes[]{
                new(1), new(2), new(3),
                },
                new VertexIndexes[]{
                new(3), new(4), new(1),
                },
                new VertexIndexes[]{
                new(7), new(6), new(5),
                },
                new VertexIndexes[]{
                new(5), new(8), new(7),
                },
                new VertexIndexes[]{
                new(6), new(2), new(1),
                },
                new VertexIndexes[]{
                new(1), new(5), new(6),
                },
                new VertexIndexes[]{
                new(7), new(3), new(2),
                },
                new VertexIndexes[]{
                new(2), new(6), new(7),
                },
                new VertexIndexes[]{
                new(8), new(4), new(3),
                },
                new VertexIndexes[]{
                new(3), new(7), new(8),
                },
                new VertexIndexes[]{
                new(5), new(1), new(4),
                },
                new VertexIndexes[]{
                new(4), new(8), new(5),
                }
            };
            
            List<Triangle> triangles = new List<Triangle>() 
            {
                new(material, triangleIndexes[0]),
                new(material, triangleIndexes[1]),
                new(material, triangleIndexes[2]),
                new(material, triangleIndexes[3]),
                new(material, triangleIndexes[4]),
                new(material, triangleIndexes[5]),
                new(material, triangleIndexes[6]),
                new(material, triangleIndexes[7]),
                new(material, triangleIndexes[8]),
                new(material, triangleIndexes[9]),
                new(material, triangleIndexes[10]),
                new(material, triangleIndexes[11]),
            };

            Model = new(vertices, new(), new(), triangles);
            Scale = 0.5f;
            UpdatePosition();
        }
        public void RotateRoundTarget(float dAngleX, float dAngleY)
        {
            angleX += dAngleX;
            angleY += dAngleY;
            UpdatePosition();
        }
        public void MoveToTarget(float d)
        {
            distanceToTarget += d;
            UpdatePosition();
        }
        public void ChangeIntensity(float d)
        {
            Intensity += d;
        }
        private void UpdatePosition()
        {
            Vector3 up = new(-MathF.Cos(angleX) * MathF.Sin(angleY), MathF.Cos(angleY), -MathF.Sin(angleX) * MathF.Sin(angleY));
            Vector3 t = new(MathF.Cos(angleX) * MathF.Cos(angleY), MathF.Sin(angleY), MathF.Sin(angleX) * MathF.Cos(angleY));
            t = Vector3.Normalize(t);
            Pos = target + t * distanceToTarget;
            Model.modelMatrix = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Pos);
        }
    }
}
