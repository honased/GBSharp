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
        private int divClocks = 0;
        private int timerClocks = 0;
        private bool timerEnabled = false;
        int timerClockGoal = 1024;
        private bool requestInterrupt = false;
        private bool checkingLow = false;
        private int internalDiv = 0;

        public Timer(Gameboy gameboy)
        {
            _gameboy = gameboy;
        }

        internal void Tick(int clocks)
        {
            if(requestInterrupt)
            {
                requestInterrupt = false;
                _gameboy.Mmu.SetInterrupt(Interrupts.Timer);
            }
            
            divClocks += clocks;
            internalDiv = Bitwise.Wrap16(internalDiv + clocks);
            while(divClocks >= 256)
            {
                divClocks -= 256;
                _gameboy.Mmu.DIV = Bitwise.Wrap16(_gameboy.Mmu.DIV + 1);

                if(timerEnabled)
                {
                    if(!checkingLow)
                    {
                        if(Bitwise.IsBitOn(internalDiv, timerClockGoal))
                        {
                            checkingLow = true;
                        }
                    }
                    else
                    {
                        if(!Bitwise.IsBitOn(internalDiv, timerClockGoal))
                        {
                            checkingLow = false;
                            _gameboy.Mmu.TIMA = Bitwise.Wrap8(_gameboy.Mmu.TIMA + 1);
                            if(_gameboy.Mmu.TIMA == 0)
                            {
                                _gameboy.Mmu.TIMA = _gameboy.Mmu.TMA;
                                requestInterrupt = true;
                            }
                        }
                    }
                }
            }

            /*if(timerEnabled)
            {
                timerClocks += clocks;
                while(timerClocks >= timerClockGoal)
                {
                    timerClocks -= timerClockGoal;
                    _gameboy.Mmu.TIMA = Bitwise.Wrap8(_gameboy.Mmu.TIMA + 1);
                    if(_gameboy.Mmu.TIMA == 0)
                    {
                        _gameboy.Mmu.TIMA = _gameboy.Mmu.TMA;
                        requestInterrupt = true;
                    }
                }
            }*/
        }

        internal void Reset()
        {
            timerEnabled = false;
            timerClockGoal = 9;
            divClocks = 0;
            timerClocks = 0;
            timerEnabled = false;
            requestInterrupt = false;
            checkingLow = false;
            internalDiv = 0;
        }

        public override string ToString()
        {
            return "DIV:" + _gameboy.Mmu.DIV.ToString() + "\tTimer Enabled:" + Bitwise.IsBitOn(_gameboy.Mmu.TAC, 2).ToString() + "\tTimer:" + _gameboy.Mmu.TIMA.ToString()
                + "\tTimer Clocks:" + timerClocks.ToString() + "\tGoal:" + timerClockGoal.ToString();
        }

        internal void Update()
        {
            timerEnabled = Bitwise.IsBitOn(_gameboy.Mmu.TAC, 2);
            if (!timerEnabled) timerClocks = 0;

            switch(_gameboy.Mmu.TAC & 0x03)
            {
                case 0: timerClockGoal = 9; break;
                case 1: timerClockGoal = 3; break;
                case 2: timerClockGoal = 5; break;
                case 3: timerClockGoal = 7; break;

                default: throw new Exception("Invalid timer setting!");
            }
        }

        internal void UpdateDiv()
        {
            //_gameboy.Mmu.TIMA = 0;
            timerClocks = 0;
            divClocks = 0;
        }
    }
}
