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
        public bool IME;
        private int setIME;
        private int clearIME;
        private bool Halt { get; set; }
        private bool HaltBug { get; set; }
        private Debugger _debugger;

        private Gameboy _gameboy;

        public CPU(Gameboy gameboy)
        {
            _gameboy = gameboy;
            _debugger = new Debugger(gameboy);

            RegisterInstructions();

            SetPalette(new PPU.Color(0, 0, 0), new PPU.Color(96, 96, 96), new PPU.Color(192, 192, 192), new PPU.Color(255, 255, 255));

            Reset(false, null);
        }

        public void Reset(bool inBios = false, Cartridge cart = null)
        {
            InitializeRegisters();

            IME = true;
            setIME = 0;
            clearIME = 0;

            Halt = false;
            HaltBug = false;

            if (cart != null)
            {
                _gameboy.Mmu.LoadCartridge(cart);
            }

            if (inBios) StartInBios();
        }

        public void SetPalette(PPU.Color color0, PPU.Color color1, PPU.Color color2, PPU.Color color3)
        {
            _gameboy.Ppu.Color0 = color0;
            _gameboy.Ppu.Color1 = color1;
            _gameboy.Ppu.Color2 = color2;
            _gameboy.Ppu.Color3 = color3;
            _gameboy.Ppu.UpdateColors();
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

            _gameboy.Mmu.StartInBios();
        }

        private int ReadByte()
        {
            int pc = LoadRegister(Registers16Bit.PC);
            SetRegister(Registers16Bit.PC, pc + 1);
            return _gameboy.Mmu.ReadByte(pc);
        }

        private int ReadWord()
        {
            int pc = LoadRegister(Registers16Bit.PC);
            int word = _gameboy.Mmu.ReadWord(pc);
            SetRegister(Registers16Bit.PC, pc + 2);
            return word;
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

            return cycles;
        }

        private int CheckInterrupts()
        {
            int IE = _gameboy.Mmu.IE;
            int IF = _gameboy.Mmu.IF;

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
                _gameboy.Mmu.IF &= ~(0x1 << interrupt);
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