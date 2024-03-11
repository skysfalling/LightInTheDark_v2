using System;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Unity.Backend
{
    [Serializable]
    public class EventTaskBot : TaskBot
    {
        [SerializeField]
        private UnityEvent _unityEvent;

		// TODO : ExecutionDelay

        public EventTaskBot()
        {
            _unityEvent = new UnityEvent();
        }

        public EventTaskBot(string name, UnityEvent unityEvent) : base()
        {
            base.name = name;
            _unityEvent = unityEvent;
        }

        public void ExecuteEvent()
        {
            try
            {
                stopwatch.Restart();
                _unityEvent.Invoke();
            }
            finally
            {
                stopwatch.Stop();
                executionTime = stopwatch.ElapsedMilliseconds;
                Debug.Log($"EventTaskBot {name} UnityEvent.Invoked");
            }
        }
    }
}

/*
public class EventTracker
{
    private int totalTasks;
    private int completedTasks;

    public UnityEvent myEvent;

    public async Task RunEventAndWait()
    {
        totalTasks = myEvent.GetPersistentEventCount();
        completedTasks = 0;

        myEvent.Invoke();

        await Task.Run(() => WaitForTasks());
    }

    private void WaitForTasks()
    {
        while (completedTasks < totalTasks)
        {
            // Wait
        }
    }

    public void TaskCompleted()
    {
        completedTasks++;
    }
}

*/