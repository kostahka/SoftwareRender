using SoftwareRender.Rasterization;
using SoftwareRender.Render;
using SoftwareRender.Render.ModelSupport;
using SoftwareRender.RenderConveyor;
using System;
using System.Numerics;
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
        private Model botModel;
        private ModelShader shader;

        private RenderCanvas rCanvas = new RenderCanvas(1920, 1080);
        private Camera camera = new Camera();
        private RenderConv conveyor;

        private DotLight light;

        private void Render()
        {
            try
            {
                rCanvas.Source.Lock();

                conveyor.BeginDraw();

                rCanvas.ClearColor(new(0.5f, 0.5f, 0.5f));

                shader.model = botModel.modelMatrix;
                shader.view = camera.view;
                shader.proj = camera.proj;
                shader.eyePos = camera.eye;
                shader.lightPos = light.Pos;
                shader.lightIntensity = light.Intensity;

                conveyor.DrawData(botModel);

                shader.model = light.Model.modelMatrix;

                conveyor.DrawData(light.Model);

                rCanvas.SwapBuffers();
            }
            finally
            {
                rCanvas.Source.Unlock();
            }
        }

        private void RenderLoop(object? sender, EventArgs e)
        {
            Render();
        }

        public MainWindow()
        {
            InitializeComponent();
            marioModel = ObjParser.parse("../../../mario-obj/source/Mario.obj");
            botModel = ObjParser.parse("../../../HardshellTransformer/Hardshell.obj");
            botModel.modelMatrix = Matrix4x4.CreateScale(1/100f);
            conveyor = new RenderConv(rCanvas);
            shader = new();
            conveyor.SetShaderProgram(shader);

            light = new(100f);

            displayImg.Source = rCanvas.Source;
            shader.lightPos = light.Pos;
            shader.lightIntensity = light.Intensity;

            Render();
            //CompositionTarget.Rendering += RenderLoop;
        }

        private bool isMouseDown = false;
        private Point mousePos = new();
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Up)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    light.ChangeIntensity(5f);
                }
                else
                {
                    camera.ChangeFOV(MathF.PI / 180);
                }
                Render();
            }
            if (e.Key == Key.Down)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    light.ChangeIntensity(-5f);
                }
                else
                {
                    camera.ChangeFOV(-MathF.PI / 180);
                }
                Render();
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
                System.Windows.Vector delta = currMousePos - mousePos;
                if(Keyboard.Modifiers == ModifierKeys.Control)
                {
                    light.RotateRoundTarget((float)delta.X / 100f, (float)delta.Y / 100f);
                }
                else
                {
                    camera.RotateRoundTarget((float)delta.X / 100f, (float)delta.Y / 100f);
                }
                Render();
            }
            mousePos = currMousePos;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                light.MoveToTarget(-e.Delta / 100f);
            }
            else
            {
                camera.MoveToTarget(-e.Delta / 100f);
            }
            Render();
        }
    }
}
