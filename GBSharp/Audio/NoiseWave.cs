using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class NoiseWave
    {
        private int Length { get; set; }
        private int LengthSet { get; set; }
        private int FrequencyTimer { get; set; }
        private bool Enabled { get; set; }
        private bool LengthEnabled { get; set; }
        private int Volume { get; set; }
        private int VolumeSet { get; set; }
        private int OutputVolume { get; set; }
        private bool EnvelopeAdd { get; set; }
        private int EnvelopeTime { get; set; }
        private int EnvelopeTimeSet { get; set; }
        private bool EnvelopeEnabled { get; set; }
        private int ShiftFrequency { get; set; }
        private bool ShiftWidth { get; set; }
        private bool DAC { get; set; }
        private int DividingRatio { get; set; }

        private int LFSR { get; set; }

        private int[] Dividers { get; set; } = new int[] { 8, 16, 32, 48, 64, 80, 96, 112 };

        private bool Trigger;

        internal Sound Emitter { get; private set; }

        public NoiseWave()
        {
            Reset();
        }

        private void Reset()
        {
            Length = 0;
            LengthSet = 0;
            Enabled = false;
            LengthEnabled = false;
            FrequencyTimer = 0;
            Volume = 0;
            VolumeSet = 0;
            LFSR = 0;
            DAC = false;
            ShiftWidth = false;
            Trigger = false;

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
            if(!Enabled || !DAC)
            {
                OutputVolume = 0;
                return;
            }

            if (FrequencyTimer-- <= 0)
            {
                FrequencyTimer = Dividers[DividingRatio] << ShiftFrequency;    // odd

                int result = (LFSR & 0x1) ^ ((LFSR >> 1) & 0x1);
                LFSR >>= 1;
                LFSR |= result << 14;
                if (ShiftWidth)
                {
                    LFSR &= ~0x40;
                    LFSR |= result << 6;
                }
                if ((LFSR & 0x1) == 0)
                {
                    OutputVolume = Volume;
                }
                else
                {
                    OutputVolume = 0;
                }
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
                case 0xFF20:
                    LengthSet = (value & 0x3F);
                    return value;

                case 0xFF21:
                    DAC = (value & 0xF8) != 0;
                    VolumeSet = (value >> 4) & 0xF;
                    EnvelopeAdd = Bitwise.IsBitOn(value, 3);
                    //EnvelopeTime = value & 0x07;
                    EnvelopeTimeSet = value & 0x07;
                    return value;

                case 0xFF22:
                    DividingRatio = value & 0x07;
                    ShiftWidth = Bitwise.IsBitOn(value, 3);
                    ShiftFrequency = (value >> 4) & 0xF;
                    return value;

                case 0xFF23:
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    Trigger = Bitwise.IsBitOn(value, 7);
                    if (Trigger) Enable();
                    return value;
            }

            throw new InvalidOperationException("Cannot write to that memory address");
        }

        internal int ReadByte(int address, int[] memory)
        {
            switch (address)
            {
                case 0xFF20:
                    return LengthSet & 0x3F;

                case 0xFF21:
                    return (EnvelopeTimeSet & 0x7) | ((EnvelopeAdd ? 1 : 0) << 3) | ((VolumeSet & 0xF) << 4);

                case 0xFF22:
                    return (DividingRatio) | ((ShiftWidth ? 1 : 0) << 3) | (ShiftFrequency << 4);

                case 0xFF23:
                    return ((LengthEnabled ? 1 : 0) << 6) | ((Trigger ? 1 : 0) << 7);
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
            EnvelopeEnabled = true;
            EnvelopeTime = EnvelopeTimeSet;

            Volume = VolumeSet;

            FrequencyTimer = Dividers[DividingRatio] << ShiftFrequency;

            LFSR = 0x7FFF;

            Length = 64 - LengthSet;

            if (Length == 0) Length = 64;
        }
    }
}
