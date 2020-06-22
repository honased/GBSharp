using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public static class CartridgeLoader
    {
        public static void LoadDataIntoMemory(MMU mmu, int[] cart, int position)
        {
            if(position >= 0 && position + cart.Length < MMU.MEMORY_SIZE)
            {
                mmu.WriteBytes(cart, position);
            }
        }

        public static int[] LoadCart(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));

            byte[] rom = br.ReadBytes((int)br.BaseStream.Length);

            br.Close();

            int[] returnRom = new int[rom.Length];

            Array.Copy(rom, returnRom, rom.Length);

            return returnRom;
        }
    }
}
