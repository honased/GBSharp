using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public static class Bitwise
    {
        public static bool IsBitOn(int value, int bit)
        {
            return ((value >> bit) & 0x01) == 0x01;
        }

        public static int SetBit(int value, int bit)
        {
            return value | (1 << bit);
        }

        public static int ClearBit(int value, int bit)
        {
            return value & ~(1 << bit);
        }
    }
}
