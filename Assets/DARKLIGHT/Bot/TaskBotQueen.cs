namespace Darklight.Bot
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.PlayerLoop;
	using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
	using UnityEditor;
	using Editor = UnityEditor.Editor;
	using CustomEditor = UnityEditor.CustomEditor;
#endif

	/// <summary>
	/// Represents a queen that manages a queue of task bots.
	/// </summary>
	public class TaskBotQueen : MonoBehaviour, ITaskEntity
	{
		private Queue<TaskBot> _executionQueue = new();

		/// <summary>
		/// Gets or sets the name of the task queen.
		/// </summary>
		public string Name { get; set; } = "TaskQueen";

		/// <summary>
		/// Gets the unique identifier of the task queen.
		/// </summary>
		public Guid GuidId { get; } = Guid.NewGuid();

		/// <summary>
		/// Gets the console for logging task bot messages.
		/// </summary>
		public Console TaskBotConsole = new Console();

		/// <summary>
		/// Gets the number of task bots in the execution queue.
		/// </summary>
		public int ExecutionQueueCount => _executionQueue.Count;

		/// <summary>
		/// Called when the task queen is awakened.
		/// </summary>
		public virtual void Awake()
		{
			TaskBotConsole.Log(this, "Awake");
		}

		/// <summary>
		/// Initializes the task queen.
		/// </summary>
		public virtual async Task Initialize()
		{
			TaskBotConsole = new Console();
			TaskBotConsole.Log(this, $"Initialize");
			TaskBotConsole.Log(this, $"Good Morning, {name}");
			await Task.CompletedTask;
		}

		/// <summary>
		/// Executes the base class initialization sequence.
		/// </summary>
		public virtual async Awaitable ExecutionSequence()
		{
			TaskBotConsole.Log(this, "BaseClassInitializationSequence");
			Debug.LogWarning($"{this.Name} :: WARNING BaseClassInitializationSequence");
			await Awaitable.WaitForSecondsAsync(0.1f);
		}

		/// <summary>
		/// Executes all the task bots in the execution queue.
		/// </summary>
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

		/// <summary>
		/// Resets the task queen by clearing the execution queue.
		/// </summary>
		public virtual void Reset()
		{
			TaskBotConsole.Log(this, "Reset");
			_executionQueue.Clear();
		}

		/// <summary>
		/// Enqueues a task bot to the execution queue.
		/// </summary>
		/// <param name="taskBot">The task bot to enqueue.</param>
		public async Awaitable Enqueue(TaskBot taskBot)
		{
			_executionQueue.Enqueue(taskBot);
			TaskBotConsole.Log(this, $"Enqueue {taskBot.Name}");
			await Awaitable.WaitForSecondsAsync(0.1f);
		}

		/// <summary>
		/// Executes a task bot.
		/// </summary>
		/// <param name="taskBot">The task bot to execute.</param>
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
				await Awaitable.MainThreadAsync(); // set back to main
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
#if UNITY_EDITOR
	[CustomEditor(typeof(TaskBotQueen), true)]
	public class TaskBotQueenEditor : Editor
	{
		private Vector2 scrollPosition;
		public TaskBotQueen queenScript;
		public Console console;
		public bool showConsole = true;

		public virtual void OnEnable()
		{
			queenScript = (TaskBotQueen)target;
			console = queenScript.TaskBotConsole;
			_ = queenScript.Initialize();
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(10);
			queenScript = (TaskBotQueen)target;
			console = queenScript.TaskBotConsole;

			Darklight.CustomInspectorGUI.CreateFoldout(ref showConsole, "Task Bot Console", async () =>
			{
				DrawConsole();

				if (GUILayout.Button("Initialize"))
				{
					await queenScript.Initialize();
				}
				else if (GUILayout.Button("Reset"))
				{
					queenScript.Reset();
				}

			});
			base.OnInspectorGUI();
		}

		void DrawConsole()
		{
			if (console == null) { return; }

			// Dark gray background
			GUIStyle backgroundStyle = new GUIStyle();
			backgroundStyle.normal.background = Darklight.CustomInspectorGUI.MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f));
			backgroundStyle.padding = new RectOffset(10, 10, 10, 10); // Padding for inner content

			// Creating a scroll view with a custom background
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, GUILayout.Height(200));
			List<string> activeConsole = console.GetActiveConsole();
			foreach (string message in activeConsole)
			{
				EditorGUILayout.LabelField(message, EditorStyles.label);
			}
			EditorGUILayout.EndScrollView();
			EditorUtility.SetDirty(target);
		}
	}
#endif

}
