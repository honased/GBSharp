using System;
using System.Collections.Generic;
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
            CartridgeLoader.LoadDataIntoMemory(mmu, CartridgeLoader.LoadCart("Roms/opus5.gb"), 0x00);

            CPU cpu = new CPU(mmu);

            while (true)
            {
                cpu.ProcessInstructions();
                //Console.ReadKey();
            }
        }
    }
}
