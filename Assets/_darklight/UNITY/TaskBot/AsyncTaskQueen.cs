using System.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Unity.Backend
{
    public interface ITaskQueen
    {
        string name { get; set; }
        List<TaskBot.Profile> taskBotProfiles { get; set; }
        void Initialize(string name);
    }

    /// <summary>
    /// Represents a queen that manages a queue of AsyncTaskBots and executes them asynchronously.
    /// </summary>
    public class AsyncTaskQueen : MonoBehaviour, ITaskQueen
    {
        public struct Profile
        {
            public string name;
            public Guid guidId;
            public List<TaskBot.Profile> taskBotProfiles;

            public Profile(AsyncTaskQueen queen)
            {
                name = queen.name;
                guidId = queen.guidId;
                taskBotProfiles = queen.taskBotProfiles;
            }
        }
        Queue<AsyncTaskBot> taskBotQueue = new();
        public new string name = "AsyncTaskQueen";
        public Guid guidId = Guid.NewGuid();
        public List<TaskBot.Profile> taskBotProfiles { get; set; } = new();
        public AsyncTaskConsole asyncTaskConsole { get; private set; } = new AsyncTaskConsole();

        public virtual void Initialize(string name)
        {
			this.name = name;
			asyncTaskConsole.Log(this, "Initialize()");
        }

        /// <summary>
        /// Creates a new AsyncTaskBot and adds it to the taskBotQueue.
        /// </summary>
        /// <param name="name">The name of the AsyncTaskBot.</param>
        /// <param name="task">The task to be executed by the AsyncTaskBot.</param>
        public void NewTaskBot(string name, Func<Task> task)
        {
            Guid guidId = Guid.NewGuid();
            AsyncTaskBot newTaskBot = new AsyncTaskBot(name, task);
            taskBotQueue.Enqueue(newTaskBot);

			asyncTaskConsole.Log(newTaskBot, "New Task Bot created and added to the queue.");

        }

        /// <summary>
        /// Executes all the AsyncTaskBots in the taskBotQueue.
        /// </summary>
        public async Task ExecuteAllBotsInQueue()
        {
			asyncTaskConsole.Log(this, $"Execute all AsyncTaskBots [ {taskBotQueue.Count} ]");

            while (taskBotQueue.Count > 0)
            {
                AsyncTaskBot taskBot = taskBotQueue.Dequeue();
                await taskBot.ExecuteAsync();
                await Task.Yield();

				TaskBot.Profile newProfile = taskBot.NewProfile();
                taskBotProfiles.Add(newProfile);
				asyncTaskConsole.Log(taskBot, $"Task Bot Finished: {newProfile.executionTime}ms");

                taskBot.Dispose();
            }
        }

        public class AsyncTaskConsole
        {
            public struct Tag
            {
                public string name;
                public Guid guidId;
                public Tag(AsyncTaskQueen queen)
                {
                    this.name = queen.name;
                    this.guidId = queen.guidId;
                }
                public Tag(AsyncTaskBot bot)
                {
                    this.name = bot.name;
                    this.guidId = bot.guidId;
                }
            }

            public Dictionary<Tag, List<string>> ConsoleDictionary { get; private set; } = new();
            public void Log(AsyncTaskQueen queen, string message)
            {
                Tag queenTag = new Tag(queen);

                if (!ConsoleDictionary.ContainsKey(queenTag))
                {
                    ConsoleDictionary.Add(queenTag, new List<string>());
                }
                ConsoleDictionary[queenTag].Add(message);
            }

            public void Log(AsyncTaskBot asyncTaskBot, string message)
            {
                Tag botTag = new Tag(asyncTaskBot);
                
				if (!ConsoleDictionary.ContainsKey(botTag))
                {
                    ConsoleDictionary.Add(botTag, new List<string>());
                }

                ConsoleDictionary[botTag].Add(message);
            }

            public List<string> GetActiveConsole()
            {
                List<string> result = new();
                foreach (Tag tag in ConsoleDictionary.Keys)
                {
                    result.Add($"{tag.name}: {tag.guidId}");
                    foreach (string value in ConsoleDictionary[tag])
                    {
                        result.Add($"\t {value}");
                    }
                }
                return result;
            }
        }

#if UNITY_EDITOR

        [CustomEditor(typeof(AsyncTaskQueen))]
        public class AsyncTaskQueenEditor : Editor
        {
            private Vector2 scrollPosition;
            private bool showConsole = true; // Foldout field for the console

            public override void OnInspectorGUI()
            {
                GUILayout.Space(10);

                AsyncTaskQueen queen = (AsyncTaskQueen)target;
                AsyncTaskConsole console = queen.asyncTaskConsole;

                // Custom style for the background
                GUIStyle backgroundStyle = new GUIStyle();
                backgroundStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f)); // Dark gray background
                backgroundStyle.padding = new RectOffset(10, 10, 10, 10); // Padding for inner content

                // Creating a foldout field for the console
                showConsole = EditorGUILayout.Foldout(showConsole, $"{name} AsyncTaskQueen Console");
                if (showConsole)
                {
                    // Creating a scroll view with a custom background
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, GUILayout.Height(200));
                    List<string> activeConsole = console.GetActiveConsole();
                    foreach (string message in activeConsole)
                    {
                        EditorGUILayout.LabelField(message, EditorStyles.label);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            // Utility function to create a texture
            private Texture2D MakeTex(int width, int height, Color col)
            {
                Color[] pix = new Color[width * height];
                for (int i = 0; i < pix.Length; ++i)
                {
                    pix[i] = col;
                }

                Texture2D result = new Texture2D(width, height);
                result.SetPixels(pix);
                result.Apply();
                return result;
            }
        }
#endif
    }
}
