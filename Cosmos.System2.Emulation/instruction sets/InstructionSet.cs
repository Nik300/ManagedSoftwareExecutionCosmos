using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//man if you're here now, just go check Instruction.cs out because there are some very cool updates going on here.
//things can be a bit confusing but hopefully by having commented everything you'll able to better understand what i did here.

namespace Cosmos.System.Emulation
{
    public class FGMSECpu: VirtualCPU32
    {
        List<uint> registers = new List<uint>();
        public override bool ClearRegData(byte regAddress)
        {
            if (regAddress >= registers.Count) return false;
            registers[regAddress] = 0x00;
            return true;
        }
        public override uint GetRegData(byte regAddress)
        {
            if (regAddress >= registers.Count)
            {
                throw new Sys.Exception("Requested address register was out of range!");
            }
            return registers[regAddress];
        }
        public override bool SetRegData(byte regAddress, uint Data)
        {
            if (regAddress >= registers.Count) return false;
            registers[regAddress] = Data;
            return true;
        }
        public override void LoadRegisters()
        {
            for (int i = 0; i < 15; i++)
            {
                registers.Add(0x00);
            }
        }
        public enum FGMSERegisters: byte
        {
            G0 = 0,
            G1 = 1,
            G2 = 2,
            G3 = 3,
            G4 = 4,
            G5 = 5,
            G6 = 6,
            G7 = 7,
            G8 = 8,
            G9 = 9,
            C1 = 10,
            C2 = 11,
            SP = 12,
            BP = 13,
            IP = 14
        }
    }
    public class CHIP8Cpu: VirtualCPU16 //oh and check out VirtualCPU.cs too
    {
        //i assume that a CHIP8 cpu has a maximum of 16bit registers, if not change it as you need

        public override bool ClearRegData(byte regAddress)
        {
            //clear register in address
            return true;
        }
        public override ushort GetRegData(byte regAddress)
        {
            //get and return data in the specified register
            return 0;
        }
        public override bool SetRegData(byte regAddress, ushort Data)
        {
            //put data in register
            return true;
        }
    }
    public class CHIP8InstructionSet : InstructionSet
    {
        public CHIP8Cpu cpu = new CHIP8Cpu(); //and yeah by this not being static, each executable will have its own cpu.
                                              //actually idk if i really wanna do this, tell me what you think about it.

        // chip-8 instructions
        public static CHIP8Instruction CHIP8_CLS = new CHIP8Instruction("CLS", 0x00E0, "") {
            Delegate = new Instruction.InstructionDelegate((Executable caller) =>
            {
                // clear buffer
               
            })
        };
        public static CHIP8Instruction CHIP8_RET = new CHIP8Instruction("RET", 0x00EE, "")
        {
            Delegate = new Instruction.InstructionDelegate((Executable caller) =>
            {
            })
        };
        public static CHIP8Instruction CHIP8_SYS = new CHIP8Instruction("RET", 0x1000, "") { };
        public static CHIP8Instruction CHIP8_JUMP = new CHIP8Instruction("RET", 0x00EE, "") { };

        // maybe creating a function like: call(byte[] code); and then it'll call the instruction as needed

        public override Instruction GetInstruction(short[] rawCode)
        {
            //here you set the value and arguments of the instruction set
            //then you add to the register pointer that manages the location into the opcode the number of times you went on reading
            //the array

            //let's assume that 5 is the register encharged of maintaining the pointer of the OPCODE

            int readNum = 0; //increase this each time you read next value in rawCode

            //here you would set the args into the instruction and determin which instruction is it
            Instruction instr = CHIP8_SYS;
            instr.arg8 = null;  //here you may put
            instr.args16 = null;//all the arguments
            instr.args32 = null;//needed by the instruction
                                //always acording to its architecture

            cpu.SetRegData(5, (ushort)(cpu.GetRegData(5) + readNum)); //and as said before here you actually set the
                                                                      //op pointer after the read instruction.

            return instr; //and here you return the instruction, ready to be called by the executable instance
        }
        public override Instruction GetInstruction(int[] rawCode)
        {
            //this function will never be usend since chip8 doesn't take 32bit opcodes
            return new CHIP8Instruction("NOP", 0, "");
        }
        public override Instruction GetInstruction(byte[] rawCode)
        {
            //this function will never be usend since chip8 doesn't take 8bit opcodes
            return new CHIP8Instruction("NOP", 0, "");
        }

