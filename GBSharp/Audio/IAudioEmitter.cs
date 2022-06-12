namespace GBSharp.Audio
{
    public interface IAudioEmitter
    {
        void AddVolumeInfo(int source, int volume, int leftVolume, int rightVolume);
        int GetPendingBufferCount();

        void Close();
    }
}
