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

        private int[] _rtcRegisters;
        private bool RTCLatch { get; set; }

        protected override void CustomInit()
        {
            ERAMEnabled = false;
            BankRom = 1;
            BankRam = 0;
            RTCEnabled = false;
            RTCLatch = false;

            _rtcRegisters = new int[5];

            // Battery Backed
            if (Battery)
            {
                byte[] save = FileManager.LoadSaveFile(Name, Checksum);
                Array.Copy(save, ERam, save.Length);
            }
        }

        public override int ReadERam(int address)
        {
            if(!ERAMEnabled || ERam.Length == 0) return 0x00;

            if (BankRam <= 0x07) return ERam[(BankRam * ERamOffset) | (address & 0x1FFF)];
            else if (BankRam <= 0x0C) return _rtcRegisters[BankRam - 0x08];

            return 0x00;
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
            if(BankRam <= 0x07) ERam[(BankRam * ERamOffset) + (address & 0x1FFF)] = value;
            else if(BankRam <= 0x0C)
            {
                // RTC Register
                
            }
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
                        ERAMWasOpen = true;
                    }
                    else
                    {
                        if (ERAMWasOpen)
                        {
                            ERAMWasOpen = false;
                            // Save game
                            //if (Battery) FileManager.SaveFile(Name, Checksum, ERam);
                        }
                    }

                    break;
                case int _ when address < 0x4000:
                    BankRom = value & 0x7F;
                    if (BankRom == 0x00) BankRom = 0x01;
                    break;
                case int _ when address < 0x6000:
                    BankRam = value;
                    break;
                case int _ when address < 0x8000:
                    if (!HasRTC) return;
                    if (value == 0x00) RTCLatch = false;
                    else if(value == 0x01)
                    {
                        if(!RTCLatch)
                        {
                            UpdateRTC();
                        }
                        RTCLatch = true;
                    }
                    break;
            }
        }

        private int GetWrappedRomBank()
        {
            int returnBank = BankRom % RomBankCount;
            if (returnBank == 0x00) returnBank = 0x01;
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

        private void UpdateRTC()
        {
            DateTime current = DateTime.Now;
            _rtcRegisters[0] = current.Second;
            _rtcRegisters[1] = current.Minute;
            _rtcRegisters[2] = current.Hour;
        }
    }
}
