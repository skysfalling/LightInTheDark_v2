using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Unity.Backend
{
    public class AsyncTaskQueen : MonoBehaviour 
    {
        // [[ PRIVATE VARIABLES ]] ===== >>
        private Queue<AsyncTaskBot> _taskBotQueue = new();

		// [[ PUBLIC VARIABLES ]] ===== >>
        public string taskQueenName = "AsyncTaskQueen";
        public List<AsyncTaskBot> AsyncTaskBots = new();

        public void NewTaskBot(string name, Func<Task> task)
        {
            Guid guidId = Guid.NewGuid();
            AsyncTaskBot newTaskBot = new AsyncTaskBot(name, task);
            _taskBotQueue.Enqueue(newTaskBot);
        }

        public async Task ExecuteAllBotsInQueue()
        {
            while (_taskBotQueue.Count > 0)
            {
                AsyncTaskBot taskBot = _taskBotQueue.Dequeue();
                await taskBot.ExecuteAsync();

                taskBot.Dispose();
            }
        }
    }
}

