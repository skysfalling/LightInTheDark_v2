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

        public EventTaskBot()
        {
            _unityEvent = new UnityEvent();
        }

        public EventTaskBot(string name, UnityEvent unityEvent) : base()
        {
            base.name = name;
            _unityEvent = unityEvent;
        }

        public override void Execute()
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
                Debug.Log($"EventTaskBot {name} completed in {executionTime} ms");
            }
        }
    }
}