using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        EditorGUILayout.LabelField("Real World Chunk Size", worldGen.realWorldChunkSize.ToString());
        EditorGUILayout.LabelField("Real World Boundary Size", worldGen.realWorldBoundarySize.ToString());

        DrawDefaultInspector(); // Draws the default inspector elements

        // Assuming you have a public integer property in WorldGeneration to set
        // Adjust "worldPlayArea_widthInChunks" if your actual property name differs
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("World Play Area Width in Chunks", GUILayout.Width(250));
        EditorGUILayout.IntSlider(worldGen.worldPlayArea_widthInChunks, -1, worldGen.worldBoundary_widthInChunks);
        EditorGUILayout.EndHorizontal();

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
        string playAreaWidthLabel = $"Play Area Width: {worldGen.worldPlayArea_widthInChunks} Chunks";
        Vector3 labelPosition = worldGen.transform.position - new Vector3(0, 0, halfSize_playArea.y + halfSize_chunkSize.y); // Position the label below the play area
        Handles.Label(labelPosition, playAreaWidthLabel);

    }
}