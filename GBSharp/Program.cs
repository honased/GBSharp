using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            MMU mmu = new MMU();

            //Console.WriteLine("Done");
            //Console.ReadKey();
            int[] cart = CartridgeLoader.LoadCart("Roms/opus5.gb");
            CartridgeLoader.LoadDataIntoMemory(mmu, cart, 0x00);

            CPU cpu = new CPU(mmu);
            PPU ppu = new PPU(mmu);

            int count = 0;

            //Console.WriteLine("VAlue at 0xFFFF:" + mmu.ReadByte(0xFFFF));
            //Console.ReadKey();


            while(true)
            {
                cpu.ExecuteFrame();
            }

            Console.ReadKey();
        }
    }
}
