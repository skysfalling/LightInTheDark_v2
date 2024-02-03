using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

[CustomEditor(typeof(WorldChunkMap))]
public class WorldChunkMapEditor : Editor
{
    private SerializedObject serializedChunkMap;
    SerializedProperty worldExitsProperty;

    private void OnEnable()
    {
        serializedChunkMap = new SerializedObject(target); // Cache the SerializedObject
        worldExitsProperty = serializedChunkMap.FindProperty("worldExits");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector elements
        serializedChunkMap.Update(); // Always start with this call

    }
}
