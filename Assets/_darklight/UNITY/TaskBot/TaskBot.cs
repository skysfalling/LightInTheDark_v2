using System;
using System.Diagnostics;
using System.Text;

namespace Darklight.Unity.Backend
{
    [Serializable]
    public class TaskBot : IDisposable
    {

		public struct Profile {
            public string name;
            public Guid guidId;
            public long executionTime;
            public Profile(TaskBot bot)
            {
				name = bot.name;
                guidId = bot.guidId;
                executionTime = bot.executionTime;
            }
        }

        public Guid guidId = Guid.NewGuid();
        public string name = "NewTaskBot";
        public long executionTime = 0;
        public Stopwatch stopwatch;

        public TaskBot()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public Profile NewProfile()
        {
            return new Profile(this);
        }

        public void Dispose()
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch = null;
        }
    }
}