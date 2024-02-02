using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldChunkMap))]
public class WorldChunkMapEditor : Editor
{
    private SerializedObject serializedChunkMap;

    private void OnEnable()
    {
        serializedChunkMap = new SerializedObject(target); // Cache the SerializedObject
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector elements
        serializedChunkMap.Update(); // Always start with this call

        WorldChunkMap worldChunkMap = (WorldChunkMap)target;

        if (GUILayout.Button("Find Golden Path"))
        {
            worldChunkMap.FindGoldenPath();
        }
        serializedChunkMap.ApplyModifiedProperties();

        // Ensure changes are registered and the inspector updates as needed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(worldChunkMap);
        }
    }
}
