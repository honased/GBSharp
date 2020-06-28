using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class Input
    {
        private MMU _mmu;

        private const int MASK_DIRECTION = 0x10, MASK_BUTTON = 0x20;

        private int directionInput = 0xF, buttonInput = 0xF;

        bool setButtonInterrupt = false, setDirectionInterrupt = false;

        public enum Button
        {
            Right = 0,
            Left = 1,
            Up = 2,
            Down = 3,
            A = 4,
            B = 5,
            Select = 6,
            Start = 7,
        }

        public Input(MMU mmu)
        {
            _mmu = mmu;
        }

        public void Tick()
        {
            if(!Bitwise.IsBitOn(_mmu.Joypad, 4)) // Direction Keys
            {
                _mmu.Joypad = (_mmu.Joypad & 0xF0) | (directionInput);
                if (setDirectionInterrupt) _mmu.SetInterrupt(Interrupts.Joypad);
            }
            if (!Bitwise.IsBitOn(_mmu.Joypad, 5)) // Button Keys
            {
                _mmu.Joypad = (_mmu.Joypad & 0xF0) | (buttonInput);
                if (setButtonInterrupt) _mmu.SetInterrupt(Interrupts.Joypad);
            }

            setButtonInterrupt = false;
            setDirectionInterrupt = false;
        }

        public void SetInput(Button button, bool Pressed)
        {
            int buttonVal = (int)button;
            int bitIndex = buttonVal % 4;
            if(buttonVal > 3)
            {
                if (Pressed)
                {
                    if (Bitwise.IsBitOn(buttonInput, bitIndex)) setButtonInterrupt = true;
                    buttonInput = Bitwise.ClearBit(buttonInput, bitIndex);
                }
                else buttonInput = Bitwise.SetBit(buttonInput, bitIndex);
            }
            else
            {
                if (Pressed)
                {
                    if (Bitwise.IsBitOn(directionInput, bitIndex)) setDirectionInterrupt = true;
                    directionInput = Bitwise.ClearBit(directionInput, bitIndex);
                }
                else directionInput = Bitwise.SetBit(directionInput, bitIndex);
            }
        }
    }
}
