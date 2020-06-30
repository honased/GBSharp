using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    internal class Timer
    {
        private MMU _mmu;
        private int divClocks = 0;
        private int timerClocks = 0;
        private bool timerEnabled = false;
        int timerClockGoal = 1024;

        public Timer(MMU mmu)
        {
            _mmu = mmu;
        }

        internal void Tick(int clocks)
        {
            divClocks += clocks;
            while(divClocks >= 256)
            {
                divClocks -= 256;
                _mmu.DIV = (_mmu.DIV + 1) % 256;
            }

            if(timerEnabled)
            {
                timerClocks += clocks;
                while(timerClocks >= timerClockGoal)
                {
                    timerClocks -= timerClockGoal;
                    _mmu.TIMA = (_mmu.TIMA + 1) % 256;
                    if(_mmu.TIMA == 0)
                    {
                        _mmu.TIMA = _mmu.TMA;
                        _mmu.SetInterrupt(Interrupts.Timer);
                    }
                }
            }
        }

        internal void Update()
        {
            timerEnabled = Bitwise.IsBitOn(_mmu.TAC, 2);

            switch(_mmu.TAC & 0x03)
            {
                case 0: timerClockGoal = 1024; break;
                case 1: timerClockGoal = 16; break;
                case 2: timerClockGoal = 64; break;
                case 3: timerClockGoal = 256; break;

                default: throw new Exception("Invalid timer setting!");
            }
        }
    }
}
