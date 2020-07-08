using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class SquareWave
    {
        private int[] DutyCycles { get; set; } =
            {0,0,0,0,0,0,0,1,
             1,0,0,0,0,0,0,1,
             1,0,0,0,0,1,1,1,
             0,1,1,1,1,1,1,0};

        private int Length { get; set; }
        private int Duty { get; set; }
        private int Frequency { get; set; }
        private int FrequencyTimer { get; set; }
        private bool Enabled { get; set; }
        private int SequencePointer { get; set; }
        private bool LengthEnabled { get; set; }
        private int Volume { get; set; }
        private int OutputVolume { get; set; }
        private int Envelope { get; set; }
        private bool EnvelopeAdd { get; set; }
        private int EnvelopeTime { get; set; }
        private int EnvelopeTimeSet { get; set; }
        private bool EnvelopeEnabled { get; set; }

        private void Reset()
        {
            Length = 0;
            SequencePointer = 0;
            Enabled = false;
            Duty = 0;
            LengthEnabled = false;
            FrequencyTimer = 0;
            Volume = 0;
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

        internal void UpdateEnvelope()
        {
            if(--EnvelopeTime <= 0)
            {
                EnvelopeTime = EnvelopeTimeSet;
                if (EnvelopeTime == 0) EnvelopeTime = 8;

                if(EnvelopeEnabled && EnvelopeTimeSet > 0)
                {
                    if(EnvelopeAdd)
                    {
                        if (Volume < 15) Volume++;
                    }
                    else
                    {
                        if (Volume > 0) Volume--;
                    }
                }

                if (Volume == 0 || Volume == 15) EnvelopeEnabled = false;
            }
        }

        internal void Step()
        {
            if(--FrequencyTimer <= 0)
            {
                FrequencyTimer = (2048 - Frequency) * 4;
                SequencePointer = (SequencePointer + 1) % 8;
            }

            if (Enabled)
            {
                OutputVolume = DutyCycles[(Duty * 8) + SequencePointer] * Volume;
            }
            else OutputVolume = 0;
        }

        internal int GetVolume()
        {
            return OutputVolume;
        }

        public int WriteByte(int address, int value)
        {
            switch(address)
            {
                case 0xFF10:

                    return value;

                case 0xFF11:
                    Duty = (value >> 6);
                    Length = (value & 0x1F);
                    return value;

                case 0xFF12:
                    Volume = (value >> 4);
                    EnvelopeAdd = Bitwise.IsBitOn(value, 3);
                    EnvelopeTime = value & 0x07;
                    EnvelopeTimeSet = EnvelopeTime;
                    return value;

                case 0xFF13:
                    Frequency = ((Frequency & ~0xFF) | value);
                    return -1;

                case 0xFF14:
                    Frequency = ((value & 0x7) << 8) | Frequency;
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    if (Bitwise.IsBitOn(value, 7)) Enable();
                    return value;
            }

            return -1;
        }

        private void Enable()
        {
            Enabled = true;
            FrequencyTimer = (2048 - Frequency) * 4;
            EnvelopeEnabled = true;
        }
    }
}
