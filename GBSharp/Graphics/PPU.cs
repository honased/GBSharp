﻿using GBSharp.Interfaces;
using System;
using System.IO;

namespace GBSharp.Graphics
{
    public class PPU : IStateable
    {
        public const int SCREEN_WIDTH = 160, SCREEN_HEIGHT = 144;
        private int[] FrameBuffer, fb1, fb2;
        private bool useFb1;
        private int[] oamSprites;

        private int[] BGPriority { get; set; }

        public int[] GetTiles(int vramBank)
        {
            int[] tiles = new int[384 * 8 * 8 * 4];

            int count = 0;
            for (int yy = 0; yy < 192; yy++)
            {
                for (int xx = 0; xx < 128; xx++)
                {
                    int colorIndex = (_tileset[vramBank, xx / 8 + ((yy / 8) * (128 / 8)), yy % 8, xx % 8]);
                    GBColor color2 = _bgPalettes[0].Colors[colorIndex];
                    tiles[count] = color2.R;
                    tiles[count + 1] = color2.G;
                    tiles[count + 2] = color2.B;
                    tiles[count + 3] = 255;
                    count += 4;
                }
            }

            return tiles;
        }

        private readonly Gameboy _gameboy;
        private int clocksCount;

        private const int OAM_CLOCK_COUNT = 80;
        private const int VRAM_CLOCK_COUNT = 172;
        private const int VBLANK_CLOCK_COUNT = 456;

        private const int OAM_SIZE = 0xA0;
        private const int SPRITE_SIZE = 0x04;
        private const int OAM_HORIZONTAL_LIMIT = 10;

        private int[, , ,] _tileset;

        public PaletteEntry[] _bgPalettes;
        public PaletteEntry[] _spPalettes;

        

        public PPU(Gameboy gameboy)
        {
            _gameboy = gameboy;
            
            Reset();
        }

        internal void Tick(int clocks)
        {
            int mode = _gameboy.Mmu.STAT & 0x03;

            if (Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 7))
            {
                clocksCount += clocks;

                if (clocksCount >= VBLANK_CLOCK_COUNT)
                {
                    _gameboy.Mmu.LY = (_gameboy.Mmu.LY + 1) % 154;
                    clocksCount -= VBLANK_CLOCK_COUNT;
                    CheckLYC();
                    if (_gameboy.Mmu.LY >= 144 && mode != 1) ChangeMode(1);
                }

                if(_gameboy.Mmu.LY < 144)
                {
                    if (clocksCount <= OAM_CLOCK_COUNT)
                    {
                        if (mode != 2) ChangeMode(2);
                    }
                    else if (clocksCount <= OAM_CLOCK_COUNT + VRAM_CLOCK_COUNT)
                    {
                        if (mode != 3) ChangeMode(3);
                    }
                    else { if (mode != 0) ChangeMode(0); }
                }
            }
        }

