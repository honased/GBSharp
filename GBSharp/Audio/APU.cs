using GBSharp.Interfaces;
using System;
using System.IO;

namespace GBSharp.Audio
{
    public class APU : IStateable
    {
        private const int FRAME_SEQUENCER_CLOCKS = 8192;
        private const int SAMPLE_GOAL = 95;
        private int FrameSequencer { get; set; }
        private int totalClocks;
        private int TotalSamples { get; set; }

        private bool[,] OutputSound { get; set; }
        private int VolumeLeft { get; set; }
        private int VolumeRight { get; set; }
        private bool OutputVinSO1 { get; set; }
        private bool OutputVinSO2 { get; set; }

        private SquareChannel squareChannel;
        private SquareChannel squareChannel2;
        private WaveChannel waveChannel;
        private NoiseChannel noiseChannel;

        private readonly Gameboy _gameboy;

        private bool On { get; set; }

        public APU(Gameboy gameboy)
        {
            _gameboy = gameboy;
            Reset();
        }

        internal void Reset()
        {
            FrameSequencer = 0;
            totalClocks = 0;
            TotalSamples = 0;

            squareChannel = new SquareChannel();
            squareChannel2 = new SquareChannel();
            waveChannel = new WaveChannel();
            noiseChannel = new NoiseChannel();

            OutputSound = new bool[2, 4];
            VolumeLeft = 0;
            VolumeRight = 0;
            On = false;

            OutputVinSO1 = false;
            OutputVinSO2 = false;
        }

        public void WriteByte(int address, int value)
        {
            if (address == 0xFF26) On = Bitwise.IsBitOn(value, 7);
            if (!On) return;

            if (address <= 0xFF14) squareChannel.WriteByte(address, value);
            else if (address <= 0xFF19) squareChannel2.WriteByte(address, value);
            else if (address <= 0xFF1E) waveChannel.WriteByte(address, value);
            else if (address >= 0xFF20 && address <= 0xFF23) noiseChannel.WriteByte(address, value);
            else if (address <= 0xFF26)
            {
                switch (address)
                {
                    case 0xFF24:
                        VolumeLeft = (value >> 4) & 0x07;
                        VolumeRight = value & 0x07;
                        OutputVinSO2 = Bitwise.IsBitOn(value, 7);
                        OutputVinSO1 = Bitwise.IsBitOn(value, 3);
                        break;

                    case 0xFF25:
                        for (int i = 0; i < 8; i++)
                        {
                            OutputSound[i / 4, i % 4] = Bitwise.IsBitOn(value, i);
                        }
                        break;
                }
            }
            else if (address >= 0xFF30 && address <= 0xFF3F) waveChannel.WriteByte(address, value);
        }

        public int ReadByte(int address, int[] memory)
        {
            if (address <= 0xFF14) return squareChannel.ReadByte(address);
            else if (address <= 0xFF19) return squareChannel2.ReadByte(address);
            else if (address <= 0xFF1E) return waveChannel.ReadByte(address);
            else if (address >= 0xFF20 && address <= 0xFF23) return noiseChannel.ReadByte(address);
            else if(address == 0xFF24)
            {
                return ((OutputVinSO2 ? 1 : 0) << 7) | ((OutputVinSO1 ? 1 : 0) << 3)
                        | ((VolumeLeft & 0x07) << 4) | (VolumeRight & 0x07);
            }
            else if(address == 0xFF25)
            {
                int output = 0;
                for (int i = 0; i < 8; i++)
                {
                    output |= ((OutputSound[i / 4, i % 4] ? 1 : 0) << i);
                }

                return output;
            }
            else if (address == 0xFF26)
            {
                int returnMem = memory[address - 0xFF00] & 0x80;
                returnMem |= (squareChannel.IsPlaying() ? 1 : 0);
                returnMem |= (squareChannel2.IsPlaying() ? 1 : 0) << 1;
                returnMem |= (waveChannel.IsPlaying() ? 1 : 0) << 2;
                returnMem |= (noiseChannel.IsPlaying() ? 1 : 0) << 3;
                returnMem |= (On ? 1 : 0) << 7;
                return returnMem;
            }

            return memory[address - 0xFF00];
        }

