using GBSharp.Audio;
using GBSharp.Graphics;
using GBSharp.Interfaces;
using GBSharp.Processor;
using System;
using System.Collections.Generic;
using System.IO;

namespace GBSharp
{
    public class Gameboy : IStateable
    {
        internal CPU Cpu { get; private set; }
        internal MMU Mmu { get; private set; }
        public PPU Ppu { get; private set; }
        internal APU Apu { get; private set; }
        internal Input Input { get; private set; }
        internal Timer Timer { get; private set; }
        internal bool IsCGB { get; private set; }

        internal DMA Dma { get; private set; }

        public int CyclesCount { get; private set; }
        public const int CPU_CYCLES = 17556 * 4;

        public Queue<int[]> frameQueue;

        public Gameboy()
        {
            Mmu = new MMU(this);
            Ppu = new PPU(this);
            Apu = new APU(this);
            Input = new Input(this);
            Timer = new Timer(this);

            Cpu = new CPU(this);
            Dma = new DMA(this);

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
            Dma.Reset();

            frameQueue = new Queue<int[]>();
        }

        public void StartInBios()
        {
            Cpu.StartInBios();
        }

        public void ExecuteFrame()
        {
            while (CyclesCount < CPU_CYCLES)
            {
                int cycles = Cpu.ExecuteCycle();

                int divisorAmount = Cpu.DoubleSpeed ? 2 : 1;

                int dmaCycles = Dma.CopyData();

                cycles *= 4;

                Timer.Tick(cycles + dmaCycles);

                Input.Tick();
                Ppu.Tick((cycles + dmaCycles) / divisorAmount);

                Apu.Tick((cycles + dmaCycles) / divisorAmount);

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

        public bool IsFrameBufferReady()
        {
            return Ppu.IsFrameBufferReady;
        }

        public int[] GetTilesBuffer(int vramBank)
        {
            return Ppu.GetTiles(vramBank);
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            Mmu.LoadCartridge(cartridge);
            if (cartridge.GameboyType == Cartridge.GBMode.GBOnly) IsCGB = false;
            else IsCGB = true;

            Mmu.LCDC = 0x91;
            Mmu.STAT = 0x81;

            if (IsCGB)
            {
                Cpu.SetRegister(CPU.Registers16Bit.AF, 0x1180);
                Cpu.SetRegister(CPU.Registers16Bit.BC, 0x0000);
                Cpu.SetRegister(CPU.Registers16Bit.DE, 0xFF56);
                Cpu.SetRegister(CPU.Registers16Bit.HL, 0x000D);
            }
            else
            {
                Cpu.SetRegister(CPU.Registers16Bit.AF, 0x1180);
                Cpu.SetRegister(CPU.Registers16Bit.BC, 0x0000);
                Cpu.SetRegister(CPU.Registers16Bit.DE, 0x0008);
                Cpu.SetRegister(CPU.Registers16Bit.HL, 0x007C);
            }

            if(false)
            {
                using(StreamReader sr = new StreamReader("test.txt"))
                {
                    int oldPC = 0;
                    while(!sr.EndOfStream)
                    {
                        int myAF = Cpu.LoadRegister(CPU.Registers16Bit.AF);
                        int myBC = Cpu.LoadRegister(CPU.Registers16Bit.BC);
                        int myDE = Cpu.LoadRegister(CPU.Registers16Bit.DE);
                        int myHL = Cpu.LoadRegister(CPU.Registers16Bit.HL);
                        int mySP = Cpu.LoadRegister(CPU.Registers16Bit.SP);
                        int myPC = Cpu.LoadRegister(CPU.Registers16Bit.PC);
                        int instruction = Mmu.ReadByte(myPC);

                        int cycles = Cpu.ExecuteCycle();

                        string[] tokens = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int readAF = Convert.ToInt32(tokens[0].Substring(2), 16) << 8;
                        if (tokens[1].Contains("C")) readAF += 16;
                        if (tokens[1].Contains("H")) readAF += 32;
                        if (tokens[1].Contains("N")) readAF += 64;
                        if (tokens[1].Contains("Z")) readAF += 128;

                        int readBC = Convert.ToInt32(tokens[2].Substring(3), 16);
                        int readDE = Convert.ToInt32(tokens[3].Substring(3), 16);
                        int readHL = Convert.ToInt32(tokens[4].Substring(3), 16);
                        int readSP = Convert.ToInt32(tokens[5].Substring(3), 16);
                        int readPC = Convert.ToInt32(tokens[6].Substring(3), 16);

                        int readCycles = Convert.ToInt32(tokens[8].Replace(")", ""));

                        if(myAF != readAF)
                        {
                            //throw new Exception("Bad AF");
                        }
                        if (myBC != readBC)
                        {
                            throw new Exception("Bad BC");
                        }
                        if (myDE != readDE)
                        {
                            throw new Exception("Bad DE");
                        }
                        if (myHL != readHL)
                        {
                            throw new Exception("Bad HL");
                        }
                        if (mySP != readSP)
                        {
                            throw new Exception("Bad SP");
                        }
                        if (readPC != myPC)
                        {
                            throw new Exception("Bad PC");
                        }

                        oldPC = myPC;
                    }
                }
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

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(IsCGB);
            stream.Write(CyclesCount);
            Cpu.SaveState(stream);
            Mmu.SaveState(stream);
            Ppu.SaveState(stream);
            Apu.SaveState(stream);
            Input.SaveState(stream);
            Timer.SaveState(stream);
            Dma.SaveState(stream);
        }

        public void LoadState(BinaryReader stream)
        {
            IsCGB = stream.ReadBoolean();
            CyclesCount = stream.ReadInt32();
            Cpu.LoadState(stream);
            Mmu.LoadState(stream);
            Ppu.LoadState(stream);
            Apu.LoadState(stream);
            Input.LoadState(stream);
            Timer.LoadState(stream);
            Dma.LoadState(stream);
        }

        public void Run()
        {
            while(true)
            {
                while(Apu.GetPendingBufferCount() < 3)
                {
                    int cycles = Cpu.ExecuteCycle();

                    int divisorAmount = Cpu.DoubleSpeed ? 2 : 1;

                    int dmaCycles = Dma.CopyData();

                    cycles *= 4;

                    Timer.Tick(cycles + dmaCycles);

                    Input.Tick();
                    Ppu.Tick((cycles + dmaCycles) / divisorAmount);

                    Apu.Tick((cycles + dmaCycles) / divisorAmount);

                    CyclesCount += cycles / divisorAmount;
                }
            }
        }
    }
}
