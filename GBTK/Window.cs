using GBSharp;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;
using System.Threading;
using GBSharp.Graphics;

namespace GBTK
{
    public class Window : GameWindow
    {
        private Gameboy _gameboy;

        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             1.0f,  1.0f, 0.0f, 1.0f, 0.0f, // top right
             1.0f, -1.0f, 0.0f, 1.0f, 1.0f, // bottom right
            -1.0f, -1.0f, 0.0f, 0.0f, 1.0f, // bottom left
            -1.0f,  1.0f, 0.0f, 0.0f, 0.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;
        private Texture _texture;
        private ALDevice device;
        private ALContext context;
        Thread thread;
        byte[] texArr;

        public Window(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {
            device =  ALC.OpenDevice("");
            context = ALC.CreateContext(device, new ALContextAttributes() { Frequency = 44100 });

            ALC.MakeContextCurrent(context);

            Console.WriteLine("Version : " + AL.Get(ALGetString.Version));
            Console.WriteLine("Vendor  : " + AL.Get(ALGetString.Vendor));
            Console.WriteLine("Renderer: " + AL.Get(ALGetString.Renderer));

            _gameboy = new Gameboy(() => { return new AudioEmitter(); });

            string path = "Roms/Games/Pokemon Silver.gbc";

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) path = args[1];

            if (!File.Exists(path))
            {
                Console.WriteLine(path);
                Console.WriteLine("Invalid usage... GBSharp [rom]");

                foreach (string s in args)
                {
                    Console.WriteLine("S: " + s);
                }

                throw new Exception();
            }

            Cartridge cartridge = Cartridge.Load(path);
            //Cartridge cartridge = GetNextTestRom();
            //CartridgeLoader.LoadDataIntoMemory(_mmu, GetNextTestRom(), 0x00);
            _gameboy.LoadCartridge(cartridge);

            thread = new Thread(new ThreadStart(_gameboy.Run));
            thread.Start();
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            // Because there's now 5 floats between the start of the first vertex and the start of the second,
            // we modify this from 3 * sizeof(float) to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the first vertex coordinate comes after the first vertex
            // and change the amount of data to 2 because there's only 2 floats for vertex coordinates
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            texArr = new byte[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT * 4];
            for(int i = 0; i < texArr.Length; i++)
            {
                texArr[i] = 255;
            }

            _texture = Texture.CreateFromRGBA(texArr, PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT);
            _texture.Use(TextureUnit.Texture0);
            VSync = VSyncMode.On;
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if(KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            _gameboy.SetInput(Input.Button.Up, KeyboardState.IsKeyDown(Keys.Up));
            _gameboy.SetInput(Input.Button.Down, KeyboardState.IsKeyDown(Keys.Down));
            _gameboy.SetInput(Input.Button.Left, KeyboardState.IsKeyDown(Keys.Left));
            _gameboy.SetInput(Input.Button.Right, KeyboardState.IsKeyDown(Keys.Right));
            _gameboy.SetInput(Input.Button.B, KeyboardState.IsKeyDown(Keys.A));
            _gameboy.SetInput(Input.Button.A, KeyboardState.IsKeyDown(Keys.S));
            _gameboy.SetInput(Input.Button.Start, KeyboardState.IsKeyDown(Keys.Space));
            _gameboy.SetInput(Input.Button.Select, KeyboardState.IsKeyDown(Keys.LeftShift));

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_gameboy.GetFrameBufferCount() > 0)
            {
                int[] buf = _gameboy.DequeueFrameBuffer();
                for (int i = 0; i < buf.Length; i++)
                {
                    texArr[i] = (byte)buf[i];
                }
                _texture.UpdateTexture(texArr);
            }
            else Console.WriteLine("Missing " + DateTime.Now.Ticks);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _texture.Use(TextureUnit.Texture0);
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            ALC.DestroyContext(context);

            ALC.CloseDevice(device);

            base.OnUnload();
        }
    }
}
