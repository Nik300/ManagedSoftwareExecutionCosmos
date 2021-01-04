using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Emulation
{
    public abstract class InstructionSet
    {
        public static Dictionary<uint, Instruction> instructions = new Dictionary<uint, Instruction>();
        public string name;
        public InstructionSet(byte identifier, string name)
        {
            this.name = name;
        }
        internal void AddInstruction(Instruction instr)
        {
            instructions.Add((uint)instr.OpCode, instr);
        }
        public abstract Instruction GetInstruction(int[] rawCode);
        public abstract Instruction GetInstruction(short[] rawCode);
        public abstract Instruction GetInstruction(byte[] rawCode);
    }
    public abstract class Instruction
    {
        public string Mnemonic;
        public string Format;
        public int OpCode;
        //man, so I found a much more efficient way to handle arguments
        //first thing first every instruction will have built-in their instructions
        //then they'll be set by the instructionset when returning the instruction
        //then the executable class calss the delegate acording to the executable's architecture
        //and the play is done
        public List<uint> args32 = null;
        public List<ushort> args16 = null;
        public List<byte> arg8 = null;
        //at this point we won't even need the difference between the delegates
        //because each instruction will take the arguments it needs
        //and if the arguments array it needs is null, it just stops the execution
        //and throws one of those weird errors that windows sometimes throws when freaking out XD
        //example: "memory at 0x102091 cannot be 'read' by program 0x986241. quitting..."
        public delegate void InstructionDelegate(Executable caller);
        public InstructionDelegate Delegate;

        public Instruction(string mnem = "NOP", int op = 0x00, string format = "")
        {
            this.Mnemonic = mnem;
            this.OpCode = op;
            this.Format = format;
        }
    }
}
