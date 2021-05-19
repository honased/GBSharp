using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class NoiseChannel : AudioChannel
    {
        private int ShiftFrequency { get; set; }
        private bool ShiftWidth { get; set; }
        private int DividingRatio { get; set; }

        private int LFSR { get; set; }

        private static readonly int[] Dividers = new int[] { 8, 16, 32, 48, 64, 80, 96, 112 };

        public NoiseChannel(Gameboy gameboy, int source) : base(gameboy, source) { }

        protected override void CustomReset()
        {
            ShiftFrequency = 0;
            ShiftWidth = false;
            DividingRatio = 0;
            LFSR = 0;
        }

        internal override void WriteByte(int address, int value)
        {
            switch (address)
            {
                case 0xFF20:
                    LengthSet = (value & 0x3F);
                    return;

                case 0xFF21:
                    WriteEnvelope(value);
                    return;

                case 0xFF22:
                    DividingRatio = value & 0x07;
                    ShiftWidth = Bitwise.IsBitOn(value, 3);
                    ShiftFrequency = (value >> 4) & 0xF;
                    return;

                case 0xFF23:
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    if (Bitwise.IsBitOn(value, 7)) Enable();
                    return;
            }

            throw new InvalidOperationException(String.Format("Cannot write to memory address 0x{0:X4}", address));
        }

        internal override int ReadByte(int address)
        {
            switch (address)
            {
                case 0xFF20:
                    return LengthSet & 0x3F;

                case 0xFF21:
                    return ReadEnvelope();

                case 0xFF22:
                    return (ShiftFrequency << 4) | ((ShiftWidth ? 1 : 0) << 3) | (DividingRatio & 0x07);

                case 0xFF23:
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

        internal override void Enable()
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

        protected override void CustomSaveState(BinaryWriter stream)
        {
            stream.Write(ShiftFrequency);
            stream.Write(ShiftWidth);
            stream.Write(DividingRatio);
            stream.Write(LFSR);
        }

        protected override void CustomLoadState(BinaryReader stream)
        {
            ShiftFrequency = stream.ReadInt32();
            ShiftWidth = stream.ReadBoolean();
            DividingRatio = stream.ReadInt32();
            LFSR = stream.ReadInt32();
        }
    }
}
