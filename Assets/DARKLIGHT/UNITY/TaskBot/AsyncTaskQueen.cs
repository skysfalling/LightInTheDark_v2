namespace Darklight.Unity.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Represents a queen that manages a queue of AsyncTaskBots and executes them asynchronously.
    /// </summary>
    public class AsyncTaskQueen : TaskQueen
    {
        public void NewAsyncTaskBot(string name, Func<Task> task)
        {
            AsyncTaskBot newTaskBot = new AsyncTaskBot(name, this, task);
            Enqueue(newTaskBot);
            Console.Log(newTaskBot, "New Task Bot Created");
        }

        public async override void ExecuteAllTasks()
        {
            await Awaitable.BackgroundThreadAsync();
            base.ExecuteAllTasks();
        }
    }
}
