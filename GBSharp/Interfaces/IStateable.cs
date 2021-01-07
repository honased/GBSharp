using System.IO;

namespace GBSharp.Interfaces
{
    interface IStateable
    {
        void SaveState(BinaryWriter stream);
        void LoadState(BinaryReader stream);
    }
}
