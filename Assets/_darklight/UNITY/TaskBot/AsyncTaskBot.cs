using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Darklight.Unity.Backend
{
    public class AsyncTaskBot : TaskBot 
    {
        public Func<Task> task;
        public AsyncTaskBot(string name, Func<Task> task)
        {
            base.name = name;
            this.task = task;
        }

		public override void Execute()
        {
            _ = ExecuteAsync();
        }

        public async Task ExecuteAsync()
        {
            try
            {
                stopwatch.Restart();
                await task();
            }
            finally
            {
                stopwatch.Stop();
                executionTime = stopwatch.ElapsedMilliseconds;
            }
        }
    }
}