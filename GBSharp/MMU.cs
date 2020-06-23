using System;
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
            _bios = new int[0x100];
            _rom = new int[2,0x4000];
            _vram = new int[0x2000];
            _eram = new int[0x2000];
            _wram = new int[0x2000];
            _oam = new int[0xA0];
            _io = new int[0x4C];
            _zram = new int[0x7F];
            SetBios(CartridgeLoader.LoadCart("InternalRoms/DMG_ROM.bin"));
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

        public void SetBios(int[] bios)
        {
            for(int i = 0; i < _bios.Length; i++)
            {
                _bios[i] = bios[i];
            }
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
            WriteByte(value >> 8, address);
        }
    }
}
