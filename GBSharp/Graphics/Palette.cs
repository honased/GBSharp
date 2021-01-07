using GBSharp.Interfaces;
using System;
using System.IO;

namespace GBSharp.Graphics
{
    public struct GBColor
    {
        public int R;
        public int G;
        public int B;

        public GBColor(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class PaletteEntry : IStateable
    {
        public GBColor[] Colors { get; set; }

        private int[] red, green, blue;

        public int PaletteIndexAddress { get; private set; }
        public int PaletteDataAddress { get; private set; }

        public PaletteEntry(bool isBackround)
        {
            red = new int[4];
            green = new int[4];
            blue = new int[4];
            Colors = new GBColor[4];

            PaletteDataAddress = isBackround ? 0xFF69 : 0xFF6B;
            PaletteIndexAddress = isBackround ? 0xFF68 : 0xFF6A;

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < red.Length; i++) red[i] = 0;
            for (int i = 0; i < green.Length; i++) green[i] = 0;
            for (int i = 0; i < blue.Length; i++) blue[i] = 0;
        }

        public void UpdateCGB(MMU mmu, int value)
        {
            int register = mmu.ReadByte(PaletteIndexAddress);
            bool increment = Bitwise.IsBitOn(register, 7);
            int index = register & 0x3F;

            int colorToModify = (index % 8) / 2;
            if ((index % 8) % 2 == 0)
            {
                red[colorToModify] = value & 0x1F;
                green[colorToModify] = (green[colorToModify] & 0x18) | (value >> 5);
            }
            else
            {
                green[colorToModify] = (green[colorToModify] & 0x07) | ((value & 0x03) << 3);
                blue[colorToModify] = (value >> 2) & 0x1F;
            }

            Colors[colorToModify].R = (int)((red[colorToModify] / 31.0) * 255);
            Colors[colorToModify].G = (int)((green[colorToModify] / 31.0) * 255);
            Colors[colorToModify].B = (int)((blue[colorToModify] / 31.0) * 255);

            if (increment)
            {
                index = (index + 1) % 64;
                mmu.WriteByte(index | (0x80), PaletteIndexAddress);
            }
        }

        public void UpdateDMG(int value)
        {
            Colors[0] = GetDMGColor(value & 0x03);
            Colors[1] = GetDMGColor((value >> 2) & 0x03);
            Colors[2] = GetDMGColor((value >> 4) & 0x03);
            Colors[3] = GetDMGColor((value >> 6) & 0x03);
        }

        private GBColor GetDMGColor(int index)
        {
            switch (index)
            {
                case 0: return new GBColor(224, 248, 208);
                case 1: return new GBColor(136, 192, 112);
                case 2: return new GBColor(52, 104, 86);
                case 3: return new GBColor(8, 24, 32);
                default: throw new Exception("Invalid color index");
            }
        }

        public int Read(MMU mmu)
        {
            int register = mmu.ReadByte(PaletteIndexAddress);
            int index = register & 0x3F;

            int colorToModify = (index % 8) / 2;
            if ((index % 8) % 2 == 0)
            {
                return (red[colorToModify] & 0x1F) | ((green[colorToModify] & 0x07) << 5);
            }
            else
            {
                return ((green[colorToModify] & 0x18) >> 3) | (blue[colorToModify] << 5);
            }
        }

        public void SaveState(BinaryWriter stream)
        {
            for (int i = 0; i < red.Length; i++) stream.Write(red[i]);
            for (int i = 0; i < green.Length; i++) stream.Write(green[i]);
            for (int i = 0; i < blue.Length; i++) stream.Write(blue[i]);
            for (int i = 0; i < Colors.Length; i++)
            {
                stream.Write(Colors[i].R);
                stream.Write(Colors[i].G);
                stream.Write(Colors[i].B);
            }
            stream.Write(PaletteDataAddress);
            stream.Write(PaletteIndexAddress);
        }

        public void LoadState(BinaryReader stream)
        {
            for (int i = 0; i < red.Length; i++) red[i] = stream.ReadInt32();
            for (int i = 0; i < green.Length; i++) green[i] = stream.ReadInt32();
            for (int i = 0; i < blue.Length; i++) blue[i] = stream.ReadInt32();
            for (int i = 0; i < Colors.Length; i++)
            {
                Colors[i].R = stream.ReadInt32();
                Colors[i].G = stream.ReadInt32();
                Colors[i].B = stream.ReadInt32();
            }
            PaletteDataAddress = stream.ReadInt32();
            PaletteIndexAddress = stream.ReadInt32();
        }
    }
}
