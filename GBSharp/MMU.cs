using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class MMU
    {
        private byte[] _memoryBank;
        public const int MEMORY_SIZE = 0xFFFF;

        public MMU()
        {
            _memoryBank = new byte[MEMORY_SIZE];
        }

        public void WriteBytes(byte[] bytes, int position)
        {
            Array.Copy(bytes, 0, _memoryBank, position, bytes.Length);
        }

        public byte ReadByte(int position)
        {
            return _memoryBank[position];
        }
    }
}
