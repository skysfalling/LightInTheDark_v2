using System;
using System.Diagnostics;
using System.Text;

namespace Darklight.Unity.Backend
{
    [Serializable]
    public class TaskBot : IDisposable
    {
        Guid _guidId = Guid.NewGuid();

        public string guidId => _guidId.ToString();
        public string name = "NewTaskBot";
        public long executionTime = 0;
        public Stopwatch stopwatch;

        public TaskBot()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch = null;
        }
    }
}