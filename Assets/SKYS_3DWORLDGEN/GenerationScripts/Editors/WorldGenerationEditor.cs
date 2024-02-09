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
        WorldGeneration.InitializeRandomSeed();
    }

    public override void OnInspectorGUI()
    {
        serializedWorldGen.Update(); // Always start with this call

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        SerializedProperty gameSeedProperty = serializedWorldGen.FindProperty("gameSeed");
        EditorGUILayout.PropertyField(gameSeedProperty);

        EditorGUILayout.LabelField("============");
        EditorGUILayout.LabelField("CurrentSeed", WorldGeneration.CurrentSeed.ToString());
        EditorGUILayout.LabelField("Cell Size {in Units}", WorldGeneration.CellSize.ToString());
        EditorGUILayout.LabelField("Chunk Dimensions {in Cells}", WorldGeneration.ChunkArea.ToString());
        EditorGUILayout.LabelField("Play Zone Area {in Chunks}", WorldGeneration.PlayZoneArea.ToString());
        EditorGUILayout.LabelField("Real Chunk Area Size {in Units}", WorldGeneration.GetRealChunkAreaSize().ToString());
        EditorGUILayout.LabelField("Real Full World Size {in Units}", WorldGeneration.GetRealFullWorldSize().ToString());

        EditorGUILayout.Space();
        if (GUILayout.Button("Start Generation"))
        {
            WorldGeneration worldGeneration = (WorldGeneration)target;
            worldGeneration.StartGeneration();
        }

        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedWorldGen.ApplyModifiedProperties();

            WorldGeneration.InitializeRandomSeed(gameSeedProperty.stringValue);

            WorldGeneration worldGen = (WorldGeneration)target;

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
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