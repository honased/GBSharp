using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class MMU
    {
        private int[] _memoryBank;
        public const int MEMORY_SIZE = 0xFFFF;

        public MMU()
        {
            _memoryBank = new int[MEMORY_SIZE];
        }

        public void WriteBytes(int[] bytes, int address)
        {
            Array.Copy(bytes, 0, _memoryBank, address, bytes.Length);
        }

        public void WriteByte(int value, int address)
        {
            _memoryBank[address] = value;
        }

        public int ReadByte(int address)
        {
            switch(address & 0xF000)
            {
                // Bios / Rom0
                case 0x0000:
                    return _memoryBank[address];

                // Rom0
                case 0x1000: case 0x2000: case 0x3000:
                    return _memoryBank[address];

                // Rom1 (16k)
                case 0x4000: case 0x5000: case 0x6000: case 0x7000:
                    return _memoryBank[address];

                // VRAM (8k)
                case 0x8000: case 0x9000:
                    return _memoryBank[address];
            }
            return _memoryBank[address];
        }
    }
}
