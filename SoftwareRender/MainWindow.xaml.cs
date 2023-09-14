﻿using SoftwareRender.Rasterization;
using SoftwareRender.Render;
using SoftwareRender.RenderConveyor;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SoftwareRender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model marioModel;
        private ModelShader shader;

        private RenderCanvas rCanvas = new RenderCanvas(1366, 780);
        private Camera camera = new Camera();
        private RenderConv conveyor;

        private void RenderLoop(object? sender, EventArgs e)
        {
            rCanvas.ClearColor(new(0.5f, 0.5f, 0.5f));

            shader.view = camera.view;
            shader.proj = camera.proj;
            shader.lightPos = camera.eye;

            conveyor.DrawData(marioModel);

            rCanvas.SwapBuffers();
        }

        public MainWindow()
        {
            InitializeComponent();
            marioModel = ObjParser.parse("../../../mario-obj/source/Mario.obj");
            conveyor = new RenderConv(rCanvas);
            shader = new();
            conveyor.SetShaderProgram(shader);

            displayImg.Source = rCanvas.Source;

            CompositionTarget.Rendering += RenderLoop;
        }

        private bool isMouseDown = false;
        private Point mousePos = new();
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Up)
            {
                camera.ChangeFOV(MathF.PI / 180);
            }
            if (e.Key == Key.Down)
            {
                camera.ChangeFOV(-MathF.PI / 180);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point currMousePos = e.GetPosition(this);
            if (isMouseDown)
            {
                Vector delta = currMousePos - mousePos;
                camera.RotateRoundTarget((float)delta.X / 100f, (float)delta.Y / 100f);
            }
            mousePos = currMousePos;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            camera.MoveToTarget(-e.Delta / 100f);
        }
    }
}
