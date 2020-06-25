﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class MMU
    {
        private int[] _bios;
        private int[,] _rom;
        private int[] _vram;
        private int[] _wram;
        private int[] _eram;
        private int[] _zram;
        private int[] _oam;
        private int[] _io;

        private CPU _cpu;

        private bool _inBios;

        public const int MEMORY_SIZE = 0xFFFF;

        public MMU()
        {
            _inBios = true;
            _rom = new int[2,0x4000];
            _vram = new int[0x2000];
            _eram = new int[0x2000];
            _wram = new int[0x2000];
            _oam = new int[0xA0];
            _io = new int[0x4C];
            _zram = new int[0x7F];

            SetBios();
        }

        public void SetCPU(CPU cpu)
        {
            _cpu = cpu;
        }

        public void WriteBytes(int[] bytes, int address)
        {
            //Array.Copy(bytes, 0, _memoryBank, address, bytes.Length);
            for(int i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i], address + i);
            }
        }

        public void SetBios()
        {
            _bios = new int[]{
            0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB, 0x21, 0x26, 0xFF, 0x0E,
            0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0,
            0x47, 0x11, 0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
            0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 0xF9,
            0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99, 0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20,
            0xF9, 0x2E, 0x0F, 0x18, 0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
            0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 0x1D, 0x20, 0xF2,
            0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62, 0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06,
            0x7B, 0xE2, 0x0C, 0x3E, 0x87, 0xE2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
            0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17, 0xC1, 0xCB, 0x11, 0x17,
            0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
            0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
            0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
            0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E, 0x3C, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x3C,
            0x21, 0x04, 0x01, 0x11, 0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x00, 0x00, 0x23, 0x7D, 0xFE, 0x34, 0x20,
            0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x00, 0x00, 0x3E, 0x01, 0xE0, 0x50
            };
        }

        public void WriteByte(int value, int address)
        {
            switch (address & 0xF000)
            {
                // Bios / Rom0
                case int _ when address < 0x4000:
                    _rom[0, address] = value;
                    break;
                case int _ when address < 0x8000:
                    _rom[1, address - 0x4000] = value;
                    break;
                case int _ when address < 0xA000:
                    _vram[address - 0x8000] = value;
                    break;
                case int _ when address < 0xC000:
                    _eram[address - 0xA000] = value;
                    break;
                case int _ when address < 0xE000:
                    _wram[address - 0xC000] = value;
                    break;
                case int _ when address < 0xFE00:
                    WriteByte(value, address - 0x2000);
                    break;
                case int _ when address < 0xFEA0:
                    _oam[address - 0xFE00] = value;
                    break;
                case int _ when address < 0xFF00:
                    break;
                case int _ when address < 0xFF4C:
                    _io[address - 0xFF00] = value;
                    break;
                case int _ when address < 0xFF80:
                    break;
                case int _ when address < 0xFFFF:
                    _zram[address - 0xFF80] = value;
                    break;
                default:
                    Console.WriteLine("Out of memory bank");
                    break;
            }
        }

        public int ReadByte(int address)
        {
            switch (address & 0xF000)
            {
                // Bios / Rom0
                case int _ when address < 0x4000:
                    if(_inBios)
                    {
                        if (address < 0x100) return _bios[address];
                        if(_cpu.LoadRegister(CPU.Registers16Bit.PC) == 0x0100) _inBios = false;
                    }
                    return _rom[0, address];
                case int _ when address < 0x8000:
                    return _rom[1, address - 0x4000];
                case int _ when address < 0xA000:
                    return _vram[address - 0x8000];
                case int _ when address < 0xC000:
                    return _eram[address - 0xA000];
                case int _ when address < 0xE000:
                    return _wram[address - 0xC000];
                case int _ when address < 0xFE00:
                    return ReadByte(address - 0x2000);
                case int _ when address < 0xFEA0:
                    return _oam[address - 0xFE00];
                case int _ when address < 0xFF00:
                    return 0;
                case int _ when address < 0xFF4C:
                    return _io[address - 0xFF00];
                case int _ when address < 0xFF80:
                    return 0;
                case int _ when address < 0xFFFF:
                    return _zram[address - 0xFF80];

                default:
                    Console.WriteLine("Out of memory bank");
                    return 0x00;
            }
        }

        public int ReadWord(int address)
        {
            return ReadByte(address) | (ReadByte(address + 1) << 8);
        }

        public void WriteWord(int value, int address)
        {
            WriteByte(value & 0x00FF, address);
            WriteByte(value >> 8, address + 1);
        }
    }
}
