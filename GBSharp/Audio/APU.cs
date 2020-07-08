using System;
using System.Collections.Generic;
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
        private Sound synth;
        private float _time;

        private SquareWave squareWave;

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
            synth = new Sound();
        }

        public int WriteByte(int address, int value)
        {
            if (address <= 0xFF14) return squareWave.WriteByte(address, value);

            return value;
        }

        public int ReadByte(int address)
        {
            return -1;
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
                            break;

                        case 1:
                            break;

                        case 2:
                            squareWave.UpdateLength();
                            break;

                        case 3:
                            break;

                        case 4:
                            squareWave.UpdateLength();
                            break;

                        case 5:
                            break;

                        case 6:
                            squareWave.UpdateLength();
                            break;

                        case 7:
                            break;

                        default:
                            throw new InvalidOperationException("Frame Sequencer can not be this value: " + FrameSequencer.ToString());
                    }
                    FrameSequencer = (FrameSequencer + 1) % 8;

                    squareWave.Step();
                }

                if (++TotalSamples >= SAMPLE_GOAL)
                {
                    TotalSamples -= SAMPLE_GOAL;

                    synth.AddVolumeInfo(squareWave.GetVolume());
                }
            }
        }

        public void UpdateSynths()
        {
            synth.Update();
        }
    }
}
