using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC5 : Cartridge
    {
        private bool ERAMEnabled { get; set; }

        private int BankRom { get; set; }
        private int BankRam { get; set; }

        private int CartridgeMode { get; set; }

        public bool RumbleEnabled { get; set; }

        protected override void CustomInit()
        {
            ERAMEnabled = false;
            BankRom = 1;
            BankRam = 0;
            CartridgeMode = 0;

            // Battery Backed
            if (Battery)
            {
                byte[] save = FileManager.LoadSaveFile(Name, Checksum);
                //Array.Copy(save, ERam, save.Length);
            }
        }

        public override int ReadERam(int address)
        {
            if(!ERAMEnabled || ERam.Length == 0) return 0x00;

            return ERam[(BankRam * ERamOffset) | (address & 0x1FFF)];
        }

        public override int ReadLowRom(int address)
        {
            return Rom[address];
        }

        public override int ReadHighRom(int address)
        {
            return Rom[(GetWrappedRomBank() * RomOffset) | (address & 0x3FFF)];
        }

        public override void WriteERam(int address, int value)
        {
            if (!ERAMEnabled || ERam.Length == 0) return;
            ERam[(BankRam * ERamOffset) + (address & 0x1FFF)] = value;
        }

        public override void WriteRom(int address, int value)
        {
            switch(address)
            {
                case int _ when address < 0x2000:
                    ERAMEnabled = ((value & 0x0F) == 0x0A);
                    break;
                case int _ when address < 0x3000:
                    BankRom = (value & 0xFF) | (BankRom & 0x100);
                    break;
                case int _ when address < 0x4000:
                    BankRom = ((value & 0x01) << 8) | (BankRom & 0xFF);
                    break;
                case int _ when address < 0x6000:
                    BankRam = value & 0x0F;
                    break;
            }
        }

        private int GetWrappedRomBank()
        {
            int returnBank = BankRom % RomBankCount;
            return returnBank;
        }

        public override string ToString()
        {
            return String.Format("ROM:{0:X2}\tRAM:{1:X2}\tCartridgeMode:{2}", BankRom, BankRam, CartridgeMode);
        }

        public override void Close()
        {
            if(Battery) FileManager.SaveFile(Name, Checksum, ERam);
        }
    }
}
