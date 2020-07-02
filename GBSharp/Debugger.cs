using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    internal class Debugger
    {
        public bool DebugMode { get; set; }

        private bool Debugging { get; set; }

        private int DebugTime { get; set; }
        private int DebugOp { get; set; }
        private int DebugPC { get; set; }

        private CPU _cpu { get; set; }
        private MMU _mmu { get; set; }
        private PPU _ppu { get; set; }
        private Timer _timer { get; set; }

        public Debugger(CPU cpu, MMU mmu, PPU ppu, Timer timer)
        {
            _cpu = cpu;
            _mmu = mmu;
            _ppu = ppu;
            _timer = timer;
            Reset();
        }

        internal void Reset()
        {
            Debugging = true;
            DebugPC = -1;
            DebugTime = 0;
            DebugOp = -1;
        }

        internal void Debug(Instruction instruction, int pc)
        {
            if (DebugMode)
            {
                if (!Debugging)
                {
                    if (DebugTime > 0)
                    {
                        if (--DebugTime == 0)
                        {
                            Debugging = true;
                        }
                    }
                    if (DebugOp >= 0)
                    {
                        if (instruction.Opcode.Equals(DebugOp))
                        {
                            Debugging = true;
                            DebugOp = -1;
                        }
                    }
                    if (DebugPC >= 0)
                    {
                        if (pc == DebugPC)
                        {
                            Debugging = true;
                            DebugPC = -1;
                        }
                    }
                }
                if (Debugging)
                {
                    Console.WriteLine("\nAF:0x{0:X4}\tBC:0x{1:X4}\tDE:0x{2:X4}\tHL:0x{3:X4}\tSP:0x{4:X4}", _cpu.LoadRegister(CPU.Registers16Bit.AF), _cpu.LoadRegister(CPU.Registers16Bit.BC), _cpu.LoadRegister(CPU.Registers16Bit.DE), _cpu.LoadRegister(CPU.Registers16Bit.HL), _cpu.LoadRegister(CPU.Registers16Bit.SP));
                    Console.WriteLine(_timer.ToString());
                    Console.WriteLine("IME:" + _cpu.IME + "\tIE:{0:X2}\tIF:{1:X2}", _mmu.IE, _mmu.IF);

                    Console.WriteLine("LCDC:{0:X2}\tSTAT:{1:X2}\tLY:{2:X2}", _mmu.LCDC, _mmu.STAT, _mmu.LY);
                    Console.WriteLine("Instruction: [0x{0:X4}] 0x{1:X2}: " + instruction.Name, pc, instruction.Opcode);

                    bool successful = false;
                    while (!successful)
                    {
                        Console.Write(">> ");
                        successful = ProcessCommands(Console.ReadLine().Trim());
                    }
                }
            }
        }

        private bool ProcessCommands(string input)
        {
            string[] tokens = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return true;

            int memLocation, waitTime;

            switch (tokens[0])
            {
                case "chk":
                    if (tokens.Length == 1) Console.WriteLine("Not enough arguments!");
                    switch (tokens[1])
                    {
                        case "mem":
                            if (tokens.Length == 2) Console.WriteLine("Not enough arguments!");
                            if (TryParseHex(tokens[2], out memLocation)) Console.WriteLine("Memory at 0x{0:X4}:" + _mmu.ReadByte(memLocation), memLocation);
                            else Console.WriteLine("Bad location given!");
                            break;
                    }
                    break;

                case "run":
                    if (tokens.Length == 1) Debugging = false;
                    else if (TryParseHex(tokens[1], out waitTime))
                    {
                        Debugging = false;
                        DebugTime = waitTime;
                    }
                    else { Console.WriteLine("Bad location given"); return false; }
                    return true;

                case "jp":
                    if (tokens.Length == 1) Console.WriteLine("Not enough arguments!");
                    else
                    {
                        switch (tokens[1])
                        {
                            case "op":
                                if (tokens.Length == 2) Console.WriteLine("Not enough arguments!");
                                else if (TryParseHex(tokens[2], out waitTime))
                                {
                                    Debugging = false;
                                    DebugOp = waitTime;
                                    return true;
                                }
                                else Console.WriteLine("Bad opcode given");
                                break;

                            case "pc":
                                if (tokens.Length == 2) Console.WriteLine("Not enough arguments!");
                                else if (TryParseHex(tokens[2], out waitTime))
                                {
                                    Debugging = false;
                                    DebugPC = waitTime;
                                    return true;
                                }
                                else Console.WriteLine("Bad pc given");
                                break;

                            default:
                                Console.WriteLine("That command does not exist!");
                                return false;
                        }
                    }

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
    }
}
