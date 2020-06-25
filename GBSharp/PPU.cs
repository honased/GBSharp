using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public class PPU
    {
        public const int SCREEN_WIDTH = 160, SCREEN_HEIGHT = 144;
        public int[] FrameBuffer { get; private set; }

        private MMU _mmu;

        public PPU(MMU mmu)
        {
            _mmu = mmu;
            // Create a new framebuffer with 4 colors
            FrameBuffer = new int[SCREEN_WIDTH * SCREEN_HEIGHT * 4];
            
            // Initialize it to white
            for(int i = 0; i < FrameBuffer.Length; i++)
            {
                FrameBuffer[i] = 255;
            }
        }
    }
}
