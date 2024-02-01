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
        EditorGUILayout.LabelField("Real World Chunk Size", worldGen.realWorldChunkSize.ToString());
        EditorGUILayout.LabelField("Real World Boundary Size", worldGen.realWorldBoundarySize.ToString());


        // Ensure changes are registered and the inspector updates as needed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(worldGen);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        worldGen.SetWorldDimensions();

        Vector3 halfSize_chunkSize = (Vector3)worldGen.realWorldChunkSize * 0.5f;
        Vector2 halfSize_playArea = (Vector2)worldGen.realWorldPlayAreaSize * 0.5f;

        // Calculate and visualize each theoretical chunk grid
        Handles.color = Color.white;
        foreach(Vector2 chunkPosition in worldGen.GetAllPlayAreaChunkPositions())
        {
            Vector3 chunkWorldPosition = new Vector3(chunkPosition.x, -worldGen.realWorldChunkSize.y, chunkPosition.y);
            Handles.DrawWireCube(chunkWorldPosition, worldGen.realWorldChunkSize);
        }

        Handles.color = Color.red;
        foreach (Vector2 chunkPosition in worldGen.GetAllBoundaryChunkPositions())
        {
            Vector3 chunkWorldPosition = new Vector3(chunkPosition.x, -worldGen.realWorldChunkSize.y, chunkPosition.y);
            Handles.DrawWireCube(chunkWorldPosition, worldGen.realWorldChunkSize);
        }

        // Label for play area width in cells
        string playAreaWidthLabel = $"Play Area Width: {WorldGeneration.WorldWidthInChunks} Chunks";
        Vector3 labelPosition = worldGen.transform.position - new Vector3(0, halfSize_playArea.y + halfSize_chunkSize.y, 0); // Position the label below the play area
        Handles.Label(labelPosition, playAreaWidthLabel);

        // Draw World Exits
        DrawWorldExits(worldGen);

    }

    private void DrawWorldExits(WorldGeneration worldGen)
    {
        // Visualize each WorldExit
        foreach (WorldExit worldExit in worldGen.worldExits)
        {
            if (worldExit == null) continue;

            Vector3 exitPosition = worldGen.GetWorldExitPosition(worldExit);

            Handles.color = Color.yellow;
            Handles.DrawWireCube(exitPosition + (Vector3.up * 2), worldGen.realWorldChunkSize);
            Handles.Label(exitPosition, $"Exit: {worldExit.edgeDirection}");
        }
    }

}