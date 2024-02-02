using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    private SerializedObject serializedWorldGen;
    SerializedProperty edgeDirectionProperty;
    SerializedProperty edgeIndexProperty;

    private void OnEnable()
    {
        // Cache the SerializedObject
        serializedWorldGen = new SerializedObject(target);
        edgeDirectionProperty = serializedWorldGen.FindProperty("edgeDirection");
        edgeIndexProperty = serializedWorldGen.FindProperty("edgeIndex");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector elements

        serializedWorldGen.Update(); // Always start with this call

        WorldGeneration worldGen = (WorldGeneration)target;
        EditorGUILayout.LabelField("Real Chunk Area Size", WorldGeneration.GetRealChunkAreaSize().ToString());
        EditorGUILayout.LabelField("Real Full World Size", WorldGeneration.GetRealFullWorldSize().ToString());

        WorldChunkMap.GetCoordinateMap(true);
        worldGen.SetWorldExitCoordinates();

        // Ensure changes are registered and the inspector updates as needed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(worldGen);
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