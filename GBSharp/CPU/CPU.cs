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
        const int CPU_CYCLES = 17556;
        private int currentCycles;
        private bool IME;
        private bool setIME = false;

        public CPU(MMU mmu)
        {
            _mmu = mmu;
            mmu.SetCPU(this);

            currentCycles = 0;

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

        public void ExecuteFrame()
        {
            while (currentCycles < CPU_CYCLES)
            {
                int pc = LoadRegister(Registers16Bit.PC);
                Instruction instruction = GetNextInstruction();
                //Console.WriteLine("[{0:X}] 0x{1:X}: " + instruction.Name, pc, instruction.Opcode);
                int cycles = instruction.Execute();
                currentCycles += cycles;

                CheckInterrupts();
            }
            currentCycles -= CPU_CYCLES;
        }

        private void CheckInterrupts()
        {
            int IE = _mmu.IE;
            int IF = _mmu.IF;

            for(int i = 0; i < 5; i++)
            {
                if((IE & IF) >> i == 1)
                {
                    ExecuteInterrupt(i);
                }
            }

            if(setIME)
            {
                setIME = false;
                IME = true;
            }
        }

        private void ExecuteInterrupt(int interrupt)
        {
            if(IME)
            {
                IME = false;
                Push(LoadRegister(Registers16Bit.PC));
                SetRegister(Registers16Bit.PC, 0x40 + (interrupt * 8));
                _mmu.IF &= ~(0x1 << interrupt);
            }
        }
    }
}
