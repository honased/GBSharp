using GBSharp.Audio;
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
        private int[] _vram;
        private int[] _wram;
        private int[] _eram;
        private int[] _zram;
        private int[] _oam;
        private int[] _io;
        private PPU _ppu;
        private Timer _timer;
        internal APU _apu;

        public int IE
        {
            get
            {
                return _zram[0x7F];
            }
            set
            {
                _zram[0x7F] = value;
            }
        }
        public int IF
        {
            get
            {
                return _io[0x0F];
            }
            set
            {
                _io[0x0F] = value;
            }
        }

        public int SCX { get { return _io[0x43]; } }
        public int SCY { get { return _io[0x42]; } }
        public int WX { get { return _io[0x4B]; } }
        public int WY { get { return _io[0x4A]; } }

        public int LCDC { get { return _io[0x40]; } }
        public int LY { get { return _io[0x44]; } set { _io[0x44] = value; } }
        public int LYC { get { return _io[0x45]; } }
        public int STAT { get { return _io[0x41]; } set { _io[0x41] = value; } }

        public int Joypad { get { return _io[0x00]; } set { _io[0x00] = value; } }

        public int DIV { get { return _io[0x04]; } set { _io[0x04] = value; } }

        public int TIMA { get { return _io[0x05]; } set { _io[0x05] = value; } }

        public int TMA { get { return _io[0x06]; } set { _io[0x06] = value; } }

        public int TAC { get { return _io[0x07]; } set { _io[0x07] = value; } }

        public int bgPalette { get { return _io[0x47]; } }

        public int GetObjPalette(int palette)
        {
            return _io[0x48 + palette];
        }

        private CPU _cpu;

        private bool _inBios;

        internal Cartridge _cartridge;

        public MMU()
        {
            _inBios = false;
            _vram = new int[0x2000];
            _eram = new int[0x2000];
            _wram = new int[0x2000];
            _oam = new int[0xA0];
            _io = new int[0x80];
            _zram = new int[0x80];

            Reset();

            SetBios();
        }

        internal void Reset()
        {
            for (int i = 0; i < _vram.Length; i++) _vram[i] = 0;
            for (int i = 0; i < _eram.Length; i++) _eram[i] = 0;
            for (int i = 0; i < _wram.Length; i++) _wram[i] = 0;
            for (int i = 0; i < _oam.Length; i++) _oam[i] = 0;
            for (int i = 0; i < _io.Length; i++) _io[i] = 0;
            for (int i = 0; i < _zram.Length; i++) _zram[i] = 0;

            WriteByte(0x91, 0xFF40);
        }

        internal void StartInBios()
        {
            _inBios = true;
        }

        internal void SetCPU(CPU cpu)
        {
            _cpu = cpu;
        }

        internal void SetPPU(PPU ppu)
        {
            _ppu = ppu;
        }

        internal void SetTimer(Timer timer)
        {
            _timer = timer;
        }

        public void WriteBytes(int[] bytes, int address)
        {
            //Array.Copy(bytes, 0, _memoryBank, address, bytes.Length);
            for(int i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i], address + i);
            }
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge;
        }

        private void SetBios()
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
            value &= 0xFF; // Ensure that it doesn't go out of bounds of being a byte (0-255)

            switch (address & 0xF000)
            {
                // Bios / Rom0
                case int _ when address < 0x8000:
                    _cartridge.WriteRom(address, value);
                    break;
                case int _ when address < 0xA000:
                    _vram[address - 0x8000] = value;
                    if(address < 0x9800) _ppu.UpdateTile(address, value);
                    break;
                case int _ when address < 0xC000:
                    _cartridge.WriteERam(address, value);
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
                case int _ when address < 0xFF80:
                    switch(address)
                    {
                        case 0xFF41:
                            value = (value & ~0x03) | (_io[0x41] & 0x03);
                            break;

                        case 0xFF04:
                            _timer.UpdateDiv();
                            value = 0;
                            break;

                        case 0xFF44:
                            value = 0;
                            break;

                        case 0xFF0F:
                            value |= 0xE0;
                            break;

                        case 0xFF46:
                            int addr = value << 8;
                            for(int i = 0; i < _oam.Length; i++)
                            {
                                _oam[i] = ReadByte(addr + i);
                            }
                            break;

                    }

                    if(address >= 0xFF10 && address <= 0xFF26)
                    {
                        value = _apu.WriteByte(address, value);
                        if (value == -1) value = ReadByte(address);
                    }

                    _io[address - 0xFF00] = value;

                    if(address == 0xFF07)
                    {
                        _timer.Update();
                    }

                    break;
                case int _ when address <= 0xFFFF:
                    _zram[address - 0xFF80] = value; 
                    break;

                default:
                    //Console.WriteLine("Out of memory bank");
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
                        if(_cpu.LoadRegister(CPU.Registers16Bit.PC) == 0x0101) _inBios = false;
                    }
                    return _cartridge.ReadLowRom(address);
                case int _ when address < 0x8000:
                    return _cartridge.ReadHighRom(address);
                case int _ when address < 0xA000:
                    return _vram[address - 0x8000];
                case int _ when address < 0xC000:
                    return _cartridge.ReadERam(address);
                case int _ when address < 0xE000:
                    return _wram[address - 0xC000];
                case int _ when address < 0xFE00:
                    return ReadByte(address - 0x2000);
                case int _ when address < 0xFEA0:
                    return _oam[address - 0xFE00];
                case int _ when address < 0xFF00:
                    return 0;
                case int _ when address < 0xFF4C:
                    if (address >= 0xFF10 && address <= 0xFF26)
                    {
                        return _apu.ReadByte(address);
                    }
                    return _io[address - 0xFF00];
                case int _ when address < 0xFF80:
                    return 0xFF;
                case int _ when address <= 0xFFFF:
                    return _zram[address - 0xFF80];

                default:
                    //Console.WriteLine("Out of memory bank");
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

        public int LoadVRAM(int addr)
        {
            return _vram[addr & 0x1FFF];
        }

        public int LoadOAM(int addr)
        {
            return _oam[addr & 0x1FFF];
        }

        internal void SetInterrupt(Interrupts interrupt)
        {
            IF = Bitwise.SetBit(IF, (int)interrupt);
        }
    }
}
