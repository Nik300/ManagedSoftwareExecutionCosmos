using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Emulation
{
    public class CHIP8Instruction : Instruction
    {
        public CHIP8Instruction(string mnem, short op, string format) : base(mnem, op, format)
        {

        }
    }
}
