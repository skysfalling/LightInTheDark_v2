using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Darklight.Unity.Backend
{
    public class AsyncTaskQueen : MonoBehaviour 
    {    
        private Queue<AsyncTaskBot> _taskBotQueue = new();

        public string Name { get; private set;}
        public  List<AsyncTaskBot.Profiler> ProfilerData { get; private set; } = new();
        public AsyncTaskQueen(string name = "AsyncTaskQueen")
        {
            Name = name;
        }

        public void NewTaskBot(string name, Func<Task> task)
        {
            Guid guidId = Guid.NewGuid();
            AsyncTaskBot newTaskBot = new AsyncTaskBot(guidId, name, task);
            _taskBotQueue.Enqueue(newTaskBot);
        }

        public async Task ExecuteAllBotsInQueue()
        {
            while (_taskBotQueue.Count > 0)
            {
                AsyncTaskBot taskBot = _taskBotQueue.Dequeue();
                await taskBot.Execute();

                ProfilerData.Add(taskBot.taskProfiler);
                taskBot.Dispose();
            }
        }
    }
}