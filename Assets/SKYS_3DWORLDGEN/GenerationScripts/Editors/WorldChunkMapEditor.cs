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

        WorldChunkMap worldChunkMap = (WorldChunkMap)target;
        worldChunkMap.InitializeWorldExits();

        // {{ FIND GOLDEN PATH AFTER UPDATING EXITS }}
        EditorGUI.BeginChangeCheck(); // Start checking for changes
        EditorGUILayout.PropertyField(worldExitsProperty); // Display and potentially edit the specific value
        if (EditorGUI.EndChangeCheck()) // Check if any changes were made
        {
            serializedChunkMap.ApplyModifiedProperties(); // Apply changes to the serialized object

            WorldCoordinateMap.ResetCoordinateMap();
            worldChunkMap.FindGoldenPath();

            EditorUtility.SetDirty(target); // Mark the object as dirty to ensure changes are saved
        }
    }

    private void OnSceneGUI()
    {
        WorldChunkMap worldChunkMap = (WorldChunkMap)target;
        worldChunkMap.InitializeWorldExits();
    }
}
