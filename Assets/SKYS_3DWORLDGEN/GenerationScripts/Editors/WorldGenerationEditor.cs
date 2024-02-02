using UnityEngine;
using UnityEditor;

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

        // >> DRAW BOUNDARY SQUARE
        Handles.color = Color.red;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3(WorldGeneration.GetRealFullWorldSize().x, 0, WorldGeneration.GetRealFullWorldSize().y));

        // Draw Chunk Grid
        //DrawChunkGrid();

        // Draw World Exits
        //DrawWorldExits();
    }

    /*
    void DrawChunkGrid()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        Handles.color = Color.white;

        // Visualize Chunk Grid
        foreach (WorldChunk chunk in worldGen.GetChunks())
        {
            if (chunk.isOnGoldenPath) { Handles.color = Color.yellow; }

            Vector3 chunkWorldPosition = new Vector3(chunk.coordinate.x, -worldGen.realWorldChunkVolume.y, chunk.coordinate.y);
            Handles.DrawWireCube(chunkWorldPosition, worldGen.realWorldChunkVolume);
        }
    }
    */

    /*
    void DrawWorldExits()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        Handles.color = Color.red;

        // Visualize each WorldExit
        foreach (WorldExit worldExit in worldGen.worldExits)
        {
            if (worldExit == null) continue;

            Vector3 exitPosition = worldGen.GetWorldExitPosition(worldExit);

            Handles.DrawWireCube(exitPosition + (Vector3.down * worldGen.realWorldChunkVolume.y), worldGen.realWorldChunkVolume);
            Handles.Label(exitPosition, $"Exit: {worldExit.edgeDirection}");
        }
    }
    */
}