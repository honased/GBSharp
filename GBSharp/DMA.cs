using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class DMA
    {
        public int Source { get; private set; }
        public int Destination { get; private set; }
        public int Length { get; private set; }
        public bool IsHDMA { get; private set; }
        public bool IsEnabled { get; private set; }
        private int _lastIndex;

        private Gameboy _gameboy;

        public DMA(Gameboy gameboy)
        {
            _gameboy = gameboy;
            Reset();
        }

        public void Reset()
        {
            IsEnabled = false;
            Source = 0;
            Destination = 0;
            Length = 0;
            IsHDMA = false;
            _lastIndex = 0;
        }

        public void StartDMA(int srcHi, int srcLo, int destHi, int destLo, int lenModStrt)
        {
            if(!IsEnabled && _gameboy.IsCGB)
            {
                Source = (srcHi << 8) | (srcLo & 0xF0);
                Destination = (destHi << 8) | (destLo & 0xF0);
                Length = ((lenModStrt & 0x7F) + 1) * 16;
                IsHDMA = Bitwise.IsBitOn(lenModStrt, 7);
                IsEnabled = true;
            }
        }

        public int CopyData()
        {
            if(IsEnabled)
            {
                // GDMA case
                if(!IsHDMA)
                {
                    int count = 0;
                    while(Length > 0)
                    {
                        Length--;
                        _gameboy.Mmu.WriteByte(_gameboy.Mmu.ReadByte(Source + count), Destination + count);
                        count++;
                    }
                    IsEnabled = false;
                    return (_gameboy.Cpu.DoubleSpeed) ? Length : Length / 2;
                }
                else
                {
                    if (_gameboy.Ppu.IsHBlankMode())
                    {
                        for(int i = _lastIndex; i < _lastIndex + 16; i++)
                        {
                            _gameboy.Mmu.WriteByte(_gameboy.Mmu.ReadByte(Source + i), Destination + i);
                        }
                        Length -= 16;
                        _lastIndex += 16;

                        if(Length == 0)
                        {
                            IsEnabled = false;
                        }

                        return (_gameboy.Cpu.DoubleSpeed) ? 16 : 8;
                    }
                }
            }
            return 0;
        }

        public int Read()
        {
            return ((IsEnabled) ? 0x80 : 0x00) | (Math.Max(0, (Length/16) - 1));
        }
    }
}
