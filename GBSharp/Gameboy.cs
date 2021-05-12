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

        internal LinkCable LinkCable { get; private set; }

        public int CyclesCount { get; private set; }
        public const int CPU_CYCLES = 17556 * 4;

        private FrameQueue frameQueue;

        public Gameboy()
        {
            Mmu = new MMU(this);
            Ppu = new PPU(this);
            Apu = new APU(this);
            Input = new Input(this);
            Timer = new Timer(this);

            Cpu = new CPU(this);
            Dma = new DMA(this);
            LinkCable = new LinkCable(this);

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
            LinkCable.Reset();

            frameQueue = new FrameQueue();
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

        internal void EnqeueFrameBuffer(int[] frame)
        {
            lock(this)
            {
                frameQueue.Enqueue(frame);
            }
        }

        public int[] DequeueFrameBuffer()
        {
            lock(this)
            {
                return frameQueue.Dequeue();
            }
        }

        public int GetFrameBufferCount()
        {
            lock(this)
            {
                return frameQueue.Count;
            }
        }
    }
}
