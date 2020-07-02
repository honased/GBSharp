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
        public int Type { get; private set; }

        public static Cartridge Load(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            byte[] data = br.ReadBytes((int)br.BaseStream.Length);
            br.Close();
            Cartridge cartridge;
            switch(data[0x0147])
            {
                case 0:
                    cartridge = new CartidgeMBC0();
                    Array.Copy(data, ((CartidgeMBC0)cartridge).rom, data.Length);
                    break;

                default:
                    throw new NotImplementedException("Cartridge type " + data[0x0147].ToString() + " not implemented yet.");
            }

            cartridge.Type = data[0x0147];

            

            StringBuilder namebuilder = new StringBuilder();

            for(var i = 0x0134; i <= 0x143; i++)
            {
                char letter = (char)data[i];
                if (letter == 0) break;
                namebuilder.Append(letter);
            }
            cartridge.Name = namebuilder.ToString();

            return cartridge;
        }

        public abstract void WriteRom(int address, int value);
        public abstract void WriteERam(int address, int value);

        public abstract int ReadLowRom(int address);
        public abstract int ReadHighRom(int address);
        public abstract int ReadERam(int address);
    }
}
