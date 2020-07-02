using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC0 : Cartridge
    {
        public int[] rom { get; private set; } = new int[0x8000];

        public override int ReadERam(int address)
        {
            return 0xFF;
        }

        public override int ReadLowRom(int address)
        {
            return rom[address];
        }

        public override int ReadHighRom(int address)
        {
            return rom[address];
        }

        public override void WriteERam(int address, int value)
        {
            // Do nothing
        }

        public override void WriteRom(int address, int value)
        {
            // Do nothing
        }
    }
}
