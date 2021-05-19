using GBSharp.Audio;
using System;
using OpenTK.Audio.OpenAL;
using System.Collections.Concurrent;
using System.Threading;

namespace GBTK
{   
    class AudioSource
    {
        public const int CHANNELS_COUNT = 2;
        public const int SAMPLES_PER_BUFFER = 2048;
        public const int SAMPLE_RATE = 44100;
        public const int MAX_BUFFER_COUNT = 6;
        private int _bufferPos;
        public int Source { get; private set; }
        private ConcurrentQueue<int> _buffers;
        private float[,] _workingBuffer;
        private byte[] _monoBuffer;
        private Thread updateThread;

        public AudioSource()
        {
            _workingBuffer = new float[CHANNELS_COUNT, SAMPLES_PER_BUFFER];
            const int bytesPerSample = 2;
            _monoBuffer = new byte[CHANNELS_COUNT * SAMPLES_PER_BUFFER * bytesPerSample];

            Source = AL.GenSource();

            _buffers = new ConcurrentQueue<int>();
            for(int i = 0; i < MAX_BUFFER_COUNT; i++)
            {
                _buffers.Enqueue(AL.GenBuffer());
            }

            _bufferPos = 0;

            updateThread = new Thread(new ThreadStart(Update));

            updateThread.Start();

            Console.WriteLine("Created");
        }

        public bool BufferFilled()
        {
            return _bufferPos >= SAMPLES_PER_BUFFER;
        }

        public void AddVolumeInfo(int volume, int leftVolume, int rightVolume)
        {
            float vol = volume / 15.0f;
            if (_bufferPos < SAMPLES_PER_BUFFER)
            {
                _workingBuffer[0, _bufferPos] = vol * (leftVolume / 7.0f);
                _workingBuffer[1, _bufferPos] = vol * (rightVolume / 7.0f);
            }

            _bufferPos++;

            if (BufferFilled()) SubmitBuffer();
        }

        private void SubmitBuffer()
        {
            _bufferPos = 0;

            ConvertBuffer(_workingBuffer, _monoBuffer);

            if (_buffers.Count <= 0) throw new Exception("Buffers length is 0");
            _buffers.TryDequeue(out int buf);
            AL.BufferData(buf, ALFormat.Stereo16, _monoBuffer, SAMPLE_RATE);
            AL.SourceQueueBuffer(Source, buf);

            Play();
        }

        private void Play()
        {
            AL.GetSource(Source, ALGetSourcei.SourceState, out int state);

            ALSourceState actualState = (ALSourceState)state;

            if(actualState != ALSourceState.Playing)
            {
                AL.SourcePlay(Source);
            }
        }

        private void Update()
        {
            while (true)
            {
                AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out int processedCount);

                while (processedCount-- > 0)
                {
                    _buffers.Enqueue(AL.SourceUnqueueBuffer(Source));
                }

                Thread.Sleep(10);
            }
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

        public int GetPendingBufferCount()
        {
            return Math.Max(MAX_BUFFER_COUNT - _buffers.Count - 1, 0);
        }
    }

    class AudioEmitter : IAudioEmitter
    {
        private const int SOURCE_COUNT = 4;
        private AudioSource[] sources;

        public AudioEmitter()
        {
            sources = new AudioSource[SOURCE_COUNT];

            // Initialize sources
            for(int i = 0; i < SOURCE_COUNT; i++)
            {
                sources[i] = new AudioSource();
                AL.Source(sources[i].Source, ALSourcef.Gain, 0.1f);
            }
        }

        ~AudioEmitter()
        {
           //_instance.Dispose();
        }

        public void AddVolumeInfo(int source, int volume, int leftVolume, int rightVolume)
        {
            sources[source].AddVolumeInfo(volume, leftVolume, rightVolume);
        }

        public int GetPendingBufferCount()
        {
            return sources[0].GetPendingBufferCount();
        }

        
    }
}
