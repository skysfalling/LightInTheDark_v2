using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class DarklightEditor
{

    public static Vector3 MultiplyVec3(Vector3 vec3_a, Vector3 vec3_b)
    {
        return new Vector3(vec3_a.x * vec3_b.x, vec3_a.y * vec3_b.y, vec3_a.z * vec3_b.z);
    }

    public static void DrawWireRectangle_withLabel(string label, Vector3 position, int size)
    {
        Handles.color = Color.black;
        Handles.DrawWireCube(position, size * new Vector3(1, 0, 1));

        // Calculate the position for the label (midpoint of the top edge)
        Vector3 labelOffset = new Vector3(-0.45f, 0, 0.45f); // Adjust the label position as needed

        // Draw the label with the width
        Vector3 labelPosition = position + (size * labelOffset);
        Handles.Label(labelPosition, label);

    }

    // Function to draw a rectangle in the scene view and display its width
    public static void DrawRectangleWithWidthLabel(Vector3 position, Vector2 size, Quaternion rotation, string labelPrefix = "Width: ")
    {
        // Calculate the corners of the rectangle
        Vector3[] rectangleCorners = new Vector3[4];
        rectangleCorners[0] = position + rotation * new Vector3(-size.x / 2, size.y / 2, 0); // Top left
        rectangleCorners[1] = position + rotation * new Vector3(size.x / 2, size.y / 2, 0); // Top right
        rectangleCorners[2] = position + rotation * new Vector3(size.x / 2, -size.y / 2, 0); // Bottom right
        rectangleCorners[3] = position + rotation * new Vector3(-size.x / 2, -size.y / 2, 0); // Bottom left

        // Draw the rectangle
        Handles.DrawSolidRectangleWithOutline(rectangleCorners, new Color(0.5f, 0.5f, 1f, 0.1f), Color.green);


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
        GUIStyle controlStyle = new GUIStyle();
        controlStyle.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
        controlStyle.alignment = TextAnchor.UpperCenter;
        controlStyle.margin = new RectOffset(20, 20, 0, 0);

        EditorGUILayout.BeginVertical(controlStyle);

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
        EditorGUILayout.LabelField($"{currentValue}", GUILayout.MaxWidth(50));
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
