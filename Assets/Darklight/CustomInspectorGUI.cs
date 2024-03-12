namespace Darklight
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public static class CustomInspectorGUI
	{
		public static GUIStyle TitleHeaderStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontSize = 24,
					fontStyle = FontStyle.Bold,
					fixedHeight = 40
				};
			}
		}

		public static GUIStyle Header1Style
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					fontSize = 20,
					fontStyle = FontStyle.Bold,
					fixedHeight = 40
				};
			}
		}

		public static GUIStyle Header2Style
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					fontSize = 16,
					fontStyle = FontStyle.Bold,
					fixedHeight = 40
				};
			}
		}

		public static GUIStyle LeftAlignedStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleLeft
				};
			}
		}

		public static GUIStyle CenteredStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter
				};
			}
		}

		public static GUIStyle RightAlignedStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleRight
				};
			}
		}

		public static GUIStyle BoldStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = FontStyle.Bold
				};
			}
		}

		public static GUIStyle BoldCenteredStyle
		{
			get
			{
				return new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.MiddleCenter
				};
			}
		}

		public static void FocusSceneView(Vector3 focusPoint)
		{
			if (SceneView.lastActiveSceneView != null)
			{
				// Set the Scene view camera pivot (center point) and size (zoom level)
				SceneView.lastActiveSceneView.pivot = focusPoint;

				// Repaint the scene view to immediately reflect changes
				SceneView.lastActiveSceneView.Repaint();
			}
		}

		public static void CreateIntegerControl(string title, int currentValue, int minValue, int maxValue, System.Action<int> setValue)
		{
			GUIStyle controlBackgroundStyle = new GUIStyle();
			controlBackgroundStyle.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
			controlBackgroundStyle.alignment = TextAnchor.MiddleCenter;
			controlBackgroundStyle.margin = new RectOffset(20, 20, 0, 0);

			EditorGUILayout.BeginVertical(controlBackgroundStyle);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField(title);
			GUILayout.FlexibleSpace();

			// +/- Buttons
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
			{
				setValue(Mathf.Max(minValue, currentValue - 1));
			}
			EditorGUILayout.LabelField($"{currentValue}", CenteredStyle, GUILayout.MaxWidth(50));
			if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
			{
				setValue(Mathf.Min(maxValue, currentValue + 1));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			GUI.backgroundColor = Color.white;
		}

		public static void CreateSettingsLabel(string label, string value)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(label);
			EditorGUILayout.LabelField(value);
			EditorGUILayout.EndHorizontal();
		}

		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}

		// Helper function to create a labeled enum dropdown in the editor
		public static void DrawLabeledEnumPopup<TEnum>(ref TEnum currentValue, string label) where TEnum : System.Enum
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label); // Adjust the width as needed

			GUILayout.FlexibleSpace();

			currentValue = (TEnum)EditorGUILayout.EnumPopup(currentValue);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		public static bool IsObjectOrChildSelected(GameObject obj)
		{
			// Check if the direct object is selected
			if (Selection.activeGameObject == obj)
			{
				return true;
			}

			// Check if any of the selected objects is a child of the inspected object
			foreach (GameObject selectedObject in Selection.gameObjects)
			{
				if (selectedObject.transform.IsChildOf(obj.transform))
				{
					return true;
				}
			}

			return false;
		}

	}
}
