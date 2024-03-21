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
#endif

	public class TaskBotQueen : MonoBehaviour, ITaskEntity
	{
		private Darklight.Console _console = new Darklight.Console();
		private Queue<TaskBot> _executionQueue = new();

		#region -- ( StateMachine ) ------------------------------- >>  
		public enum State { NULL, AWAKE, INIT, WAIT, LOAD, EXECUTE, CLEAN, ERROR }
		State _currentState = State.NULL;
		public State CurrentState
		{
			get { return _currentState; }
			set
			{
				_currentState = value;
				OnStateChanged(value);
			}
		}
		private void OnStateChanged(State newState)
		{
			TaskBotConsole.Log($"StateChange => [ {newState} ]");
			switch (newState)
			{
				case State.NULL:
				case State.AWAKE:
					break;
				case State.INIT:
					break;
			}
		}
		#endregion

		public string Name { get; set; } = "TaskQueen";
		public Guid GuidId { get; } = Guid.NewGuid();
		public Darklight.Console TaskBotConsole => _console;
		public int ExecutionQueueCount => _executionQueue.Count;

		public virtual void Awake()
		{
			CurrentState = State.AWAKE;
		}

		public virtual async Task Initialize()
		{
			CurrentState = State.INIT;

			TaskBotConsole.Log($"Good Morning, {name}");
			await Task.CompletedTask;
		}

		/// <summary>
		/// Enqueues a task bot to the execution queue.
		/// </summary>
		/// <param name="taskBot">The task bot to enqueue.</param>
		public Task Enqueue(TaskBot taskBot)
		{
			CurrentState = State.LOAD;

			_executionQueue.Enqueue(taskBot);
			TaskBotConsole.Log($"Enqueue {taskBot.Name}");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes an individual TaskBot
		/// </summary>
		/// <param name="taskBot"></param>
		/// <returns></returns>
		public async Awaitable ExecuteBot(TaskBot taskBot)
		{
			CurrentState = State.EXECUTE;

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
				TaskBotConsole.Log($"Try to Execute {taskBot.Name}");
				await taskBot.ExecuteTask();
				await Awaitable.MainThreadAsync(); // default back to main thread
			}
			catch (OperationCanceledException e)
			{
				TaskBotConsole.Log($"\t ERROR: TaskBot {taskBot.Name} was cancelled: {e.Message}");
				Debug.Log($"\t ERROR: TaskBot {taskBot.Name} was cancelled: {e.StackTrace}");
			}
			catch (Exception e)
			{
				TaskBotConsole.Log($"ERROR: Executing {taskBot.Name}: {e.Message}");
				UnityEngine.Debug.Log($"ERROR: Executing {taskBot.Name}");
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
			CurrentState = State.EXECUTE;

			TaskBotConsole.Log($"Preparing to execute all TaskBots [{_executionQueue.Count}] on the main thread.");

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
			TaskBotConsole.Log("Reset");
			_executionQueue.Clear();
		}
	}
#if UNITY_EDITOR
	[CustomEditor(typeof(TaskBotQueen), true)]
	public class TaskBotQueenEditor : Editor
	{
		private Vector2 scrollPosition;
		private SerializedObject _serializedObject;
		public TaskBotQueen queenScript;
		public Darklight.Console console;
		public bool showConsole = true;

		public virtual void OnEnable()
		{
			_serializedObject = new SerializedObject(target);
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

			_serializedObject = new SerializedObject(target);
			CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);
		}

		void DrawConsole()
		{
			if (console == null) { return; }

			// Dark gray background
			GUIStyle backgroundStyle = new GUIStyle();
			backgroundStyle.normal.background = CustomInspectorGUI.MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f));
			backgroundStyle.padding = new RectOffset(0, 0, 0, 0); // Padding for inner content

			// Creating a scroll view with a custom background
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, GUILayout.Height(100));
			List<string> activeConsole = console.GetActiveConsole();
			foreach (string message in activeConsole)
			{
				EditorGUILayout.LabelField(message, CustomGUIStyles.LeftAlignedStyle);
			}
			EditorGUILayout.EndScrollView();
			EditorUtility.SetDirty(target);
		}
	}
#endif

}
