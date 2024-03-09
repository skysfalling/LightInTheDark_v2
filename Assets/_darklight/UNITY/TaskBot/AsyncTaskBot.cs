using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Darklight.Unity.Backend
{
    /// <summary>
    /// Represents an asynchronous task bot that extends the TaskBot class.
    /// </summary>
    public class AsyncTaskBot : TaskBot 
    {
        /// <summary>
        /// Delegate that represents an asynchronous task.
        /// </summary>
        public Func<Task> task;

        /// <summary>
        /// Constructor for the AsyncTaskBot class.
        /// </summary>
        /// <param name="name">The name of the AsyncTaskBot.</param>
        /// <param name="task">The asynchronous task to be executed.</param>
        public AsyncTaskBot(string name, Func<Task> task)
        { 
            base.name = name;
            this.task = task;
        }

        /// <summary>
        /// Overrides the Execute method of the base class.
        /// </summary>
        public override void Execute()
        {
            _ = ExecuteAsync();
        }

        /// <summary>
        /// Executes the asynchronous task.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            try
            {
                stopwatch.Restart(); // Restarts the stopwatch to measure execution time
                await task(); // Executes the asynchronous task
                await Task.Yield(); // Yields the current thread to allow other tasks to execute
            }
            finally
            {
                stopwatch.Stop(); // Stops the stopwatch
                executionTime = stopwatch.ElapsedMilliseconds; // Sets the execution time in milliseconds
            }
        }
    }
}