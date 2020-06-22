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
            PC = 0;

            RegisterInstructions();
            InitializeRegisters();
        }

        private int ReadByte()
        {
            return _mmu.ReadByte(PC++);
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
