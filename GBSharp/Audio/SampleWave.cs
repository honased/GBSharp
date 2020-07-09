using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class SampleWave
    {
        private int[] Samples { get; set; }

        private int Length { get; set; }
        private int Frequency { get; set; }
        private int FrequencyTimer { get; set; }
        private bool Enabled { get; set; }
        private int SequencePointer { get; set; }
        private bool LengthEnabled { get; set; }
        private int VolumeLevel { get; set; }
        private int OutputVolume { get; set; }

        private bool BitEnabled { get; set; }

        private int SamplePosition { get; set; }

        internal Sound Emitter { get; private set; }

        public SampleWave()
        {
            Reset();
        }

        private void Reset()
        {
            Length = 0;
            SequencePointer = 0;
            Enabled = false;
            LengthEnabled = false;
            FrequencyTimer = 0;
            VolumeLevel = 0;

            Samples = new int[0x10];

            SamplePosition = 0;
            BitEnabled = false;

            Emitter = new Sound();
        }

        internal void UpdateLength()
        {
            if(LengthEnabled && Length > 0)
            {
                if(--Length == 0)
                {
                    Enabled = false;
                }
            }
        }

        internal void Step()
        {
            if(--FrequencyTimer <= 0)
            {
                FrequencyTimer = (2048 - Frequency) * 2;
                SamplePosition = (SamplePosition + 1) & 0x1F;

                if (BitEnabled && Enabled)
                {
                    int position = SamplePosition / 2;
                    int outputByte = Samples[position];
                    if ((SamplePosition & 0x1) == 0) outputByte >>= 4;
                    outputByte &= 0xF;

                    if (VolumeLevel > 0)
                    {
                        outputByte >>= VolumeLevel - 1;
                    }
                    else outputByte = 0;
                    OutputVolume = outputByte;
                }
                else OutputVolume = 0;
            }
        }

        internal int GetVolume()
        {
            return OutputVolume;
        }

        public int WriteByte(int address, int value)
        {
            switch(address)
            {
                case 0xFF1A:
                    BitEnabled = Bitwise.IsBitOn(value, 7);
                    return value;

                case 0xFF1B:
                    Length = value;
                    return value;

                case 0xFF1C:
                    VolumeLevel = (value >> 5) & 0x03;
                    return value;

                case 0xFF1D:
                    Frequency = (Frequency & 0x700) | value;
                    return value;

                case 0xFF1E:
                    Frequency = ((value & 0x7) << 8) | (Frequency & 0xFF);
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    if (Bitwise.IsBitOn(value, 7)) Enable();
                    return value;

                case int _ when address >= 0xFF30 && address <= 0xFF3f:
                    Samples[address - 0xFF30] = value;
                    return value;
            }

            throw new InvalidOperationException("Cannot write to that memory address");
        }

        internal int ReadByte(int address, int[] memory)
        {
            switch (address)
            {
                case 0xFF1A:
                    return memory[0x10];

                case 0xFF1B:
                    return memory[0x11] | (0x3F);

                case 0xFF1C:
                    return memory[0x12];

                case 0xFF1D:
                    return 0xFF;

                case 0xFF1E:
                    return memory[0x14] | 0x87;
            }

            throw new InvalidOperationException("Can't read from this memory spot!");
        }

        internal bool IsPlaying()
        {
            return Length > 0;
        }

        private void Enable()
        {
            Enabled = true;
            FrequencyTimer = (2048 - Frequency) * 2;

            if (Length == 0) Length = 256;

            SamplePosition = 0;
        }
    }
}
