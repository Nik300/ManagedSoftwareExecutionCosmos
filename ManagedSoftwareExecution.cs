using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ManagedSoftwareImplementation.SoftwareExecution
{
    struct REGISTERS
    {
        public byte EAX;
        public byte EBX;
        public byte ECX;
        public byte EDX;

        public byte SYSCALL_REF;

        public byte GFLAG;
        public byte EEFLAG;

        public byte CR0;
        public byte CR1;
        public byte CR2;
        public byte CR3;
        public byte CR4;

        public byte ESP;
        public byte EBP;

        public byte EFB;

        public byte EIP;
        public byte EDP;
    }

    class Software
    {
        public static byte[] file;
        private delegate void DEL(byte[] kArgs);
        public delegate void SystemCallFunc(Software sender);
        public SystemCallFunc[] syscalls = new SystemCallFunc[256];
        public readonly Dictionary<byte, Delegate> opcodes;
        public readonly Dictionary<byte, int> opcodesLen;
        public REGISTERS regs = new REGISTERS();
        List<byte> returns = new List<byte>();
        public bool run = true;
        private bool last_command = true;
        private readonly byte execArch;
        public byte[] VRAM { get; protected private set; }
        public readonly byte[] opcode;

        private void NOP(byte[] kArgs)
        {
            return;
        }
        private void MOV(byte[] kArgs)
        {
            if (kArgs[1] == regs.EIP)
            {
                SetPointerData(0x00, regs.EEFLAG);
                if (GetPointerData(regs.CR0) != 0x04)
                {
                    SetPointerData(0x01, regs.EEFLAG);
                    return;
                }
            }
            SetPointerData(GetPointerData(kArgs[0]), kArgs[1]);
            SetPointerData(0, kArgs[0]);
        }
        private void SET(byte[] kArgs)
        {
            if (kArgs[1] == regs.EIP)
            {
                SetPointerData(0x00, regs.EEFLAG);
                if (GetPointerData(regs.CR0) != 0x04)
                {
                    SetPointerData(0x01, regs.EEFLAG);
                    return;
                }
            }
            SetPointerData(GetPointerData(kArgs[0]), kArgs[1]);
        }
        private void SYSCALL(byte[] kArgs)
        {
            SystemCallFunc syscall = syscalls[kArgs[0]];
            syscall(this);
        }
        private void push(byte[] kArgs)
        {
            SetPointerData(GetPointerData(kArgs[0]), regs.EBP);
            RefreshBasePointer((byte)(regs.EBP + 0x01));
        }
        private void pop(byte[] kArgs)
        {
            SetPointerData(GetPointerData((byte)(regs.EBP - 0x01)), kArgs[0]);
            RefreshBasePointer((byte)(regs.EBP - 0x01));
        }
        private void jump(byte[] kArgs)
        {
            SetPointerData(kArgs[0], regs.EIP);
        }
        private void call(byte[] kArgs)
        {
            returns.Add((byte)(GetPointerData(regs.EIP)));
            jump(kArgs);
        }
        private void ret(byte[] kArgs)
        {
            byte toJump = returns[returns.ToArray().Length - 1];
            returns.Remove(toJump);
            jump(new byte[] { toJump });
        }
        public void end(byte[] kArgs)
        {
            run = false;
        }
        public void compare(byte[] kArgs)
        {
            if (GetPointerData(kArgs[0]) <= GetPointerData(kArgs[1])) last_command = false;
            else last_command = true;
        }
        public void jumpif(byte[] kArgs)
        {
            if (last_command) jump(kArgs);
        }
        public void jumpifnot(byte[] kArgs)
        {
            if (!last_command) jump(kArgs);
        }
        public void add(byte[] kArgs)
        {
            SetPointerData((byte)(GetPointerData(kArgs[0]) + GetPointerData(kArgs[1])), kArgs[1]);
        }
        public void sub(byte[] kArgs)
        {
            SetPointerData((byte)(GetPointerData(kArgs[0]) - GetPointerData(kArgs[1])), kArgs[1]);
        }
        public void get(byte[] kArgs)
        {
            SetPointerData(GetPointerData(GetPointerData(kArgs[0])), kArgs[1]);
        }
        public void put(byte[] kArgs)
        {
            SetPointerData(GetPointerData(kArgs[0]), GetPointerData(kArgs[1]));
        }
        public Software(byte[] raw_code)
        {
            file = raw_code;
            //INIT ARCH type
            execArch = file[0];

            //INIT VMEM
            List<byte> ram = new List<byte>();
            for (int i = 1; i < file.Length; i++)
            {
                if (file[i] == 0xFF && file[i + 1] == 0xFA)
                    break;

                ram.Add(file[i]);
            }

            VRAM = ram.ToArray();

            //INIT opcode
            List<byte> _opcode = new List<byte>();
            for (int i = VRAM.Length + 1; i < file.Length; i++)
            {
                if (file[i] == 0xFF || file[i] == 0xFA)
                    continue;
                _opcode.Add(file[i]);
            }

            opcode = _opcode.ToArray();

            //INIT REGISTERS
            if (execArch == 0x09)
                load32Regs();
            
            //INIT OPCODES
            Dictionary<byte, Delegate> ocodes = new Dictionary<byte, Delegate>();
            Dictionary<byte, int> ocodesLen = new Dictionary<byte, int>();
            //NOP - 0x00
            DEL del = new DEL(NOP);
            ocodes.Add(0x00, del);
            ocodesLen.Add(0x00, 0);
            //MOV - 0x01
            del = new DEL(MOV);
            ocodes.Add(0x01, del);
            ocodesLen.Add(0x01, 2);
            //SET - 0x02
            del = new DEL(SET);
            ocodes.Add(0x02, del);
            ocodesLen.Add(0x02, 2);
            //SYSCALL - 0x03
            del = new DEL(SYSCALL);
            ocodes.Add(0x03, del);
            ocodesLen.Add(0x03, 1);
            //POP - 0x04
            del = new DEL(pop);
            ocodes.Add(0x04, del);
            ocodesLen.Add(0x04, 1);
            //PUSH - 0x05
            del = new DEL(push);
            ocodes.Add(0x05, del);
            ocodesLen.Add(0x05, 1);
            //JUMP - 0x06
            del = new DEL(jump);
            ocodes.Add(0x06, del);
            ocodesLen.Add(0x06, 1);
            //CALL - 0x07
            del = new DEL(call);
            ocodes.Add(0x07, del);
            ocodesLen.Add(0x07, 1);
            //RET - 0x08
            del = new DEL(ret);
            ocodes.Add(0x08, del);
            ocodesLen.Add(0x08, 0);
            //END - 0x09
            del = new DEL(end);
            ocodes.Add(0x09, del);
            ocodesLen.Add(0x09, 0);
            //COMPARE - 0x0a
            del = new DEL(compare);
            ocodes.Add(0x0a, del);
            ocodesLen.Add(0x0a, 2);
            //JE - 0x0b
            del = new DEL(jumpif);
            ocodes.Add(0x0b, del);
            ocodesLen.Add(0x0b, 1);
            //ADD - 0x0c
            del = new DEL(add);
            ocodes.Add(0x0c, del);
            ocodesLen.Add(0x0c, 2);
            //SUB - 0x0d
            del = new DEL(sub);
            ocodes.Add(0x0d, del);
            ocodesLen.Add(0x0d, 2);
            //JNE - 0x0e
            del = new DEL(jumpifnot);
            ocodes.Add(0x0e, del);
            ocodesLen.Add(0x0e, 2);
            //GET - 0x0f
            del = new DEL(get);
            ocodes.Add(0x0f, del);
            ocodesLen.Add(0x0f, 2);
            //PUT - 0x10
            del = new DEL(put);
            ocodes.Add(0x10, del);
            ocodesLen.Add(0x10, 2);

            opcodes = ocodes;
            opcodesLen = ocodesLen;

            //clear all temp
            ram = new List<byte>();
            _opcode = new List<byte>();
            ocodes = new Dictionary<byte, Delegate>();
            ocodesLen = new Dictionary<byte, int>();

            //initialize stack by pushing a null argument
            push(new byte[] { 0x00 });
        }

        public void AddSysCall(SystemCallFunc syscallfunc, int syscallnum)
        {
            syscalls[syscallnum] = syscallfunc;
        }

        private void load32Regs()
        {
            regs.EAX = 0x00;
            regs.EBX = 0x01;
            regs.ECX = 0x02;
            regs.EDX = 0x03;

            regs.SYSCALL_REF = 0x04;

            regs.GFLAG = 0x05;
            regs.EEFLAG = 0x06;

            regs.CR0 = 0x07;
            regs.CR1 = 0x08;
            regs.CR2 = 0x09;
            regs.CR3 = 0x0a;
            regs.CR4 = 0x0b;

            regs.EFB = 0x0c;

            regs.ESP = 0x0d;
            regs.EBP = 0x0e;

            regs.EIP = (byte)(regs.EFB + 1);
            regs.EDP = (byte)(regs.EIP + 1);
        }

        public Software(string path)
        {
            file = System.IO.File.ReadAllBytes(path);
        }

        public void runNextInstr()
        {
            try
            {
                if (!run) return;
                byte ocode = getOPByte();
                List<byte> kargs = new List<byte>();
                for (int i = 1; i <= opcodesLen[ocode]; i++)
                {
                    byte a = getOPByte();
                    if (a == 0xFB)
                    {
                        SetPointerData(getOPByte(), regs.EFB);
                        kargs.Add(regs.EFB);
                    }
                    else if (a == 0xFF)
                    {
                        byte point = GetPointerData(getOPByte());
                        kargs.Add(point);
                        Console.WriteLine($"pointer: {point}");
                    }
                    else
                        kargs.Add(a);
                }
                DEL del = (DEL)opcodes[ocode];
                del(kargs.ToArray());
            }
            catch (Exception ex)
            {
                Panic(ex);
            }
        }

        public void Panic(Exception ex)
        {
            Console.WriteLine($"This application committed an illegal action and has been shut down\nINFO: \"{ex.Message}\"");
            run = false;
            Console.WriteLine($"Beginning dump of Application's memory...");
            if (!File.Exists("0:\\dump.dat"))
                File.Create("0:\\dump.dat").Close();
            File.WriteAllBytes("0:\\dump.dat", VRAM);
            Console.WriteLine($"Dump complete! Clearing Application's memory and leaving RAM...");
            for (int i = 0; i < VRAM.Length; i++)
                VRAM[i] = 0x00;
            VRAM = null;
            Console.WriteLine($"Clear complete! Now leaving...");
        }

        public byte GetPointerData(byte pointer)
        {
            return VRAM[pointer];
        }

        public void SetPointerData(byte data, byte pointer)
        {
            VRAM[pointer] = data;
        }

        private byte getOPByte()
        {
            try
            {
                byte res = opcode[GetPointerData(regs.EIP)];
                SetPointerData((byte)(GetPointerData(regs.EIP) + 0x01), regs.EIP);
                return res;
            }
            catch (Exception ex)
            {
                Panic(ex);
                return 0x00;
            }
        }

        private void RefreshBasePointer(byte newPos)
        {
            byte EIP = GetPointerData(regs.EIP);
            byte EDP = GetPointerData(regs.EDP);
            SetPointerData(0x00, regs.EFB);
            SetPointerData(0x00, regs.EDP);
            regs.EBP = newPos;
            regs.EIP = (byte)(regs.EBP + 1);
            regs.EDP = (byte)(regs.EIP + 1);
            SetPointerData(EIP, regs.EIP);
            SetPointerData(EDP, regs.EDP);
        }
    }
}
