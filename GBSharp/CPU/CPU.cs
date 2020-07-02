using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Timer _timer;
        const int CPU_CYCLES = 17556;
        private int currentCycles;
        internal bool IME;
        private int setIME = 0;
        private int clearIME = 0;
        private bool Halt { get; set; }
        private bool HaltBug { get; set; }
        private Debugger _debugger;

        public CPU(MMU mmu, PPU ppu, Input input)
        {
            _mmu = mmu;
            _ppu = ppu;
            _input = input;
            _timer = new Timer(_mmu);
            _debugger = new Debugger(this, mmu, ppu, _timer);
            mmu.SetCPU(this);
            mmu.SetPPU(_ppu);
            mmu.SetTimer(_timer);
            Halt = false;
            HaltBug = false;

            currentCycles = 0;

            RegisterInstructions();
            InitializeRegisters();
        }

        public void Debug()
        {
            _debugger.DebugMode = true;
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
            setIME = 0;
            clearIME = 0;

            _mmu.Reset();
            _ppu.Reset();
            _timer.Reset();
            
            Halt = false;
            HaltBug = false;

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
            // 1FFE - Tetris
            while (currentCycles < CPU_CYCLES)
            {
                int cycles = CheckInterrupts();

                if(cycles == 0)
                {
                    int pc = LoadRegister(Registers16Bit.PC);

                    Instruction instruction = GetNextInstruction();

                    _debugger.Debug(instruction, pc);

                    cycles = instruction.Execute();
                }
                
                currentCycles += cycles;
                _timer.Tick(cycles);
                _ppu.Tick(cycles);
                _input.Tick();
            }
            currentCycles -= CPU_CYCLES;
        }

        private int CheckInterrupts()
        {
            int IE = _mmu.IE;
            int IF = _mmu.IF;

            switch(setIME)
            {
                case 2: setIME = 1; break;
                case 1: setIME = 0;  IME = true; break;
                default: setIME = 0; break;
            }

            switch (clearIME)
            {
                case 2: clearIME = 1; break;
                case 1: clearIME = 0; IME = false; break;
                default: clearIME = 0; break;
            }

            for (int i = 0; i < 5; i++)
            {
                if ((IE & IF) >> i == 1)
                {
                    return ExecuteInterrupt(i);
                }
            }

            return 0;
        }

        private int ExecuteInterrupt(int interrupt)
        {
            if (Halt) Halt = false;

            if (IME)
            {
                IME = false;
                Push(LoadRegister(Registers16Bit.PC));
                SetRegister(Registers16Bit.PC, 0x40 + (interrupt * 8));
                _mmu.IF &= ~(0x1 << interrupt);
                return 1;
            }

            return 0;
        }

        public int TestInstruction(int opcode, bool cb=false)
        {
            Instruction instruction = cb ? _cbInstructions[opcode] : _instructions[opcode];
            if (instruction == null) return -1;
            int cycles = instruction.Execute();
            Reset();
            return cycles;
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