        public CHIP8InstructionSet(): base(0x00, "Chip8 instruction set")
        {
            AddInstruction(CHIP8_CLS);
        }

    }
    public class FGMSECInstructionSet : InstructionSet
    {
        public FGMSECpu CPU = new FGMSECpu();
        public FGMSECInstructionSet(): base(0x02, "FGMSE instruction set")
        {
            CPU.LoadRegisters();
        }
        public static void Install()
        {
            try
            {
                FGMSEInstruction NOP = new FGMSEInstruction("NOP", 0, "NOP");
                instructions.Add(0, NOP);

                //math instructions

                // MOV instruction - 1
                FGMSEInstruction instr = new FGMSEInstruction("MOV", 4, "MOV");
                instr.Delegate = instr.MOV;
                instructions.Add(1, instr);

                // SET instruction - 2
                instr = new FGMSEInstruction("SET", 4, "SET");
                instr.Delegate = instr.SET;
                instructions.Add(2, instr);

                // ADD instruction - 3
                instr = new FGMSEInstruction("ADD", 4, "ADD");
                instr.Delegate = instr.ADD;
                instructions.Add(3, instr);

                // SUB instruction - 4
                instr = new FGMSEInstruction("SUB", 4, "SUB");
                instr.Delegate = instr.SUB;
                instructions.Add(4, instr);

                // DIV instruction - 5
                instr = new FGMSEInstruction("DIV", 4, "DIV");
                instr.Delegate = instr.DIV;
                instructions.Add(5, instr);

                // MUL instruction - 6
                instr = new FGMSEInstruction("MUL", 4, "MUL");
                instr.Delegate = instr.MUL;
                instructions.Add(6, instr);

                //system instructions

                // JMP instruction - 7
                instr = new FGMSEInstruction("JMP", 2, "JMP");
                instr.Delegate = instr.JMP;
                instructions.Add(7, instr);

                // CALL instruction - 8
                instr = new FGMSEInstruction("CALL", 2, "CALL");
                instr.Delegate = instr.CALL;
                instructions.Add(8, instr);

                // RET instruction - 9
                instr = new FGMSEInstruction("RET", 0, "RET");
                instr.Delegate = instr.RET;
                instructions.Add(9, instr);

                // SYSCALL instruction - 10
                instr = new FGMSEInstruction("SYSCALL", 2, "SYSCALL");
                instr.Delegate = instr.SYSCALL;
                instructions.Add(10, instr);

                // END instruction - 11
                instr = new FGMSEInstruction("END", 0, "END");
                instr.Delegate = instr.END;
                instructions.Add(11, instr);

                //conditional instructions

                // CMP instruction - 12
                instr = new FGMSEInstruction("CMP", 4, "CMP");
                instr.Delegate = instr.CMP;
                instructions.Add(12, instr);

                // EQU instruction - 13
                instr = new FGMSEInstruction("EQU", 4, "EQU");
                instr.Delegate = instr.EQU;
                instructions.Add(13, instr);

                // JE instruction - 14
                instr = new FGMSEInstruction("JE", 2, "JE");
                instr.Delegate = instr.JE;
                instructions.Add(14, instr);

                // JNE instruction - 15
                instr = new FGMSEInstruction("JNE", 2, "JNE");
                instr.Delegate = instr.JNE;
                instructions.Add(15, instr);

                // CE instruction - 16
                instr = new FGMSEInstruction("CE", 2, "CE");
                instr.Delegate = instr.CE;
                instructions.Add(16, instr);

                // CNE instruction - 17
                instr = new FGMSEInstruction("CNE", 2, "CNE");
                instr.Delegate = instr.CNE;
                instructions.Add(17, instr);

                // SCE instruction - 18
                instr = new FGMSEInstruction("SCE", 2, "SCE");
                instr.Delegate = instr.SCE;
                instructions.Add(18, instr);

                // SCNE instruction - 19
                instr = new FGMSEInstruction("SCNE", 2, "SCNE");
                instr.Delegate = instr.SCNE;
                instructions.Add(19, instr);

                // RE instruction - 20
                instr = new FGMSEInstruction("RE", 0, "RE");
                instr.Delegate = instr.RE;
                instructions.Add(20, instr);

                // RNE instruction - 21
                instr = new FGMSEInstruction("RNE", 0, "RNE");
                instr.Delegate = instr.RNE;
                instructions.Add(21, instr);

                // EE instruction - 22
                instr = new FGMSEInstruction("EE", 0, "EE");
                instr.Delegate = instr.EE;
                instructions.Add(22, instr);

                // ENE instruction - 23
                instr = new FGMSEInstruction("ENE", 0, "ENE");
                instr.Delegate = instr.ENE;
                instructions.Add(23, instr);

                // PUSH instruction - 24
                instr = new FGMSEInstruction("PUSH", 2, "PUSH");
                instr.Delegate = instr.PUSH;
                instructions.Add(24, instr);

                // PUSH instruction - 25
                instr = new FGMSEInstruction("POP", 2, "POP");
                instr.Delegate = instr.POP;
                instructions.Add(25, instr);

                Types.Sets.Add(0x02, new FGMSECInstructionSet());

            }
            catch (Sys.Exception ex)
            {
                Sys.Console.WriteLine($"Fatal error loading instructions: {ex.Message}");
            }
        }
        public override Instruction GetInstruction(byte[] rawCode)
        {
            throw new Sys.NotImplementedException();
        }
        public override Instruction GetInstruction(int[] rawCode)
        {
            uint pos = CPU.GetRegData((byte)FGMSECpu.FGMSERegisters.IP);
            Instruction instr;
            try
            {
                instr = instructions[(uint)rawCode[(int)pos]];
            }
            catch(Sys.Exception ex)
            {
                Sys.Console.WriteLine($"Instruction not found! ({(uint)rawCode[(int)pos]})");
                return null;
            }
            try
            {
                pos++;
                List<uint> tempArgs = new List<uint>();
                for (int i = 0; i < instr.OpCode; i++)
                {
                    tempArgs.Add((uint)rawCode[(int)pos]);
                    if ((uint)rawCode[(int)pos] == 0xFE001AFC)
                    {
                        i--;
                    }
                    pos++;
                }
                instr.args32 = tempArgs;
                CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.IP, pos);
            }
            catch (global::System.Exception ex)
            {
                global::System.Console.WriteLine($"Error while loading instruction: {ex.Message}");
            }
            return instr;
        }
        public override Instruction GetInstruction(short[] rawCode)
        {
            throw new Sys.NotImplementedException();
        }
    }
}
