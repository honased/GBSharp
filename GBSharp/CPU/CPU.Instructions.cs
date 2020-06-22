using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBSharp
{
    public partial class CPU
    {
        private Instruction[] _instructions;

        private void RegisterInstructions()
        {
            _instructions = new Instruction[0xFF];

            AddInstruction(0x31, new Instruction("LD SP, d16"));
        }

        private void AddInstruction(int location, Instruction instruction)
        {
            _instructions[location] = instruction;
        }

        private class Instruction
        {
            public string Name { get; private set; }

            public Instruction(string name)
            {
                Name = name;
            }
        }
    }
}
