using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Audio
{
    class Sound
    {
        private DynamicSoundEffectInstance _instance;
        private const int ChannelsCount = 2;
        private const int SamplesPerBuffer = 739;
        private const int SampleRate = 44100;
        private float[,] _workingBuffer;
        private byte[] _monoBuffer;
        private int _bufferPos;
        private double time;

        public Sound()
        {
            _instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Stereo);
            _workingBuffer = new float[ChannelsCount, SamplesPerBuffer];
            const int bytesPerSample = 2;
            _monoBuffer = new byte[ChannelsCount * SamplesPerBuffer * bytesPerSample];

            _instance.Play();
            _bufferPos = 0;
        }

        internal void AddVolumeInfo(int volume)
        {
            if(_bufferPos < SamplesPerBuffer)
            {
                _workingBuffer[0, _bufferPos] = volume;
                _workingBuffer[1, _bufferPos] = volume;
                time += 1.0 / SampleRate;
            }

            _bufferPos++;
        }

        private void SubmitBuffer()
        {
            _bufferPos = 0;

            /*for (int i = 0; i < SamplesPerBuffer; i++)
            {
                // Here is where you sample your wave function
                _workingBuffer[0, i] = (float)((SineWave(time, 440) > 00) ? 0.1 : 0); // Left Channel
                _workingBuffer[1, i] = _workingBuffer[0, i]; // Right Channel

                // Advance time passed since beginning
                // Since the amount of samples in a second equals the chosen SampleRate
                // Then each sample should advance the time by 1 / SampleRate
                time += 1.0 / SampleRate;
            }*/

            SoundHelper.ConvertBuffer(_workingBuffer, _monoBuffer);
            _instance.SubmitBuffer(_monoBuffer);
        }

        private double SineWave(double time ,double frequency)
        {
            return Math.Sin(time * 2 * Math.PI * frequency);
        }

        internal void Update()
        {
            SubmitBuffer();
        }
    }
}
