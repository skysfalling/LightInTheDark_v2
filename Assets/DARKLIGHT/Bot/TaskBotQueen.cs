namespace Darklight.Bot
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using Debug = UnityEngine.Debug;
	using Darklight;

#if UNITY_EDITOR
	using UnityEditor;
	using Editor = UnityEditor.Editor;
	using CustomEditor = UnityEditor.CustomEditor;
	using System.Linq;
#endif

	public class TaskBotQueen : MonoBehaviour, ITaskEntity
	{
		private Darklight.Console _console = new Darklight.Console();
		private Queue<TaskBot> _executionQueue = new Queue<TaskBot>();

		#region -- ( StateMachine ) ------------------------------- >>  
		public enum State { NULL, AWAKE, INITIALIZE, LOAD_DATA, WAIT, ADD_TO_QUEUE, EXECUTION, CLEAN, ERROR }
		State _currentState = State.NULL;
		public State CurrentState
		{
			get { return _currentState; }
			set
			{
				OnStateChanged(value);
				_currentState = value;
			}
		}
		private void OnStateChanged(State newState)
		{
			if (newState == CurrentState) { return; }
			switch (newState)
			{
				case State.NULL:
				case State.AWAKE:
					break;
				case State.INITIALIZE:
					break;
			}
		}
		#endregion

		public bool Initialized { get; private set; } = false;
		public string Name { get; set; } = "TaskQueen";
		public Guid GuidId { get; } = Guid.NewGuid();
		public Darklight.Console TaskBotConsole => _console;
		public string LogPrefix => $"<{CurrentState}>";
		public int ExecutionQueueCount => _executionQueue.Count;

		public virtual void Awake()
		{
			CurrentState = State.AWAKE; // show state change
			TaskBotConsole.Log($"{LogPrefix}");
		}

		public virtual async Task Initialize()
		{
			CurrentState = State.INITIALIZE;
			TaskBotConsole.Log($"{LogPrefix}"); // show state change
			Initialized = true;
			await Task.CompletedTask;
		}

		/// <summary>
		/// Enqueues a task bot to the execution queue.
		/// </summary>
		/// <param name="taskBot">The task bot to enqueue.</param>
		public Task Enqueue(TaskBot taskBot, bool log = true)
		{
			CurrentState = State.ADD_TO_QUEUE;

			if (log)
				TaskBotConsole.Log($"{LogPrefix} EnqueueTaskBot {taskBot.Name}", 1);

			_executionQueue.Enqueue(taskBot);
			return Task.CompletedTask;
		}

		public async Task EnqueueList(IEnumerable<TaskBot> bots, bool log = true)
		{
			CurrentState = State.ADD_TO_QUEUE;

			if (log)
				TaskBotConsole.Log($"{LogPrefix} EnqueueList<TaskBot> Count:{bots.Count()}", 0);

			foreach (TaskBot newBot in bots)
			{
				await Enqueue(newBot, log);
			}
		}

		public async Task EnqueueClones<T>(string cloneName, IEnumerable<T> items, Func<T, TaskBot> CreateNewClone, bool log = true)
		{
			CurrentState = State.ADD_TO_QUEUE;

			if (log)
				TaskBotConsole.Log($"{LogPrefix} EnqueueClones {cloneName} x {items.ToList().Count} Clones", 0);

			foreach (T item in items)
			{
				TaskBot newBot = CreateNewClone(item);
				await Enqueue(newBot, log);
			}
		}

		/// <summary>
		/// Executes an individual TaskBot
		/// </summary>
		/// <param name="taskBot"></param>
		/// <returns></returns>
		public async Awaitable ExecuteBot(TaskBot taskBot)
		{
			CurrentState = State.EXECUTION;

			// Assign the TaskBot to Execute on the background thread
			if (taskBot.executeOnBackgroundThread)
			{
				await Awaitable.BackgroundThreadAsync();
			}
			// Default to Main Thread
			else { await Awaitable.MainThreadAsync(); }

			try
			{
				TaskBotConsole.Log($"Try to Execute {taskBot.Name}");
				await taskBot.ExecuteTask();
				await Awaitable.MainThreadAsync(); // default back to main thread
			}
			catch (OperationCanceledException e)
			{
				TaskBotConsole.Log($"TaskBot {taskBot.Name} was cancelled: {e.Message}");
				Debug.Log($"TaskBot {taskBot.Name} was cancelled: {e.StackTrace}");
			}
			catch (Exception e)
			{
				TaskBotConsole.Log($"ERROR: Executing {taskBot.Name}: {e.Message}");
				Debug.Log($"ERROR: Executing {taskBot.Name}");
			}
			finally
			{
				TaskBotConsole.Log($"\t COMPLETE: Finished Executing {taskBot.Name}");
			}
		}

		/// <summary>
		/// Iteratively executes all the task bots in the execution queue.
		/// </summary>
		public virtual async Awaitable ExecuteAllTasks()
		{
			CurrentState = State.EXECUTION;

			TaskBotConsole.Log($"{LogPrefix} START TaskBots [{ExecutionQueueCount}]");

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

			TaskBotConsole.Log($"Finished Executing [{_executionQueue.Count}] TaskBots");
			CurrentState = State.CLEAN;
		}

		/// <summary>
		/// Resets the task queen by clearing the execution queue.
		/// </summary>
		public virtual void Reset()
		{
			_executionQueue.Clear();
			Initialized = false;

			TaskBotConsole.Log("Reset");
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(TaskBotQueen), true)]
	public class TaskBotQueenEditor : Editor
	{
		private Vector2 scrollPosition;
		private SerializedObject _serializedObject;
		public TaskBotQueen taskBotQueen;
		public Darklight.Console console;
		public bool showConsole = true;
		public override void OnInspectorGUI()
		{
			GUILayout.Space(10);
			taskBotQueen = (TaskBotQueen)target;
			console = taskBotQueen.TaskBotConsole;

			CustomInspectorGUI.CreateFoldout(ref showConsole, "Task Bot Console", async () =>
			{
				console.DrawInEditor();
				if (!taskBotQueen.Initialized && GUILayout.Button("Initialize"))
				{
					await taskBotQueen.Initialize();
				}
				else if (taskBotQueen.Initialized)
				{
					if (GUILayout.Button("Execute All"))
					{
						await taskBotQueen.ExecuteAllTasks();
					}
					else if (GUILayout.Button("Reset"))
					{
						taskBotQueen.Reset();
					}
				}
			});

			_serializedObject = new SerializedObject(target);
			CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);
		}
	}
#endif

}
