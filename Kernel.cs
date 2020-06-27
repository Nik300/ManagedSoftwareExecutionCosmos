using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace ManagedSoftwareImplementation
{
    public class Kernel : Sys.Kernel
    {
        SoftwareExecution.Software software;
        
        private void SysCallPrintLN()
        {
            Console.WriteLine((char)(software.GetPointerData((byte)(software.regs.EFB))));
        }
        private void SysCallPrint()
        {
            Console.Write((char)(software.GetPointerData((byte)(software.regs.EFB))));
        }
        private void SysCallClear()
        {
            if (software.regs.CR0 == 0x01)
                Console.Clear();
            else if (software.regs.CR0 == 0x02)
                Console.SetCursorPosition(software.regs.CR1, software.regs.CR2);
        }

        protected override void BeforeRun()
        {
            software = new SoftwareExecution.Software();
        }

        protected override void Run()
        {
            software.AddSysCall(SysCallPrintLN, 0);
            software.AddSysCall(SysCallPrint, 1);
            software.runNextInstr();
            software.runNextInstr();
            software.runNextInstr();
            for (; ; );
        }
    }
}
