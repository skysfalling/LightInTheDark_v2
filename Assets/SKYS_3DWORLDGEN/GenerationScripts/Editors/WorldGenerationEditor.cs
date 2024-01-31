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
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        worldGen.SetWorldDimensions();

        // Visualize the full playArea
        Handles.color = Color.white;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3Int(worldGen.realWorldPlayAreaSize.x, 0, worldGen.realWorldPlayAreaSize.y));

        // Visualize the full world boundary
        Handles.color = Color.red;
        Handles.DrawWireCube(worldGen.transform.position, new Vector3Int(worldGen.realWorldBoundarySize.x, 0, worldGen.realWorldBoundarySize.y));

        // Calculate and visualize each theoretical chunk grid
        Handles.color = Color.green;

        Vector3 chunkSize = new Vector3(worldGen.realWorldChunkSize.x, 0, worldGen.realWorldChunkSize.z);
        Vector3 halfSize_chunkSize = chunkSize * 0.5f;

        Vector2 fullSize_playArea = (Vector2)worldGen.realWorldPlayAreaSize;
        Vector2 halfSize_playArea = fullSize_playArea * 0.5f;

        for (int x = 0; x < worldGen.worldPlayArea_widthInChunks; x++)
        {
            for (int z = 0; z < worldGen.worldPlayArea_widthInChunks; z++)
            {
                Vector3 chunkPosition = new Vector3(x * chunkSize.x, 0, z * chunkSize.z);
                chunkPosition -= new Vector3(halfSize_playArea.x, 0, halfSize_playArea.y); // adjust for playAreaCenter
                chunkPosition += new Vector3(halfSize_chunkSize.x, 0, halfSize_chunkSize.z); // adjust for chunkCenter


                Handles.DrawWireCube(chunkPosition, chunkSize);
            }
        }
    }
}