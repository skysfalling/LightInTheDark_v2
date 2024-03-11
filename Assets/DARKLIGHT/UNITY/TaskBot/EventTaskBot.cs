using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.Unity.Backend
{
    [Serializable]
    public class EventTaskBot : TaskBot
    {
        [SerializeField] private UnityEvent _unityEvent;

        public EventTaskBot(TaskQueen queen, string name) : base(name, queen, null)
        {
            Func<Task> eventTask;
            eventTask = delegate ()
            {
                return Task.Run(() =>
                {
                    _unityEvent.Invoke();
                });
            };
            this.task = eventTask;
        }
    }
}