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

            int count = 0;

            while(true)
            {
                count += cpu.ProcessInstructions();
            }

            Console.ReadKey();
        }
    }
}
