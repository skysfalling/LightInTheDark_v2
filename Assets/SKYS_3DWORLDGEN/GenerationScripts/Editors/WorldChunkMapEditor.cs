#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldChunkMap))]
public class WorldChunkMapEditor : Editor
{
    SerializedObject serializedObject;
    SerializedProperty zonesProperty;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target);
        zonesProperty = serializedObject.FindProperty("zones");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField($"World Chunk Map Initialized => {WorldChunkMap.chunkMapInitialized}");
        EditorGUILayout.Space();

        DrawDefaultInspector();

        EditorGUI.BeginChangeCheck();
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedObject.ApplyModifiedProperties();


            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        WorldChunkMap map = (WorldChunkMap)target;

    }

    private void DrawRectangleArea(Vector3 worldPos, Vector2Int area, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(worldPos, area), fillColor, Color.clear);
    }

    private Vector3[] GetRectangleVertices(Vector3 center, Vector2 area)
    {
        Vector2 halfArea = area * 0.5f;
        Vector3[] vertices = new Vector3[4]
        {
            center + new Vector3(-halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, halfArea.y),
            center + new Vector3(-halfArea.x, 0, halfArea.y)
        };

        return vertices;
    }
}
#endif