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
        public PPU Ppu { get; private set; }
        internal APU Apu { get; private set; }
        internal Input Input { get; private set; }
        internal Timer Timer { get; private set; }
        internal bool IsCGB { get; private set; }

        internal DMA Dma { get; private set; }

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

            if (IsCGB) Cpu.SetRegister(CPU.Registers8Bit.A, 0x11);
            else Cpu.SetRegister(CPU.Registers8Bit.A, 0x01);
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
