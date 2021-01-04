using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Emulation.Threading
{
    public class TaskManager
    {
        public static List<Thread> LoadedThreads { get; private set; } = new List<Thread>();
        internal static void Start(Thread thread)
        {
            LoadedThreads.Add(thread);
        }
        public static void Next()
        {
            foreach (Thread thread in LoadedThreads)
            {
                DateTime startTime = DateTime.Now;
                DateTime lastTime = startTime;
                while (lastTime.Second < startTime.Second + 1)
                {
                    thread.exec.NextInstruction();
                    lastTime = DateTime.Now;
                }
            }
        }
    }
}
