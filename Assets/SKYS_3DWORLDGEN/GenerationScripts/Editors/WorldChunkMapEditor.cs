#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldChunkMap))]
public class WorldChunkMapEditor : Editor
{
    SerializedObject serializedObject;
    SerializedProperty property;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target);
        //property = serializedObject.FindProperty("yourProperty");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


    }

    private void OnSceneGUI()
    {
        WorldChunkMap map = (WorldChunkMap)target;
        List<WorldChunk> chunkMap = WorldChunkMap.GetChunkMap();
        if (chunkMap.Count > 0)
        {
            foreach (WorldChunk chunk in chunkMap)
            {
                Handles.color = chunk.debugColor;
                //Handles.DrawWireCube(chunk.GroundMeshSpawnPosition, chunk.GroundMeshDimensions);
                DrawRectangleArea(chunk.GroundPosition, WorldGeneration.GetRealChunkAreaSize(), chunk.debugColor);
            }
        }
    }

    private void DrawRectangleArea(Vector3 worldPos, Vector2Int area, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(worldPos, area), fillColor, Color.clear);
    }

    private Vector3[] GetRectangleVertices(Vector3 center, Vector2 area)
    {
        Vector2 halfArea = area * 0.5f;
        Vector3[] vertices = new Vector3[4]
        {
            center + new Vector3(-halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, halfArea.y),
            center + new Vector3(-halfArea.x, 0, halfArea.y)
        };

        return vertices;
    }
}
#endif