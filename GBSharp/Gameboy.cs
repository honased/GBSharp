using GBSharp.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class Gameboy
    {
        internal CPU Cpu { get; private set; }
        internal MMU Mmu { get; private set; }
        internal PPU Ppu { get; private set; }
        internal APU Apu { get; private set; }
        internal Input Input { get; private set; }
        internal Timer Timer { get; private set; }
        internal bool IsCGB { get; private set; }

        public int CyclesCount { get; private set; }
        public const int CPU_CYCLES = 17556 * 4;

        public Gameboy()
        {
            Mmu = new MMU(this);
            Ppu = new PPU(this);
            Apu = new APU(this);
            Input = new Input(this);
            Timer = new Timer(this);

            Cpu = new CPU(this);

            Reset();
        }

        public void Reset(bool inBios = false, Cartridge cartridge = null)
        {
            CyclesCount = 0;
            Cpu.Reset(inBios, cartridge);
            Mmu.Reset();
            Ppu.Reset();
            Apu.Reset();
            Input.Reset();
            Timer.Reset();
        }

        public void StartInBios()
        {
            Cpu.StartInBios();
        }

        public void ExecuteFrame()
        {
            while (CyclesCount < CPU_CYCLES)
            {
                int cycles = Cpu.ExecuteCycle(out bool interr);
                Timer.Tick(cycles);

                int divisorAmount = Cpu.DoubleSpeed ? 2 : 1;

                cycles *= 4;

                Input.Tick();
                Ppu.Tick(cycles / divisorAmount);

                Apu.Tick(cycles / divisorAmount);

                CyclesCount += cycles / divisorAmount;
            }
            CyclesCount -= CPU_CYCLES;
        }

        public void Close()
        {
            Mmu._cartridge.Close();
        }

        public ref int[] GetFrameBuffer()
        {
            ref int[] buffer = ref Ppu.GetFrameBuffer();
            return ref buffer;
        }

        public int[] GetTilesBuffer()
        {
            return Ppu.Tiles;
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            Mmu.LoadCartridge(cartridge);
            if (cartridge.GameboyType == Cartridge.GBMode.GBOnly) IsCGB = false;
            else IsCGB = true;

            if (IsCGB) Cpu.SetRegister(CPU.Registers8Bit.A, 0x11);
            else Cpu.SetRegister(CPU.Registers8Bit.A, 0x01);

            bool debug = false;

            if (debug)
            {
                List<int> expectedVals = new List<int>();
                using (StreamReader sr = new StreamReader("F:\\rboy-master\\cpu_debug.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] tokens = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string t in tokens)
                        {
                            if (t == "true") expectedVals.Add(1);
                            else if (t == "false") expectedVals.Add(0);
                            else expectedVals.Add(Convert.ToInt32(t));
                        }
                    }
                }

                Console.WriteLine("Finished Reading");

                int pos = 0;
                while (pos < expectedVals.Count)
                {
                    int instruction = Mmu.ReadByte(Cpu.LoadRegister(CPU.Registers16Bit.PC));

                    if (pos == 0x27a9f7)
                    {
                        int deb = 0;
                    }

                    int cycles = Cpu.ExecuteCycle(out bool interrupt);

                    int divisorAmount = Cpu.DoubleSpeed ? 2 : 1;

                    if (interrupt) instruction = -1;

                    int expInstr = expectedVals[pos];
                    int expCycle = expectedVals[pos + 1];
                    //bool expZ = expectedVals[pos + 2] == 1;
                    int expMode = expectedVals[pos + 2];
                    int expLy = expectedVals[pos + 3];
                    int expAF = expectedVals[pos + 4];
                    int expBC = expectedVals[pos + 5];
                    int expDE = expectedVals[pos + 6];
                    int expHL = expectedVals[pos + 7];
                    int expSP = expectedVals[pos + 8];
                    int expPC = expectedVals[pos + 9];
                    int expSTAT = expectedVals[pos + 10];
                    int expIF = expectedVals[pos + 11];
                    int expRamVal = expectedVals[pos + 12];

                    int actualAF = Cpu.LoadRegister(CPU.Registers16Bit.AF);
                    int actualBC = Cpu.LoadRegister(CPU.Registers16Bit.BC);
                    int actualDE = Cpu.LoadRegister(CPU.Registers16Bit.DE);
                    int actualHL = Cpu.LoadRegister(CPU.Registers16Bit.HL);
                    int actualSP = Cpu.LoadRegister(CPU.Registers16Bit.SP);
                    int actualPC = Cpu.LoadRegister(CPU.Registers16Bit.PC);
                    int actualStat = Mmu.ReadByte(0xFF41);
                    int actualIF = Mmu.ReadByte(0xFF0F);
                    int actualRamVal = Mmu.ReadByte(0xFF24);

                    bool actualZ = Cpu.IsFlagOn(CPU.Flags.Z);

                    int actualMode = (Mmu.ReadByte(0xFF41)) & 0x03;
                    int actualLy = Mmu.LY;

                    if (instruction != expInstr)
                    {
                        throw new Exception();
                    }
                    if ((cycles * 4) != expCycle)
                    {
                        throw new Exception();
                    }
                    if (actualMode != expMode)
                    {
                        throw new Exception();
                    }
                    if (actualLy != expLy)
                    {
                        throw new Exception();
                    }
                    if (actualAF != expAF)
                    {
                        //throw new Exception();
                    }
                    if (actualBC != expBC)
                    {
                        throw new Exception();
                    }
                    if (actualDE != expDE)
                    {
                        throw new Exception();
                    }
                    if (actualHL != expHL)
                    {
                        throw new Exception();
                    }
                    if (actualSP != expSP)
                    {
                        throw new Exception();
                    }
                    if (actualPC != expPC)
                    {
                        throw new Exception();
                    }
                    if (actualStat != expSTAT)
                    {
                        throw new Exception();
                    }
                    if (actualIF != expIF)
                    {
                        throw new Exception();
                    }
                    if (actualRamVal != expRamVal)
                    {
                        //throw new Exception();
                    }

                    pos += 13;

                    Timer.Tick(cycles);

                    Input.Tick();
                    Ppu.Tick((cycles * 4) / divisorAmount);
                    Apu.Tick((cycles * 4) / divisorAmount);
                }
                Console.WriteLine("FINISHED");
                Console.ReadKey();
            }
        }

        public void SetInput(Input.Button button, bool pressed)
        {
            Input.SetInput(button, pressed);
        }

        public void Debug()
        {
            Cpu.Debug();
        }
    }
}
