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

        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
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
        if (WorldCoordinateMap.coordMapInitialized == false) { return; }

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
                    DrawRectangleAtCoord(coord, Color.clear);
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.Coordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinate.TYPE.BORDER:
                    DrawRectangleAtCoord(coord, Color.red);
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.Coordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinate.TYPE.CLOSED:
                    DrawRectangleAtCoord(coord, Color.black);
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.Coordinate}"), coordinatelabelStyle);
                    break;
            }
        }

        // DRAW PATHS
        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
        foreach (WorldExitPath path in worldCoordMap.worldExitPaths)
        {
            DrawExitPath(path);
        }

        // DRAW ZONES
        foreach (WorldZone zone in worldCoordMap.worldZones)
        {
            DrawZone(zone);
        }

    }

    void DrawExitPath(WorldExitPath path)
    {
        if (path == null || !path.IsInitialized()) return;

        Color pathColorRGBA = path.GetPathColorRGBA();

        // Draw Exits
        WorldCoordinate startExitCoord = path.startExit.Coordinate;
        WorldCoordinate endExitCoord = path.endExit.Coordinate;
        DrawRectangleAtCoord(startExitCoord, pathColorRGBA);
        DrawRectangleAtCoord(endExitCoord, pathColorRGBA);

        // Draw Paths
        List<WorldCoordinate> pathCoords = path.GetPathCoordinates();
        if (pathCoords == null) return;
        foreach (WorldCoordinate coord in pathCoords)
        {
            DrawRectangleAtCoord(coord, pathColorRGBA);
        }
    }

    void DrawZone(WorldZone zone)
    {
        if (zone == null || !zone.IsInitialized()) return;

        Color zoneColorRGBA = WorldZone.GetRGBAfromZoneColorType(zone.zoneColor);

        // Draw Zones
        List<WorldCoordinate> zoneCoords = zone.GetZoneCoordinates();
        foreach (WorldCoordinate coord in zoneCoords)
        {
            DrawRectangleAtCoord(coord, zoneColorRGBA);
        }
    }

    private void DrawRectangleAtCoord(WorldCoordinate coord, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(coord.WorldPosition, WorldGeneration.GetRealChunkAreaSize()), fillColor, Color.clear);
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