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
        private int SweepTime { get; set; }
        private int SweepTimeSet { get; set; }
        private bool SweepDecrease { get; set; }
        private int SweepShift { get; set; }
        private int SweepOld { get; set; }
        private bool SweepEnabled { get; set; }

        private void Reset()
        {
            Length = 0;
            SequencePointer = 0;
            Enabled = false;
            Duty = 0;
            LengthEnabled = false;
            FrequencyTimer = 0;
            Volume = 0;
            SweepTime = 0;
            SweepDecrease = false;
            SweepShift = 0;
            SweepEnabled = false;
            SweepTimeSet = 0;
            SweepOld = 0;
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

        internal void UpdateSweep()
        {
            if(--SweepTime <= 0)
            {
                SweepTime = SweepTimeSet;
                if (SweepTime == 0) SweepTime = 8;
                if(SweepTimeSet > 0 && SweepEnabled)
                {
                    int newFrequency = CalculateSweep();
                    if(newFrequency <= 2047 && SweepShift > 0)
                    {
                        SweepOld = newFrequency;
                        Frequency = newFrequency;
                    }
                }
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
                    SweepShift = value & 0x07;
                    SweepDecrease = Bitwise.IsBitOn(value, 3);
                    SweepTimeSet = (value & 0x70) >> 4;
                    return value;

                case 0xFF11:
                    Duty = (value >> 6);
                    Length = (value & 0x3F);
                    return value;

                case 0xFF12:
                    Volume = (value >> 4);
                    EnvelopeAdd = Bitwise.IsBitOn(value, 3);
                    EnvelopeTime = value & 0x07;
                    EnvelopeTimeSet = EnvelopeTime;
                    return value;

                case 0xFF13:
                    Frequency = (Frequency & 0x700) | value;
                    return -1;

                case 0xFF14:
                    Frequency = ((value & 0x7) << 8) | (Frequency & 0xFF);
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

            SweepOld = Frequency;
            SweepTime = SweepTimeSet;
            if (SweepTime == 0) SweepTime = 8;
            SweepEnabled = SweepShift > 0 || SweepTime > 0;
        }

        private int CalculateSweep()
        {
            int returnFrequency = SweepOld >> SweepShift;
            if (SweepDecrease) returnFrequency = SweepOld - returnFrequency;
            else returnFrequency += SweepOld;

            return returnFrequency;
        }
    }
}
