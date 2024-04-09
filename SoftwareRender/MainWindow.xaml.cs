using SoftwareRender.Rasterization;
using SoftwareRender.Render;
using SoftwareRender.Render.ModelSupport;
using SoftwareRender.RenderConveyor;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;

namespace SoftwareRender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort = new SerialPort();
        private Thread _readThread;
        private bool _readPort = true;
        private float joystickX = 0;
        private float joystickY = 0;
        private bool buttonA = false;
        private bool buttonB = false;
        private bool buttonC = false;
        private bool buttonD = false;
        private const float minDeltaJoystick = 0.1f;

        private Model marioModel;
        private Model botModel;
        private Model chessModel;
        private Model doomSlayerModel;
        private Model cyberMancubusModel;
        private Model shovelKnightModel;
        private ModelShader shader;

        private RenderCanvas rCanvas = new RenderCanvas(1366, 768);
        private Camera camera = new Camera();
        private RenderConv conveyor;

        private DotLight light = new DotLight(new(10.0f));

        public async void ReadPort()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.ReadExisting();
            }
            while (_readPort)
            {
                if (_serialPort.IsOpen)
                {
                    string input;
                    try
                    {
                        input = _serialPort.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        _readPort = false;
                        break;
                    }

                    var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    switch (tokens[0][0])
                    {
                        case 'X':
                            joystickX = (int.Parse(tokens[1]) / 4096.0f) * 2.0f - 1.0f;
                            if (MathF.Abs(joystickX) < minDeltaJoystick)
                                joystickX = 0;
                            break;
                        case 'Y':
                            joystickY = (int.Parse(tokens[1]) / 4096.0f) * 2.0f - 1.0f;
                            if (MathF.Abs(joystickY) < minDeltaJoystick)
                                joystickY = 0;
                            break;
                        case 'A':
                            buttonA = int.Parse(tokens[1]) == 0;
                            break;
                        case 'B':
                            buttonB = int.Parse(tokens[1]) == 0;
                            break;
                        case 'C':
                            buttonC = int.Parse(tokens[1]) == 0;
                            break;
                        case 'D':
                            buttonD = int.Parse(tokens[1]) == 0;
                            break;
                    }
                }

            }
        }
        private void Render()
        {
            try
            {
                rCanvas.Source.Lock();

                conveyor.BeginDraw();

                rCanvas.ClearColor(new(0.5f, 0.5f, 0.5f));

                shader.model = shovelKnightModel.modelMatrix;

                conveyor.DrawData(shovelKnightModel);

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

        private long last_tick;
        private void InputLoop(object? sender, EventArgs e)
        {
            long current_tick = DateTime.Now.Ticks;
            int delta_ticks = (int)(current_tick - last_tick);

            float speed = delta_ticks / 1000000.0f;
            if (speed > 2.0f)
                speed = 2.0f;

            if(joystickX != 0 || joystickY != 0)
            {
                if(buttonA)
                {
                    camera.MoveToTarget(-joystickY * speed / 2.0f);
                }
                else if(buttonB)
                {
                    light.RotateRoundTarget(-joystickX * speed / 5.0f, joystickY * speed / 5.0f);
                }
                else if (buttonC)
                {
                    light.MoveToTarget(-joystickY * speed / 2.0f);
                }
                else if(buttonD)
                {
                    light.ChangeIntensity(joystickY * speed * 4);
                }
                else
                {
                    camera.RotateRoundTarget(-joystickX * speed / 5.0f, joystickY * speed / 5.0f);
                }
                Render();
                last_tick = current_tick;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            //marioModel = ObjParser.parse("../../../mario-obj/source/Mario.obj");
            //botModel = ObjParser.parse("../../../HardshellTransformer/Hardshell.obj");
            //chessModel = ObjParser.parse("../../../plane-obj/plane.obj");
            //doomSlayerModel = ObjParser.parse("../../../Models/Doom Slayer/doomslayer.obj");
            //cyberMancubusModel = ObjParser.parse("../../../Models/Cyber Mancubus/mancubus.obj");
            shovelKnightModel = ObjParser.parse("../../../Models/Shovel Knight/shovel_low.obj");
            //botModel.modelMatrix = Matrix4x4.CreateScale(1/100f);
            //doomSlayerModel.modelMatrix = Matrix4x4.CreateScale(2) * Matrix4x4.CreateTranslation(new(0, 1.2f, 0));
            //cyberMancubusModel.modelMatrix = Matrix4x4.CreateScale(1.2f) * Matrix4x4.CreateTranslation(new(0, 1.2f, 0));
            conveyor = new RenderConv(rCanvas);
            shader = new(camera, light);
            conveyor.SetShaderProgram(shader);

            displayImg.Source = rCanvas.Source;

            Render();
            //CompositionTarget.Rendering += RenderLoop;
            CompositionTarget.Rendering += InputLoop;

            _serialPort.PortName = "COM5";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;

            string[] portNames = SerialPort.GetPortNames();

            if (portNames.Contains("COM5"))
            {
                _serialPort.Open();
            }

            _readThread = new Thread(ReadPort);
            _readThread.Start();
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

        private void renderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider? slider = sender as Slider;
            if(slider != null)
            {
                if(slider.Tag != null)
                {
                    string? tag = slider.Tag.ToString();
                    switch (tag)
                    {
                        case "Ao":
                            //shader.Ao = (float)slider.Value / 100.0f;
                            break;
                        case "Roughness":
                            //shader.Roughness = (float)slider.Value / 100.0f;
                            break;
                        case "Metallic":
                            //shader.Metallic = (float)slider.Value / 100.0f;
                            break;
                    }
                    Render();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _readPort = false;
            _serialPort.Close();
            _readThread.Join();
        }
    }
}