        internal ref int[] GetFrameBuffer()
        {
            if (useFb1) return ref fb2;
            else return ref fb1;

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
            fb1 = new int[SCREEN_WIDTH * SCREEN_HEIGHT * 4];
            fb2 = new int[SCREEN_WIDTH * SCREEN_HEIGHT * 4];
            FrameBuffer = fb1;
            useFb1 = true;

            // Initialize it to white
            for (int i = 0; i < FrameBuffer.Length; i++)
            {
                FrameBuffer[i] = 255;
            }
            clocksCount = 0;

            //ChangeMode(2);

            _tileset = new int[2, 384, 8, 8];

            for (int bank = 0; bank < 2; bank++)
            {
                for (int i = 0; i < 384; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            _tileset[bank, i, j, x] = 0;
                        }
                    }
                }
            }

            BGPriority = new int[SCREEN_WIDTH];

            _bgPalettes = new PaletteEntry[8];
            for (int i = 0; i < _bgPalettes.Length; i++) _bgPalettes[i] = new PaletteEntry(true);

            _spPalettes = new PaletteEntry[8];
            for (int i = 0; i < _spPalettes.Length; i++) _spPalettes[i] = new PaletteEntry(false);

            oamSprites = new int[OAM_HORIZONTAL_LIMIT];
        }

        private void RenderLine()
        {
            if(Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 7))
            {
                if (Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 0)) DrawBackground();
                else
                {
                    for (int i = 0; i < SCREEN_WIDTH; i++) BGPriority[i] = 0;
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
            bool bgPriority;

            for(int xx = 0; xx < SCREEN_WIDTH; xx++)
            {
                bool hFlip = false, vFlip = false;
                int paletteNumber = 0, vramBank = 0;

                bool isInWindow = (inWindowY && xx >= wx);

                int x = isInWindow ? (xx - wx) / 8 : Bitwise.Wrap8(xx + sx) / 8;
                int actualY = isInWindow ? windowY : y;

                int tile;// = _gameboy.Mmu.LoadVRAM(0x9800 + y + x);
                int mapLocation = isInWindow ? windowTileMapLocation : bgTileMapLocation;

                if (shouldValueBeSigned) tile = (sbyte)_gameboy.Mmu.LoadVRAM0(mapLocation + actualY + x);
                else tile = (byte)_gameboy.Mmu.LoadVRAM0(mapLocation + actualY + x);

                if (_gameboy.IsCGB)
                {
                    int value;
                    if (shouldValueBeSigned) value = (sbyte)_gameboy.Mmu.LoadVRAM1(mapLocation + actualY + x);
                    else value = (byte)_gameboy.Mmu.LoadVRAM1(mapLocation + actualY + x);

                    hFlip = Bitwise.IsBitOn(value, 5);
                    vFlip = Bitwise.IsBitOn(value, 6);
                    paletteNumber = value & 0x07;
                    vramBank = (value >> 3) & 0x01;
                    bgPriority = Bitwise.IsBitOn(value, 7);
                }
                else bgPriority = false;

                int drawX = hFlip ? 7 - (xx % 8) : xx;
                int drawY = vFlip ? 7 - (ly % 8) : ly;
                
                if (isInWindow)
                {
                    drawX = hFlip ? 7 - ((xx - wx) % 8) : (xx - wx);
                    drawY = vFlip ? 7 - ((ly - wy) % 8) : (ly - wy);

                    drawX %= 8;
                    drawY %= 8;
                }
                else
                {
                    if (hFlip) drawX = 7 - ((xx + sx) % 8);
                    else drawX = (drawX + sx) % 8;

                    if (vFlip) drawY = 7 - ((ly + sy) % 8);
                    else drawY = (drawY + sy) % 8;
                }

                int pixel = _tileset[vramBank, tileInitLocation + tile, drawY, drawX];

                if(!bgPriority) BGPriority[xx] = (pixel != 0) ? 1 : 0;
                else BGPriority[xx] = (pixel != 0) ? 2: 3;

                GBColor color = _bgPalettes[paletteNumber].Colors[pixel];
                FrameBuffer[startingIndex] = color.R;
                FrameBuffer[startingIndex + 1] = color.G;
                FrameBuffer[startingIndex + 2] = color.B;
                FrameBuffer[startingIndex + 3] = 255;
                startingIndex += 4;
            }
        }

        private void DrawSprites()
        {
            int ly = _gameboy.Mmu.LY;
            int lcdc = _gameboy.Mmu.LCDC;
            bool isSpriteHeight16 = Bitwise.IsBitOn(lcdc, 2);
            int spriteHeight = isSpriteHeight16 ? 16 : 8;
            int spriteCount = 0;

            for (int i = 0; i < OAM_SIZE; i += SPRITE_SIZE)
            {
                int spriteY = _gameboy.Mmu.LoadOAM(i) - 16;
                if (ly >= spriteY && ly < spriteY + spriteHeight)
                {
                    oamSprites[spriteCount++] = i;
                }
                if (spriteCount >= OAM_HORIZONTAL_LIMIT) break;
            }

            for(int i = spriteCount - 1; i >= 0; i--)
            {
                int index = oamSprites[i];
                int spriteY = _gameboy.Mmu.LoadOAM(index) - 16;
                int spriteX = _gameboy.Mmu.LoadOAM(index + 1) - 8;
                int tileNumber = _gameboy.Mmu.LoadOAM(index + 2);
                int upperTile = tileNumber & 0xFE;
                int lowerTile = tileNumber | 0x01;
                int attribs = _gameboy.Mmu.LoadOAM(index + 3);
                bool objAboveBg = !Bitwise.IsBitOn(attribs, 7);
                bool yFlip = Bitwise.IsBitOn(attribs, 6);
                bool xFlip = Bitwise.IsBitOn(attribs, 5);
                int paletteNumber = Bitwise.IsBitOn(attribs, 4) ? 1 : 0;
                int vramBank = 0;

                if(_gameboy.IsCGB)
                {
                    vramBank = Bitwise.IsBitOn(attribs, 3) ? 1 : 0;
                    paletteNumber = attribs & 0x07;
                }

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
                            int pixel = _tileset[vramBank, tileNumber, drawY % 8, drawX];

                            //int colorIndex = GetSpriteColorIndexFromPalette(pixel, paletteNumber);
                            if (pixel != 0 && BGPriority[spriteX + x] != 2)
                            {
                                if (objAboveBg || BGPriority[spriteX + x] == 0 || (!objAboveBg && BGPriority[spriteX + x] == 3))
                                {
                                    GBColor color = _spPalettes[paletteNumber].Colors[pixel];
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

        internal void UpdateTile(int address, int vramBank)
        {
            address &= 0x1FFE;

            int tile = (address >> 4) & 511;
            int y = (address >> 1) & 7;

            for(int x = 0; x < 8; x++)
            {
                int sx = 1 << (7 - x);

                if(vramBank == 0) _tileset[vramBank, tile, y, x] = (((_gameboy.Mmu.LoadVRAM0(address) & sx) != 0) ? 1 : 0) + (((_gameboy.Mmu.LoadVRAM0(address + 1) & sx) != 0) ? 2 : 0);
                else _tileset[vramBank, tile, y, x] = (((_gameboy.Mmu.LoadVRAM1(address) & sx) != 0) ? 1 : 0) + (((_gameboy.Mmu.LoadVRAM1(address + 1) & sx) != 0) ? 2 : 0);
            }
        }

        private void ChangeMode(int mode)
        {
            _gameboy.Mmu.STAT = (_gameboy.Mmu.STAT & ~0x03) | (mode & 0x03);

            // Handle Interrupts
            switch(mode)
            {
                case 0:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 3)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    RenderLine();
                    break;

                case 1:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 4)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    _gameboy.Mmu.SetInterrupt(Interrupts.VBlank);
                    useFb1 = !useFb1;
                    if (useFb1) FrameBuffer = fb1;
                    else FrameBuffer = fb2;

                    _gameboy.EnqeueFrameBuffer(FrameBuffer);

                    break;

                case 2:
                    if (Bitwise.IsBitOn(_gameboy.Mmu.STAT, 5)) _gameboy.Mmu.SetInterrupt(Interrupts.LCDStat);
                    break;
            }
        }

        internal void UpdateBackgroundPalettes(int value)
        {
            if (_gameboy.IsCGB)
            {
                if ((_gameboy.Mmu.STAT & 0x03) != 3)
                {
                    int paletteIndex = _gameboy.Mmu.ReadByte(0xFF68) & 0x3F;
                    _bgPalettes[paletteIndex / 8].UpdateCGB(_gameboy.Mmu, value);
                }
            }
            else
            {
                _bgPalettes[0].UpdateDMG(value);
            }
        }

        internal int ReadBackgroundPalettes()
        {
            int paletteIndex = _gameboy.Mmu.ReadByte(0xFF68) & 0x3F;
            return _bgPalettes[paletteIndex / 8].Read(_gameboy.Mmu);
        }

        internal void UpdateSpritePalettes(int value, int index)
        {
            if (_gameboy.IsCGB)
            {
                if ((_gameboy.Mmu.STAT & 0x03) != 3)
                {
                    int paletteIndex = _gameboy.Mmu.ReadByte(0xFF6A) & 0x3F;
                    _spPalettes[paletteIndex / 8].UpdateCGB(_gameboy.Mmu, value);
                }
            }
            else
            {
                _spPalettes[index].UpdateDMG(value);
            }
        }

        internal int ReadSpritePalettes()
        {
            int paletteIndex = _gameboy.Mmu.ReadByte(0xFF6A) & 0x3F;
            return _spPalettes[paletteIndex / 8].Read(_gameboy.Mmu);
        }

        internal void CheckIfLCDOff(int value)
        {
            bool lcdIsOn = Bitwise.IsBitOn(_gameboy.Mmu.LCDC, 7);
            bool newLCDState = Bitwise.IsBitOn(value, 7);

            if(lcdIsOn && !newLCDState)
            {
                _gameboy.Mmu.LY = 0;
                clocksCount = 0;
                _gameboy.Mmu.STAT &= ~0x03; // Reset mode to 0 if screen is off
            }
        }

        public bool IsHBlankMode()
        {
            return (_gameboy.Mmu.STAT & 0x03) == 0;
        }

        

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(clocksCount);
            int iLen = _tileset.GetLength(0), jLen = _tileset.GetLength(1), kLen = _tileset.GetLength(2), lLen = _tileset.GetLength(3);
            for (int i = 0; i < iLen; i++)
            {
                for (int j = 0; j < jLen; j++)
                {
                    for (int k = 0; k < kLen; k++)
                    {
                        for(int l = 0; l < lLen; l++)
                        {
                            stream.Write(_tileset[i, j, k, l]);
                        }
                    }
                }
            }

            for (int i = 0; i < _bgPalettes.Length; i++) _bgPalettes[i].SaveState(stream);
            for (int i = 0; i < _spPalettes.Length; i++) _spPalettes[i].SaveState(stream);
        }

        public void LoadState(BinaryReader stream)
        {
            clocksCount = stream.ReadInt32();
            int iLen = _tileset.GetLength(0), jLen = _tileset.GetLength(1), kLen = _tileset.GetLength(2), lLen = _tileset.GetLength(3);
            for (int i = 0; i < iLen; i++)
            {
                for (int j = 0; j < jLen; j++)
                {
                    for (int k = 0; k < kLen; k++)
                    {
                        for (int l = 0; l < lLen; l++)
                        {
                            _tileset[i, j, k, l] = stream.ReadInt32();
                        }
                    }
                }
            }

            for (int i = 0; i < _bgPalettes.Length; i++) _bgPalettes[i].LoadState(stream);
            for (int i = 0; i < _spPalettes.Length; i++) _spPalettes[i].LoadState(stream);
        }
    }
}
