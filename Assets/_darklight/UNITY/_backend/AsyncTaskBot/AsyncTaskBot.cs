using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Darklight.Unity.Backend
{
    public class AsyncTaskBot : IDisposable
    {
        /// <summary> Holds reference data </summary>
        public struct Profiler
        {
            public Guid guidId;
            public string name;
            public long executionTime;
            public Profiler(Guid guidId, string name)
            {
                this.guidId = guidId;
                this.name = name;
                this.executionTime = 0;
            }
        }

        public Profiler taskProfiler;
        private Stopwatch _stopwatch;
        private Func<Task> _task;
        public AsyncTaskBot(Guid guidID, string name, Func<Task> task)
        {
            taskProfiler = new Profiler(guidID, name);
            _task = task;
            _stopwatch = Stopwatch.StartNew();
        }

        public async Task Execute()
        {
            try
            {
                _stopwatch.Restart();
                await _task();
            }
            finally
            {
                _stopwatch.Stop();
                taskProfiler.executionTime = _stopwatch.ElapsedMilliseconds;
            }
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _stopwatch.Reset();
            _stopwatch = null;
        }
    }
}