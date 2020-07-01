using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBSharp;
using System.Diagnostics.Contracts;

namespace GBSharpTests
{
    [TestClass]
    public class RegisterTests
    {
        private MMU _mmu;
        private PPU _ppu;
        private CPU _cpu;
        private Input _input;

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

        public RegisterTests()
        {
            _mmu = new MMU();
            _ppu = new PPU(_mmu);
            _input = new Input(_mmu);
            _cpu = new CPU(_mmu, _ppu, _input);
        }

        [TestMethod]
        public void TestInstructionsCycles()
        {
            for (var i = 0; i < 0x100; i++)
            {
                int expected = _cycles[i];
                if (expected == 0 || expected == 9) continue;
                int actual = _cpu.TestInstruction(i);
                if (actual == -1) continue;
                Assert.AreEqual(expected, actual, "Failed on instruction {0:X2}. Expected {1}, got {2}.", i, expected, actual);
            }
        }

        [TestMethod]
        public void TestRegisterAF()
        {
            for(int i = 0; i < 255; i++)
            {
                for(int j = 0; j < 255; j++)
                {
                    int[] registers = HoldOtherRegisters(CPU.Registers16Bit.AF);

                    int expected16 = (i << 8) | (j & 0xF0);

                    int oldF = _cpu.LoadRegister(CPU.Registers8Bit.F);

                    _cpu.SetRegister(CPU.Registers8Bit.A, i);

                    // Assert a = i and f = oldF
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.A), i, "Setting A to " + i.ToString() + " resulted in A reading as " + _cpu.LoadRegister(CPU.Registers8Bit.A));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.F), oldF, "Setting A to " + i.ToString() + " resulted in F reading as " + _cpu.LoadRegister(CPU.Registers8Bit.F));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers16Bit.AF), (i << 8) | (oldF), "Setting A to " + i.ToString() + " resulted in AF reading as " + _cpu.LoadRegister(CPU.Registers16Bit.AF));

                    int oldA = _cpu.LoadRegister(CPU.Registers8Bit.A);
                    _cpu.SetRegister(CPU.Registers8Bit.F, j);
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.F), j & 0xF0, "Setting F to " + j.ToString() + " resulted in F reading as " + _cpu.LoadRegister(CPU.Registers8Bit.F));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.A), oldA, "Setting F to " + j.ToString() + " resulted in A reading as " + _cpu.LoadRegister(CPU.Registers8Bit.A) + " when it should be " + oldA);
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers16Bit.AF), expected16, "Setting F to " + j.ToString() + " resulted in AF reading as " + _cpu.LoadRegister(CPU.Registers16Bit.AF));

                    AssertOtherRegisters(CPU.Registers16Bit.AF, registers);
                }
            }
        }

        [TestMethod]
        public void TestRegisterBC()
        {
            for (int i = 0; i < 255; i++)
            {
                for (int j = 0; j < 255; j++)
                {
                    int[] registers = HoldOtherRegisters(CPU.Registers16Bit.BC);

                    int expected16 = (i << 8) | j;

                    int oldC = _cpu.LoadRegister(CPU.Registers8Bit.C);

                    _cpu.SetRegister(CPU.Registers8Bit.B, i);

                    // Assert a = i and f = oldF
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.B), i, "Setting B to " + i.ToString() + " resulted in B reading as " + _cpu.LoadRegister(CPU.Registers8Bit.B));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.C), oldC, "Setting B to " + i.ToString() + " resulted in C reading as " + _cpu.LoadRegister(CPU.Registers8Bit.C));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers16Bit.BC), (i << 8) | (oldC), "Setting B to " + i.ToString() + " resulted in BC reading as " + _cpu.LoadRegister(CPU.Registers16Bit.BC));

                    int oldB = _cpu.LoadRegister(CPU.Registers8Bit.B);
                    _cpu.SetRegister(CPU.Registers8Bit.C, j);
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.C), j, "Setting C to " + j.ToString() + " resulted in C reading as " + _cpu.LoadRegister(CPU.Registers8Bit.C));
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers8Bit.B), oldB, "Setting C to " + j.ToString() + " resulted in B reading as " + _cpu.LoadRegister(CPU.Registers8Bit.B) + " when it should be " + oldB);
                    Assert.AreEqual(_cpu.LoadRegister(CPU.Registers16Bit.BC), expected16, "Setting C to " + j.ToString() + " resulted in BC reading as " + _cpu.LoadRegister(CPU.Registers16Bit.BC));

                    AssertOtherRegisters(CPU.Registers16Bit.BC, registers);
                }
            }
        }

        [TestMethod]
        public void CheckFlags()
        {
            _cpu.SetRegister(CPU.Registers8Bit.F, 0);
            Assert.AreEqual(0, _cpu.LoadRegister(CPU.Registers8Bit.F), "Register was not properly cleared");
            
            for(int i = 0; i < 16; i++)
            {
                _cpu.SetFlag(CPU.Flags.Z, (i / 8) != 0);
                _cpu.SetFlag(CPU.Flags.Z, (i / 4) == 0);
                _cpu.SetFlag(CPU.Flags.Z, (i / 2) == 0);
                _cpu.SetFlag(CPU.Flags.Z, (i / 8) == 0);
            }
        }

        private int[] HoldOtherRegisters(CPU.Registers16Bit register)
        {
            int[] registers = new int[(int)CPU.Registers16Bit.PC];
            for(int i = 0; i < (int)CPU.Registers16Bit.PC; i++)
            {
                if (i != (int)register) registers[i] = _cpu.LoadRegister((CPU.Registers16Bit)i);
            }
            return registers;
        }

        private void AssertOtherRegisters(CPU.Registers16Bit register, int[] registers)
        {
            for (int i = 0; i < (int)CPU.Registers16Bit.PC; i++)
            {
                if(i != (int)register) Assert.AreEqual(_cpu.LoadRegister((CPU.Registers16Bit)i), registers[i], "Register " + (((CPU.Registers16Bit)i).ToString()) + " was modified");
            }
        }
    }
}
