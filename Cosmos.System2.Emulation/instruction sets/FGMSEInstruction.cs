using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Emulation
{
    public class FGMSEInstruction: Instruction
    {
        public FGMSEInstruction(string mnem = "NOP", int op = 0, string format = "", InstructionDelegate Delegate = null): base(mnem, op, format)
        {
            if (Delegate == null)
            {
                this.Delegate = new InstructionDelegate((Executable caller) => { });
            }
            else
                this.Delegate = Delegate;
        }
        private static int GetPointerAddress(List<uint> args, FGMSECInstructionSet instrSet, Executable caller, out int shift)
        {
            int addr = 0;
            shift = 1;
            if (isRegister(args[0]))
            {
                addr = (int)instrSet.CPU.GetRegData(BitConverter.GetBytes(args[1])[3]);
            }
            else if (isData(args[0]))
            {
                addr = (int)args[1];
            }
            else if (isPointer(args[0]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 1; i < args.Count; i++)
                    _args.Add(args[i]);
                addr = caller.Memory.ReadInt32(GetPointerAddress(_args, instrSet, caller, out int addShift));
                shift += addShift;
            }
            else
            {
                global::System.Console.WriteLine($"No way to set address found ({args[0]})");
                addr = 0;
                instrSet.CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.C1, 0xFF00FFA1);
            }
            return addr;
        }
        private static uint returnValue(List<uint> args, FGMSECInstructionSet instrSet, Executable caller, out int shift, bool ClearValue = false)
        {
            uint data = 0x00;
            shift = 0;
            if (isRegister(args[0]))
            {
                data = instrSet.CPU.GetRegData(BitConverter.GetBytes(args[1])[3]);
                if (ClearValue)
                    instrSet.CPU.ClearRegData(BitConverter.GetBytes(args[1])[3]);
            }
            else if (isData(args[0]))
            {
                data = args[1];
            }
            else if (isPointer(args[0]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 1; i < args.Count; i++)
                    _args.Add(args[i]);
                int addr = GetPointerAddress(_args, instrSet, caller, out shift);
                data = (uint)caller.Memory.ReadInt32(addr);
                if (ClearValue)
                    caller.Memory.WriteInt32(addr, 0x00);
            }
            else
            {
                throw new Exception("Cannot retrive value of one or more elements");
            }
            return data;
        }
        private static uint returnValueFromAddress(List<uint> args, FGMSECInstructionSet instrSet, Executable caller, out int shift)
        {
            uint data = 0x00;
            shift = 0;
            if (isRegister(args[0]))
            {
                data = instrSet.CPU.GetRegData(BitConverter.GetBytes(args[1])[3]);
            }
            else if (isPointer(args[0]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 1; i < args.Count; i++)
                    _args.Add(args[i]);
                int addr = GetPointerAddress(_args, instrSet, caller, out shift);
                data = (uint)caller.Memory.ReadInt32(addr);
            }
            else
            {
                throw new Exception("Cannot retrive value of one or more elements");
            }
            return data;
        }

        //0xFE001AFA = register
        //0xFE001AFB = data
        //0xFE001AFC = pointer
        private static bool isRegister(uint preset)
        {
            preset -= 0xFE000000;
            return preset == 0x1AFA;
        }
        private static bool isData(uint preset)
        {
            preset -= 0xFE000000;
            return preset == 0x1AFB;
        }
        private static bool isPointer(uint preset)
        {
            preset -= 0xFE000000;
            return preset == 0x1AFC;
        }

        //Instructions delegates

        //math delegates
        public void MOV(Executable caller) // moves element to another position - len: 4
        {
            uint data;
            var instrSet = ((FGMSECInstructionSet)caller.usingInstructionSet);
            data = returnValue(args32, instrSet, caller, out int shift, true);
            if (isRegister(args32[2 + shift]))
            {
                instrSet.CPU.SetRegData(BitConverter.GetBytes(args32[3 + shift])[3], data);
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                int addr = GetPointerAddress(_args, instrSet, caller, out _);
                caller.Memory.WriteInt32(addr, (int)data);
            }
        }
        public void SET(Executable caller) // sets data to determined position - len: 4
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            List<uint> args = new List<uint>();
            args.Add(args32[0]);
            args.Add(args32[1]);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (isRegister(args32[2 + shift]))
            {
                instrSet.CPU.SetRegData(BitConverter.GetBytes(args32[3 + shift])[3], data);
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), (int)data);
            }
        }
        public void ADD(Executable caller) // adds data to determined position - len: 4
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            List<uint> args = new List<uint>();
            args.Add(args32[0]);
            args.Add(args32[1]);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (isRegister(args32[2 + shift]))
            {
                byte reg = BitConverter.GetBytes(args32[3 + shift])[3];
                instrSet.CPU.SetRegData(reg, (uint)(instrSet.CPU.GetRegData(reg) + data));
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), (int)(caller.Memory.ReadInt32((int)args32[3]) + data));
            }
        }
        public void SUB(Executable caller) // subtracts data from determined position - len: 4
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            List<uint> args = new List<uint>();
            args.Add(args32[0]);
            args.Add(args32[1]);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (isRegister(args32[2 + shift]))
            {
                byte reg = BitConverter.GetBytes(args32[3 + shift])[3];
                instrSet.CPU.SetRegData(reg, (uint)(instrSet.CPU.GetRegData(reg) - data));
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), (int)(caller.Memory.ReadInt32((int)args32[3]) - data));
            }
        }
        public void DIV(Executable caller) // devides data from determined position - len: 4
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            List<uint> args = new List<uint>();
            args.Add(args32[0]);
            args.Add(args32[1]);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (isRegister(args32[2 + shift]))
            {
                byte reg = BitConverter.GetBytes(args32[3 + shift])[3];
                instrSet.CPU.SetRegData(reg, (uint)(instrSet.CPU.GetRegData(reg) / data));
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), (int)(caller.Memory.ReadInt32((int)args32[3]) / data));
            }
        }
        public void MUL(Executable caller) // multiplies data to determined position - len: 4
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            List<uint> args = new List<uint>();
            args.Add(args32[0]);
            args.Add(args32[1]);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (isRegister(args32[2 + shift]))
            {
                byte reg = BitConverter.GetBytes(args32[3 + shift])[3];
                instrSet.CPU.SetRegData(reg, (uint)(instrSet.CPU.GetRegData(reg) * data));
            }
            else if (isPointer(args32[2 + shift]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 3 + shift; i < args32.Count; i++)
                    _args.Add(args32[i]);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), (int)(caller.Memory.ReadInt32((int)args32[3]) * data));
            }
        }

        //OP Delegates
        public void JMP(Executable caller) // jumps to determined position
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            data = returnValue(args32, instrSet, caller, out int shift);

            instrSet.CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.IP, data);
        }
        public void CALL(Executable caller) // calls determined position
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            data = returnValue(args32, instrSet, caller, out int shift);

            caller.Returns.Add((int)(instrSet.CPU.GetRegData((byte)FGMSECpu.FGMSERegisters.IP)));
            instrSet.CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.IP, data);
        }
        public void RET(Executable caller) // returns to the previous called section
        {
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);

            if (caller.Returns.Count == 0) return;

            instrSet.CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.IP, (uint)caller.Returns[caller.Returns.Count - 1]);
            List<int> temp = new List<int>();
            for (int i = 0; i < caller.Returns.Count - 1; i++)
                temp.Add(caller.Returns[i]);
            caller.Returns = temp;
        }
        public void SYSCALL(Executable caller) // calls the specified system call
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            data = returnValue(args32, instrSet, caller, out int shift);
            if (!caller.RunSystemCall((int)data))
                instrSet.CPU.SetRegData((byte)FGMSECpu.FGMSERegisters.C1, 0xFEFEFEFE);
        }
        public void END(Executable caller) // terminates the execution of the specified program
        {
            caller.running = false;
        }

        //  CONDITIONS INSTRUCTIONS
        public void CMP(Executable caller) // compares two datas between them
        {
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            uint arg1 = returnValue(args32, instrSet, caller, out int shift);
            List<uint> _args = new List<uint>();
            for (int i = 2 + shift; i < args32.Count; i++)
                _args.Add(args32[i]);
            uint arg2 = returnValue(_args, instrSet, caller, out _);
            caller.lastInstruction = arg1 > arg2;
        }
        public void EQU(Executable caller) // compares two datas between them
        {
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            uint arg1 = returnValue(args32, instrSet, caller, out int shift);
            List<uint> _args = new List<uint>();
            for (int i = 2 + shift; i < args32.Count; i++)
                _args.Add(args32[i]);
            uint arg2 = returnValue(_args, instrSet, caller, out _);
            caller.lastInstruction = arg1 == arg2;
        }
        public void JE(Executable caller)
        {
            if (caller.lastInstruction) JMP(caller);
        }
        public void JNE(Executable caller)
        {
            if (!caller.lastInstruction) JMP(caller);
        }
        public void CE(Executable caller)
        {
            if (caller.lastInstruction) CALL(caller);
        }
        public void CNE(Executable caller)
        {
            if (!caller.lastInstruction) CALL(caller);
        }
        public void SCE(Executable caller)
        {
            if (caller.lastInstruction) SYSCALL(caller);
        }
        public void SCNE(Executable caller)
        {
            if (!caller.lastInstruction) SYSCALL(caller);
        }
        public void RE(Executable caller)
        {
            if (caller.lastInstruction) RET(caller);
        }
        public void RNE(Executable caller)
        {
            if (!caller.lastInstruction) RET(caller);
        }
        public void EE(Executable caller)
        {
            if (caller.lastInstruction) END(caller);
        }
        public void ENE(Executable caller)
        {
            if (!caller.lastInstruction) END(caller);
        }

        //Stack delegates
        public void PUSH(Executable caller)
        {
            uint data;
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            data = returnValue(args32, instrSet, caller, out int shift);

            caller.Memory.Stack.Push((int)data);
        }
        public void POP(Executable caller)
        {
            var instrSet = (FGMSECInstructionSet)(caller.usingInstructionSet);
            if (isRegister(args32[0]))
            {
                byte reg = BitConverter.GetBytes(args32[1])[3];
                int data = 0;
                
                caller.Memory.Stack.Try_Pop(ref data);
                instrSet.CPU.SetRegData(reg, (uint)(data));
            }
            else if (isPointer(args32[0]))
            {
                List<uint> _args = new List<uint>();
                for (int i = 1; i < args32.Count; i++)
                    _args.Add(args32[i]);
                int data = 0;
                caller.Memory.Stack.Try_Pop(ref data);
                caller.Memory.WriteInt32(GetPointerAddress(_args, instrSet, caller, out _), data);
            }
        }
    }
}
