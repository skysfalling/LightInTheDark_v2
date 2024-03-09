using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Darklight.Unity.Backend
{
	public class AsyncTaskConsole : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}



[CustomEditor(typeof(AsyncTaskConsole))]
public class AsyncTaskConsoleEditor : Editor
{
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AsyncTaskConsole consoleScript = (AsyncTaskConsole)target;
		AsyncTaskQueen queen = consoleScript.GetComponent<AsyncTaskQueen>();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("TaskBot Console", EditorStyles.boldLabel);
        
        // Custom style for the background
        GUIStyle backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.1f, 0.1f, 1.0f)); // Dark gray background
        backgroundStyle.padding = new RectOffset(10, 10, 10, 10); // Padding for inner content

        // Creating a scroll view with a custom background
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, GUILayout.Height(200));
        foreach (AsyncTaskBot taskBot in queen.taskBotQueue)
        {
            EditorGUILayout.LabelField(taskBot.name, EditorStyles.boldLabel); // Display the name in bold

            EditorGUI.indentLevel++; // Increase the indent level

            // Display the properties
            // Replace "PropertyName" and "taskBot.Property" with the actual property names and values
            EditorGUILayout.LabelField("guiID:", taskBot.guidId.ToString());
			EditorGUILayout.LabelField("executionTime:", taskBot.executionTime.ToString());

            EditorGUI.indentLevel--; // Decrease the indent level
        }
        EditorGUILayout.EndScrollView();
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

