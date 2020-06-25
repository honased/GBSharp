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
            SetRegister(Registers16Bit.PC, 0x100);

            int missingCount = 0;
            for(int i = 0; i < _instructions.Length; i++)
            {
                if(_instructions[i] == null)
                {
                    missingCount++;
                    Console.WriteLine("Missing instruction 0x{0:X}", i);
                }
            }
            Console.WriteLine("Total implemented: " + (_instructions.Length - missingCount) + ".\nTotal missing: " + missingCount);
            Console.ReadKey();
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
            //Console.WriteLine("[{0:X}] 0x{1:X}: " + instruction.Name, pc, instruction.Opcode);
            int result = instruction.Execute();
            if(_mmu.ReadWord(LoadRegister(Registers16Bit.SP)) == 0xFF)
            {
                Console.WriteLine("HELLO WORLD");
            }
            return result;
            //Console.WriteLine("HL VAL:" + LoadRegister(Registers16Bit.HL));
            //Console.ReadKey();
            //Console.WriteLine();
        }
    }
}
