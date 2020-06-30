using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GBSharp;
using System.IO;

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
        CPU _cpu;
        MMU _mmu;
        PPU _ppu;
        Input _input;
        Texture2D _frame;
        int _currentTestRom;

        KeyboardState oldState;

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
            _mmu = new MMU();
            _ppu = new PPU(_mmu);
            _input = new Input(_mmu);
            _cpu = new CPU(_mmu, _ppu, _input);
            //_cpu.DebugMode = true;
            //_cpu.StartInBios();

            _frame = new Texture2D(GraphicsDevice, PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT);

            CartridgeLoader.LoadDataIntoMemory(_mmu, CartridgeLoader.LoadCart("Gekkio/push_timing.gb"), 0x00);
            //CartridgeLoader.LoadDataIntoMemory(_mmu, GetNextTestRom(), 0x00);

            IsFixedTimeStep = true;

            oldState = Keyboard.GetState();

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

        private int[] GetNextTestRom()
        {
            string[] files = Directory.GetFiles("Blargs");
            int[] cart = CartridgeLoader.LoadCart(files[_currentTestRom++]);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            var _keyState = Keyboard.GetState();

            _input.SetInput(Input.Button.Up, _keyState.IsKeyDown(Keys.Up));
            _input.SetInput(Input.Button.Down, _keyState.IsKeyDown(Keys.Down));
            _input.SetInput(Input.Button.Left, _keyState.IsKeyDown(Keys.Left));
            _input.SetInput(Input.Button.Right, _keyState.IsKeyDown(Keys.Right));
            _input.SetInput(Input.Button.B, _keyState.IsKeyDown(Keys.A));
            _input.SetInput(Input.Button.A, _keyState.IsKeyDown(Keys.S));
            _input.SetInput(Input.Button.Start, _keyState.IsKeyDown(Keys.Space));
            _input.SetInput(Input.Button.Select, _keyState.IsKeyDown(Keys.LeftShift));

            _cpu.ExecuteFrame();

            Color[] colors = new Color[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT];

            for(int i = 0; i < _ppu.FrameBuffer.Length; i+=4)
            {
                colors[i / 4] = new Color(_ppu.FrameBuffer[i], _ppu.FrameBuffer[i + 1], _ppu.FrameBuffer[i + 2], _ppu.FrameBuffer[i + 3]);
            }

            _frame.SetData<Color>(colors);

            if(_keyState.IsKeyDown(Keys.R) && !oldState.IsKeyDown(Keys.R))
            {
                _cpu.Reset(false, GetNextTestRom());
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            Vector2 origin = new Vector2(PPU.SCREEN_WIDTH / 2, PPU.SCREEN_HEIGHT / 2);
            spriteBatch.Draw(_frame, new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2), null, Color.White, 0, origin, 3, SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
