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
        short PC;

        public CPU(MMU mmu)
        {
            _mmu = mmu;
            mmu.SetCPU(this);
            PC = 0;

            RegisterInstructions();
            InitializeRegisters();
        }

        private int ReadByte()
        {
            return _mmu.ReadByte(PC++);
        }

        private int ReadWord()
        {
            int word = _mmu.ReadWord(PC);
            PC += 2;
            return word;
        }

        public void ProcessInstructions()
        {
            int pc = PC;
            Instruction instruction = GetNextInstruction();
            string instructionName = (instruction == null) ? "Unknown" : instruction.Name;
            instruction.Execute();
            //Console.WriteLine("[{0:X}] 0x{1:X}: " + instructionName, PC - 1, instruction.Opcode);
            //Console.WriteLine();
        }
    }
}
