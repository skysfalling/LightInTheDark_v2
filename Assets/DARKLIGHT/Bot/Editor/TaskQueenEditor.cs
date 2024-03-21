namespace Darklight.Bot.Editor
{
	using System.Collections.Generic;
	using System.Text;
	using Darklight.Bot;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(TaskBotQueen), true)]
	public class TaskBotQueenEditor : Editor
	{
		private Vector2 scrollPosition;
		public TaskBotQueen queenScript;
		public Console console;

		public virtual void OnEnable()
		{
			queenScript = (TaskBotQueen)target;
			console = queenScript.TaskBotConsole;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(10);
			queenScript = (TaskBotQueen)target;
			console = queenScript.TaskBotConsole;

			// Dark gray background
			GUIStyle backgroundStyle = new GUIStyle();
			backgroundStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f));
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

			base.OnInspectorGUI();
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
