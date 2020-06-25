using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

            AddInstruction(0x17, new Instruction("RLA", Instruction_RLA));

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

            AddInstruction(0xBF, new Instruction("CP A", Instruction_CP) { registers8bit = Registers8Bit.A });
            AddInstruction(0xB8, new Instruction("CP B", Instruction_CP) { registers8bit = Registers8Bit.B });
            AddInstruction(0xB9, new Instruction("CP C", Instruction_CP) { registers8bit = Registers8Bit.C });
            AddInstruction(0xBA, new Instruction("CP D", Instruction_CP) { registers8bit = Registers8Bit.D });
            AddInstruction(0xBB, new Instruction("CP E", Instruction_CP) { registers8bit = Registers8Bit.E });
            AddInstruction(0xBC, new Instruction("CP H", Instruction_CP) { registers8bit = Registers8Bit.H });
            AddInstruction(0xBD, new Instruction("CP L", Instruction_CP) { registers8bit = Registers8Bit.L });
            AddInstruction(0xBE, new Instruction("CP (HL)", Instruction_CP) { registers16bit = Registers16Bit.HL });
            AddInstruction(0xFE, new Instruction("CP #", Instruction_CP));

            AddInstruction(0x97, new Instruction("SUB A", Instruction_SUB) { registers8bit = Registers8Bit.A, shouldFlagBeSet = false });
            AddInstruction(0x90, new Instruction("SUB B", Instruction_SUB) { registers8bit = Registers8Bit.B, shouldFlagBeSet = false });
            AddInstruction(0x91, new Instruction("SUB C", Instruction_SUB) { registers8bit = Registers8Bit.C, shouldFlagBeSet = false });
            AddInstruction(0x92, new Instruction("SUB D", Instruction_SUB) { registers8bit = Registers8Bit.D, shouldFlagBeSet = false });
            AddInstruction(0x93, new Instruction("SUB E", Instruction_SUB) { registers8bit = Registers8Bit.E, shouldFlagBeSet = false });
            AddInstruction(0x94, new Instruction("SUB H", Instruction_SUB) { registers8bit = Registers8Bit.H, shouldFlagBeSet = false });
            AddInstruction(0x95, new Instruction("SUB L", Instruction_SUB) { registers8bit = Registers8Bit.L, shouldFlagBeSet = false });
            AddInstruction(0x96, new Instruction("SUB (HL)", Instruction_SUB) { registers16bit = Registers16Bit.HL, shouldFlagBeSet = false });
            AddInstruction(0xD6, new Instruction("SUB #", Instruction_SUB) { shouldFlagBeSet = false });

            AddInstruction(0x9F, new Instruction("SBC A", Instruction_SUB) { registers8bit = Registers8Bit.A, shouldFlagBeSet = true });
            AddInstruction(0x98, new Instruction("SBC B", Instruction_SUB) { registers8bit = Registers8Bit.B, shouldFlagBeSet = true });
            AddInstruction(0x99, new Instruction("SBC C", Instruction_SUB) { registers8bit = Registers8Bit.C, shouldFlagBeSet = true });
            AddInstruction(0x9A, new Instruction("SBC D", Instruction_SUB) { registers8bit = Registers8Bit.D, shouldFlagBeSet = true });
            AddInstruction(0x9B, new Instruction("SBC E", Instruction_SUB) { registers8bit = Registers8Bit.E, shouldFlagBeSet = true });
            AddInstruction(0x9C, new Instruction("SBC H", Instruction_SUB) { registers8bit = Registers8Bit.H, shouldFlagBeSet = true });
            AddInstruction(0x9D, new Instruction("SBC L", Instruction_SUB) { registers8bit = Registers8Bit.L, shouldFlagBeSet = true });
            AddInstruction(0x9E, new Instruction("SBC (HL)", Instruction_SUB) { registers16bit = Registers16Bit.HL, shouldFlagBeSet = true });

            AddInstruction(0x18, new Instruction("JR n", Instruction_JR));
            AddInstruction(0x20, new Instruction("JR NZ,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = false });
            AddInstruction(0x28, new Instruction("JR Z,n", Instruction_JR) { flag = Flags.Z, shouldFlagBeSet = true });
            AddInstruction(0x30, new Instruction("JR NC,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = false });
            AddInstruction(0x38, new Instruction("JR C,n", Instruction_JR) { flag = Flags.C, shouldFlagBeSet = true });

            AddInstruction(0xF5, new Instruction("PUSH AF", Instruction_Push) { registers16bit = Registers16Bit.AF });
            AddInstruction(0xC5, new Instruction("PUSH BC", Instruction_Push) { registers16bit = Registers16Bit.BC });
            AddInstruction(0xD5, new Instruction("PUSH DE", Instruction_Push) { registers16bit = Registers16Bit.DE });
            AddInstruction(0xE5, new Instruction("PUSH HL", Instruction_Push) { registers16bit = Registers16Bit.HL });

            AddInstruction(0xF1, new Instruction("POP AF", Instruction_Pop) { registers16bit = Registers16Bit.AF });
            AddInstruction(0xC1, new Instruction("POP BC", Instruction_Pop) { registers16bit = Registers16Bit.BC });
            AddInstruction(0xD1, new Instruction("POP DE", Instruction_Pop) { registers16bit = Registers16Bit.DE });
            AddInstruction(0xE1, new Instruction("POP HL", Instruction_Pop) { registers16bit = Registers16Bit.HL });

            AddInstruction(0xC9, new Instruction("RET", Instruction_RET));

            AddInstruction(0xC0, new Instruction("RET NZ", Instruction_RET) { flag = Flags.Z, shouldFlagBeSet = false });
            AddInstruction(0xC8, new Instruction("RET Z", Instruction_RET) { flag = Flags.Z, shouldFlagBeSet = true });
            AddInstruction(0xD0, new Instruction("RET NC", Instruction_RET) { flag = Flags.C, shouldFlagBeSet = false });
            AddInstruction(0xD8, new Instruction("RET C", Instruction_RET) { flag = Flags.C, shouldFlagBeSet = true });

            AddInstruction(0xCD, new Instruction("CALL nn", Instruction_Call));
            AddInstruction(0xC4, new Instruction("CALL NZ,nn", Instruction_Call) { flag = Flags.Z, shouldFlagBeSet = false });
            AddInstruction(0xCC, new Instruction("CALL Z,nn", Instruction_Call) { flag = Flags.Z, shouldFlagBeSet = true });
            AddInstruction(0xD4, new Instruction("CALL NC,nn", Instruction_Call) { flag = Flags.C, shouldFlagBeSet = false });
            AddInstruction(0xDC, new Instruction("CALL C,nn", Instruction_Call) { flag = Flags.C, shouldFlagBeSet = true });

            AddInstruction(0x32, new Instruction("LD (HL-),A", Instruction_LDD_HL_A) { index = -1 });
            AddInstruction(0x22, new Instruction("LD (HL+),A", Instruction_LDD_HL_A) { index = 1 });

            AddInstruction(0x3A, new Instruction("LD A,(HL-)", Instruction_LDD_A_HL) { index = -1 });
            AddInstruction(0x2A, new Instruction("LD A,(HL+)", Instruction_LDD_A_HL) { index = 1 });

            AddInstruction(0xF2, new Instruction("LD A,(C)", Instruction_LD_A_C) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.C });
            AddInstruction(0xE2, new Instruction("LD (C),A", Instruction_LD_A_C) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.A });

            AddInstruction(0xE0, new Instruction("LDH (n),A", Instruction_LD_A_n) { registers8bit2 = Registers8Bit.A});
            AddInstruction(0xF0, new Instruction("LDH A,(n)", Instruction_LD_A_n) { registers8bit = Registers8Bit.A});

            AddInstruction(0x06, new Instruction("LD B,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.B });
            AddInstruction(0x0E, new Instruction("LD C,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.C });
            AddInstruction(0x16, new Instruction("LD D,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.D });
            AddInstruction(0x1E, new Instruction("LD E,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.E });
            AddInstruction(0x26, new Instruction("LD H,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.H });
            AddInstruction(0x2E, new Instruction("LD L,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.L });
            AddInstruction(0x3E, new Instruction("LD A,n", Instruction_LDnn_n) { registers8bit = Registers8Bit.A });

            AddInstruction(0x7F, new Instruction("LD A,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x78, new Instruction("LD A,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x79, new Instruction("LD A,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x7A, new Instruction("LD A,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x7B, new Instruction("LD A,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x7C, new Instruction("LD A,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x7D, new Instruction("LD A,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.A, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x0A, new Instruction("LD A,(BC)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.A, registers16Bit2 = Registers16Bit.BC });
            AddInstruction(0x1A, new Instruction("LD A,(DE)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.A, registers16Bit2 = Registers16Bit.DE });
            AddInstruction(0x7E, new Instruction("LD A,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.A, registers16Bit2 = Registers16Bit.HL });
            AddInstruction(0xFA, new Instruction("LD A,(nn)", Instruction_LD_r1_nn) { registers8bit = Registers8Bit.A });
            AddInstruction(0x02, new Instruction("LD (BC),A", Instruction_LD_n_A) { registers16bit = Registers16Bit.BC, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x12, new Instruction("LD (DE),A", Instruction_LD_n_A) { registers16bit = Registers16Bit.DE, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x77, new Instruction("LD (HL),A", Instruction_LD_n_A) {registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.A });
            AddInstruction(0xEA, new Instruction("LD (nn),A", Instruction_LD_nn_A));

            AddInstruction(0x47, new Instruction("LD B,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x40, new Instruction("LD B,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x41, new Instruction("LD B,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x42, new Instruction("LD B,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x43, new Instruction("LD B,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x44, new Instruction("LD B,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x45, new Instruction("LD B,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.B, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x46, new Instruction("LD B,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.B, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x4F, new Instruction("LD C,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x48, new Instruction("LD C,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x49, new Instruction("LD C,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x4A, new Instruction("LD C,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x4B, new Instruction("LD C,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x4C, new Instruction("LD C,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x4D, new Instruction("LD C,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.C, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x4E, new Instruction("LD C,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.C, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x57, new Instruction("LD D,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x50, new Instruction("LD D,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x51, new Instruction("LD D,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x52, new Instruction("LD D,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x53, new Instruction("LD D,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x54, new Instruction("LD D,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x55, new Instruction("LD D,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.D, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x56, new Instruction("LD D,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.D, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x5F, new Instruction("LD E,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x58, new Instruction("LD E,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x59, new Instruction("LD E,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x5A, new Instruction("LD E,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x5B, new Instruction("LD E,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x5C, new Instruction("LD E,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x5D, new Instruction("LD E,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.E, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x5E, new Instruction("LD E,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.E, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x67, new Instruction("LD H,A", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x60, new Instruction("LD H,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x61, new Instruction("LD H,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x62, new Instruction("LD H,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x63, new Instruction("LD H,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x64, new Instruction("LD H,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x65, new Instruction("LD H,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.H, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x66, new Instruction("LD H,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.H, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x6F, new Instruction("LD L,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.A });
            AddInstruction(0x68, new Instruction("LD L,B", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x69, new Instruction("LD L,C", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x6A, new Instruction("LD L,D", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x6B, new Instruction("LD L,E", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x6C, new Instruction("LD L,H", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x6D, new Instruction("LD L,L", Instruction_LD_r1_r2) { registers8bit = Registers8Bit.L, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x6E, new Instruction("LD L,(HL)", Instruction_LD_r1_HL) { registers8bit = Registers8Bit.L, registers16Bit2 = Registers16Bit.HL });

            AddInstruction(0x70, new Instruction("LD (HL),B", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.B });
            AddInstruction(0x71, new Instruction("LD (HL),C", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.C });
            AddInstruction(0x72, new Instruction("LD (HL),D", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.D });
            AddInstruction(0x73, new Instruction("LD (HL),E", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.E });
            AddInstruction(0x74, new Instruction("LD (HL),H", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.H });
            AddInstruction(0x75, new Instruction("LD (HL),L", Instruction_LD_n_A) { registers16bit = Registers16Bit.HL, registers8bit2 = Registers8Bit.L });
            AddInstruction(0x36, new Instruction("LD (HL),n", Instruction_LD_HL_N));

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

            AddCBInstruction(0x17, new Instruction("RL A", Instruction_RL) { registers8bit = Registers8Bit.A });
            AddCBInstruction(0x10, new Instruction("RL B", Instruction_RL) { registers8bit = Registers8Bit.B });
            AddCBInstruction(0x11, new Instruction("RL C", Instruction_RL) { registers8bit = Registers8Bit.C });
            AddCBInstruction(0x12, new Instruction("RL D", Instruction_RL) { registers8bit = Registers8Bit.D });
            AddCBInstruction(0x13, new Instruction("RL E", Instruction_RL) { registers8bit = Registers8Bit.E });
            AddCBInstruction(0x14, new Instruction("RL H", Instruction_RL) { registers8bit = Registers8Bit.H });
            AddCBInstruction(0x15, new Instruction("RL L", Instruction_RL) { registers8bit = Registers8Bit.L });
            AddCBInstruction(0x16, new Instruction("RL (HL)", Instruction_RL) { registers16bit = Registers16Bit.HL });
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
                int pc = LoadRegister(Registers16Bit.PC);
                SetRegister(Registers16Bit.PC, pc + n);
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
            SetFlag(Flags.H, (add1 & 0x0F) + 1 > 0x0F);

            if (instruction.registers16bit == Registers16Bit.HL) _mmu.WriteByte(result, LoadRegister(Registers16Bit.HL));
            else SetRegister(instruction.registers8bit, result);

            return (instruction.registers16bit == Registers16Bit.HL) ? 3 : 1;
        }

        private int Instruction_INC16(Instruction instruction)
        {
            int result = LoadRegister(instruction.registers16bit) + 1;
            if (result > 65535) result = 0;
            SetRegister(instruction.registers16bit, result);
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

            if (instruction.registers16bit == Registers16Bit.HL) _mmu.WriteByte(result, LoadRegister(Registers16Bit.HL));
            else SetRegister(instruction.registers8bit, result);

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, (dec1 & 0x0F) == 0);

            return (instruction.registers16bit == Registers16Bit.HL) ? 3 : 1;
        }

        private int Instruction_DEC16(Instruction instruction)
        {
            int result = LoadRegister(instruction.registers16bit) - 1;
            if (result < 0) result = 65535;
            SetRegister(instruction.registers16bit, result);
            return 2;
        }

        private int Instruction_LDD_HL_A(Instruction instruction)
        {
            int position = LoadRegister(Registers16Bit.HL);
            _mmu.WriteByte(LoadRegister(Registers8Bit.A), position);
            SetRegister(Registers16Bit.HL, position + instruction.index);
            return 2;
        }

        private int Instruction_LDD_A_HL(Instruction instruction)
        {
            int position = LoadRegister(Registers16Bit.HL);
            SetRegister(Registers8Bit.A, _mmu.ReadByte(position));
            SetRegister(Registers16Bit.HL, position + instruction.index);
            return 2;
        }

        private int Instruction_LDn_nn(Instruction instruction)
        {
            SetRegister(instruction.registers16bit, ReadWord());
            return 3;
        }

        private int Instruction_LDnn_n(Instruction instruction)
        {
            SetRegister(instruction.registers8bit, ReadByte());
            return 2;
        }

        private int Instruction_LD_r1_r2(Instruction instruction)
        {
            SetRegister(instruction.registers8bit, LoadRegister(instruction.registers8bit2));
            return 1;
        }

        private int Instruction_LD_r1_HL(Instruction instruction)
        {
            SetRegister(instruction.registers8bit, _mmu.ReadByte(LoadRegister(instruction.registers16Bit2)));
            return 2;
        }

        private int Instruction_LD_r1_nn(Instruction instruction)
        {
            SetRegister(instruction.registers8bit, _mmu.ReadByte(ReadWord()));
            return 4;
        }

        private int Instruction_LD_n_A(Instruction instruction)
        {
            _mmu.WriteByte(LoadRegister(instruction.registers8bit2), LoadRegister(instruction.registers16bit));
            return 2;
        }

        private int Instruction_LD_nn_A(Instruction instruction)
        {
            _mmu.WriteByte(LoadRegister(Registers8Bit.A), ReadWord());
            return 2;
        }

        private int Instruction_LD_HL_r2(Instruction instruction)
        {
            _mmu.WriteByte(LoadRegister(instruction.registers8bit2), LoadRegister(Registers16Bit.HL));
            return 2;
        }

        private int Instruction_LD_HL_n(Instruction instruction)
        {
            _mmu.WriteByte(ReadByte(), LoadRegister(Registers16Bit.HL));
            return 3;
        }

        private int Instruction_LD_A_C(Instruction instruction)
        {
            if (instruction.registers8bit != Registers8Bit.C)
            {
                SetRegister(instruction.registers8bit, _mmu.ReadByte(0xFF00 + LoadRegister(instruction.registers8bit2)));
            }
            else
            {
                _mmu.WriteByte(LoadRegister(instruction.registers8bit2), 0xFF00 + LoadRegister(instruction.registers8bit));
            }
            return 2;
        }

        private int Instruction_LD_A_n(Instruction instruction)
        {
            if (instruction.registers8bit == Registers8Bit.A)
            {
                SetRegister(Registers8Bit.A, _mmu.ReadByte(0xFF00 + ReadByte()));
            }
            else
            {
                _mmu.WriteByte(LoadRegister(Registers8Bit.A), 0xFF00 + ReadByte());
            }
            return 3;
        }

        private int Instruction_LD_HL_N(Instruction instruction)
        {
            _mmu.WriteByte(ReadByte(), LoadRegister(Registers16Bit.HL));
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

        private int Instruction_Push(Instruction instruction)
        {
            Push(LoadRegister(instruction.registers16bit));
            return 4;
        }

        private int Instruction_Call(Instruction instruction)
        {
            if(instruction.flag == Flags.None) return Call(true);
            else
            {
                return Call(IsFlagOn(instruction.flag) == instruction.shouldFlagBeSet);
            }
        }

        private int Instruction_RET(Instruction instruction)
        {
            SetRegister(Registers16Bit.PC, Pop());

            return 2;
        }

        private int Instruction_RET_CC(Instruction instruction)
        {
            if(IsFlagOn(instruction.flag) == instruction.shouldFlagBeSet)
            {
                SetRegister(Registers16Bit.PC, Pop());
            }    
            return 2;
        }

        private void Push(int value)
        {
            int sp = LoadRegister(Registers16Bit.SP) - 2;
            SetRegister(Registers16Bit.SP, sp);
            _mmu.WriteWord(value, sp);
        }

        private int Call(bool doIt)
        {
            if(doIt)
            {
                Push(LoadRegister(Registers16Bit.PC) + 2);
                int value = ReadWord();
                SetRegister(Registers16Bit.PC, value);
                //return 6;
            }
            else
            {
                ReadWord();
                //return 3;
            }
            return 6;
        }

        private int Instruction_RL(Instruction instruction)
        {
            SetFlag(Flags.N | Flags.H, false);

            byte initialValue;

            if (instruction.registers16bit == Registers16Bit.HL) initialValue = (byte)_mmu.ReadByte(LoadRegister(Registers16Bit.HL));
            else initialValue = (byte)LoadRegister(instruction.registers8bit);

            byte result = (byte)((initialValue << 1) | (IsFlagOn(Flags.C) ? 1 : 0));

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.C, (initialValue & 0x80) != 0);

            if (instruction.registers16bit == Registers16Bit.HL)
            {
                _mmu.WriteByte(result, LoadRegister(Registers16Bit.HL));
                return 4;
            }
            else
            {
                SetRegister(instruction.registers8bit, result);
                return 2;
            }
        }

        private int Instruction_RLA(Instruction instruction)
        {
            bool previousCFlag = IsFlagOn(Flags.C);
            SetFlag(Flags.N | Flags.H, false);

            byte result = (byte)LoadRegister(Registers8Bit.A);

            SetFlag(Flags.C, (result & 0x80) != 0);
            SetRegister(Registers8Bit.A, (byte)((result << 1) | (previousCFlag ? 1 : 0)));
            return 1;
        }

        private int Pop()
        {
            int sp = LoadRegister(Registers16Bit.SP);
            int val = _mmu.ReadWord(sp);
            SetRegister(Registers16Bit.SP, sp + 2);

            return val;
        }

        private int Instruction_Pop(Instruction instruction)
        {
            SetRegister(instruction.registers16bit, Pop());
            return 3;
        }

        private int Sub(int val, bool includeCarry)
        {
            int registerA = LoadRegister(Registers8Bit.A);
            int cVal = (includeCarry && IsFlagOn(Flags.C)) ? 1 : 0;
            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, registerA == val + cVal);
            SetFlag(Flags.H, (registerA & 0x0F) < (val & 0x0F) + cVal);
            SetFlag(Flags.C, registerA < val + cVal);

            return ((registerA - (val + cVal)) + 255) % 255;
        }

        private int Instruction_CP(Instruction instruction)
        {
            if (instruction.registers8bit != Registers8Bit.None)
            {
                Sub(LoadRegister(instruction.registers8bit), false);
                return 1;
            }
            else if(instruction.registers16bit != Registers16Bit.None)
            {
                Sub(_mmu.ReadByte(LoadRegister(instruction.registers16bit)), false);
                return 2;
            }
            else
            {
                Sub(ReadByte(), false);
                return 2;
            }
        }

        private int Instruction_SUB(Instruction instruction)
        {
            if (instruction.registers8bit != Registers8Bit.None)
            {
                SetRegister(Registers8Bit.A, Sub(LoadRegister(instruction.registers8bit), instruction.shouldFlagBeSet));
                return 1;
            }
            else if (instruction.registers16bit != Registers16Bit.None)
            {
                SetRegister(Registers8Bit.A, Sub(_mmu.ReadByte(LoadRegister(instruction.registers16bit)), instruction.shouldFlagBeSet));
                return 2;
            }
            else
            {
                SetRegister(Registers8Bit.A, Sub(ReadByte(), instruction.shouldFlagBeSet));
                return 2;
            }
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
