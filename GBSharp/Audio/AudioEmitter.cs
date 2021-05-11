// Adapted from code written by David Gouveia at https://www.david-gouveia.com/creating-a-basic-synth-in-xna-part-ii

using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class AudioEmitter
    {
        private DynamicSoundEffectInstance _instance;
        private const int ChannelsCount = 2;
        private const int SamplesPerBuffer = 2048;
        private const int SampleRate = 44100;
        private float[,] _workingBuffer;
        private byte[] _monoBuffer;
        private int _bufferPos;

        public AudioEmitter()
        {
            _instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Stereo);
            _workingBuffer = new float[ChannelsCount, SamplesPerBuffer];
            const int bytesPerSample = 2;
            _monoBuffer = new byte[ChannelsCount * SamplesPerBuffer * bytesPerSample];

            _instance.Volume = 0.2f;

            _instance.Play();
            _bufferPos = 0;
        }

        ~AudioEmitter()
        {
            _instance.Dispose();
        }

        internal void AddVolumeInfo(int volume, int leftVolume, int rightVolume)
        {
            float vol = volume / 15.0f;
            if(_bufferPos < SamplesPerBuffer)
            {
                _workingBuffer[0, _bufferPos] = vol * (leftVolume / 7.0f);
                _workingBuffer[1, _bufferPos] = vol * (rightVolume / 7.0f);
            }

            _bufferPos++;

            if (BufferFilled()) SubmitBuffer();
        }

        internal bool BufferFilled()
        {
            return _bufferPos >= SamplesPerBuffer;
        }

        private void SubmitBuffer()
        {
            _bufferPos = 0;

            ConvertBuffer(_workingBuffer, _monoBuffer);
            if(_instance.PendingBufferCount < 3) _instance.SubmitBuffer(_monoBuffer);
        }

        internal int GetPendingBufferCount()
        {
            return _instance.PendingBufferCount;
        }

        private static void ConvertBuffer(float[,] from, byte[] to)
        {
            const int bytesPerSample = 2;
            int channels = from.GetLength(0);
            int bufferSize = from.GetLength(1);

            for (int i = 0; i < bufferSize; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    float floatSample = from[c, i];

                    // Clamp float sample between -1.0 and 1.0
                    if (floatSample < -1.0f) floatSample = 1.0f;
                    else if (floatSample > 1.0f) floatSample = 1.0f;

                    short shortSample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);
                    int index = i * channels * bytesPerSample + c * bytesPerSample;
                    if (!BitConverter.IsLittleEndian)
                    {
                        to[index] = (byte)(shortSample >> 8);
                        to[index + 1] = (byte)shortSample;
                    }
                    else
                    {
                        to[index] = (byte)shortSample;
                        to[index + 1] = (byte)(shortSample >> 8);
                    }
                }
            }
        }

    }
}
