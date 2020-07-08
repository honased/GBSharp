using GBSharp.Audio;
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
        private APU _apu;
        const int CPU_CYCLES = 17556;
        private int currentCycles;
        public bool IME;
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
            _apu = new APU();
            mmu._apu = _apu;
            mmu.SetCPU(this);
            mmu.SetPPU(_ppu);
            mmu.SetTimer(_timer);
            Halt = false;
            HaltBug = false;
            IME = true;

            currentCycles = 0;

            RegisterInstructions();
            InitializeRegisters();

            SetPalette(new PPU.Color(0, 0, 0), new PPU.Color(96, 96, 96), new PPU.Color(192, 192, 192), new PPU.Color(255, 255, 255));
        }

        public void SetPalette(PPU.Color color0, PPU.Color color1, PPU.Color color2, PPU.Color color3)
        {
            _ppu.Color0 = color0;
            _ppu.Color1 = color1;
            _ppu.Color2 = color2;
            _ppu.Color3 = color3;
            _ppu.UpdateColors();
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

        public void Reset(bool inBios=false, Cartridge cart = null)
        {
            InitializeRegisters();

            IME = true;
            setIME = 0;
            clearIME = 0;

            _mmu.Reset();
            _ppu.Reset();
            _timer.Reset();
            
            Halt = false;
            HaltBug = false;

            if(cart != null)
            {
                _mmu.LoadCartridge(cart);
            }

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
                currentCycles += ExecuteCycle();
            }
            currentCycles -= CPU_CYCLES;
        }

        public int ExecuteCycle()
        {
            int cycles = CheckInterrupts();

            if (cycles == 0)
            {

                int pc = LoadRegister(Registers16Bit.PC);

                Instruction instruction = GetNextInstruction();

                _debugger.Debug(instruction, pc);

                cycles = instruction.Execute();
            }

            _timer.Tick(cycles);
            _input.Tick();
            _ppu.Tick(cycles);
            _apu.Tick(cycles);

            return cycles;
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