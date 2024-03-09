using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Unity.Backend
{
    [RequireComponent(typeof(AsyncTaskConsole))]
    public class AsyncTaskQueen : MonoBehaviour
    {
        // [[ PRIVATE VARIABLES ]] ===== >>
        public Queue<AsyncTaskBot> taskBotQueue = new();

        // [[ PUBLIC VARIABLES ]] ===== >>
        public string taskQueenName = "AsyncTaskQueen";

        // Initializes the AsyncTaskQueen
        public virtual void Initialize()
        {
            throw new NotImplementedException();
        }

        // Creates a new AsyncTaskBot and adds it to the taskBotQueue
        public void NewTaskBot(string name, Func<Task> task)
        {
            Guid guidId = Guid.NewGuid();
            AsyncTaskBot newTaskBot = new AsyncTaskBot(name, task);
            taskBotQueue.Enqueue(newTaskBot);

            Debug.Log("New AsyncTaskBot added to the taskBotQueue. Name: " + name);
        }

        // Executes all the AsyncTaskBots in the taskBotQueue
        public async Task ExecuteAllBotsInQueue()
        {
            while (taskBotQueue.Count > 0)
            {
                AsyncTaskBot taskBot = taskBotQueue.Dequeue();
                await taskBot.ExecuteAsync();
                await Task.Yield();

                taskBot.Dispose();
                Debug.Log("AsyncTaskBot executed and disposed.");
            }
        }
    }
}
