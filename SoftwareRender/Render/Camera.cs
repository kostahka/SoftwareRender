using System;
using System.Numerics;

namespace SoftwareRender.Render
{
    class Camera
    {
        public Matrix4x4 proj { get; private set; }
        public Matrix4x4 view { get; private set; }
        public Vector3 eye { get; private set; }

        float FOV = (float)Math.PI / 4;
        float aspect = 1366/768f;
        float zFar = 200.0f;
        float zNear = 0.5f;

        float distanceToTarget = 8;
        float angleX = 0;
        float angleY = 0;
        Vector3 target = new Vector3(0, 3, 0);

        public void RotateRoundTarget(float dAngleX, float dAngleY)
        {
            angleX += dAngleX;   
            angleY += dAngleY;
            UpdateView();
        }
        public void ChangeFOV(float dFOV)
        {
            FOV += dFOV;
            if (FOV <= 0)
                FOV = MathF.PI / 180;
            UpdateProj();
        }
        public void MoveToTarget(float d)
        {
            distanceToTarget += d;
            UpdateView();
        }
        private void UpdateView()
        {
            Vector3 up = new(-MathF.Cos(angleX) * MathF.Sin(angleY), MathF.Cos(angleY), -MathF.Sin(angleX) * MathF.Sin(angleY));
            Vector3 t = new(MathF.Cos(angleX) * MathF.Cos(angleY), MathF.Sin(angleY), MathF.Sin(angleX) * MathF.Cos(angleY));
            t = Vector3.Normalize(t);
            eye = target + t * distanceToTarget;
            view = Matrix4x4.CreateLookAt(eye, target, up);
        }
        private void UpdateProj()
        {
            proj = Matrix4x4.CreatePerspectiveFieldOfView(FOV, aspect, zNear, zFar);
        }
        public Camera() 
        {
            UpdateProj();
            UpdateView();
        }
    }
}
