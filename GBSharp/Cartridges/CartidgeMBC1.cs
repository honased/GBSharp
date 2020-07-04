using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC1 : Cartridge
    {
        private bool ERAM_Enabled { get; set; }

        private int BANK_ROM { get; set; }
        private int BANK_RAM { get; set; }

        private int CartridgeMode { get; set; }

        protected override void CustomInit()
        {
            ERAM_Enabled = false;
            BANK_ROM = 1;
            BANK_RAM = 0;
            CartridgeMode = 0;
        }

        public override int ReadERam(int address)
        {
            if(!ERAM_Enabled) return 0xFF;

            return ERam[(BANK_RAM * ERamOffset) | (address & 0x1FFF)];
        }

        public override int ReadLowRom(int address)
        {
            return Rom[address];
        }

        public override int ReadHighRom(int address)
        {
            return Rom[(BANK_ROM * RomOffset) | (address & 0x3FFF)];
        }

        public override void WriteERam(int address, int value)
        {
            if (!ERAM_Enabled || ERam.Length == 0) return;
            ERam[(BANK_RAM * ERamOffset) + (address & 0x1FFF)] = value;
        }

        public override void WriteRom(int address, int value)
        {
            switch(address)
            {
                case int _ when address < 0x2000:
                    ERAM_Enabled = ((value & 0x0F) == 0x0A);
                    break;
                case int _ when address < 0x4000:
                    BANK_ROM = value & 0x1F;
                    if ((BANK_ROM & 0x0F) == 0x00) BANK_ROM |= 0x01;
                    break;
                case int _ when address < 0x6000:
                    if(CartridgeMode == 0)
                    {
                        BANK_ROM = (BANK_ROM & 0x1F) | ((value & 0x03) << 5);
                    }
                    else
                    {
                        BANK_RAM = value & 0x03;
                    }
                    break;
                case int _ when address < 0x8000:
                    CartridgeMode = value & 0x01;
                    if (CartridgeMode == 0) BANK_RAM = 0x00;
                    else
                    {
                        BANK_ROM = Bitwise.ClearBit(Bitwise.ClearBit(BANK_ROM, 5), 6);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            return String.Format("ROM:{0:X2}\tRAM:{1:X2}\tCartridgeMode:{2}", BANK_ROM, BANK_RAM, CartridgeMode);
        }
    }
}
