using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    abstract class AudioChannel
    {
        protected int Length { get; set; }
        protected int LengthSet { get; set; }
        protected int Frequency { get; set; }
        protected int FrequencyTimer { get; set; }
        protected bool Enabled { get; set; }
        protected bool DAC { get; set; }
        protected int SequencePointer { get; set; }
        protected bool LengthEnabled { get; set; }
        protected int Volume { get; set; }
        protected int VolumeSet { get; set; }
        protected int OutputVolume { get; set; }
        protected bool EnvelopeAdd { get; set; }
        protected int EnvelopeTime { get; set; }
        protected int EnvelopeTimeSet { get; set; }
        protected bool EnvelopeEnabled { get; set; }

        internal AudioEmitter Emitter { get; private set; }

        public AudioChannel()
        {
            Reset();
        }

        internal void Reset()
        {
            Length = 0;
            LengthSet = 0;
            Frequency = 0;
            FrequencyTimer = 0;
            Enabled = false;
            DAC = false;
            SequencePointer = 0;
            LengthEnabled = false;
            Volume = 0;
            VolumeSet = 0;
            OutputVolume = 0;
            EnvelopeAdd = false;
            EnvelopeTime = 0;
            EnvelopeTimeSet = 0;
            EnvelopeEnabled = false;
            Emitter = new AudioEmitter();

            CustomReset();
        }

        internal void UpdateLength()
        {
            if (!Enabled || !DAC) return;

            if (LengthEnabled && Length > 0)
            {
                if (--Length == 0)
                {
                    Enabled = false;
                }
            }
        }

        internal void UpdateEnvelope()
        {
            if (!Enabled || !DAC) return;

            if (--EnvelopeTime <= 0)
            {
                EnvelopeTime = EnvelopeTimeSet;
                if (EnvelopeTime == 0) EnvelopeTime = 8;

                if (EnvelopeEnabled && EnvelopeTimeSet > 0)
                {
                    if (EnvelopeAdd)
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

        internal int GetVolume()
        {
            return OutputVolume;
        }

        internal bool IsPlaying()
        {
            return Length > 0;
        }

        protected void WriteEnvelope(int value)
        {
            VolumeSet = (value >> 4);
            EnvelopeAdd = Bitwise.IsBitOn(value, 3);
            EnvelopeTimeSet = value & 0x07;
            DAC = (value & 0xF8) != 0;
        }

        protected int ReadEnvelope()
        {
            return (EnvelopeTimeSet & 0x7) | ((EnvelopeAdd ? 1 : 0) << 3) | ((VolumeSet & 0xF) << 4);
        }

        internal void AddVolumeInfo(int leftVolume, int rightVolume)
        {
            Emitter.AddVolumeInfo(OutputVolume, leftVolume, rightVolume);
        }

        protected abstract void CustomReset();

        internal abstract void WriteByte(int address, int value);

        internal abstract int ReadByte(int address);

        internal abstract void Enable();

        internal abstract void Tick();
    }
}
