using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UIElements;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WorldCoordinateMap), typeof(WorldChunkMap), typeof(WorldCellMap))]
public class WorldMap : MonoBehaviour
{
    public void UpdateWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        worldCoordinateMap.UpdateCoordinateMap();
        worldChunkMap.UpdateChunkMap();
    }

    public void ResetWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
        WorldCellMap worldCellMap = GetComponent<WorldCellMap>();

        worldCoordinateMap.DestroyCoordinateMap();
        worldChunkMap.DestroyChunkMap();
        worldCellMap.Reset();

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    private bool showWorldCoordinateMap = true;
    private bool showWorldChunkMap = true;
    private Vector2 scrollPosition;

    public void OnEnable()
    {
        WorldMap worldMap = (WorldMap)target;
        worldMap.ResetWorldMap();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WorldMap worldMap = (WorldMap)target;
        WorldCoordinateMap worldCoordinateMap = worldMap.GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = worldMap.GetComponent<WorldChunkMap>();

        WorldGeneration worldGeneration = FindObjectOfType<WorldGeneration>();

        // ================================================= >>

        UpdateGUIWorldMap();

        EditorGUILayout.BeginHorizontal();
        // Check if the editor is not in play mode
        if (!EditorApplication.isPlaying)
        {
            // Display an error message or perform your checks here
            EditorGUILayout.HelpBox("You can only generate in Play Mode.", MessageType.Error);
        }
        else if (GUILayout.Button("Generate"))
        {
            worldGeneration.StartGeneration();
        }


        if (GUILayout.Button("Update Map"))
        {
            worldMap.UpdateWorldMap();
        }

        if (GUILayout.Button("Full Reset"))
        {
            worldMap.ResetWorldMap();
        }
        EditorGUILayout.EndHorizontal();

        // ================================================= >>

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }
    }

    private void UpdateGUIWorldMap()
    {
        // Control the size of each box representing a coordinate
        float mapGUIBoxSize = 10f;
        int mapWidth = WorldGeneration.GetFullWorldArea().x;
        int mapHeight = WorldGeneration.GetFullWorldArea().y;

        // Begin a scroll view to handle maps that won't fit in the inspector window
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mapWidth * (mapGUIBoxSize * 1.5f)), GUILayout.Height(mapHeight * (mapGUIBoxSize * 1.5f)));

        // Attempt to center the map grid horizontally
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Add space before to center the content

        // Create a flexible layout to start drawing the map
        GUILayout.BeginVertical();
        for (int y = mapHeight - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < mapWidth; x++)
            {
                if (WorldCoordinateMap.coordMapInitialized)
                {
                    WorldCoordinate worldCoord = WorldCoordinateMap.GetCoordinateAt(new Vector2Int(x, y));
                    if (worldCoord != null)
                    {
                        // Draw a box for each type of coordinate with different colors
                        Color color = worldCoord.debugColor;
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
                        EditorGUI.DrawRect(rect, color);
                        continue; // continue to next coordinate
                    }
                }

                // Draw a default box for null coordinates
                GUILayout.Box("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void OnSceneGUI()
    {

        DrawWorldMap();
    }

    private void DrawCoordinateMap()
    {

    }

    private void DrawWorldMap()
    {
        if (WorldCoordinateMap.coordMapInitialized == false || WorldChunkMap.chunkMapInitialized == false) { return; }

        // Start by defining a GUIStyle for your labels
        GUIStyle coordinatelabelStyle = new GUIStyle();
        coordinatelabelStyle.fontSize = 10; // Adjust font size
        coordinatelabelStyle.normal.textColor = Color.black; // Text color
        coordinatelabelStyle.alignment = TextAnchor.MiddleCenter; // Center the text

        // << DRAW BASE COORDINATE MAP >>
        List<WorldCoordinate> coordList = WorldCoordinateMap.CoordinateList;
        foreach (WorldCoordinate coord in coordList)
        {
            switch (coord.type)
            {
                case WorldCoordinate.TYPE.NULL:
                case WorldCoordinate.TYPE.BORDER:
                case WorldCoordinate.TYPE.CLOSED:
                    DrawRectangleAtWorldCoordinate(coord, coord.debugColor);
                    break;
            }
        }

        // << DRAW CHUNK MAP >>
        foreach (WorldChunk chunk in WorldChunkMap.ChunkList)
        {
            switch (chunk.worldCoordinate.type)
            {
                case WorldCoordinate.TYPE.PATH:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    Handles.Label(chunk.groundPosition, new GUIContent($"PATH {chunk.coordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinate.TYPE.ZONE:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    Handles.Label(chunk.groundPosition, new GUIContent($"ZONE {chunk.coordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinate.TYPE.EXIT:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    Handles.Label(chunk.groundPosition, new GUIContent($"EXIT {chunk.coordinate}"), coordinatelabelStyle);
                    break;
            }
        }
    }

    private void DrawRectangleAtChunkGround(WorldChunk worldChunk, Color fillColor)
    {
        if (WorldChunkMap.chunkMapInitialized == false || worldChunk == null) return;

        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(worldChunk.GetGroundWorldPosition(),
            WorldGeneration.GetRealChunkAreaSize()),
            fillColor, Color.clear);
    }

    private void DrawRectangleAtWorldCoordinate(WorldCoordinate coord, Color fillColor)
    {
        if (WorldCoordinateMap.coordMapInitialized == false || coord == null) return;

        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(coord.WorldPosition,
            WorldGeneration.GetRealChunkAreaSize()),
            fillColor, Color.clear);
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
