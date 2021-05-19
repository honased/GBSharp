using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class WaveChannel : AudioChannel
    {
        private int[] Samples { get; set; }

        protected override void CustomReset()
        {
            Samples = new int[0x10];
        }

        public WaveChannel(Gameboy gameboy, int source) : base(gameboy, source) { }

        internal override void WriteByte(int address, int value)
        {
            switch (address)
            {
                case 0xFF1A:
                    DAC = Bitwise.IsBitOn(value, 7);
                    return;

                case 0xFF1B:
                    LengthSet = value;
                    return;

                case 0xFF1C:
                    Volume = (value >> 5) & 0x03;
                    return;

                case 0xFF1D:
                    Frequency = (Frequency & 0x700) | value;
                    return;

                case 0xFF1E:
                    Frequency = ((value & 0x7) << 8) | (Frequency & 0xFF);
                    LengthEnabled = Bitwise.IsBitOn(value, 6);
                    if (Bitwise.IsBitOn(value, 7)) Enable();
                    return;

                case int _ when address >= 0xFF30 && address <= 0xFF3f:
                    Samples[address - 0xFF30] = value;
                    return;
            }

            throw new InvalidOperationException(String.Format("Cannot write to memory address 0x{0:X4}", address));
        }

        internal override int ReadByte(int address)
        {
            switch (address)
            {
                case 0xFF1A:
                    return (DAC ? 1 : 0) << 7;

                case 0xFF1B:
                    return LengthSet;

                case 0xFF1C:
                    return (Volume << 5);

                case 0xFF1D:
                    return 0x00;

                case 0xFF1E:
                    return (LengthEnabled ? 1 : 0) << 6;

                case int _ when address >= 0xFF30 && address <= 0xFF3f:
                    return Samples[address - 0xFF30];
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
                FrequencyTimer = (2048 - Frequency) * 2;
                SequencePointer = (SequencePointer + 1) & 0x1F;

                int position = SequencePointer / 2;
                int outputByte = Samples[position];
                if ((SequencePointer & 0x1) == 0) outputByte >>= 4;
                outputByte &= 0xF;

                if (Volume > 0)
                {
                    outputByte >>= Volume - 1;
                }
                else outputByte = 0;
                OutputVolume = outputByte;
            }
        }

        internal override void Enable()
        {
            Enabled = true;
            FrequencyTimer = (2048 - Frequency) * 2;

            Length = 256 - LengthSet;

            if (Length == 0) Length = 256;

            SequencePointer = 0;
        }

        protected override void CustomSaveState(BinaryWriter stream)
        {
            for (int i = 0; i < Samples.Length; i++) stream.Write(Samples[i]);
        }

        protected override void CustomLoadState(BinaryReader stream)
        {
            for (int i = 0; i < Samples.Length; i++) Samples[i] = stream.ReadInt32();
        }
    }
}
