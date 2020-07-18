using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class SquareChannel : AudioChannel
    {
        private static readonly int[] DUTY_CYCLES =
            {0,0,0,0,0,0,0,1,
             1,0,0,0,0,0,0,1,
             1,0,0,0,0,1,1,1,
             0,1,1,1,1,1,1,0};

        private int Duty { get; set; }
        private int SweepTime { get; set; }
        private int SweepTimeSet { get; set; }
        private bool SweepDecrease { get; set; }
        private int SweepShift { get; set; }
        private int SweepOld { get; set; }
        private bool SweepEnabled { get; set; }

        protected override void CustomReset()
        {
            Duty = 0;
            SweepTime = 0;
            SweepTimeSet = 0;
            SweepDecrease = false;
            SweepShift = 0;
            SweepOld = 0;
            SweepEnabled = false;
        }

        internal override void WriteByte(int address, int value)
        {
            switch (address)
            {
                case 0xFF10:
                    SweepShift = value & 0x07;
                    SweepDecrease = Bitwise.IsBitOn(value, 3);
                    SweepTimeSet = (value & 0x70) >> 4;
                    return;

                case 0xFF11:
                case 0xFF16:
                    Duty = (value >> 6);
                    LengthSet = 64 - (value & 0x3F);
                    return;

                case 0xFF12:
                case 0xFF17:
                    WriteEnvelope(value);
                    return;

                case 0xFF13:
                case 0xFF18:
                    Frequency = (Frequency & 0x700) | value;
                    return;

                case 0xFF14:
                case 0xFF19:
                    Frequency = ((value & 0x7) << 8) | (Frequency & 0xFF);
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    if (Bitwise.IsBitOn(value, 7)) Enable();
                    return;

                case 0xFF15:
                    return;
            }

            throw new InvalidOperationException(String.Format("Cannot write to memory address 0x{0:X4}", address));
        }

        internal override int ReadByte(int address)
        {
            switch (address)
            {
                case 0xFF10:
                    return (SweepTimeSet << 4) | ((SweepDecrease ? 1 : 0) << 3) | (SweepShift & 0x07);

                case 0xFF11:
                case 0xFF16:
                    return (Duty << 6) | (Length & 0x3F);

                case 0xFF12:
                case 0xFF17:
                    return ReadEnvelope();

                case 0xFF13:
                case 0xFF18:
                    return 0x00;

                case 0xFF14:
                case 0xFF19:
                    return (LengthEnabled ? 1 : 0) << 6;
            }

            throw new InvalidOperationException(String.Format("Cannot read from memory address 0x{0:X4}", address));
        }

        internal override void Tick()
        {
            if (!Enabled || !DAC)
            {
                OutputVolume = 0;
                return;
            }

            if (--FrequencyTimer <= 0)
            {
                FrequencyTimer = (2048 - Frequency) * 4;
                SequencePointer = (SequencePointer + 1) % 8;
            }

            OutputVolume = DUTY_CYCLES[(Duty * 8) + SequencePointer] * Volume;
        }

        internal override void Enable()
        {
            Enabled = true;
            FrequencyTimer = (2048 - Frequency) * 4;
            EnvelopeEnabled = true;

            Length = 64 - LengthSet;

            if (Length == 0) Length = 64;

            Volume = VolumeSet;

            SweepOld = Frequency;
            SweepTime = SweepTimeSet;
            if (SweepTime == 0) SweepTime = 8;
            SweepEnabled = SweepShift > 0 || SweepTime > 0;
        }

        internal void UpdateSweep()
        {
            if (!Enabled || !DAC) return;

            if (--SweepTime <= 0)
            {
                SweepTime = SweepTimeSet;
                if (SweepTime == 0) SweepTime = 8;
                if (SweepTimeSet > 0 && SweepEnabled)
                {
                    int newFrequency = CalculateSweep();
                    if (newFrequency <= 2047 && SweepShift > 0)
                    {
                        SweepOld = newFrequency;
                        Frequency = newFrequency;
                    }
                }
            }
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
