#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCoordinateMap))]
public class WorldCoordinateMapEditor : Editor
{
    SerializedObject serializedCoordinateMap;
    SerializedProperty worldExitPathsProperty;
    private GUIStyle h1Style;
    private GUIStyle h2Style;

    private void OnEnable()
    {
        serializedCoordinateMap = new SerializedObject(target);
        worldExitPathsProperty = serializedCoordinateMap.FindProperty("worldExitPaths");

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
    }

    public override void OnInspectorGUI()
    {
        serializedCoordinateMap.Update();
        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;


        EditorGUILayout.LabelField("Exit Paths", h1Style);
        EditorGUILayout.Space();

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        // Display each WorldExitPath with a foldout
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
        }

        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedCoordinateMap.ApplyModifiedProperties();

            // Get your WorldCoordinateMap component
            worldCoordMap.InitializeWorldExitPaths();
            DrawMap();

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
        if (worldCoordMap.mapInitialized == false) { return; }

        DrawMap();
    }

    private void DrawMap()
    {
        List<WorldCoordinate> coordMap = WorldCoordinateMap.GetCoordinateMap();
        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();
        Vector2Int realChunkArea = WorldGeneration.GetRealChunkAreaSize();
        Vector3 chunkHeightOffset = realChunkDimensions.y * Vector3.down * 0.5f;

        // << DRAW BASE COORDINATE MAP >>
        // Start by defining a GUIStyle for your labels
        GUIStyle coordinatelabelStyle = new GUIStyle();
        coordinatelabelStyle.fontSize = 10; // Adjust font size
        coordinatelabelStyle.normal.textColor = Color.white; // Text color
        coordinatelabelStyle.alignment = TextAnchor.MiddleCenter; // Center the text
        foreach (WorldCoordinate coord in coordMap)
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
                case WorldCoordinate.TYPE.ZONE:
                    DrawRectangleAtCoord(coord, Color.green);
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

    }

    void DrawExitPath(WorldExitPath path)
    {
        if (path == null || !path.IsInitialized()) return;

        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();
        Handles.color = path.GetPathColorRGBA();

        // Draw Exits
        WorldCoordinate startExitCoord = path.startExit.Coordinate;
        WorldCoordinate endExitCoord = path.endExit.Coordinate;
        Handles.DrawWireCube(startExitCoord.WorldPosition, realChunkDimensions);
        Handles.DrawWireCube(endExitCoord.WorldPosition, realChunkDimensions);

        // Draw Paths
        List<WorldCoordinate> pathCoords = path.GetPathCoordinates();
        foreach (WorldCoordinate coord in pathCoords)
        {
            Handles.DrawWireCube(coord.WorldPosition, realChunkDimensions);
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