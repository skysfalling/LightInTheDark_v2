using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Darklight.Unity.Backend
{
    /// <summary>
    /// Represents an asynchronous task bot that extends the TaskBot class.
    /// </summary>
    public class AsyncTaskBot : TaskBot
    {
        public AsyncTaskBot(string name, TaskQueen queen, Func<Task> taskDelegate) : base(name, queen, taskDelegate)
        {

        }
    }
}
