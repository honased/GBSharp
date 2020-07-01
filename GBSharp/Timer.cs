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
            divClocks += clocks*4;
            while(divClocks >= 256)
            {
                divClocks -= 256;
                _mmu.DIV = Bitwise.Wrap8(_mmu.DIV + 1);
            }

            if(timerEnabled)
            {
                timerClocks += clocks*4;
                while(timerClocks >= timerClockGoal)
                {
                    timerClocks -= timerClockGoal;
                    _mmu.TIMA = Bitwise.Wrap8(_mmu.TIMA + 1);
                    if(_mmu.TIMA == 0)
                    {
                        _mmu.TIMA = _mmu.TMA;
                        _mmu.SetInterrupt(Interrupts.Timer);
                    }
                }
            }
        }

        internal void Reset()
        {
            timerEnabled = false;
            timerClockGoal = 1024;
            divClocks = 0;
            timerClocks = 0;
            timerEnabled = false;
        }

        public override string ToString()
        {
            return "DIV:" + _mmu.DIV.ToString() + "\tTimer Enabled:" + Bitwise.IsBitOn(_mmu.TAC, 2).ToString() + "\tTimer:" + _mmu.TIMA.ToString()
                + "\tTimer Clocks:" + timerClocks.ToString() + "\tGoal:" + timerClockGoal.ToString();
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
