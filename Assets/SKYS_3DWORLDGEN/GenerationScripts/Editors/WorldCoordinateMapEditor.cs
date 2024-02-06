#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCoordinateMap))]
public class WorldCoordinateMapEditor : Editor
{
    SerializedObject serializedCoordinateMap;
    SerializedProperty worldExitPathsProperty;

    private void OnEnable()
    {
        serializedCoordinateMap = new SerializedObject(target);
        worldExitPathsProperty = serializedCoordinateMap.FindProperty("worldExitPaths");

        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
        worldCoordMap.InitializeWorldExitPaths();
    }

    public override void OnInspectorGUI()
    {
        serializedCoordinateMap.Update();

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        // Display each WorldExitPath with a foldout
        if (worldExitPathsProperty != null)
        {
            EditorGUILayout.LabelField("World Exit Paths", EditorStyles.boldLabel); // Optional: Add a section label

            for (int i = 0; i < worldExitPathsProperty.arraySize; i++)
            {
                SerializedProperty exitProperty = worldExitPathsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(exitProperty, new GUIContent($"World Exit Path {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Exit Path"))
            {
                worldExitPathsProperty.arraySize++;
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
            WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;

            // Call your initialization method here
            worldCoordMap.InitializeWorldExitPaths();

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
        List<WorldCoordinate> coordMap = WorldCoordinateMap.GetCoordinateMap();
        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();
        Vector2Int realChunkArea = WorldGeneration.GetRealChunkAreaSize();
        Vector3 chunkHeightOffset = realChunkDimensions.y * Vector3.down * 0.5f;

        // << DRAW BASE COORDINATE MAP >>
        foreach (WorldCoordinate coord in coordMap)
        {
            switch (coord.type)
            {
                case WorldCoordinate.TYPE.NULL:
                    Handles.color = Color.white;
                    Handles.DrawWireCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.BORDER:
                    Handles.color = Color.red;
                    Handles.DrawWireCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.CLOSED:
                    DrawRectangleArea(coord, realChunkArea, Color.black);
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

        Vector2Int realChunkArea = WorldGeneration.GetRealChunkAreaSize();
        Color pathColor = path.GetPathColorRGBA();

        // Draw Exits
        WorldCoordinate startCoord = path.startExit.Coordinate;
        WorldCoordinate endCoord = path.endExit.Coordinate;

        DrawRectangleArea(startCoord, realChunkArea, pathColor);
        DrawRectangleArea(endCoord, realChunkArea, pathColor);

        // Draw Paths
        List<WorldCoordinate> pathCoords = path.GetPathCoordinates();
        foreach (WorldCoordinate coord in pathCoords)
        {
            DrawRectangleArea(coord, realChunkArea, pathColor);
        }
    }

    private void DrawRectangleArea(WorldCoordinate coord, Vector2Int area, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(coord.WorldPosition, area), fillColor, Color.clear);
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