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
        private PPU _ppu;
        private Input _input;
        const int CPU_CYCLES = 17556;
        private int currentCycles;
        private bool IME;
        private bool setIME = false;

        public CPU(MMU mmu, PPU ppu, Input input)
        {
            _mmu = mmu;
            _ppu = ppu;
            _input = input;
            mmu.SetCPU(this);
            mmu.SetPPU(_ppu);

            currentCycles = 0;

            RegisterInstructions();
            InitializeRegisters();

            int missingCount = 0;
            Console.WriteLine("Regular Instructions\n--------------------");
            for(int i = 0; i < _instructions.Length; i++)
            {
                if(_instructions[i] == null)
                {
                    missingCount++;
                    Console.WriteLine("Missing instruction 0x{0:X}", i);
                }
            }
            Console.WriteLine("Total implemented: " + (_instructions.Length - missingCount) + ".\nTotal missing: " + missingCount);

            missingCount = 0;
            Console.WriteLine("CB Instructions\n---------------");
            for (int i = 0; i < _cbInstructions.Length; i++)
            {
                if (_cbInstructions[i] == null)
                {
                    missingCount++;
                    Console.WriteLine("Missing CB instruction 0x{0:X}", i);
                }
            }
            Console.WriteLine("Total CB implemented: " + (_cbInstructions.Length - missingCount) + ".\nTotal missing: " + missingCount);
            //Console.ReadKey();
        }

        public void StartInBios()
        {
            SetRegister(Registers16Bit.AF, 0);
            SetRegister(Registers16Bit.BC, 0);
            SetRegister(Registers16Bit.DE, 0);
            SetRegister(Registers16Bit.HL, 0);
            SetRegister(Registers16Bit.PC, 0);
            SetRegister(Registers16Bit.SP, 0);

            _mmu.StartInBios();
        }

        public void Reset(bool inBios=false, int[] cart = null)
        {
            InitializeRegisters();

            IME = false;
            setIME = false;

            _mmu.Reset();
            _ppu.Reset();

            if(cart != null) CartridgeLoader.LoadDataIntoMemory(_mmu, cart, 0);

            if (inBios) StartInBios();
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
                _ppu.Tick(cycles);
                _input.Tick();

                CheckInterrupts();
            }
            currentCycles -= CPU_CYCLES;
            //Console.WriteLine("REnder frame: " + PPU.RenderCount);
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

public enum Interrupts
{
    VBlank,
    LCDStat,
    Timer,
    Serial,
    Joypad
}