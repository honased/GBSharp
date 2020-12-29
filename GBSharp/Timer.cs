using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    internal class Timer
    {
        private Gameboy _gameboy;
        private bool timerEnabled = false;
        int timerBit;
        private int cycles;
        private bool checkingLow;
        private int internalDiv;
        private bool overflow;

        public Timer(Gameboy gameboy)
        {
            _gameboy = gameboy;
            Reset();
        }

        internal void Tick(int clocks)
        {
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
                    //_gameboy.Mmu.SetInterrupt(Interrupts.Timer);
                    cycles = 0;
                    overflow = false;
                    _gameboy.Mmu.TIMA = _gameboy.Mmu.TMA;
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
            if((cycles / 4) != 1)
            {
                overflow = false;
                cycles = 0;
            }
        }
    }
}
