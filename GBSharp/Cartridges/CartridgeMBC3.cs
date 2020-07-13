using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC3 : Cartridge
    {
        private bool ERAMEnabled { get; set; }

        private int BankRom { get; set; }
        private int BankRam { get; set; }

        private bool ERAMWasOpen { get; set; }

        public bool HasRTC { get; set; }

        private bool RTCEnabled { get; set; }

        protected override void CustomInit()
        {
            ERAMEnabled = false;
            BankRom = 1;
            BankRam = 0;
            RTCEnabled = false;

            // Battery Backed
            if (Battery)
            {
                byte[] save = FileManager.LoadSaveFile(Name, Checksum);
                Array.Copy(save, ERam, save.Length);
            }
        }

        public override int ReadERam(int address)
        {
            if(!ERAMEnabled) return 0xFF;

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
                    RTCEnabled = ERAMEnabled;

                    if (ERAMEnabled)
                    {
                        if (!ERAMWasOpen) Console.WriteLine("Eram Open");
                        ERAMWasOpen = true;
                    }
                    else
                    {
                        if (ERAMWasOpen)
                        {
                            ERAMWasOpen = false;
                            // Save game
                            Console.WriteLine("ERAM Closed");
                            if (Battery) FileManager.SaveFile(Name, Checksum, ERam);
                        }
                    }

                    break;
                case int _ when address < 0x4000:
                    BankRom = value & 0x7F;
                    if (BankRom == 0x00) BankRom++;
                    break;
                case int _ when address < 0x6000:
                    BankRam = value;
                    break;
                case int _ when address < 0x8000:
                    
                    break;
            }
        }

        private int GetWrappedRomBank()
        {
            int returnBank = BankRom % RomBankCount;
            if (returnBank == 0x00 || returnBank == 0x20 || returnBank == 0x40 || returnBank == 0x60) returnBank++;
            return returnBank;
        }

        public override string ToString()
        {
            return String.Format("ROM:{0:X2}\tRAM:{1:X2}", BankRom, BankRam);
        }

        public override void Close()
        {
            if(Battery) FileManager.SaveFile(Name, Checksum, ERam);
        }
    }
}
