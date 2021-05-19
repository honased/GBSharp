using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp.Graphics
{
    internal class FrameQueue
    {
        private int index;
        public int Count { get; private set; }

        private const int MAX_LENGTH = 5;

        private int[][] queue;

        public FrameQueue()
        {
            queue = new int[MAX_LENGTH][];

            // Initialize Frame Buffers
            for(int i = 0; i < MAX_LENGTH; i++)
            {
                queue[i] = new int[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT * 4];
            }

            index = 0;
            Count = 0;
        }

        public void Clear()
        {
            index = 0;
            Count = 0;
        }

        public void Enqueue(int[] buffer)
        {
            while(Count >= MAX_LENGTH)
            {
                Dequeue();
            }

            // Copy contents of array
            Array.Copy(buffer, queue[(index + Count) % MAX_LENGTH], buffer.Length);

            Count++;
        }

        public int[] Dequeue()
        {
            int[] returnBuf = queue[index];

            Count--;
            index = (index + 1) % MAX_LENGTH;

            return returnBuf;
        }
    }
}
