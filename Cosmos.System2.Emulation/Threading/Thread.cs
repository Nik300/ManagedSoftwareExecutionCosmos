using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Emulation.Threading
{
    public class Thread
    {
        internal readonly Executable exec;
        public Thread(Executable exec)
        {
            this.exec = exec;
        }
        public Thread(byte[] exec)
        {
            this.exec = new Executable(exec);
        }

        public void Start()
        {
            //exec.ReadData();
            TaskManager.Start(this);
        }

    }
}
