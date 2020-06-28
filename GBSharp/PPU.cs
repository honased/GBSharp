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
        public int[] FrameBuffer { get; private set; }

        private MMU _mmu;
        private int clocksCount;

        private const int OAM_CLOCK_COUNT = 20;
        private const int VRAM_CLOCK_COUNT = 43;
        private const int HBLANK_CLOCK_COUNT = 51;
        private const int VBLANK_CLOCK_COUNT = 114;

        public static int RenderCount = 0;

        private int[, ,] _tileset;

        private Color[] colors;

        private struct Color
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

        int testCount = 0;

        public PPU(MMU mmu)
        {
            _mmu = mmu;
            // Create a new framebuffer with 4 colors
            FrameBuffer = new int[SCREEN_WIDTH * SCREEN_HEIGHT * 4];

            colors = new Color[] { new Color(255, 255, 255), new Color(192, 192, 192), new Color(96, 96, 96), new Color(0, 0, 0) };
            
            // Initialize it to white
            for(int i = 0; i < FrameBuffer.Length; i++)
            {
                FrameBuffer[i] = 255;
            }
            clocksCount = 0;

            ChangeMode(2);

            _tileset = new int[512, 8, 8];

            for(int i = 0; i < 512; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    for(int x = 0; x < 8; x++)
                    {
                        _tileset[i, j, x] = 0;
                    }
                }
            }
        }

        public void Tick(int clocks)
        {
            clocksCount += clocks;

            int mode = _mmu.STAT & 0x03;

            switch(mode)
            {
                case 2:
                    if(clocksCount >= OAM_CLOCK_COUNT)
                    {
                        clocksCount -= OAM_CLOCK_COUNT;
                        ChangeMode(3);
                    }
                    break;

                case 3:
                    if(clocksCount >= VRAM_CLOCK_COUNT)
                    {
                        clocksCount -= VRAM_CLOCK_COUNT;
                        ChangeMode(0);
                        RenderLine();
                    }
                    break;

                case 0:
                    if(clocksCount >= HBLANK_CLOCK_COUNT)
                    {
                        clocksCount -= HBLANK_CLOCK_COUNT;

                        if (++_mmu.LY == 144)
                        {
                            ChangeMode(1);
                            _mmu.SetInterrupt(Interrupts.VBlank);
                        }
                        else ChangeMode(2);
                    }
                    break;

                case 1:
                    if(clocksCount >= VBLANK_CLOCK_COUNT)
                    {
                        clocksCount -= VBLANK_CLOCK_COUNT;

                        if(++_mmu.LY > 153)
                        {
                            _mmu.LY = 0;
                            ChangeMode(2);
                        }
                    }
                    break;
            }

            if(_mmu.LY == _mmu.LYC)
            {
                _mmu.STAT = Bitwise.SetBit(_mmu.STAT, 2);
                if (Bitwise.IsBitOn(_mmu.STAT, 6)) _mmu.SetInterrupt(Interrupts.LCDStat);
            }
            else _mmu.STAT = Bitwise.ClearBit(_mmu.STAT, 2);
        }

        private void RenderLine()
        {
            RenderCount++;
            if(Bitwise.IsBitOn(_mmu.LCDC, 7) && Bitwise.IsBitOn(_mmu.LCDC, 0))
            {
                RenderBackground();
            }
        }

        private void RenderBackground()
        {
            int sx = _mmu.SCX;
            int sy = _mmu.SCY;
            int lcdc = _mmu.LCDC;
            int ly = _mmu.LY;

            int y = (((ly + sy) / 8) * 32 + 1024) % 1024;

            int bgTileMapLocation = Bitwise.IsBitOn(lcdc, 3) ? 0x9C00 : 0x9800;

            int startingIndex = ly * SCREEN_WIDTH * 4;

            bool shouldValueBeSigned = Bitwise.IsBitOn(lcdc, 4);
            int tileInitLocation = shouldValueBeSigned ? 0 : 256 + 128;

            for(int xx = 0; xx < SCREEN_WIDTH; xx++)
            {
                int x = (((xx + sx) / 8) + 32) % 32;

                int tile;// = _mmu.LoadVRAM(0x9800 + y + x);

                if (shouldValueBeSigned) tile = (sbyte)_mmu.LoadVRAM(bgTileMapLocation + y + x);
                else tile = (byte)_mmu.LoadVRAM(bgTileMapLocation + y + x);

                int colorIndex = GetColorIndexFromPalette(_tileset[tile, (ly + sy) % 8, (xx + sx) % 8]);
                Color color = colors[colorIndex];
                FrameBuffer[startingIndex] = color.R;
                FrameBuffer[startingIndex + 1] = color.G;
                FrameBuffer[startingIndex + 2] = color.B;
                FrameBuffer[startingIndex + 3] = 255;
                startingIndex += 4;
            }
            

            /*int count = 0;
            for(int yy = 0; yy < 32; yy++)
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

        public void UpdateTile(int address, int value)
        {
            address &= 0x1FFE;

            int tile = (address >> 4) & 511;
            int y = (address >> 1) & 7;

            for(int x = 0; x < 8; x++)
            {
                int sx = 1 << (7 - x);

                _tileset[tile, y, x] = (((_mmu.LoadVRAM(address) & sx) != 0) ? 1 : 0) + (((_mmu.LoadVRAM(address + 1) & sx) != 0) ? 2 : 0);
            }
        }

        private int GetColorIndexFromPalette(int pixel)
        {
            return (_mmu.bgPalette >> (2 * pixel)) & 0x03;
        }

        private void ChangeMode(int mode)
        {
            _mmu.STAT = (_mmu.STAT & ~0x03) | (mode & 0x03);

            // Handle Interrupts
            switch(mode)
            {
                case 0:
                    if (Bitwise.IsBitOn(_mmu.STAT, 3)) _mmu.SetInterrupt(Interrupts.LCDStat);
                    break;

                case 1:
                    if (Bitwise.IsBitOn(_mmu.STAT, 4)) _mmu.SetInterrupt(Interrupts.LCDStat);
                    break;

                case 2:
                    if (Bitwise.IsBitOn(_mmu.STAT, 5)) _mmu.SetInterrupt(Interrupts.LCDStat);
                    break;
            }
        }
    }
}
