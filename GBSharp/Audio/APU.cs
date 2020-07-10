using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace GBSharp.Audio
{
    public class APU
    {
        private const int FREQUENCY = 44100;
        private const int CHANNELS = 2;
        private const int SAMPLE_SIZE = 1000;
        private const int FRAME_SEQUENCER_CLOCKS = 8192;
        private const int SAMPLE_GOAL = 95;
        private int FrameSequencer { get; set; }
        private int totalClocks;
        private int TotalSamples { get; set; }

        private bool[,] OutputSound { get; set; }
        private int VolumeLeft { get; set; }
        private int VolumeRight { get; set; }

        private SquareWave squareWave;
        private SquareWaveTwo squareWave2;
        private SampleWave sampleWave;
        private NoiseWave noiseWave;

        internal MMU _mmu;

        public APU()
        {
            Reset();
        }

        internal void Reset()
        {
            FrameSequencer = 0;
            totalClocks = 0;
            TotalSamples = 0;

            squareWave = new SquareWave();
            squareWave2 = new SquareWaveTwo();
            sampleWave = new SampleWave();
            noiseWave = new NoiseWave();

            OutputSound = new bool[2, 4];
            VolumeLeft = 0;
            VolumeRight = 0;
        }

        public int WriteByte(int address, int value)
        {
            if (address <= 0xFF14) return squareWave.WriteByte(address, value);
            if (address <= 0xFF19) return squareWave2.WriteByte(address, value);

            if (address <= 0xFF1E) return sampleWave.WriteByte(address, value);
            if (address >= 0xFF20 && address <= 0xFF23) return noiseWave.WriteByte(address, value);
            if (address <= 0xFF26)
            {
                switch(address)
                {
                    case 0xFF24:
                        VolumeLeft = (value >> 4) & 0x07;
                        VolumeRight = value & 0x07;
                        return value;

                    case 0xFF25:
                        for(int i = 0; i < 8; i++)
                        {
                            OutputSound[i / 4, i % 4] = Bitwise.IsBitOn(value, i);
                        }
                        return value;

                    case 0xFF26:
                        int returnMem = value & 0x80;
                        returnMem |= (squareWave.IsPlaying() ? 1 : 0);
                        returnMem |= (squareWave2.IsPlaying() ? 1 : 0) << 1;
                        return returnMem;
                }
            }

            if (address >= 0xFF30 && address <= 0xFF3F) return sampleWave.WriteByte(address, value);

            return value;
        }

        public int ReadByte(int address, int[] memory)
        {
            if (address <= 0xFF14) return squareWave.ReadByte(address, memory);
            if (address <= 0xFF19) return squareWave2.ReadByte(address, memory);
            if (address <= 0xFF1E) return sampleWave.ReadByte(address, memory);
            if (address >= 0xFF20 && address <= 0xFF23) return noiseWave.ReadByte(address, memory);
            if (address == 0xFF26)
            {
                int returnMem = memory[address - 0xFF00] & 0x80;
                returnMem |= (squareWave.IsPlaying() ? 1 : 0);
                returnMem |= (squareWave2.IsPlaying() ? 1 : 0) << 1;
                return returnMem;
            }

            return memory[address - 0xFF00];
        }

        public void Tick(int clocks)
        {
            clocks *= 4;
            while (clocks-- > 0)
            {
                totalClocks += 1;
                if (totalClocks >= FRAME_SEQUENCER_CLOCKS)
                {
                    totalClocks -= FRAME_SEQUENCER_CLOCKS;
                    switch (FrameSequencer)
                    {
                        case 0:
                            squareWave.UpdateLength();
                            squareWave2.UpdateLength();
                            sampleWave.UpdateLength();
                            noiseWave.UpdateLength();
                            break;

                        case 1:
                            break;

                        case 2:
                            squareWave.UpdateLength();
                            squareWave.UpdateSweep();

                            squareWave2.UpdateLength();
                            sampleWave.UpdateLength();
                            noiseWave.UpdateLength();
                            break;

                        case 3:
                            break;

                        case 4:
                            squareWave.UpdateLength();
                            squareWave2.UpdateLength();
                            sampleWave.UpdateLength();
                            noiseWave.UpdateLength();
                            break;

                        case 5:
                            break;

                        case 6:
                            squareWave.UpdateLength();
                            squareWave.UpdateSweep();

                            squareWave2.UpdateLength();
                            sampleWave.UpdateLength();
                            noiseWave.UpdateLength();
                            break;

                        case 7:
                            squareWave.UpdateEnvelope();
                            squareWave2.UpdateEnvelope();
                            noiseWave.UpdateEnvelope();
                            break;

                        default:
                            throw new InvalidOperationException("Frame Sequencer can not be this value: " + FrameSequencer.ToString());
                    }
                    FrameSequencer = (FrameSequencer + 1) % 8;
                }

                squareWave.Step();
                squareWave2.Step();
                sampleWave.Step();
                noiseWave.Step();

                if (++TotalSamples >= SAMPLE_GOAL)
                {
                    TotalSamples -= SAMPLE_GOAL;

                    squareWave.Emitter.AddVolumeInfo(squareWave.GetVolume(), VolumeLeft * (OutputSound[1, 0] ? 1 : 0), VolumeRight * (OutputSound[0, 0] ? 1 : 0));
                    squareWave2.Emitter.AddVolumeInfo(squareWave2.GetVolume(), VolumeLeft * (OutputSound[1, 1] ? 1 : 0), VolumeRight * (OutputSound[0, 1] ? 1 : 0));
                    sampleWave.Emitter.AddVolumeInfo(sampleWave.GetVolume(), VolumeLeft * (OutputSound[1, 2] ? 1 : 0), VolumeRight * (OutputSound[0, 2] ? 1 : 0));
                    noiseWave.Emitter.AddVolumeInfo(noiseWave.GetVolume(), VolumeLeft * (OutputSound[1, 3] ? 1 : 0), VolumeRight * (OutputSound[0, 3] ? 1 : 0));
                }
            }
        }
    }
}
