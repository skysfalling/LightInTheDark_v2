using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldGeneration))]
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
        WorldChunkMap.GetCoordinateMap();
        
        // Ensure changes are registered and the inspector updates as needed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(worldChunkMap);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;

        // >> SET WORLD DIMENSIONS
        worldGen.InitializeWorldDimensions();

        // >> DRAW BOUNDARY SQUARES
        Handles.color = Color.white;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3(WorldGeneration.GetRealPlayAreaSize().x, 0, WorldGeneration.GetRealPlayAreaSize().y));

        Handles.color = Color.red;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3(WorldGeneration.GetRealFullWorldSize().x, 0, WorldGeneration.GetRealFullWorldSize().y));
    }
}
