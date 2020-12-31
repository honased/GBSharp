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
        private bool setIME;
        private bool Halt { get; set; }
        private bool HaltBug { get; set; }
        private Debugger _debugger;

        private Gameboy _gameboy;

        internal bool DoubleSpeed { get; private set; }

        public CPU(Gameboy gameboy)
        {
            _gameboy = gameboy;
            _debugger = new Debugger(gameboy);

            RegisterInstructions();

            Reset(false, null);
        }

        public void Reset(bool inBios = false, Cartridge cart = null)
        {
            InitializeRegisters();

            IME = true;
            setIME = false;

            Halt = false;
            HaltBug = false;
            DoubleSpeed = false;

            if (cart != null)
            {
                _gameboy.Mmu.LoadCartridge(cart);
            }

            if (inBios) StartInBios();
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
            if (setIME)
            {
                setIME = false;
                IME = true;
            }

            int pc = LoadRegister(Registers16Bit.PC);
            Instruction instruction = GetNextInstruction();
            _debugger.Debug(instruction, pc);
            int cycles = instruction.Execute();


            int interruptCycles = CheckInterrupts();

            cycles += interruptCycles;

            return cycles;
        }

        private int CheckInterrupts()
        {
            if ((_gameboy.Mmu.IE & _gameboy.Mmu.IF) != 0)
            {
                Halt = false;
                if(IME) return ExecuteInterrupt();
            }

            return 0;
        }

        private int ExecuteInterrupt()
        {
            IME = false;

            int pc = LoadRegister(Registers16Bit.PC);
            int sp = Bitwise.Wrap16(LoadRegister(Registers16Bit.SP) - 1);
            _gameboy.Mmu.WriteByte(pc >> 8, sp);

            int vector = 0;

            for (int i = 0; i < 5; i++)
            {
                if ((((_gameboy.Mmu.IE & _gameboy.Mmu.IF) >> i) & 0x01) == 1)
                {
                    vector = 0x40 + (8 * i);
                    _gameboy.Mmu.IF &= ~(0x1 << i);
                    break;
                }
            }

            SetRegister(Registers16Bit.PC, vector);

            sp = Bitwise.Wrap16(sp - 1);
            _gameboy.Mmu.WriteByte(pc & 0xFF, sp);
            SetRegister(Registers16Bit.SP, sp);

            return 4;
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