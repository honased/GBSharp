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
            CartridgeLoader.LoadDataIntoMemory(mmu, CartridgeLoader.LoadCart("Roms/opus5.gb"), 0x00);

            CPU cpu = new CPU(mmu);

            int instructionCount = 0;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (instructionCount < 1000000000)
            {
                instructionCount += cpu.ProcessInstructions();
                //Console.ReadKey();
            }
            watch.Stop();
            Console.WriteLine("Total time: " + watch.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
