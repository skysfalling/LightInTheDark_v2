using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class DarklightEditor
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

    public static GUIStyle CenteredStyle
    {
        get
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold            };
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

    // The helper function to draw a button and invoke a callback when pressed
    public static void DrawHandlesButton_atTransform(Transform transform, float size, Action onButtonPressed, Handles.CapFunction capFunction)
    {
        float pickSize = size * 2f;

        if (Handles.Button(transform.position, Quaternion.identity, size, pickSize, capFunction))
        {
            onButtonPressed?.Invoke();
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
        EditorGUILayout.LabelField($"{currentValue}", CenteredStyle ,GUILayout.MaxWidth(50));
        if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
        {
            setValue(Mathf.Min(maxValue, currentValue + 1));
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}
