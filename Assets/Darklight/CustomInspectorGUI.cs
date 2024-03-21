namespace Darklight
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public static class CustomInspectorGUI
	{
		public static bool DrawDefaultInspectorWithoutSelfReference(SerializedObject obj)
		{
			EditorGUI.BeginChangeCheck();
			obj.UpdateIfRequiredOrScript();
			SerializedProperty iterator = obj.GetIterator();
			iterator.NextVisible(true); // skip first property
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					EditorGUILayout.PropertyField(iterator, true);
				}

				enterChildren = false;
			}

			obj.ApplyModifiedProperties();
			return EditorGUI.EndChangeCheck();
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
			EditorGUILayout.LabelField($"{currentValue}", CustomGUIStyles.CenteredStyle, GUILayout.MaxWidth(50));
			if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
			{
				setValue(Mathf.Min(maxValue, currentValue + 1));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			GUI.backgroundColor = Color.white;
		}

		/// <summary>
		/// Creates a foldout in the Unity Editor and executes the given Action if the foldout is expanded.
		/// </summary>
		/// <param name="foldoutLabel">Label for the foldout.</param>
		/// <param name="isFoldoutExpanded">Reference to the variable tracking the foldout's expanded state.</param>
		/// <param name="innerAction">Action to execute when the foldout is expanded. This action contains the UI elements to be drawn inside the foldout.</param>
		public static void CreateFoldout(ref bool isFoldoutExpanded, string foldoutLabel, Action innerAction)
		{
			// Draw the foldout
			isFoldoutExpanded = EditorGUILayout.Foldout(isFoldoutExpanded, foldoutLabel, true, EditorStyles.foldoutHeader);

			// If the foldout is expanded, execute the action
			if (isFoldoutExpanded && innerAction != null)
			{
				EditorGUI.indentLevel++; // Indent the contents of the foldout for better readability
				innerAction.Invoke(); // Execute the provided Action
				EditorGUI.indentLevel--; // Reset indentation
			}
		}

		public static void CreateSettingsLabel(string label, string value)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(label);
			EditorGUILayout.LabelField(value);
			EditorGUILayout.EndHorizontal();
		}
		// Helper function to create a labeled enum dropdown in the editor
		public static void CreateEnumLabel<TEnum>(ref TEnum currentValue, string label) where TEnum : System.Enum
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label); // Adjust the width as needed

			GUILayout.FlexibleSpace();

			currentValue = (TEnum)EditorGUILayout.EnumPopup(currentValue);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
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
