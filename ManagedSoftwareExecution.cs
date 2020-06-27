using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;

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
    }

    class Software
    {
        [ManifestResourceStream(ResourceName = "ManagedSoftwareImplementation.test_softwares_sources.test")] public static byte[] file;
        private delegate void DEL(byte[] kArgs);
        public delegate void SystemCallFunc();
        public SystemCallFunc[] syscalls = new SystemCallFunc[256];
        public readonly Dictionary<byte, Delegate> opcodes;
        public readonly Dictionary<byte, int> opcodesLen;
        public REGISTERS regs = new REGISTERS();
        private readonly byte execArch;
        private readonly byte[] VRAM;
        private readonly byte[] opcode;
        private int opindex = 0;

        private void MOV(byte[] kArgs)
        {
            SetPointerData(GetPointerData(kArgs[0]), kArgs[1]);
            SetPointerData(0, kArgs[0]);
        }
        private void SET(byte[] kArgs)
        {
            SetPointerData(GetPointerData(kArgs[0]), kArgs[1]);
        }
        private void SYSCALL(byte[] kArgs)
        {
            SystemCallFunc syscall = syscalls[kArgs[0]];
            syscall();
        }
        private void push(byte[] kArgs)
        {
            SetPointerData(GetPointerData(kArgs[0]), regs.EBP);
            regs.EBP = (byte)(regs.EBP + 0x01);
            SetPointerData(0, regs.EBP);
        }
        private void pop(byte[] kArgs)
        {
            SetPointerData(GetPointerData((byte)(regs.EBP - 0x01)), kArgs[0]);
            regs.EBP = (byte)(regs.EBP - 0x01);
            SetPointerData(0, regs.EBP);
        }

        public Software()
        {
            //Console.WriteLine("Calling..");
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
            //MOV - 0x01
            DEL del = new DEL(MOV);
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
            //PUSH - 0x04
            del = new DEL(pop);
            ocodes.Add(0x04, del);
            ocodesLen.Add(0x04, 1);
            //POP - 0x05
            del = new DEL(push);
            ocodes.Add(0x05, del);
            ocodesLen.Add(0x05, 1);


            opcodes = ocodes;
            opcodesLen = ocodesLen;

            //clear all temp
            ram = new List<byte>();
            _opcode = new List<byte>();
            ocodes = new Dictionary<byte, Delegate>();
            ocodesLen = new Dictionary<byte, int>();
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

            regs.ESP = 0x0c;
            regs.EBP = 0x0d;

            regs.EFB = (byte)(regs.EBP + 1);
        }

        public Software(string path)
        {
        }

        public void runNextInstr()
        {
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
                else
                    kargs.Add(a);
            }
            DEL del = (DEL)opcodes[ocode];
            del(kargs.ToArray());
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
                byte res = opcode[opindex];
                opindex += 1;
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine($"APPLICATION END UNEXPECTED: {e.Message}");
                Console.WriteLine($"Last known address: 0x{opindex}/0x{opcode.Length}");
                return 0x00;
            }
        }

        private void refreshBasePointer(byte newPos)
        {
            SetPointerData(0x00, regs.EBP);
            SetPointerData(0x00, regs.EFB);
            regs.EBP = newPos;
            regs.EFB = (byte)(regs.EBP + 1);
        }
    }
}
