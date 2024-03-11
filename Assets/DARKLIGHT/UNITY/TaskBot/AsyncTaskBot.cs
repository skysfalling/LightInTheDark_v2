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
        public Func<Task> TaskDelegate { get; private set; }

        /// <summary>
        /// Constructor for the AsyncTaskBot class.
        /// </summary>
        /// <param name="name">The name of the AsyncTaskBot.</param>
        /// <param name="taskDelegate">The asynchronous task to be executed.</param>
        public AsyncTaskBot(string name, Func<Task> taskDelegate)
        {
            base.name = name;
            this.TaskDelegate = taskDelegate;
        }

        /// <summary>
        /// Executes the asynchronous task.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                await TaskDelegate(); // Now directly awaiting the taskFunc, supporting true asynchronous operations
            }
            catch (Exception ex)
            {
                Debug.LogError($"AsyncTaskBot '{name}' encountered an error: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                executionTime = stopwatch.ElapsedMilliseconds;
            }
        }
    }
}