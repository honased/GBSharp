using GBSharp.Cartridges;
using GBSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GBSharp
{
    public abstract class Cartridge : IStateable
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

        internal GBMode GameboyType { get; private set; }

        internal enum GBMode
        {
            GBOnly = 0,
            CBGAndGB = 1,
            CGBOnly = 2
        }

        public static Cartridge Load(Stream stream)
        {
            List<byte> data = new List<byte>();
            int bit = stream.ReadByte();
            while(bit != -1)
            {
                data.Add((byte)bit);
                bit = stream.ReadByte();
            }
            return CreateCartridge(data.ToArray());
        }

        public static Cartridge Load(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            byte[] data = br.ReadBytes((int)br.BaseStream.Length);
            br.Close();
            return CreateCartridge(data);
        }

        public static Cartridge Load(byte[] data)
        {
            return CreateCartridge(data);
        }

        private static Cartridge CreateCartridge(byte[] data)
        {
            Cartridge cartridge;
            int cartType = data[0x0147];
            switch (cartType)
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

                case 0x0F:
                case 0x10:
                    cartridge = new CartidgeMBC3() { Battery = true, HasRTC = true };
                    break;

                case 0x11:
                case 0x12:
                    cartridge = new CartidgeMBC3();
                    break;
                case 0x13:
                    cartridge = new CartidgeMBC3() { Battery = true };
                    break;

                case 0x19:
                    cartridge = new CartidgeMBC5() { Battery = false };
                    break;

                case 0x1A:
                case 0x1C:
                case 0x1D:
                    cartridge = new CartidgeMBC5() { Battery = false };
                    break;

                case 0x1B:
                case 0x1E:
                    cartridge = new CartidgeMBC5() { Battery = true };
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

            switch(data[0x0143])
            {
                case 0x80: GameboyType = GBMode.CBGAndGB; break;
                case 0xC0: GameboyType = GBMode.CGBOnly; break;
                default: GameboyType = GBMode.GBOnly; break;
            }

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

        protected abstract void CustomSaveState(BinaryWriter stream);
        protected abstract void CustomLoadState(BinaryReader stream);

        public void SaveState(BinaryWriter stream)
        {
            for (int i = 0; i < ERam.Length; i++) stream.Write(ERam[i]);
            CustomSaveState(stream);
        }

        public void LoadState(BinaryReader stream)
        {
            for (int i = 0; i < ERam.Length; i++) ERam[i] = stream.ReadInt32();
            CustomLoadState(stream);
        }
    }
}
