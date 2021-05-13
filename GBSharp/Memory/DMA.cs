using GBSharp.Interfaces;
using System;
using System.IO;

namespace GBSharp
{
    public class DMA : IStateable
    {
        public int Source { get; private set; }
        public int Destination { get; private set; }
        public int Length { get; private set; }
        public bool IsHDMA { get; private set; }
        public bool IsEnabled { get; private set; }
        private int _lastIndex;

        private readonly Gameboy _gameboy;

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
                Destination = ((destHi & 0x1F) << 8) | (destLo & 0xF0);
                Length = ((lenModStrt & 0x7F) + 1) * 16;
                IsHDMA = Bitwise.IsBitOn(lenModStrt, 7);
                Source &= 0xFFF0;
                Destination = (Destination & 0x1FFF) | 0x8000;
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
            return (Length == 0) ? 0xFF : Math.Max(0, (Length/16) - 1);
        }

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(Source);
            stream.Write(Destination);
            stream.Write(Length);
            stream.Write(IsHDMA);
            stream.Write(IsEnabled);
            stream.Write(_lastIndex);
        }

        public void LoadState(BinaryReader stream)
        {
            Source = stream.ReadInt32();
            Destination = stream.ReadInt32();
            Length = stream.ReadInt32();
            IsHDMA = stream.ReadBoolean();
            IsEnabled = stream.ReadBoolean();
            _lastIndex = stream.ReadInt32();
        }
    }
}
