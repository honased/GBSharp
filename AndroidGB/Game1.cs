using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GBSharp;
using System;
using Microsoft.Xna.Framework.Input.Touch;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using System.Threading.Tasks;
using GBSharp.Graphics;

namespace AndroidGB
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D _frame;
        Gameboy _gameboy;
        float gameScale;
        DPAD dpad;
        Button start;
        Button select;
        Button btnB;
        Button btnA;
        SpriteFont font;
        Cartridge cartridge;
        Task<Cartridge> task;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            //Window.OrientationChanged += OnWindowChange;
            //OnWindowChange(null, null);
        }

        private void OnWindowChange(Object sender, EventArgs e)
        {
            gameScale = 2f;

            int width = graphics.PreferredBackBufferHeight;
            int height = graphics.PreferredBackBufferWidth;
            
            if(Window.CurrentOrientation == DisplayOrientation.LandscapeLeft || Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
            {
                width = graphics.PreferredBackBufferWidth;
                height = graphics.PreferredBackBufferHeight;
            }

            while (PPU.SCREEN_WIDTH * gameScale < width && PPU.SCREEN_HEIGHT * gameScale < height)
            {
                gameScale++;
            }
            gameScale -= 1;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _gameboy = new Gameboy();

            FileManager.SavePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/GBSharp/";
            
            dpad = new DPAD(300, 1920 - 300, CreateCircleOutline(200), CreateCircleOutline(100));
            start = new Button(1080 / 2 + 100, 1920 - 650, CreateCircleOutline(50));
            select = new Button(1080 / 2 - 100, 1920 - 650, CreateCircleOutline(50));

            btnB = new Button(1080 - 300, 1920 - 200, CreateCircleOutline(85));
            btnA = new Button(1080 - 150, 1920 - 350, CreateCircleOutline(85));

            task = GetGame();

            gameScale = 2f;

            while (PPU.SCREEN_WIDTH * gameScale < graphics.PreferredBackBufferHeight && PPU.SCREEN_HEIGHT * gameScale < graphics.PreferredBackBufferWidth)
            {
                gameScale++;
            }
            gameScale -= 1;

            _frame = new Texture2D(GraphicsDevice, PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT);

            //_gameboy.StartInBios();

            base.Initialize();
        }

        static async Task<Cartridge> GetGame()
        {
            try
            {
                FileData data = await CrossFilePicker.Current.PickFile();
                if (data == null) return null;
                return Cartridge.Load(data.DataArray);
            }
            catch
            {
                return null;
            }
        }

        Texture2D CreateCircleTexture(int radius)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq)
                    {
                        colorData[index] = Color.White;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        public Texture2D CreateCircleOutline(int radius)
        {
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            // Colour the entire texture transparent first.
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Work out the minimum step necessary using trigonometry + sine approximation.
            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //font = Content.Load<SpriteFont>("font");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (task.IsCompleted)
            {
                if(cartridge == null)
                {
                    cartridge = task.Result;
                    _gameboy.LoadCartridge(cartridge);
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    Exit();

                dpad.Reset();
                start.Reset();
                select.Reset();
                btnB.Reset();
                btnA.Reset();

                TouchCollection tc = TouchPanel.GetState();

                foreach (TouchLocation tl in tc)
                {
                    if (dpad.BeingHandled && tl.Id == dpad.TLIndex) continue;

                    int mx = (int)tl.Position.X, my = (int)tl.Position.Y;
                    if(tl.State == TouchLocationState.Pressed) dpad.SetInput(mx, my, tl.Id);
                    start.HandleInput(mx, my);
                    select.HandleInput(mx, my);
                    btnB.HandleInput(mx, my);
                    btnA.HandleInput(mx, my);
                }

                if(dpad.BeingHandled)
                {
                    bool stillValid = tc.FindById(dpad.TLIndex, out var tl);
                    if(!stillValid || tl.State == TouchLocationState.Released)
                    {
                        dpad.ResetInput();
                    }
                    else
                    {
                        dpad.HandleInput((int)tl.Position.X, (int)tl.Position.Y);
                    }
                }

                float deadzone = .35f;
                _gameboy.SetInput(Input.Button.Up, dpad.YAxis < -deadzone);
                _gameboy.SetInput(Input.Button.Down, dpad.YAxis > deadzone);
                _gameboy.SetInput(Input.Button.Left, dpad.XAxis < -deadzone);
                _gameboy.SetInput(Input.Button.Right, dpad.XAxis > deadzone);

                _gameboy.SetInput(Input.Button.Start, start.Down);
                _gameboy.SetInput(Input.Button.Select, select.Down);
                _gameboy.SetInput(Input.Button.B, btnB.Down);
                _gameboy.SetInput(Input.Button.A, btnA.Down);

                _gameboy.ExecuteFrame();

                Color[] colors = new Color[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT];

                int[] frameBuffer = _gameboy.GetFrameBuffer();

                for (int i = 0; i < frameBuffer.Length; i += 4)
                {
                    colors[i / 4] = new Color(frameBuffer[i], frameBuffer[i + 1], frameBuffer[i + 2], frameBuffer[i + 3]);
                }

                _frame.SetData<Color>(colors);
            }

            base.Update(gameTime);

            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (task.IsCompleted)
            {

                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

                Vector2 origin = new Vector2(PPU.SCREEN_WIDTH / 2, PPU.SCREEN_HEIGHT / 2);
                spriteBatch.Draw(_frame, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 3f), null, Color.White, 0, origin, gameScale, SpriteEffects.None, 0f);

                dpad.Render(spriteBatch);
                start.Render(spriteBatch);
                select.Render(spriteBatch);
                btnB.Render(spriteBatch);
                btnA.Render(spriteBatch);

                

                //spriteBatch.DrawString(font, "DPAD:" + dpad.XAxis.ToString() + "," + dpad.YAxis.ToString(), new Vector2(10, 10), Color.White);

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
