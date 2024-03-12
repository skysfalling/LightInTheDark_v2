namespace Darklight.Bot
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class TaskQueen : MonoBehaviour, ITaskEntity
	{
		private Queue<TaskBot> _executionQueue = new();
		public string Name { get; set; } = "TaskQueen";
		public Guid GuidId { get; } = Guid.NewGuid();
		public Console Console = new Console();
		public int ExecutionQueueCount => _executionQueue.Count;

		public void Awake()
		{
			Console.Log(this, "Awake");
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

				try
				{
					Console.Log(this, $"Try to Execute {taskBot.Name}");
					await taskBot.ExecuteTask();
				}
				catch (OperationCanceledException e)
				{
					Console.Log(this, $"\t ERROR: TaskBot {taskBot.Name} was cancelled: {e.Message}");
				}
				catch (Exception e)
				{
					Console.Log(this, $"ERROR: Executing {taskBot.Name}: {e.Message}");
					UnityEngine.Debug.Log($"ERROR: Executing {taskBot.Name}: See Console for details.");
				}
				finally
				{
					Console.Log(this, $"\t COMPLETE: Finished Executing {taskBot.Name}");
				}
			}

			Console.Log(this, $"Finished Executing [{_executionQueue.Count}] TaskBots");
		}
	}
}
