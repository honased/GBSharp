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
        private int tick = 0;

        bool setButtonInterrupt = false, setDirectionInterrupt = false;

        public enum Button
        {
            Right = 0x11,
            A = 0x21,
            Left = 0x12,
            B = 0x22,
            Up = 0x14,
            Select = 0x24,
            Down = 0x18,
            Start = 0x28
        }

        public Input(MMU mmu)
        {
            _mmu = mmu;
            tick = 0;
        }

        public void Tick()
        {
            if(!Bitwise.IsBitOn(_mmu.Joypad, 4)) // Direction Keys
            {
                _mmu.Joypad = (_mmu.Joypad & 0xF0) | (directionInput);
                if (setDirectionInterrupt) _mmu.SetInterrupt(4);
            }
            if (!Bitwise.IsBitOn(_mmu.Joypad, 5)) // Button Keys
            {
                _mmu.Joypad = (_mmu.Joypad & 0xF0) | (buttonInput);
                if (setButtonInterrupt) _mmu.SetInterrupt(4);
            }

            if (tick++ > 12000)
            {
                Console.WriteLine("{0:X}", directionInput);
                tick = 0;
            }

            setButtonInterrupt = false;
            setDirectionInterrupt = false;
        }

        private int ConvertButtonToBitIndex(Button button)
        {
            int buttonVal = ((int)button) & 0xF;
            switch(buttonVal)
            {
                case 0x01: return 0;
                case 0x02: return 1;
                case 0x04: return 2;
                case 0x08: return 3;
                default: return -1;
            }
        }

        public void SetInput(Button button, bool Pressed)
        {
            int buttonVal = (int)button;
            int bitIndex = ConvertButtonToBitIndex(button);
            if((buttonVal & MASK_BUTTON) > 0xF)
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
