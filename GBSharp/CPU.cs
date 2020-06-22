using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class CPU
    {
        private MMU _mmu;
        short PC;

        public CPU(MMU mmu)
        {
            _mmu = mmu;
            PC = 0;
        }

        public void ProcessOpcodes()
        {
            byte opcode = _mmu.ReadByte(PC++);
            Console.WriteLine("[" + PC + "]:" + opcode);
        }
    }
}
