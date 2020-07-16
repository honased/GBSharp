using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GBSharp;
using System.IO;
using Android.OS;
using Java.Security;
using Android.Content.PM;
using System.Security;
using Android.App.Usage;
using Android.Webkit;
using System.IO.IsolatedStorage;
using Android.Content.Res;
using Android.App;
using Java.IO;
using System;
using Java.Nio.FileNio;
using Android.Runtime;
using System.Collections.Generic;
using Android.Views;
using Microsoft.Xna.Framework.Input.Touch;

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

            string path = "GBSharp/Links Awakening.gb";
            
            dpad = new DPAD(300, 1920 - 300, CreateCircleTexture(200));
            start = new Button(1080 / 2 + 100, 1920 - 650, CreateCircleTexture(50));
            select = new Button(1080 / 2 - 100, 1920 - 650, CreateCircleTexture(50));

            btnB = new Button(1080 - 300, 1920 - 200, CreateCircleTexture(85));
            btnA = new Button(1080 - 150, 1920 - 350, CreateCircleTexture(85));

            /*IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            Cartridge cartridge;

            if(store.FileExists("Kirby.gb"))
            {
                var fs = store.OpenFile("Kirby.gb", FileMode.Open);
                using(StreamReader sr = new StreamReader(fs))
                {
                    string test = sr.ReadLine();
                }
                fs.Close();
            }
            else
            {
                string file = "";
                using (StreamReader sr = new StreamReader(Game.Activity.Assets.Open("Kirby.gb")))
                {
                    file = sr.ReadToEnd();
                }

                var fs = store.CreateFile("Kirby.gb");
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(file);
                }
                fs.Close();
            }*/

            //StreamWriter writer = new StreamWriter(Environment.DownloadCacheDirectory.AbsolutePath + "/test.txt");
            //writer.WriteLine("Hello");
            //writer.Close();

            Cartridge cartridge = Cartridge.Load(TitleContainer.OpenStream("Kirby.gb"));

            _gameboy.LoadCartridge(cartridge);

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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            dpad.Reset();
            start.Reset();
            select.Reset();
            btnB.Reset();
            btnA.Reset();

            TouchCollection tc = TouchPanel.GetState();

            foreach(TouchLocation tl in tc)
            {
                int mx = (int)tl.Position.X, my = (int)tl.Position.Y;
                dpad.HandleInput(mx, my);
                start.HandleInput(mx, my);
                select.HandleInput(mx, my);
                btnB.HandleInput(mx, my);
                btnA.HandleInput(mx, my);
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

            // TODO: Add your update logic here

            base.Update(gameTime);

            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

            Vector2 origin = new Vector2(PPU.SCREEN_WIDTH / 2, PPU.SCREEN_HEIGHT / 2);
            spriteBatch.Draw(_frame, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 3f), null, Color.White, 0, origin, gameScale, SpriteEffects.None, 0f);

            dpad.Render(spriteBatch);
            start.Render(spriteBatch);
            select.Render(spriteBatch);
            btnB.Render(spriteBatch);
            btnA.Render(spriteBatch);

            spriteBatch.DrawString(font, "DPAD:" + dpad.XAxis.ToString() + "," + dpad.YAxis.ToString(), new Vector2(10, 10), Color.White);

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