        public void Tick(int clocks)
        {
            //clocks *= 4;
            while (clocks-- > 0)
            {
                totalClocks += 1;
                if (totalClocks >= FRAME_SEQUENCER_CLOCKS)
                {
                    totalClocks -= FRAME_SEQUENCER_CLOCKS;
                    switch (FrameSequencer)
                    {
                        case 0:
                            squareChannel.UpdateLength();
                            squareChannel2.UpdateLength();
                            waveChannel.UpdateLength();
                            noiseChannel.UpdateLength();
                            break;

                        case 1:
                            break;

                        case 2:
                            squareChannel.UpdateLength();
                            squareChannel.UpdateSweep();

                            squareChannel2.UpdateLength();
                            waveChannel.UpdateLength();
                            noiseChannel.UpdateLength();
                            break;

                        case 3:
                            break;

                        case 4:
                            squareChannel.UpdateLength();
                            squareChannel2.UpdateLength();
                            waveChannel.UpdateLength();
                            noiseChannel.UpdateLength();
                            break;

                        case 5:
                            break;

                        case 6:
                            squareChannel.UpdateLength();
                            squareChannel.UpdateSweep();

                            squareChannel2.UpdateLength();
                            waveChannel.UpdateLength();
                            noiseChannel.UpdateLength();
                            break;

                        case 7:
                            squareChannel.UpdateEnvelope();
                            squareChannel2.UpdateEnvelope();
                            noiseChannel.UpdateEnvelope();
                            break;

                        default:
                            throw new InvalidOperationException("Frame Sequencer can not be this value: " + FrameSequencer.ToString());
                    }
                    FrameSequencer = (FrameSequencer + 1) % 8;
                }

                squareChannel.Tick();
                squareChannel2.Tick();
                waveChannel.Tick();
                noiseChannel.Tick();

                if (++TotalSamples >= SAMPLE_GOAL)
                {
                    TotalSamples -= SAMPLE_GOAL;

                    squareChannel.AddVolumeInfo(VolumeLeft * (OutputSound[1, 0] ? 1 : 0), VolumeRight * (OutputSound[0, 0] ? 1 : 0));
                    squareChannel2.AddVolumeInfo(VolumeLeft * (OutputSound[1, 1] ? 1 : 0), VolumeRight * (OutputSound[0, 1] ? 1 : 0));
                    waveChannel.AddVolumeInfo(VolumeLeft * (OutputSound[1, 2] ? 1 : 0), VolumeRight * (OutputSound[0, 2] ? 1 : 0));
                    noiseChannel.AddVolumeInfo(VolumeLeft * (OutputSound[1, 3] ? 1 : 0), VolumeRight * (OutputSound[0, 3] ? 1 : 0));
                }
            }
        }

        public void SaveState(BinaryWriter stream)
        {
            stream.Write(FrameSequencer);
            stream.Write(totalClocks);
            stream.Write(TotalSamples);

            int iLen = OutputSound.GetLength(0);
            int jLen = OutputSound.GetLength(1);
            for (int i = 0; i < iLen; i++)
            {
                for(int j = 0; j < jLen; j++)
                {
                    stream.Write(OutputSound[i, j]);
                }
            }

            stream.Write(VolumeLeft);
            stream.Write(VolumeRight);
            stream.Write(OutputVinSO1);
            stream.Write(OutputVinSO2);
            stream.Write(On);
            squareChannel.SaveState(stream);
            squareChannel2.SaveState(stream);
            waveChannel.SaveState(stream);
            noiseChannel.SaveState(stream);
        }

        public void LoadState(BinaryReader stream)
        {
            FrameSequencer = stream.ReadInt32();
            totalClocks = stream.ReadInt32();
            TotalSamples = stream.ReadInt32();

            int iLen = OutputSound.GetLength(0);
            int jLen = OutputSound.GetLength(1);
            for (int i = 0; i < iLen; i++)
            {
                for (int j = 0; j < jLen; j++)
                {
                    OutputSound[i, j] = stream.ReadBoolean();
                }
            }

            VolumeLeft = stream.ReadInt32();
            VolumeRight = stream.ReadInt32();
            OutputVinSO1 = stream.ReadBoolean();
            OutputVinSO2 = stream.ReadBoolean();
            On = stream.ReadBoolean();
            squareChannel.LoadState(stream);
            squareChannel2.LoadState(stream);
            waveChannel.LoadState(stream);
            noiseChannel.LoadState(stream);
        }

        internal int GetPendingBufferCount()
        {
            return squareChannel.GetPendingBufferCount();
        }
    }
}
