using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GBSharp;
using System.IO;
using System;
using GBSharp.Audio;

namespace MonoGB
{
    // Check gekkio

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D _frame;
        Texture2D _tiles;
        SpriteFont font;
        int _currentTestRom;
        float gameScale;
        Gameboy _gameboy;
        Color[] colors;

        KeyboardState oldState;

        private bool _debugMode;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            _debugMode = true;

            //_cpu.SetPalette(new PPU.Color(8, 24, 32), new PPU.Color(52, 104, 86), new PPU.Color(136, 192, 112), new PPU.Color(224, 248, 208));

            graphics.PreferMultiSampling = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            if (!_debugMode)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            }
            graphics.ApplyChanges();

            gameScale = 2f;

            while(PPU.SCREEN_WIDTH * gameScale < graphics.PreferredBackBufferWidth && PPU.SCREEN_HEIGHT * gameScale < graphics.PreferredBackBufferHeight)
            {
                gameScale++;
            }

            gameScale -= 1;

            //_gameboy.Debug();
            //_cpu.StartInBios();
            //_gameboy.StartInBios();

            _frame = new Texture2D(GraphicsDevice, PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT);
            _tiles = new Texture2D(GraphicsDevice, 128, 192);

            //CartridgeLoader.LoadDataIntoMemory(_mmu, CartridgeLoader.LoadCart("Roms/opus5.gb"), 0x00);

            string path = "Roms/Games/Kirbys Dream Land.gb";

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) path = args[1];

            if(!File.Exists(path))
            {
                Console.WriteLine(path);
                Console.WriteLine("Invalid usage... GBSharp [rom]");

                Exit();
                throw new Exception();
            }

            Cartridge cartridge = Cartridge.Load(path);
            //Cartridge cartridge = GetNextTestRom();
            //CartridgeLoader.LoadDataIntoMemory(_mmu, GetNextTestRom(), 0x00);
            _gameboy.LoadCartridge(cartridge);

            this.Window.Title = cartridge.Name;

            oldState = Keyboard.GetState();

            colors = new Color[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT];

            Exiting += new EventHandler<EventArgs>(OnExit);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");

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

        private Cartridge GetNextTestRom()
        {
            string[] files = Directory.GetFiles("Roms/Blargg/Mem");
            Console.WriteLine("Loading Rom " + files[_currentTestRom] + "...");
            Cartridge cart = Cartridge.Load(files[_currentTestRom++]);
            if (_currentTestRom >= files.Length) _currentTestRom = 0;
            return cart;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (!IsActive) return;

            var _keyState = Keyboard.GetState();
            var _padState = GamePad.GetState(0);

            float deadzone = .15f; ;
            _gameboy.SetInput(Input.Button.Up, _keyState.IsKeyDown(Keys.Up) || _padState.DPad.Up == ButtonState.Pressed || _padState.ThumbSticks.Left.Y > deadzone);
            _gameboy.SetInput(Input.Button.Down, _keyState.IsKeyDown(Keys.Down) || _padState.DPad.Down == ButtonState.Pressed || _padState.ThumbSticks.Left.Y < -deadzone);
            _gameboy.SetInput(Input.Button.Left, _keyState.IsKeyDown(Keys.Left) || _padState.DPad.Left == ButtonState.Pressed || _padState.ThumbSticks.Left.X < -deadzone);
            _gameboy.SetInput(Input.Button.Right, _keyState.IsKeyDown(Keys.Right) || _padState.DPad.Right == ButtonState.Pressed || _padState.ThumbSticks.Left.X > deadzone);
            _gameboy.SetInput(Input.Button.B, _keyState.IsKeyDown(Keys.A) || _padState.Buttons.X == ButtonState.Pressed);
            _gameboy.SetInput(Input.Button.A, _keyState.IsKeyDown(Keys.S) || _padState.Buttons.A == ButtonState.Pressed);
            _gameboy.SetInput(Input.Button.Start, _keyState.IsKeyDown(Keys.Space) || _padState.Buttons.Start == ButtonState.Pressed);
            _gameboy.SetInput(Input.Button.Select, _keyState.IsKeyDown(Keys.LeftShift) || _padState.Buttons.Back == ButtonState.Pressed);

            _gameboy.ExecuteFrame();

            int[] frameBuffer = _gameboy.GetFrameBuffer();

            for(int i = 0; i < frameBuffer.Length; i+=4)
            {
                colors[i / 4] = new Color(frameBuffer[i], frameBuffer[i + 1], frameBuffer[i + 2], frameBuffer[i + 3]);
            }

            _frame.SetData<Color>(colors);

            if (_debugMode)
            {
                //                 tiles w   h
                /*colors = new Color[384 * 8 * 8];
                int[] ppuTiles = _gameboy.GetTilesBuffer();
                for (int i = 0; i < ppuTiles.Length; i += 4)
                {
                    colors[i / 4] = new Color(ppuTiles[i], ppuTiles[i + 1], ppuTiles[i + 2], ppuTiles[i + 3]);
                }

                _tiles.SetData<Color>(colors);*/
            }

            if (_keyState.IsKeyDown(Keys.R) && !oldState.IsKeyDown(Keys.R))
            {
                _gameboy.Reset(false, GetNextTestRom());
                //_cpu.Debug();
            }

            if(_keyState.IsKeyDown(Keys.Tab) && !oldState.IsKeyDown(Keys.Tab))
            {
                //_cpu.Debug();
            }

            oldState = _keyState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            if(!_debugMode)
            {
                Vector2 origin = new Vector2(PPU.SCREEN_WIDTH / 2, PPU.SCREEN_HEIGHT / 2);
                spriteBatch.Draw(_frame, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), null, Color.White, 0, origin, gameScale, SpriteEffects.None, 0f);
            }
            else
            {
                //spriteBatch.Draw(_tiles, new Vector2(GraphicsDevice.Viewport.Width, 0), null, Color.White, 0, new Vector2(_tiles.Width, 0), 2f, SpriteEffects.None, 0f);
                spriteBatch.Draw(_frame, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(font, "FPS: " + (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), new Vector2(0, GraphicsDevice.Viewport.Height - 20), Color.White);
            
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void OnExit(Object sender, EventArgs args)
        {
            _gameboy.Close();
        }
    }
}
