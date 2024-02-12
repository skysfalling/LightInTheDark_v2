#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCoordinateMap))]
public class WorldCoordinateMapEditor : Editor
{
    SerializedObject serializedCoordinateMap;
    private GUIStyle h1Style;
    private GUIStyle h2Style;
    private GUIStyle worldStyle;

    private void OnEnable()
    {
        serializedCoordinateMap = new SerializedObject(target);

        // Initialize GUIStyle for H1 header
        h1Style = new GUIStyle()
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        h1Style.normal.textColor = Color.grey;

        // Initialize GUIStyle for H2 header
        h2Style = new GUIStyle(h1Style) // Inherit from H1 and override as needed
        {
            fontSize = 14,
            fontStyle = FontStyle.Italic
        };
        h2Style.normal.textColor = Color.grey;

        worldStyle = new GUIStyle()
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        worldStyle.normal.textColor = Color.black;
    }

    public override void OnInspectorGUI()
    {
        serializedCoordinateMap.Update();
        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;

        EditorGUILayout.LabelField($"World Coordinate Map Initialized => {WorldCoordinateMap.coordMapInitialized}");
        EditorGUILayout.Space();

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        //======================================================== ////
        // WORLD PATHS
        //======================================================== ////
        EditorGUILayout.LabelField("Exit Paths", h1Style);
        EditorGUILayout.Space();
        #region World Exit Paths List

        SerializedProperty worldExitPathsProperty = serializedCoordinateMap.FindProperty("worldExitPaths");
        if (worldExitPathsProperty != null)
        {
            for (int i = 0; i < worldExitPathsProperty.arraySize; i++)
            {
                SerializedProperty exitProperty = worldExitPathsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(exitProperty, new GUIContent($"World Exit Path {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Exit Path"))
            {
                worldCoordMap.CreateWorldExitPath();
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (worldExitPathsProperty.arraySize > 0)
                {
                    worldExitPathsProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        #endregion

        //======================================================== ////
        // WORLD ZONES
        //======================================================== ////
        EditorGUILayout.LabelField("World Zones", h1Style);
        EditorGUILayout.Space();

        // Display each WorldZone with a foldout
        SerializedProperty worldZonesProperty = serializedCoordinateMap.FindProperty("worldZones");
        if (worldZonesProperty != null)
        {
            for (int i = 0; i < worldZonesProperty.arraySize; i++)
            {
                SerializedProperty zoneProperty = worldZonesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(zoneProperty, new GUIContent($"World Zone {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Zone"))
            {
                worldCoordMap.CreateWorldZone();
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (worldZonesProperty.arraySize > 0)
                {
                    worldZonesProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }

        //======================================================== ////
        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedCoordinateMap.ApplyModifiedProperties();

            worldCoordMap.UpdateCoordinateMap();

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {

        DrawMap();
    }

    private void DrawMap()
    {
        if (WorldCoordinateMap.coordMapInitialized == false || WorldChunkMap.chunkMapInitialized == false) { return; }

        List<WorldCoordinate> coordList = WorldCoordinateMap.CoordinateList;
        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();
        Vector2Int realChunkArea = WorldGeneration.GetRealChunkAreaSize();
        Vector3 chunkHeightOffset = realChunkDimensions.y * Vector3.down * 0.5f;

        // << DRAW BASE COORDINATE MAP >>
        // Start by defining a GUIStyle for your labels
        GUIStyle coordinatelabelStyle = new GUIStyle();
        coordinatelabelStyle.fontSize = 10; // Adjust font size
        coordinatelabelStyle.normal.textColor = Color.white; // Text color
        coordinatelabelStyle.alignment = TextAnchor.MiddleCenter; // Center the text
        foreach (WorldCoordinate coord in coordList)
        {
            switch (coord.type)
            {
                case WorldCoordinate.TYPE.NULL:
                case WorldCoordinate.TYPE.BORDER:
                case WorldCoordinate.TYPE.CLOSED:
                    DrawRectangleAtWorldCoordinate(coord, coord.debugColor);
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.Coordinate}"), coordinatelabelStyle);
                    break;
            }
        }

        // Draw the chunks
        foreach (WorldChunk chunk in WorldChunkMap.ChunkList)
        {
            switch (chunk.worldCoordinate.type)
            {
                case WorldCoordinate.TYPE.PATH:
                case WorldCoordinate.TYPE.ZONE:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    Handles.Label(chunk.groundPosition, new GUIContent($"{chunk.coordinate}"), coordinatelabelStyle);
                    break;
            }
        }
    }

    void DrawExitPath(WorldExitPath exitPath)
    {
        if (exitPath == null || !exitPath.IsInitialized()) return;

        Color pathColorRGBA = exitPath.GetPathColorRGBA();

        // Draw Exits
        WorldCoordinate startExitCoord = exitPath.startExit.WorldCoordinate;
        WorldCoordinate endExitCoord = exitPath.endExit.WorldCoordinate;
        DrawRectangleAtWorldCoordinate(startExitCoord, pathColorRGBA);
        DrawRectangleAtWorldCoordinate(endExitCoord, pathColorRGBA);

        if (WorldChunkMap.chunkMapInitialized)
        {

            // Draw Chunks
            List<WorldChunk> chunks = exitPath.GetPathChunks();
            if (chunks == null || chunks.Count == 0) return;
            foreach (WorldChunk chunk in chunks)
            {
                DrawRectangleAtChunkGround(chunk, pathColorRGBA);
            }

            // Draw Exits
            DrawRectangleAtChunkGround(exitPath.startExit.Chunk, pathColorRGBA);
            DrawRectangleAtChunkGround(exitPath.endExit.Chunk, pathColorRGBA);
            DrawLabel(exitPath.startExit.Chunk.GetGroundWorldPosition(), $"Path Start", worldStyle, pathColorRGBA);
            DrawLabel(exitPath.endExit.Chunk.GetGroundWorldPosition(), $"Path Exit", worldStyle, pathColorRGBA);
        }


    }

    void DrawZone(WorldZone zone)
    {
        if (zone == null || !zone.IsInitialized()) return;

        Color zoneColorRGBA = WorldZone.GetRGBAfromDebugColor(zone.zoneColor);

        // Draw Zones
        List<WorldChunk> zoneChunks = zone.GetZoneChunks();
        foreach (WorldChunk chunk in zoneChunks)
        {
            DrawRectangleAtChunkGround(chunk, zoneColorRGBA);
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

    /// <summary>
    /// Draws a label in the Scene view with customizable style and optional background.
    /// </summary>
    /// <param name="position">The world space position where the label will be drawn.</param>
    /// <param name="text">The text of the label.</param>
    /// <param name="labelStyle">The style to be applied to the label.</param>
    /// <param name="backgroundColor">Optional background color. If null, no background will be drawn.</param>
    public void DrawLabel(Vector3 position, string text, GUIStyle labelStyle, Color? backgroundColor = null)
    {
        Handles.BeginGUI();
        // Convert world position to GUI position
        Vector2 guiPosition = HandleUtility.WorldToGUIPoint(position);

        // Calculate the size of the label
        Vector2 size = labelStyle.CalcSize(new GUIContent(text));
        Rect rect = new Rect(guiPosition.x, guiPosition.y, size.x, size.y);

        // Draw background if color is specified
        if (backgroundColor.HasValue)
        {
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.Value;
            GUI.Box(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), GUIContent.none);
            GUI.backgroundColor = previousColor;
        }

        // Draw the label
        GUI.Label(rect, text, labelStyle);
        Handles.EndGUI();
    }
}
#endif