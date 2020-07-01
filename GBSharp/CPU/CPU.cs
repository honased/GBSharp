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
        private bool IME;
        private int setIME = 0;
        private int clearIME = 0;
        private int debugTime = 0;

        public bool DebugMode { get; set; }

        bool debugging = true;

        public CPU(MMU mmu, PPU ppu, Input input)
        {
            _mmu = mmu;
            _ppu = ppu;
            _input = input;
            _timer = new Timer(_mmu);
            mmu.SetCPU(this);
            mmu.SetPPU(_ppu);
            mmu.SetTimer(_timer);

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
            setIME = 0;
            clearIME = 0;

            debugging = true;

            _mmu.Reset();
            _ppu.Reset();
            _timer.Reset();

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
                int cycles = CheckInterrupts();

                if(cycles == 0)
                {
                    int pc = LoadRegister(Registers16Bit.PC);

                    Instruction instruction = GetNextInstruction();
                    //Console.WriteLine("[{0:X}] 0x{1:X}: " + instruction.Name, pc, instruction.Opcode);

                    if(DebugMode)
                    {
                        if(!debugging)
                        {
                            if (debugTime > 0)
                            {
                                if (--debugTime == 0)
                                {
                                    debugging = true;
                                }
                            }
                        }
                        if(debugging)
                        {
                            Console.WriteLine("\nAF:0x{0:X4}\tBC:0x{1:X4}\tDE:0x{2:X4}\tHL:0x{3:X4}\tSP:0x{4:X4}", LoadRegister(Registers16Bit.AF), LoadRegister(Registers16Bit.BC), LoadRegister(Registers16Bit.DE), LoadRegister(Registers16Bit.HL), LoadRegister(Registers16Bit.SP));
                            Console.WriteLine(_timer.ToString());
                            Console.WriteLine("Instruction: [0x{0:X4}] 0x{1:X2}: " + instruction.Name, pc, instruction.Opcode);

                            bool successful = false;
                            while(!successful)
                            {
                                Console.Write(">> ");
                                successful = ProcessCommands(Console.ReadLine().Trim());
                            }
                        }
                    }

                    cycles = instruction.Execute();
                }
                
                currentCycles += cycles;
                _timer.Tick(cycles);
                _ppu.Tick(cycles);
                _input.Tick();
            }
            currentCycles -= CPU_CYCLES;
            //Console.WriteLine("REnder frame: " + PPU.RenderCount);
        }

        private bool ProcessCommands(string input)
        {
            string[] tokens = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return true;

            int memLocation, waitTime;

            switch(tokens[0])
            {
                case "chk":
                    if (tokens.Length == 1) Console.WriteLine("Not enough arguments!");
                    switch(tokens[1])
                    {
                        case "mem":
                            if (tokens.Length == 2) Console.WriteLine("Not enough arguments!");
                            if (TryParseHex(tokens[2], out memLocation)) Console.WriteLine("Memory at 0x{0:X4}:" + _mmu.ReadByte(memLocation), memLocation);
                            else Console.WriteLine("Bad location given!");
                            break;
                    }
                    break;

                case "run":
                    if (tokens.Length == 1) debugging = false;
                    else if (TryParseHex(tokens[1], out waitTime))
                    {
                        debugging = false;
                        debugTime = waitTime;
                    }
                    else Console.WriteLine("Bad location given");
                    return true;

                case "jp":
                    if (tokens.Length == 1) Console.WriteLine("Not enough arguments!");
                    
                    

                    break;

                default:
                    Console.WriteLine("Unknown command");
                    break;
            }

            return false;
        }

        private bool TryParseHex(string hex, out int result)
        {
            result = 0;

            try
            {
                if (hex.Length > 2 && hex[0] == '0' && hex[1] == 'x')
                {
                    result = Convert.ToInt32(hex, 16);
                    return true;
                }
                else
                {
                    result = Convert.ToInt32(hex);
                    return true;
                }
                
            }
            catch (FormatException e)
            {
                return false;
            }
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
                default: setIME = 0; break;
            }

            if (IME)
            {
                for (int i = 0; i < 5; i++)
                {
                    if ((IE & IF) >> i == 1)
                    {
                        ExecuteInterrupt(i);
                        return 1;
                    }
                }
            }

            return 0;
        }

        private void ExecuteInterrupt(int interrupt)
        {
            IME = false;
            Push(LoadRegister(Registers16Bit.PC));
            SetRegister(Registers16Bit.PC, 0x40 + (interrupt * 8));
            _mmu.IF &= ~(0x1 << interrupt);
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