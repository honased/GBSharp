using GBSharp.Cartridges;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public abstract class Cartridge
    {
        public string Name { get; private set; }
        public int CartridgeType { get; private set; }

        protected int Checksum { get; private set; }

        protected int[] Rom { get; set; }
        protected int[] ERam { get; set; }

        protected int RomOffset { get; private set; } = 0x4000;
        protected int ERamOffset { get; private set; } = 0x2000;

        protected int RomBankCount { get; private set; }

        protected bool Battery { get; private set; }

        public static Cartridge Load(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            byte[] data = br.ReadBytes((int)br.BaseStream.Length);
            br.Close();
            Cartridge cartridge;
            int cartType = data[0x0147];
            switch(cartType)
            {
                case 0:
                    cartridge = new CartidgeMBC0();
                    break;
                case 1:
                case 2:
                case 3:
                    cartridge = new CartidgeMBC1();
                    if (cartType == 3) cartridge.Battery = true;
                    break;

                default:
                    throw new NotImplementedException(String.Format("Cartridge type 0x{0:X2} not implemented yet.", data[0x0147]));
            }

            cartridge.Init(data);

            return cartridge;
        }

        private void Init(byte[] data)
        {
            // Setup rom
            Rom = new int[GetRomSize(data[0x0148])];

            RomBankCount = (Rom.Length / (1024 * 16));

            Array.Copy(data, Rom, data.Length);

            // Setup ram
            ERam = new int[GetERamSize(Rom[0x0149])];

            // Set name
            StringBuilder namebuilder = new StringBuilder();
            for (var i = 0x0134; i <= 0x143; i++)
            {
                char letter = (char)Rom[i];
                if (letter == 0) break;
                namebuilder.Append(letter);
            }
            Name = namebuilder.ToString();

            CartridgeType = Rom[0x0147];
            Checksum = (Rom[0x14E] << 8) | Rom[0x14F];

            CustomInit();
        }

        protected abstract void CustomInit();

        public abstract void Close();

        private static int GetRomSize(int type)
        {
            switch(type)
            {
                case 0: return (1024 * 32);
                case 1: return (1024 * 64);
                case 2: return (1024 * 128);
                case 3: return (1024 * 256);
                case 4: return (1024 * 512);
                case 5: return (1024 * 1024);
                case 6: return (1024 * 2048);
                case 7: return (1024 * 4096);
                case 8: return (1024 * 8192);
                default: throw new InvalidOperationException("There is no rom size type of " + type.ToString() + "!");
            }
        }

        private static int GetERamSize(int type)
        {
            switch (type)
            {
                case 0: return (0);
                case 1: return (1024 * 2);
                case 2: return (1024 * 8);
                case 3: return (1024 * 32);
                case 4: return (1024 * 128);
                case 5: return (1024 * 64);
                default: throw new InvalidOperationException("There is no rom size type of " + type.ToString() + "!");
            }
        }

        public abstract void WriteRom(int address, int value);
        public abstract void WriteERam(int address, int value);

        public abstract int ReadLowRom(int address);
        public abstract int ReadHighRom(int address);
        public abstract int ReadERam(int address);
    }
}
