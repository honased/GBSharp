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

            AddInstruction(0x03, new Instruction("INC BC", Instruction_INC16) { registers16bit = Registers16Bit.BC });
            AddInstruction(0x13, new Instruction("INC DE", Instruction_INC16) { registers16bit = Registers16Bit.DE });
            AddInstruction(0x23, new Instruction("INC HL", Instruction_INC16) { registers16bit = Registers16Bit.HL });
            AddInstruction(0x33, new Instruction("INC SP", Instruction_INC16) { registers16bit = Registers16Bit.SP });

            AddInstruction(0x3C, new Instruction("INC A", Instruction_INC8) { registers8bit = Registers8Bit.A });
            AddInstruction(0x04, new Instruction("INC B", Instruction_INC8) { registers8bit = Registers8Bit.B });
            AddInstruction(0x0C, new Instruction("INC C", Instruction_INC8) { registers8bit = Registers8Bit.C });
            AddInstruction(0x14, new Instruction("INC D", Instruction_INC8) { registers8bit = Registers8Bit.D });
            AddInstruction(0x1C, new Instruction("INC E", Instruction_INC8) { registers8bit = Registers8Bit.E });
            AddInstruction(0x24, new Instruction("INC H", Instruction_INC8) { registers8bit = Registers8Bit.H });
            AddInstruction(0x2C, new Instruction("INC L", Instruction_INC8) { registers8bit = Registers8Bit.L });
            AddInstruction(0x34, new Instruction("INC (HL)", Instruction_INC8) { registers16bit = Registers16Bit.HL });

            AddInstruction(0x0B, new Instruction("DEC BC", Instruction_DEC16) { registers16bit = Registers16Bit.BC });
            AddInstruction(0x1B, new Instruction("DEC DE", Instruction_DEC16) { registers16bit = Registers16Bit.DE });
            AddInstruction(0x2B, new Instruction("DEC HL", Instruction_DEC16) { registers16bit = Registers16Bit.HL });
            AddInstruction(0x3B, new Instruction("DEC SP", Instruction_DEC16) { registers16bit = Registers16Bit.SP });

            AddInstruction(0x3D, new Instruction("DEC A", Instruction_DEC8) { registers8bit = Registers8Bit.A });
            AddInstruction(0x05, new Instruction("DEC B", Instruction_DEC8) { registers8bit = Registers8Bit.B });
            AddInstruction(0x0D, new Instruction("DEC C", Instruction_DEC8) { registers8bit = Registers8Bit.C });
            AddInstruction(0x15, new Instruction("DEC D", Instruction_DEC8) { registers8bit = Registers8Bit.D });
            AddInstruction(0x1D, new Instruction("DEC E", Instruction_DEC8) { registers8bit = Registers8Bit.E });
            AddInstruction(0x25, new Instruction("DEC H", Instruction_DEC8) { registers8bit = Registers8Bit.H });
            AddInstruction(0x2D, new Instruction("DEC L", Instruction_DEC8) { registers8bit = Registers8Bit.L });
            AddInstruction(0x35, new Instruction("DEC (HL)", Instruction_DEC8) { registers16bit = Registers16Bit.HL });

            AddInstruction(0x18, new Instruction("JR n", Instruction_JR));
            AddInstruction(0x20, new Instruction("JR NZ,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = false });
            AddInstruction(0x28, new Instruction("JR Z,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = true });
            AddInstruction(0x30, new Instruction("JR NC,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = false });
            AddInstruction(0x38, new Instruction("JR C,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = true });

            AddInstruction(0x32, new Instruction("LD (HL-),A", Instruction_LDD_HL_A));

            AddInstruction(0x06, new Instruction("LD B,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.B });
            AddInstruction(0x0E, new Instruction("LD C,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.C });
            AddInstruction(0x16, new Instruction("LD D,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.D });
            AddInstruction(0x1E, new Instruction("LD E,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.E });
            AddInstruction(0x26, new Instruction("LD H,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.H });
            AddInstruction(0x2E, new Instruction("LD L,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.L });
            AddInstruction(0x3E, new Instruction("LD A,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.A });

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

        private int Instruction_INC8(Instruction instruction)
        {
            SetFlag(Flags.N, false);

            int add1;

            if (instruction.registers16bit == Registers16Bit.HL) add1 = _mmu.ReadByte(LoadRegister(Registers16Bit.HL));
            else add1 = LoadRegister(instruction.registers8bit);

            int result = add1 + 1;
            if (result > 255) result = 0;

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, (((add1 & 0xf) + (1 & 0xf)) & 0x10) == 0x10);

            return (instruction.registers16bit == Registers16Bit.HL) ? 3 : 1;
        }

        private int Instruction_INC16(Instruction instruction)
        {
            SetRegister(instruction.registers16bit, LoadRegister(instruction.registers16bit) + 1);
            return 2;
        }

        private int Instruction_DEC8(Instruction instruction)
        {
            SetFlag(Flags.N, true);

            int dec1;

            if (instruction.registers16bit == Registers16Bit.HL) dec1 = _mmu.ReadByte(LoadRegister(Registers16Bit.HL));
            else dec1 = LoadRegister(instruction.registers8bit);

            int result = dec1 - 1;
            if (result < 0) result = 255;

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, (dec1 & 0x0F) == 0);

            return (instruction.registers16bit == Registers16Bit.HL) ? 3 : 1;
        }

        private int Instruction_DEC16(Instruction instruction)
        {
            SetRegister(instruction.registers16bit, LoadRegister(instruction.registers16bit) - 1);
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

        private int Instruction_LDnn_n(Instruction instruction)
        {
            SetRegister(instruction.registers8bit, ReadByte());
            return 2;
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
