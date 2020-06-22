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
            CPU cpu = new CPU(mmu);

            CartridgeLoader.LoadDataIntoMemory(mmu, CartridgeLoader.LoadCart("InternalRoms/DMG_ROM.bin"), 0);

            while (true)
            {
                cpu.ProcessOpcodes();
                Console.ReadKey();
            }
        }
    }
}
