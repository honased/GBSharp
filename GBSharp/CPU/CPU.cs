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

        public void ProcessInstructions()
        {
            byte opcode = _mmu.ReadByte(PC++);
            Instruction instruction = _instructions[opcode];
            string instructionName = (instruction == null) ? "Unknown" : instruction.Name;
            Console.WriteLine("[" + PC + "] {0:X}:" + instructionName, PC-1);
        }
    }
}
