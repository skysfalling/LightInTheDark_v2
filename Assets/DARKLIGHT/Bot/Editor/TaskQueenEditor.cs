namespace Darklight.Bot.Editor
{
	using System.Collections.Generic;
	using System.Text;
	using Darklight.Bot;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(TaskQueen), true)]
	public class TaskQueenEditor : Editor
	{
		private Vector2 scrollPosition;
		public TaskQueen queenScript;
		public Console console;

		public virtual void OnEnable()
		{
			queenScript = (TaskQueen)target;
			console = queenScript.TaskBotConsole;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			GUILayout.Space(10);

			TaskQueen queen = (TaskQueen)target;
			Console console = queen.TaskBotConsole;

			// Custom style for the background
			GUIStyle backgroundStyle = new GUIStyle();
			backgroundStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f)); // Dark gray background
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
}
