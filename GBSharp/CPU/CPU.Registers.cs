using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public partial class CPU
    {
        private int[] _registers;

        public void InitializeRegisters()
        {
            _registers = new int[6];
            //SetRegister(Registers16Bit.AF, 0x01B0);
            //SetRegister(Registers16Bit.BC, 0x0013);
            //SetRegister(Registers16Bit.DE, 0x00D8);
            //SetRegister(Registers16Bit.HL, 0x014D);
            //SetRegister(Registers16Bit.SP, 0xFFFE);
        }

        public enum Registers8Bit
        {
            A,
            F,
            B,
            C,
            D,
            E,
            H,
            L,
            None = -1
        }

        public enum Registers16Bit
        {
            AF,
            BC,
            DE,
            HL,
            SP,
            PC,
            None = -1
        }

        public enum Flags
        {
            Z = 128,
            N = 64,
            H = 32,
            C = 16,
            None = 0
        }

        private void SetFlag(int flags, bool on)
        {
            int currentFlag = LoadRegister(Registers8Bit.F);

            if (on) SetRegister(Registers8Bit.F, currentFlag | flags);
            else SetRegister(Registers8Bit.F, currentFlag & (~flags));
        }

        private void SetFlag(Flags flag, bool on)
        {
            SetFlag((int)flag, on);
        }

        private bool IsFlagOn(int flags)
        {
            return (LoadRegister(Registers8Bit.F) & flags) > 0;
        }

        private bool IsFlagOn(Flags flag)
        {
            return IsFlagOn((int)flag);
        }

        public int LoadRegister(Registers8Bit register)
        {
            int index = ((int)register) / 2;
            if(((int)register) % 2 == 0) return (_registers[index] >> 8);
            return _registers[index] & 0x00FF;
        }

        public int LoadRegister(Registers16Bit register)
        {
            return _registers[((int)register)];
        }

        private void SetRegister(Registers8Bit register, int value)
        {
            int index = ((int)register) / 2;
            int newValue = value;
            if (((int)register) % 2 == 0)
            {
                newValue <<= 8;
                _registers[index] = (_registers[index] & 0x00FF) | (newValue & 0xFF00);
            }
            else
            {
                _registers[index] = (_registers[index] & 0xFF00) | (newValue & 0x00FF);
            }
            if(LoadRegister(Registers8Bit.A) > 255)
            {
                Console.WriteLine("TEST");
            }
        }

        private void SetRegister(Registers16Bit register, int value)
        {
            _registers[((int)register)] = value;
        }
    }
}
