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
        public string Name { get; set; }

        private InstructionMethod method;

        public CPU.Registers16Bit registers16bit;
        public CPU.Registers8Bit registers8bit;
        public CPU.Registers16Bit registers16Bit2;
        public CPU.Registers8Bit registers8bit2;

        public CPU.Flags flag;
        public CPU.Flags flag2;

        public bool shouldFlagBeSet;
        public bool shouldFlag2BeSet;

        public int index;

        public int Opcode { get; set; }

        public Instruction(string name, InstructionMethod method)
        {
            Name = name;
            this.method = method;

            registers16bit = CPU.Registers16Bit.None;
            registers16Bit2 = CPU.Registers16Bit.None;

            registers8bit = CPU.Registers8Bit.None;
            registers8bit2 = CPU.Registers8Bit.None;
            index = -1;

            flag = CPU.Flags.None;
            flag2 = CPU.Flags.None;
        }

        public int Execute()
        {
            return method.Invoke(this);
        }
    }

    public partial class CPU
    {
        private Instruction[] _instructions;
        private Instruction[] _cbInstructions;

        private Instruction GetNextInstruction()
        {
            int instruction = ReadByte();
            if(instruction != 0xCB)
            {
                if(_instructions[instruction] != null) return _instructions[instruction];
                Instruction unknownInstruction = new Instruction("Unknown?", Instruction_Unknown);
                unknownInstruction.Opcode = instruction;
                return unknownInstruction;
            }
            else
            {
                instruction = ReadByte();
                if (_cbInstructions[instruction] != null) return _cbInstructions[instruction];
                Instruction unknownInstruction = new Instruction("[CB] Unknown?", Instruction_Unknown);
                unknownInstruction.Opcode = instruction;
                return unknownInstruction;
            }
        }

        private void RegisterInstructions()
        {
            _instructions = new Instruction[0xFF];
            _cbInstructions = new Instruction[0xFF];

            AddInstruction(0x00, new Instruction("NOP", Instruction_NOP));

            AddInstruction(0x18, new Instruction("JR n", Instruction_JR));
            AddInstruction(0x20, new Instruction("JR NZ,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = false });
            AddInstruction(0x28, new Instruction("JR Z,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = true });
            AddInstruction(0x30, new Instruction("JR NC,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = false });
            AddInstruction(0x38, new Instruction("JR C,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = true });

            AddInstruction(0x32, new Instruction("LD (HL-),A", Instruction_LDD_HL_A));

            // LD n,nn
            AddInstruction(0x01, new Instruction("LD BC, nn", Instruction_LDn_nn) { registers16bit = Registers16Bit.BC, });
            AddInstruction(0x11, new Instruction("LD DE, nn", Instruction_LDn_nn) { registers16bit = Registers16Bit.DE, });
            AddInstruction(0x21, new Instruction("LD HL, nn", Instruction_LDn_nn) { registers16bit = Registers16Bit.HL, });
            AddInstruction(0x31, new Instruction("LD SP, nn", Instruction_LDn_nn) { registers16bit = Registers16Bit.SP, });

            // XOR n
            AddInstruction(0xAF, new Instruction("XOR A", Instruction_XOR_n) { registers8bit = Registers8Bit.A, });
            AddInstruction(0xA8, new Instruction("XOR B", Instruction_XOR_n) { registers8bit = Registers8Bit.B, });
            AddInstruction(0xA9, new Instruction("XOR C", Instruction_XOR_n) { registers8bit = Registers8Bit.C, });
            AddInstruction(0xAA, new Instruction("XOR D", Instruction_XOR_n) { registers8bit = Registers8Bit.D, });
            AddInstruction(0xAB, new Instruction("XOR E", Instruction_XOR_n) { registers8bit = Registers8Bit.E, });
            AddInstruction(0xAC, new Instruction("XOR H", Instruction_XOR_n) { registers8bit = Registers8Bit.H, });
            AddInstruction(0xAD, new Instruction("XOR L", Instruction_XOR_n) { registers8bit = Registers8Bit.L, });
            AddInstruction(0xAE, new Instruction("XOR (HL)", Instruction_XOR_n) { registers16bit = Registers16Bit.HL, });
            AddInstruction(0xEE, new Instruction("XOR *", Instruction_XOR_n));

            // CB Instructions
            for (int i = 0; i < 8; i++) AddCBInstruction(0x40 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",B", Instruction_Bit) { index = i, registers8bit = Registers8Bit.B });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x41 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",C", Instruction_Bit) { index = i, registers8bit = Registers8Bit.C });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x42 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",D", Instruction_Bit) { index = i, registers8bit = Registers8Bit.D });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x43 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",E", Instruction_Bit) { index = i, registers8bit = Registers8Bit.E });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x44 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",H", Instruction_Bit) { index = i, registers8bit = Registers8Bit.H });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x45 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",L", Instruction_Bit) { index = i, registers8bit = Registers8Bit.L });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x46 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",(HL)", Instruction_Bit) { index = i, registers16bit = Registers16Bit.HL });
            for (int i = 0; i < 8; i++) AddCBInstruction(0x47 + (i / 2) * 16 + (8 * ((i % 2 == 1) ? 1 : 0)), new Instruction("BIT " + i.ToString() + ",A", Instruction_Bit) { index = i, registers8bit = Registers8Bit.A });
        }

        private void AddInstruction(int location, Instruction instruction)
        {
            if (_instructions[location] != null) Console.WriteLine("Overwriting instruction at 0x{0:X}", location);
            _instructions[location] = instruction;
            instruction.Opcode = location;
        }

        private void AddCBInstruction(int location, Instruction instruction)
        {
            if (_cbInstructions[location] != null) Console.WriteLine("Overwriting cb_instruction at 0x{0:X}", location);

            //Console.WriteLine("Adding instruction " + instruction.Name + " at 0x{0:X}", location);

            _cbInstructions[location] = instruction;
            instruction.Name = "[CB] " + instruction.Name;
            instruction.Opcode = location;
        }

        private int Instruction_Unknown(Instruction instruction)
        {
            Console.WriteLine("Warning: Instruction [0x{0:X}] Unknown!", instruction.Opcode);
            return 1;
        }

        private int Instruction_NOP(Instruction instruction)
        {
            return 1;
        }

        private int Instruction_JR(Instruction instruction)
        {
            sbyte n = (sbyte)ReadByte();
            if (instruction.flag == Flags.None || IsFlagOn(instruction.flag) == instruction.shouldFlagBeSet)
            {
                PC += n;
            }
            return 2;
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
            SetRegister(instruction.registers16bit, ReadByte() | (ReadByte() << 8));
            return 3;
        }

        private int Instruction_XOR_n(Instruction instruction)
        {
            SetFlag(Flags.C | Flags.H | Flags.N, false);
            int cycles = 2;

            int result = LoadRegister(Registers8Bit.A);
            if (instruction.registers16bit == Registers16Bit.HL) result ^= _mmu.ReadByte(LoadRegister(Registers16Bit.HL));
            else if (instruction.registers8bit == Registers8Bit.None) result ^= ReadByte();
            else
            {
                cycles = 1;
                result ^= LoadRegister(instruction.registers8bit);
            }

            SetFlag(Flags.Z, result == 0);

            return cycles;
        }

        public int Instruction_Bit(Instruction instruction)
        {
            SetFlag(Flags.N, false);
            SetFlag(Flags.H, true);

            int data = -1;
            if (instruction.registers16bit == Registers16Bit.HL) data = LoadRegister(Registers16Bit.HL);
            else data = LoadRegister(instruction.registers8bit);

            SetFlag(Flags.Z, (data & (1 << instruction.index)) == 0);

            return (instruction.registers16bit == Registers16Bit.HL) ? 2 : 1;
        }
    }
}
