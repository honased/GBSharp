﻿using GBSharp.Audio;
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

        public void ExecuteFrame()
        {
            while (CyclesCount < CPU_CYCLES)
            {
                int cycles = Cpu.ExecuteCycle();
                CyclesCount += cycles;

                Timer.Tick(cycles);
                Input.Tick();
                Ppu.Tick(cycles);
                Apu.Tick(cycles);
            }
            CyclesCount -= CPU_CYCLES;
        }

        public int[] GetFrameBuffer()
        {
            return Ppu.FrameBuffer;
        }

        public int[] GetTilesBuffer()
        {
            return Ppu.Tiles;
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            Mmu.LoadCartridge(cartridge);
        }

        public void SetInput(Input.Button button, bool pressed)
        {
            Input.SetInput(button, pressed);
        }
    }
}
