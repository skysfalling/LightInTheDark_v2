using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    private SerializedObject serializedWorldGen;
    private bool toggleBoundaries;

    private void OnEnable()
    {
        // Cache the SerializedObject
        serializedWorldGen = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector elements

        serializedWorldGen.Update(); // Always start with this call

        WorldGeneration worldGen = (WorldGeneration)target;
        EditorGUILayout.LabelField("Cell Size {in Units}", WorldGeneration.CellSize.ToString());
        EditorGUILayout.LabelField("Chunk Dimensions {in Cells}", WorldGeneration.ChunkArea.ToString());
        EditorGUILayout.LabelField("Play Zone Area {in Chunks}", WorldGeneration.PlayZoneArea.ToString());
        EditorGUILayout.LabelField("Real Chunk Area Size {in Units}", WorldGeneration.GetRealChunkAreaSize().ToString());
        EditorGUILayout.LabelField("Real Full World Size {in Units}", WorldGeneration.GetRealFullWorldSize().ToString());

        // Ensure changes are registered and the inspector updates as needed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(worldGen);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        // >> DRAW BOUNDARY SQUARES
        Handles.color = Color.white;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3(WorldGeneration.GetRealPlayAreaSize().x, 0, WorldGeneration.GetRealPlayAreaSize().y));

        Handles.color = Color.red;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3(WorldGeneration.GetRealFullWorldSize().x, 0, WorldGeneration.GetRealFullWorldSize().y));
    }
}