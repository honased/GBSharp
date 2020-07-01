using GBSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GBSharpTests
{
    [TestClass]
    class InstructionTests
    {
        private MMU _mmu;
        private PPU _ppu;
        private CPU _cpu;
        private Input _input;

        public InstructionTests()
        {
            _mmu = new MMU();
            _ppu = new PPU(_mmu);
            _input = new Input(_mmu);
            _cpu = new CPU(_mmu, _ppu, _input);
        }

        private int[] _cycles = new int[]
        {
            1, 3, 2, 2, 1, 1, 2, 1, 5, 2, 2, 2, 1, 1, 2, 1, 
            1, 3, 2, 2, 1, 1, 2, 1, 3, 2, 2, 2, 1, 1, 2, 1,
            0, 3, 2, 2, 1, 1, 2, 1, 0, 2, 2, 2, 1, 1, 2, 1,
            0, 3, 2, 2, 3, 3, 3, 1, 0, 2, 2, 2, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            2, 2, 2, 2, 2, 2, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            0, 3, 0, 4, 0, 4, 2, 4, 0, 4, 0, 1, 0, 6, 2, 4,
            0, 3, 0, 9, 0, 4, 2, 4, 0, 4, 0, 9, 0, 9, 2, 4,
            3, 3, 2, 9, 9, 4, 2, 4, 4, 1, 4, 9, 9, 9, 2, 4,
            3, 3, 2, 1, 9, 4, 2, 4, 3, 2, 4, 1, 9, 9, 2, 4,
        };

        [TestMethod]
        public void TestInstructionsCycles()
        {
            for(var i = 0; i < 0x100; i++)
            {
                int expected = _cycles[i];
                int actual = _cpu.TestInstruction(i);
                Assert.AreEqual(expected, actual, "Failed on instruction {0:X2}. Expected {1}, got {2}.", i, expected, actual);
            }
        }
    }
}
