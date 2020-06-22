using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public delegate int InstructionMethod(Instruction instruction);

    public class Instruction
    {
        public string Name { get; private set; }

        private InstructionMethod method;

        public CPU.Registers16Bit registers16Bit;
        public CPU.Registers8Bit registers8bit;
        public CPU.Registers16Bit registers16Bit2;
        public CPU.Registers8Bit registers8bit2;

        public Instruction(string name, InstructionMethod method)
        {
            Name = name;
            this.method = method;

            registers16Bit = CPU.Registers16Bit.None;
            registers16Bit2 = CPU.Registers16Bit.None;

            registers8bit = CPU.Registers8Bit.None;
            registers8bit2 = CPU.Registers8Bit.None;
        }

        public int Execute()
        {
            return method.Invoke(this);
        }
    }

    public partial class CPU
    {
        private Instruction[] _instructions;

        private void RegisterInstructions()
        {
            _instructions = new Instruction[0xFF];

            AddInstruction(0x32, new Instruction("LD (HL-),A", Instruction_LDD_HL_A));

            // LD n,nn
            AddInstruction(0x01, new Instruction("LD BC, nn", Instruction_LDn_nn) { registers16Bit = Registers16Bit.BC, });
            AddInstruction(0x11, new Instruction("LD DE, nn", Instruction_LDn_nn) { registers16Bit = Registers16Bit.DE, });
            AddInstruction(0x21, new Instruction("LD HL, nn", Instruction_LDn_nn) { registers16Bit = Registers16Bit.HL, });
            AddInstruction(0x31, new Instruction("LD SP, nn", Instruction_LDn_nn) { registers16Bit = Registers16Bit.SP, });

            // XOR n
            AddInstruction(0xAF, new Instruction("XOR A", Instruction_XOR_n) { registers8bit = Registers8Bit.A, });
            AddInstruction(0xA8, new Instruction("XOR B", Instruction_XOR_n) { registers8bit = Registers8Bit.B, });
            AddInstruction(0xA9, new Instruction("XOR C", Instruction_XOR_n) { registers8bit = Registers8Bit.C, });
            AddInstruction(0xAA, new Instruction("XOR D", Instruction_XOR_n) { registers8bit = Registers8Bit.D, });
            AddInstruction(0xAB, new Instruction("XOR E", Instruction_XOR_n) { registers8bit = Registers8Bit.E, });
            AddInstruction(0xAC, new Instruction("XOR H", Instruction_XOR_n) { registers8bit = Registers8Bit.H, });
            AddInstruction(0xAD, new Instruction("XOR L", Instruction_XOR_n) { registers8bit = Registers8Bit.L, });
            AddInstruction(0xAE, new Instruction("XOR (HL)", Instruction_XOR_n) { registers16Bit = Registers16Bit.HL, });
            AddInstruction(0xEE, new Instruction("XOR *", Instruction_XOR_n));
        }

        private void AddInstruction(int location, Instruction instruction)
        {
            _instructions[location] = instruction;
        }

        private int Instruction_LDD_HL_A(Instruction instruction)
        {
            int position = LoadRegister(Registers16Bit.HL);
            _mmu.WriteByte(LoadRegister(Registers8Bit.A), position);
            SetRegister(Registers16Bit.HL, position - 1);
            return 2;
        }

        private int Instruction_LDn_nn(Instruction instruction)
        {
            SetRegister(Registers16Bit.SP, ReadByte() | (ReadByte() << 8));
            return 3;
        }

        private int Instruction_XOR_n(Instruction instruction)
        {
            SetFlag(Flags.C | Flags.H | Flags.N, false);
            int cycles = 2;

            int result = LoadRegister(Registers8Bit.A);
            if (instruction.registers16Bit == Registers16Bit.HL) result ^= _mmu.ReadByte(LoadRegister(Registers16Bit.HL));
            else if (instruction.registers8bit == Registers8Bit.None) result ^= ReadByte();
            else
            {
                cycles = 1;
                result ^= LoadRegister(instruction.registers8bit);
            }

            SetFlag(Flags.Z, result == 0);

            return cycles;
        }
    }
}
