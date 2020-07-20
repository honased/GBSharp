using GBSharp.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public const int CPU_CYCLES = 17556;

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
                int divisorAmount = Cpu.DoubleSpeed ? 2 : 1;

                int accumalitiveCycles = 0;

                for(int i = 0; i < divisorAmount; i++)
                {
                    int cycles = Cpu.ExecuteCycle();
                    Timer.Tick(cycles);
                    accumalitiveCycles += cycles;
                }

                accumalitiveCycles /= divisorAmount;

                CyclesCount += accumalitiveCycles;

                Input.Tick();
                Ppu.Tick(accumalitiveCycles);
                //Apu.Tick(accumalitiveCycles);
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
