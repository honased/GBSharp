using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class PPU
    {
        public const int SCREEN_WIDTH = 160, SCREEN_HEIGHT = 144;
        private int[] FrameBuffer;

        private bool[] BGPriority { get; set; }

        public int[] Tiles
        {
            get
            {
                int[] tiles = new int[384 * 8 * 8 * 4];

                int count = 0;
                for (int yy = 0; yy < 192; yy++)
                {
                    for (int xx = 0; xx < 128; xx++)
                    {
                        int colorIndex = GetColorIndexFromPalette(_tileset[xx / 8 + ((yy / 8) * (128 / 8)), yy % 8, xx % 8]);
                        Color color2 = colors[colorIndex];
                        tiles[count] = color2.R;
                        tiles[count + 1] = color2.G;
                        tiles[count + 2] = color2.B;
                        tiles[count + 3] = 255;
                        count += 4;
                    }
                }

                return tiles;
            }
        }

        private Gameboy _gameboy;
        private int clocksCount;

        private const int OAM_CLOCK_COUNT = 20;
        private const int VRAM_CLOCK_COUNT = 43;
        private const int HBLANK_CLOCK_COUNT = 51;
        private const int VBLANK_CLOCK_COUNT = 114;

        private const int OAM_SIZE = 0xA0;
        private const int SPRITE_SIZE = 0x04;

        private int[, ,] _tileset;

        private Color[] colors;

        public Color Color0 { get; set; }
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color Color3 { get; set; }

        public PPU(Gameboy gameboy)
        {
            _gameboy = gameboy;
            
            Reset();
        }

        internal void Tick(int clocks)
        {
            clocksCount += clocks;


            int mode = _gameboy.Mmu.STAT & 0x03;

            if (Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 7))
            {
                switch (mode)
                {
                    case 2:
                        if (clocksCount >= OAM_CLOCK_COUNT)
                        {
                            clocksCount -= OAM_CLOCK_COUNT;
                            ChangeMode(3);
                        }
                        break;

                    case 3:
                        if (clocksCount >= VRAM_CLOCK_COUNT)
                        {
                            clocksCount -= VRAM_CLOCK_COUNT;
                            ChangeMode(0);
                            RenderLine();
                        }
                        break;

                    case 0:
                        if (clocksCount >= HBLANK_CLOCK_COUNT)
                        {
                            clocksCount -= HBLANK_CLOCK_COUNT;
                            CheckLYC();

                            if (++_gameboy.Mmu.LY == 144)
                            {
                                ChangeMode(1);
                                _gameboy.Mmu.SetInterrupt(Interrupts.VBlank);
                            }
                            else ChangeMode(2);
                        }
                        break;

                    case 1:
                        if (clocksCount >= VBLANK_CLOCK_COUNT)
                        {
                            clocksCount -= VBLANK_CLOCK_COUNT;
                            CheckLYC();

                            if (++_gameboy.Mmu.LY > 153)
                            {
                                _gameboy.Mmu.LY = 0;
                                ChangeMode(2);
                            }
                        }
                        break;
                }
            }
            else
            {
                _gameboy.Mmu.LY = 0;
                _gameboy.Mmu.STAT &= ~0x03;
                clocksCount = 0;
            }
        }

        internal ref int[] GetFrameBuffer()
        {
            return ref FrameBuffer;
        }

        private void CheckLYC()
        {
            if (_gameboy.Mmu.LY == _gameboy.Mmu.LYC)
            {
                _gameboy.Mmu.STAT = Bitwise.SetBit(_gameboy.Mmu.STAT, 2);
                if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 6)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
            }
            else _gameboy.Mmu.STAT = Bitwise.ClearBit(_gameboy.Mmu.STAT, 2);
        }

        internal void Reset()
        {
            FrameBuffer = new int[SCREEN_WIDTH * SCREEN_HEIGHT * 4];

            colors = new Color[] { Color3, Color2, Color1, Color0 }; //{ new Color(255, 255, 255), new Color(192, 192, 192), new Color(96, 96, 96), new Color(0, 0, 0) };

            // Initialize it to white
            for (int i = 0; i < FrameBuffer.Length; i++)
            {
                FrameBuffer[i] = 255;
            }
            clocksCount = 0;

            ChangeMode(2);

            _tileset = new int[384, 8, 8];

            for (int i = 0; i < 384; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        _tileset[i, j, x] = 0;
                    }
                }
            }

            BGPriority = new bool[SCREEN_WIDTH];
        }

        private void RenderLine()
        {
            if(Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 7))
            {
                if(Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 0)) DrawBackground();
                else
                {
                    for (int i = 0; i < SCREEN_WIDTH; i++) BGPriority[i] = false;
                }
                if(Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 1)) DrawSprites();
            }
        }

        private void DrawBackground()
        {
            int sx = _gameboy.Mmu.SCX;
            int sy = _gameboy.Mmu.SCY;
            int lcdc = _gameboy.Mmu.LCDC;
            int ly = _gameboy.Mmu.LY;
            int wx = _gameboy.Mmu.WX - 7;
            int wy = _gameboy.Mmu.WY;

            bool windowEnabled = Bitwise.IsBitOn(lcdc, 5);

            bool inWindowY = windowEnabled && ly >= wy;

            int y = (Bitwise.Wrap8(ly + sy) / 8) * 32;
            int windowY = (((ly - wy) / 8) * 32 + 1024) % 1024;

            int bgTileMapLocation = Bitwise.IsBitOn(lcdc, 3) ? 0x9C00 : 0x9800;
            int windowTileMapLocation = Bitwise.IsBitOn(lcdc, 6) ? 0x9C00 : 0x9800;

            int startingIndex = ly * SCREEN_WIDTH * 4;

            bool shouldValueBeSigned = !Bitwise.IsBitOn(lcdc, 4);
            int tileInitLocation = shouldValueBeSigned ? 256 : 0;

            for(int xx = 0; xx < SCREEN_WIDTH; xx++)
            {
                bool isInWindow = (inWindowY && xx >= wx);
                int x = isInWindow ? (((xx) / 8) + 32) % 32 : Bitwise.Wrap8(xx + sx) / 8;
                int actualY = isInWindow ? windowY : y;

                int tile;// = _gameboy.Mmu.LoadVRAM(0x9800 + y + x);
                int mapLocation = isInWindow ? windowTileMapLocation : bgTileMapLocation;

                if (shouldValueBeSigned) tile = (sbyte)_gameboy.Mmu.LoadVRAM(mapLocation + actualY + x);
                else tile = (byte)_gameboy.Mmu.LoadVRAM(mapLocation + actualY + x);

                int pixel = isInWindow ? _tileset[tileInitLocation + tile, (ly) % 8, (xx) % 8]
                    : _tileset[tileInitLocation + tile, (ly + sy) % 8, (xx + sx) % 8];

                BGPriority[xx] = (pixel != 0);

                int colorIndex = GetColorIndexFromPalette(pixel);
                Color color = colors[colorIndex];
                FrameBuffer[startingIndex] = color.R;
                FrameBuffer[startingIndex + 1] = color.G;
                FrameBuffer[startingIndex + 2] = color.B;
                FrameBuffer[startingIndex + 3] = 255;
                startingIndex += 4;
            }
            

            /*int count = 0;
            for(int yy = 0; yy < SCREEN_HEIGHT; yy++)
            {
                for (int xx = 0; xx < SCREEN_WIDTH; xx++)
                {
                    int colorIndex = GetColorIndexFromPalette(_tileset[xx / 8 + ((yy / 8) * (SCREEN_WIDTH / 8)), yy % 8, xx % 8]);
                    Color color2 = colors[colorIndex];
                    FrameBuffer[count] = color2.R;
                    FrameBuffer[count + 1] = color2.G;
                    FrameBuffer[count + 2] = color2.B;
                    FrameBuffer[count + 3] = 255;
                    count += 4;
                }
            }*/
        }

        private void DrawSprites()
        {
            int ly = _gameboy.Mmu.LY;
            int lcdc = _gameboy.Mmu.LCDC;
            bool isSpriteHeight16 = Bitwise.IsBitOn(lcdc, 2);
            int spriteHeight = isSpriteHeight16 ? 16 : 8;
            for(int i = OAM_SIZE - SPRITE_SIZE; i >= 0; i -= SPRITE_SIZE)
            {
                int spriteY = _gameboy.Mmu.LoadOAM(i) - 16;
                int spriteX = _gameboy.Mmu.LoadOAM(i + 1) - 8;
                int tileNumber = _gameboy.Mmu.LoadOAM(i + 2);
                int upperTile = tileNumber & 0xFE;
                int lowerTile = tileNumber | 0x01;
                int attribs = _gameboy.Mmu.LoadOAM(i + 3);
                bool objAboveBg = !Bitwise.IsBitOn(attribs, 7);
                bool yFlip = Bitwise.IsBitOn(attribs, 6);
                bool xFlip = Bitwise.IsBitOn(attribs, 5);
                int paletteNumber = Bitwise.IsBitOn(attribs, 4) ? 1 : 0;

                if(ly >= spriteY && ly < spriteY + spriteHeight)
                {
                    int writePosition = (ly * SCREEN_WIDTH * 4) + (spriteX * 4);
                    for(int x = 0; x < 8; x++)
                    {
                        if (spriteX + x >= 0 && spriteX + x < SCREEN_WIDTH)
                        {
                            int drawY = yFlip ? spriteHeight - 1 - (ly - spriteY) : ly - spriteY;
                            int drawX = xFlip ? 7 - x : x;

                            if (isSpriteHeight16)
                            {
                                if (drawY < 8) tileNumber = upperTile;
                                else tileNumber = lowerTile;
                            }
                            int pixel = _tileset[tileNumber, drawY % 8, drawX];

                            int colorIndex = GetSpriteColorIndexFromPalette(pixel, paletteNumber);
                            if (pixel != 0)
                            {
                                if (objAboveBg || !BGPriority[spriteX + x])
                                {
                                    Color color = colors[colorIndex];
                                    FrameBuffer[writePosition] = color.R;
                                    FrameBuffer[writePosition + 1] = color.G;
                                    FrameBuffer[writePosition + 2] = color.B;
                                    FrameBuffer[writePosition + 3] = 255;
                                }
                            }
                        }
                        writePosition += 4;
                    }
                }
            }
        }

        internal void UpdateTile(int address, int value)
        {
            address &= 0x1FFE;

            int tile = (address >> 4) & 511;
            int y = (address >> 1) & 7;

            for(int x = 0; x < 8; x++)
            {
                int sx = 1 << (7 - x);

                _tileset[tile, y, x] = (((_gameboy.Mmu.LoadVRAM(address) & sx) != 0) ? 1 : 0) + (((_gameboy.Mmu.LoadVRAM(address + 1) & sx) != 0) ? 2 : 0);
            }
        }

        private int GetColorIndexFromPalette(int pixel)
        {
            return (_gameboy.Mmu.bgPalette >> (2 * pixel)) & 0x03;
        }

        private int GetSpriteColorIndexFromPalette(int pixel, int palette)
        {
            return (_gameboy.Mmu.GetObjPalette(palette) >> (2 * pixel)) & 0x03;
        }

        private void ChangeMode(int mode)
        {
            _gameboy.Mmu.STAT = (_gameboy.Mmu.STAT & ~0x03) | (mode & 0x03);

            // Handle Interrupts
            switch(mode)
            {
                case 0:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 3)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    break;

                case 1:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 4)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    break;

                case 2:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 5)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    break;
            }
        }

        internal void UpdateColors()
        {
            colors[0] = Color3;
            colors[1] = Color2;
            colors[2] = Color1;
            colors[3] = Color0;
        }

        public struct Color
        {
            public int R;
            public int G;
            public int B;

            public Color(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }
    }
}
