namespace Darklight.Bot
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.PlayerLoop;
	using Debug = UnityEngine.Debug;

	public class TaskQueen : MonoBehaviour, ITaskEntity
	{
		private Queue<TaskBot> _executionQueue = new();
		public string Name { get; set; } = "TaskQueen";
		public Guid GuidId { get; } = Guid.NewGuid();
		public Console TaskBotConsole = new Console();
		public int ExecutionQueueCount => _executionQueue.Count;

		public void Awake()
		{
			TaskBotConsole.Log(this, "Awake");
		}

		public virtual async Task Initialize()
		{
			TaskBotConsole.Log(this, $"Initialize");
			TaskBotConsole.Log(this, $"Good Morning, {name}");
			await Task.CompletedTask;
		}
		public virtual async Awaitable InitializationSequence()
		{
			TaskBotConsole.Log(this, "BaseClassInitializationSequence");
			Debug.LogWarning($"{this.Name} :: WARNING BaseClassInitializationSequence");
			await Awaitable.WaitForSecondsAsync(0.1f);
		}

		public virtual async Awaitable ExecuteAllTasks()
		{
			TaskBotConsole.Log(this, $"Preparing to execute all TaskBots [{_executionQueue.Count}] on the main thread.");

			while (_executionQueue.Count > 0)
			{
				// Dequeue the next TaskBot
				TaskBot taskBot = null;
				lock (_executionQueue)
				{
					if (_executionQueue.Count > 0)
					{
						taskBot = _executionQueue.Dequeue();
					}
				}

				// Try to Execute the TaskBot
				await ExecuteBot(taskBot);
			}

			TaskBotConsole.Log(this, $"Finished Executing [{_executionQueue.Count}] TaskBots");
		}

		public virtual void Reset()
		{
			TaskBotConsole.Log(this, "Reset");
			_executionQueue.Clear();
		}

		public async Awaitable Enqueue(TaskBot taskBot)
		{
			_executionQueue.Enqueue(taskBot);
			TaskBotConsole.Log(this, $"Enqueue {taskBot.Name}");
			await Awaitable.WaitForSecondsAsync(0.1f);
		}

		public async Awaitable ExecuteBot(TaskBot taskBot)
		{
			// Assign the TaskBot to Execute on the background thread
			if (taskBot.executeOnBackgroundThread)
			{
				await Awaitable.BackgroundThreadAsync();
			}
			// Default to Main Thread
			else
			{
				await Awaitable.MainThreadAsync();
			}

			try
			{
				TaskBotConsole.Log(this, $"Try to Execute {taskBot.Name}");
				await taskBot.ExecuteTask();
			}
			catch (OperationCanceledException e)
			{
				TaskBotConsole.Log(this, $"\t ERROR: TaskBot {taskBot.Name} was cancelled: {e.Message}");
				Debug.Log($"\t ERROR: TaskBot {taskBot.Name} was cancelled: {e.StackTrace}");
			}
			catch (Exception e)
			{
				TaskBotConsole.Log(this, $"ERROR: Executing {taskBot.Name}: {e.Message}");
				UnityEngine.Debug.Log($"ERROR: Executing {taskBot.Name}");
			}
			finally
			{
				TaskBotConsole.Log(this, $"\t COMPLETE: Finished Executing {taskBot.Name}");
			}
		}
	}
}
