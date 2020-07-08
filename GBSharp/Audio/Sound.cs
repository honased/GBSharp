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
        private const int SamplesPerBuffer = 2048;
        private const int SampleRate = 44100;
        private float[,] _workingBuffer;
        private byte[] _monoBuffer;
        private int _bufferPos;

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
            float vol = volume / 15.0f;
            if(_bufferPos < SamplesPerBuffer)
            {
                _workingBuffer[0, _bufferPos] = vol;
                _workingBuffer[1, _bufferPos] = vol;
            }

            _bufferPos++;
        }

        internal bool BufferFilled()
        {
            return _bufferPos >= SamplesPerBuffer;
        }

        private void SubmitBuffer()
        {
            _bufferPos = 0;

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
