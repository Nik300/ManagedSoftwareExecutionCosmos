using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ManagedSoftwareImplementation.SoftwareExecution
{
    class TasksManager
    {
        readonly protected List<Software> Tasks;
        public bool execute = false;
        public TasksManager()
        {
            Tasks = new List<Software>();
        }
        public void StartExecution()
        {
            try
            {
                execute = true;
                while (execute)
                {
                    foreach (Software Task in Tasks)
                    {
                        for (; ; )
                        {
                            Task.runNextInstr();
                            if (Task.run == false)
                            {
                                Tasks.Remove(Task);
                                if (Tasks.ToArray().Length == 0) execute = false;
                            }
                            if (Task.opcode[Task.GetPointerData(Task.regs.EIP)] == 0x00)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public void StopExecution()
        {
            execute = false;
        }
        public void Start(Software Task)
        {
            Tasks.Add(Task);
        }
    }
}
