using System.IO;
using GBSharp.Interfaces;

namespace GBSharp
{
    public class Input : IStateable
    {
        private readonly Gameboy _gameboy;

        private int directionInput, buttonInput;

        bool setButtonInterrupt, setDirectionInterrupt;

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

        public Input(Gameboy gameboy)
        {
            _gameboy = gameboy;

            Reset();
        }

        public void Reset()
        {
            setDirectionInterrupt = false;
            setButtonInterrupt = false;
            directionInput = 0xF;
            buttonInput = 0xF;
        }

        public void Tick()
        {
            if(!Bitwise.IsBitOn(_gameboy.Mmu.Joypad, 4)) // Direction Keys
            {
                _gameboy.Mmu.Joypad = (_gameboy.Mmu.Joypad & 0xF0) | (directionInput);
                if (setDirectionInterrupt) _gameboy.Mmu.SetInterrupt(Interrupts.Joypad);
            }
            if (!Bitwise.IsBitOn(_gameboy.Mmu.Joypad, 5)) // Button Keys
            {
                _gameboy.Mmu.Joypad = (_gameboy.Mmu.Joypad & 0xF0) | (buttonInput);
                if (setButtonInterrupt) _gameboy.Mmu.SetInterrupt(Interrupts.Joypad);
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

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(directionInput);
            stream.Write(buttonInput);
            stream.Write(setButtonInterrupt);
            stream.Write(setDirectionInterrupt);
        }

        public void LoadState(BinaryReader stream)
        {
            directionInput = stream.ReadInt32();
            buttonInput = stream.ReadInt32();
            setButtonInterrupt = stream.ReadBoolean();
            setDirectionInterrupt = stream.ReadBoolean();
        }
    }
}
