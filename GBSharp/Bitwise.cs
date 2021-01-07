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

        public static int Wrap8(int value)
        {
            return (value + 256) % 256;
        }

        public static int Wrap16(int value)
        {
            return (value + 65536) % 65536;
        }
    }
}
