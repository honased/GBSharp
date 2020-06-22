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
            L
        }

        public enum Registers16Bit
        {
            AF,
            BC,
            DE,
            HL,
            SP,
            PC
        }

        public enum Flags
        {
            Z = 128,
            N = 64,
            H = 32,
            C = 16
        }

        private void SetFlag(int flags, bool on)
        {
            int currentFlag = LoadRegister(Registers8Bit.F);

            if (on) SetRegister(Registers8Bit.F, currentFlag | flags);
            else SetRegister(Registers8Bit.F, currentFlag & (~flags));
        }

        /*private void SetFlag(Flags flag, bool on)
        {
            SetFlag((int)flag, on);
        }*/

        private byte LoadRegister(Registers8Bit register)
        {
            int index = ((int)register) / 2;
            if(((int)register) % 2 == 0) return (byte)(_registers[index] >> 8);
            return (byte)_registers[index];
        }

        private short LoadRegister(Registers16Bit register)
        {
            return (short)_registers[((int)register) / 2];
        }

        private void SetRegister(Registers8Bit register, int value)
        {
            int index = ((int)register) / 2;
            short newValue = (short)value;
            if (((int)register) % 2 == 0)
            {
                newValue <<= 8;
                _registers[index] = (short)((_registers[index] & 0x00FF) | newValue);
            }
            else
            {
                _registers[index] = (short)((_registers[index] & 0xFF00) | newValue);
            }
        }

        private void SetRegister(Registers16Bit register, int value)
        {
            _registers[((int)register) / 2] = value;
        }
    }
}
