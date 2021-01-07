using System;
using System.IO;
using GBSharp.Interfaces;

namespace GBSharp.Processor
{
    internal class Timer : IStateable
    {
        private readonly Gameboy _gameboy;
        private bool timerEnabled = false;
        private int timerBit;
        private int cycles;
        private bool checkingLow;
        private int internalDiv;
        private bool overflow;
        private bool timaWritten;

        public Timer(Gameboy gameboy)
        {
            _gameboy = gameboy;
            Reset();
        }

        internal void Tick(int clocks)
        {
            if(timaWritten && cycles < 4)
            {
                timaWritten = false;
                cycles = 0;
                overflow = false;
            }

            while(clocks-- > 0)
            {
                internalDiv = Bitwise.Wrap16(internalDiv + 1);
                if (overflow) cycles++;

                if (!checkingLow)
                {
                    checkingLow = timerEnabled && Bitwise.IsBitOn(internalDiv, timerBit);
                }
                else
                {
                    checkingLow = timerEnabled && Bitwise.IsBitOn(internalDiv, timerBit);
                    if(!checkingLow)
                    {
                        _gameboy.Mmu.TIMA = Bitwise.Wrap8(_gameboy.Mmu.TIMA + 1);
                        if (_gameboy.Mmu.TIMA == 0) overflow = true;
                    }
                }

                if (cycles == 4)
                {
                    _gameboy.Mmu.SetInterrupt(Interrupts.Timer);
                    cycles = 0;
                    overflow = false;
                    _gameboy.Mmu.TIMA = _gameboy.Mmu.TMA;
                    timaWritten = false;
                }
            }

            

            _gameboy.Mmu.DIV = (internalDiv & 0xFF00) >> 8;
        }

        internal void Reset()
        {
            timerEnabled = false;
            timerBit = 9;
            timerEnabled = false;
            cycles = 0;
            checkingLow = false;
            internalDiv = 0;
            overflow = false;
            timaWritten = false;
        }

        public override string ToString()
        {
            return $"DIV:{_gameboy.Mmu.DIV.ToString("X2")}\tTIMA:{_gameboy.Mmu.TIMA.ToString("X2")}\tTMA:{_gameboy.Mmu.TMA.ToString("X2")}\tTAC:{_gameboy.Mmu.TAC.ToString("X2")}";
        }

        internal void Update()
        {
            timerEnabled = Bitwise.IsBitOn(_gameboy.Mmu.TAC, 2);

            switch(_gameboy.Mmu.TAC & 0x03)
            {
                case 0: timerBit = 9; break;
                case 1: timerBit = 3; break;
                case 2: timerBit = 5; break;
                case 3: timerBit = 7; break;

                default: throw new Exception("Invalid timer setting!");
            }
        }

        internal void UpdateDiv()
        {
            internalDiv = 0;
        }

        internal void UpdateTIMA()
        {
            timaWritten = true;
        }

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(timerEnabled);
            stream.Write(timerBit);
            stream.Write(cycles);
            stream.Write(checkingLow);
            stream.Write(internalDiv);
            stream.Write(overflow);
            stream.Write(timaWritten);
        }

        public void LoadState(BinaryReader stream)
        {
            timerEnabled = stream.ReadBoolean();
            timerBit = stream.ReadInt32();
            cycles = stream.ReadInt32();
            checkingLow = stream.ReadBoolean();
            internalDiv = stream.ReadInt32();
            overflow = stream.ReadBoolean();
            timaWritten = stream.ReadBoolean();
        }
    }
}
