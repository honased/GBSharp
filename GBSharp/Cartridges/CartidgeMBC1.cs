﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Cartridges
{
    class CartidgeMBC1 : Cartridge
    {
        private bool ERAMEnabled { get; set; }

        private int BankRom { get; set; }
        private int BankRam { get; set; }

        private int CartridgeMode { get; set; }

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
                Array.Copy(save, ERam, save.Length);
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
                case int _ when address < 0x4000:
                    BankRom = value & 0x1F;
                    if (BankRom == 0x00 || BankRom == 0x20 || BankRom == 0x40 || BankRom == 0x60) BankRom++;
                    break;
                case int _ when address < 0x6000:
                    if(CartridgeMode == 0)
                    {
                        BankRom = (BankRom & 0x1F) | ((value & 0x03) << 5);
                    }
                    else
                    {
                        BankRam = value & 0x03;
                    }
                    break;
                case int _ when address < 0x8000:
                    CartridgeMode = value & 0x01;
                    if (CartridgeMode == 0) BankRam = 0x00;
                    else
                    {
                        BankRom = Bitwise.ClearBit(Bitwise.ClearBit(BankRom, 5), 6);
                    }
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
            return String.Format("ROM:{0:X2}\tRAM:{1:X2}\tCartridgeMode:{2}", BankRom, BankRam, CartridgeMode);
        }

        public override void Close()
        {
            if(Battery) FileManager.SaveFile(Name, Checksum, ERam);
        }

        protected override void CustomSaveState(BinaryWriter stream)
        {
            stream.Write(ERAMEnabled);
            stream.Write(BankRom);
            stream.Write(BankRam);
            stream.Write(CartridgeMode);
        }

        protected override void CustomLoadState(BinaryReader stream)
        {
            ERAMEnabled = stream.ReadBoolean();
            BankRom = stream.ReadInt32();
            BankRam = stream.ReadInt32();
            CartridgeMode = stream.ReadInt32();
        }
    }
}
