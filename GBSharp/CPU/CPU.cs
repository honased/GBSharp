using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public partial class CPU
    {
        private MMU _mmu;

        public CPU(MMU mmu)
        {
            _mmu = mmu;
            mmu.SetCPU(this);

            RegisterInstructions();
            InitializeRegisters();
            //SetRegister(Registers16Bit.PC, 0x100);
        }

        private int ReadByte()
        {
            int pc = LoadRegister(Registers16Bit.PC);
            SetRegister(Registers16Bit.PC, pc + 1);
            return _mmu.ReadByte(pc);
        }

        private int ReadWord()
        {
            int pc = LoadRegister(Registers16Bit.PC);
            int word = _mmu.ReadWord(pc);
            SetRegister(Registers16Bit.PC, pc + 2);
            return word;
        }

        public int ProcessInstructions()
        {
            int pc = LoadRegister(Registers16Bit.PC);
            Instruction instruction = GetNextInstruction();
            int clock = instruction.Execute();

            SetRegister(Registers16Bit.PC, 0);

            return clock;
            //Console.WriteLine("[{0:X}] 0x{1:X}: " + instruction.Name, pc - 1, instruction.Opcode);
            //Console.ReadKey();
            //Console.WriteLine();
        }
    }
}
