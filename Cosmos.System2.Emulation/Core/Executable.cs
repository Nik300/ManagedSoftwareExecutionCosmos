using Sys = System;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Cosmos.System.Emulation
{
    public class Executable
    {
        public string FileName { get; private set; }
        public byte Type = 0; // 01 = 8-bit, 02 = 16-bit, 03 = 32-bit
        private byte InstructionSet; // 00 - Chip8, 01 - yours, 02 mine, 03 our combined
        public byte[] Data { get; private set; }
        public byte[] op8 { get; private set; }
        public short[] op16 { get; private set; }
        public int[] op32 { get; private set; }
        public VirtualMemory Memory { get; private set; }
        public InstructionSet usingInstructionSet;

        public bool running = true;

        public bool lastInstruction = true;

        public delegate void SystemCall(Executable caller);
        private protected List<SystemCall> SystemCalls = new List<SystemCall>();

        public List<int> Returns = new List<int>();

        public Executable(string file, InstructionSet instructionSet = null, byte type = 0)
        {
            FileName = file;
            Data = null;
            Type = type;
            usingInstructionSet = instructionSet;
        }
        public Executable(byte[] data, InstructionSet instructionSet = null, byte type = 0)
        {
            FileName = null;
            Data = data;
            Type = type;
            usingInstructionSet = instructionSet;
        }

        public void NextInstruction()
        {
            try
            {
                if (!running) return;
                if (Type == 1)
                {
                    Instruction instr8 = usingInstructionSet.GetInstruction(op8);
                    instr8.Delegate(this);
                }
                else if (Type == 2)
                {
                    Instruction instr16 = usingInstructionSet.GetInstruction(op16);
                    instr16.Delegate(this);
                }
                else if (Type == 3)
                {
                    Instruction instr32 = usingInstructionSet.GetInstruction(op32);
                    instr32.Delegate(this);
                }
            }
            catch(Sys.Exception ex)
            {
                Sys.Console.WriteLine($"Error while executing program instructions:\n{ex.Message}");
                Sys.Console.WriteLine("Dumping memory...");
                string file = $@"0:\System\bin\dump_{DateTime.Now.Day}{DateTime.Now.Month}_{DateTime.Now.Hour}{DateTime.Now.Minute}_{DateTime.Now.Second}.dat";
                File.Create(file).Close();
                List<byte> Bytes = new List<byte>();
                for (int i = 0; i < Memory.Data.Count; i++)
                {
                    byte[] number = BitConverter.GetBytes(Memory.Data[i]);
                    for (int x = 0; x < 4; x++)
                    {
                        Bytes.Add(number[x]);
                    }
                }
                File.WriteAllBytes(file, Bytes.ToArray());
                Sys.Console.WriteLine($"Dump complete!\nProgram memory dumped at: {file}");
                running = false;
            }
        }

        public int AddSystemCall(SystemCall systemCall)
        {
            SystemCalls.Add(systemCall);
            return SystemCalls.Count - 1;
        }
        public bool RunSystemCall(int syscall)
        {
            if (syscall >= SystemCalls.Count)
                return false;

            SystemCalls[syscall](this);

            return true;
        }

        public void ReadData()
        {
            try
            {

                if (FileName != null && Data == null)
                {
                    if (!File.Exists(FileName))
                    {
                        throw new FileNotFoundException();
                    }
                    Data = File.ReadAllBytes(FileName);
                }

                if (Type == 0)
                    Type = Data[0];
                InstructionSet = Data[1];
                List<int> tempRam = new List<int>();

                int i;

                for (i = 2; i < Data.Length; i += 4)
                {
                    List<byte> temp = new List<byte>();
                    if (Data[i] == 0xFF && Data[i + 1] == 0xFA && Data[i + 2] == 0xFF && Data[i + 3] == 0xFA && Data[i + 4] == 0xFB)
                    {
                        i += 5;
                        break;
                    }
                    for (int c = 0; c < 4; c++)
                        temp.Add(Data[i + c]);
                    int d = BitConverter.ToInt32(temp.ToArray(), 0);
                    tempRam.Add(d);
                }

                if (Type != 1 && Type != 2 && Type != 3)
                    throw new NotImplementedException();

                List<byte> tempOP = new List<byte>();

                for (; i < Data.Length; i++)
                {
                    tempOP.Add(Data[i]);
                }
                if (Type == 1)
                {
                    op8 = tempOP.ToArray();
                }
                else if (Type == 2)
                {
                    List<short> temp16OP = new List<short>();
                    for (i = 0; i < tempOP.Count; i++)
                    {
                        List<byte> toAdd = new List<byte>();
                        for (int c = 0; c < 2; c++)
                            toAdd.Add(tempOP[c + i]);
                        temp16OP.Add(BitConverter.ToInt16(toAdd.ToArray(), 0));
                    }
                    op16 = temp16OP.ToArray();
                }
                else if (Type == 3)
                {
                    List<int> temp32OP = new List<int>();
                    for (i = 0; i < tempOP.Count; i += 4)
                    {
                        List<byte> toAdd = new List<byte>();
                        for (int c = 0; c < 4; c++)
                            toAdd.Add(tempOP[c + i]);
                        toAdd.Reverse();
                        temp32OP.Add(BitConverter.ToInt32(toAdd.ToArray(), 0));
                    }
                    op32 = temp32OP.ToArray();
                }

                Data = null;

                Memory = new VirtualMemory();
                Memory.AddArray(tempRam);
                if (usingInstructionSet == null)
                {
                    usingInstructionSet = Types.Sets[InstructionSet];
                }
            }
            catch (Exception ex)
            {
                Sys.Console.WriteLine($"Error reading executable data: {ex.Message}");
            }
        }
    }
}
