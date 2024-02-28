using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class DarklightGizmos
{
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

    public static void DrawSquareAtPosition(Vector3 position, int size, Vector3 direction, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(position, size * Vector2.one, direction),
            fillColor, Color.clear);
    }

    private static Vector3[] GetRectangleVertices(Vector3 center, Vector2 area, Vector3 normalDirection)
    {
        Vector2 halfArea = area * 0.5f;
        Vector3[] vertices = new Vector3[4]
        {
        new Vector3(-halfArea.x, 0, -halfArea.y),
        new Vector3(halfArea.x, 0, -halfArea.y),
        new Vector3(halfArea.x, 0, halfArea.y),
        new Vector3(-halfArea.x, 0, halfArea.y)
        };

        // Calculate the rotation from the up direction to the normal direction
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normalDirection);

        // Apply rotation to each vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotation * vertices[i] + center;
        }

        return vertices;
    }
}
