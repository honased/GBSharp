using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC0 : Cartridge
    {
        protected override void CustomInit()
        {
            // Do nothing
        }

        public override int ReadERam(int address)
        {
            return 0xFF;
        }

        public override int ReadLowRom(int address)
        {
            return Rom[address];
        }

        public override int ReadHighRom(int address)
        {
            return Rom[address];
        }

        public override void WriteERam(int address, int value)
        {
            // Do nothing
        }

        public override void WriteRom(int address, int value)
        {
            // Do nothing
        }

        public override void Close()
        {
            // Do nothing
        }

        protected override void CustomSaveState(BinaryWriter stream)
        {
            
        }

        protected override void CustomLoadState(BinaryReader stream)
        {
            
        }
    }
}
