namespace Darklight.Unity.Backend
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class TaskQueen : MonoBehaviour
    {
        private Queue<TaskBot> _executionQueue = new();
        public string Name { get; private set; } = "TaskQueen";
        public Guid GuidId { get; } = Guid.NewGuid();
		public TaskQueenConsole Console = new TaskQueenConsole();
        public int ExecutionQueueCount => _executionQueue.Count;

        public void Awake()
        {
            Console.Log(this, "Awake");
            this.gameObject.AddComponent<TaskQueenTest>();
        }

        public virtual void Initialize(string name)
        {
            Name = name;
            Console.Log(this, $"Initialize");
            Console.Log(this, $"Good Morning, {name}");
        }

        public void Enqueue(TaskBot taskBot)
        {
            _executionQueue.Enqueue(taskBot);
            Console.Log(this, $"Enqueue {taskBot.Name}");
        }

        public virtual async void ExecuteAllTasks()
        {
            Console.Log(this, $"Preparing to execute all TaskBots [{_executionQueue.Count}] on the main thread.");

            while (_executionQueue.Count > 0)
            {
                TaskBot taskBot = null;
                lock (_executionQueue)
                {
                    if (_executionQueue.Count > 0)
                    {
                        taskBot = _executionQueue.Dequeue();
                    }
                }
                await taskBot.ExecuteTask();
            }
        }
    }

    public class TaskQueenTest : MonoBehaviour
    {
        public void Start()
        {
            TaskQueen queen = new TaskQueen();
            queen.Initialize("Queen");
            TaskBot task1 = new TaskBot("Task 1", queen, async () =>
            {
                await Awaitable.WaitForSecondsAsync(1);
                Debug.Log("Task 1 ");
            });
            TaskBot task2 = new TaskBot("Task 2", queen, async () =>
            {
                await Awaitable.WaitForSecondsAsync(2);
                Debug.Log("Task 2");
            });

            queen.Enqueue(task1);
            queen.Enqueue(task2);
            queen.ExecuteAllTasks();
        }


    }

}
