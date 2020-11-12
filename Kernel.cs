using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using IL2CPU.API.Attribs;
using ManagedSoftwareImplementation.SoftwareExecution;
using Cosmos.System.FileSystem;

namespace ManagedSoftwareImplementation
{
    public class Kernel : Sys.Kernel
    {
        [ManifestResourceStream(ResourceName = "ManagedSoftwareImplementation.test_softwares_sources.test.o")] public static byte[] file;

        SoftwareExecution.Software software;
        
        private void SysCallPrintLN(SoftwareExecution.Software sender)
        {
            Console.WriteLine((char)(sender.GetPointerData((byte)(sender.regs.EAX))));
        }
        private void SysCallPrint(SoftwareExecution.Software sender)
        {
            Console.Write((char)(sender.GetPointerData((byte)(sender.regs.EAX))));
        }
        private void SysCallClear(SoftwareExecution.Software sender)
        {
            Console.Clear();
        }

        protected override void BeforeRun()
        {
            software = new SoftwareExecution.Software(file);
            var vfs = new Cosmos.System.FileSystem.CosmosVFS();
            Cosmos.System.FileSystem.VFS.VFSManager.RegisterVFS(vfs);
            Console.Clear();
        }

        protected override void Run()
        {
            Console.WriteLine("#####Managed Software Execution#####\n[Ver: Alpha 3.7, Build: 09112020_0950]\n##TEST PROGRAMS:\n");
            software.AddSysCall(SysCallPrintLN, 0);
            software.AddSysCall(SysCallPrint, 1);
            software.AddSysCall(SysCallClear, 2);
            TasksManager tasksManager = new TasksManager();
            tasksManager.Start(software);
            tasksManager.StartExecution();
            for (; ; );
        }
    }
}
