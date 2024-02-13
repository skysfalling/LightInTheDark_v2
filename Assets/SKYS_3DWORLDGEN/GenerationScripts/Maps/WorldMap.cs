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

        UpdateWorldMap();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    private bool _showInitializationFoldout = false;
    private Vector2 scrollPosition;

    GUIStyle titleHeaderStyle;
    GUIStyle centeredStyle;

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

        titleHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        #region DRAW GUI WORLD MAP ===============================================

        EditorGUILayout.LabelField("World Map", titleHeaderStyle, GUILayout.Height(25));
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(); // Start a vertical group
        GUILayout.FlexibleSpace(); // Push everything after this down

            EditorGUILayout.BeginHorizontal(); // Start a horizontal group
            GUILayout.FlexibleSpace(); // Push everything after this to the right, centering the content

            DrawGUIWorldMap();

            GUILayout.FlexibleSpace(); // Push everything before this to the left, ensuring centering
            EditorGUILayout.EndHorizontal(); // End the horizontal group

        GUILayout.FlexibleSpace(); // Push everything before this up, creating space at the bottom


        // GENERATE BUTTON >>>>>>>>>>>>>>>>>
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Generate"))
                worldGeneration.StartGeneration();
        }
        else { EditorGUILayout.HelpBox("You can only generate in Play Mode.", MessageType.Error); }

        // RESET BUTTON >>>>>>>>>>>>>>>>>>>>
        if (GUILayout.Button("Full Reset"))
        {
            worldMap.ResetWorldMap();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.EndVertical(); // End the vertical group
        #endregion


        #region GENERATION PARAMETERS ===========================================

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        CreateIntegerControl("Cell Size", WorldGeneration.CellSize, 1, 8, (value) => WorldGeneration.CellSize = value);
        CreateIntegerControl("Chunk Width", WorldGeneration.ChunkWidth.x, 1, 10, (value) => WorldGeneration.ChunkWidth = new Vector2Int(value, value));
        CreateIntegerControl("Chunk Depth", WorldGeneration.ChunkDepth, 1, 10, (value) => WorldGeneration.ChunkDepth = value);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        CreateIntegerControl("Playable Area", WorldGeneration.PlayableArea.x, 1, 10, (value) => WorldGeneration.PlayableArea = new Vector2Int(value, value));
        CreateIntegerControl("Boundary Offset", WorldGeneration.BoundaryOffset, 1, 10, (value) => WorldGeneration.BoundaryOffset = value);
        CreateIntegerControl("Max Ground Height", WorldGeneration.MaxGroundHeight, 1, 10, (value) => WorldGeneration.MaxGroundHeight = value);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        #endregion

        EditorGUILayout.Space();

        #region INITIALIZATION LAYOUT ===============================================

        _showInitializationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showInitializationFoldout, new GUIContent("Initialization"));
        if (_showInitializationFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);

            // Column 1 -> WORLD COORDINATE MAP
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Toggle("World Coordinate Map", WorldCoordinateMap.coordMapInitialized);
            EditorGUILayout.Toggle("Zones", WorldCoordinateMap.zonesInitialized);
            EditorGUILayout.Toggle("Exit Paths", WorldCoordinateMap.exitPathsInitialized);
            EditorGUILayout.EndVertical();

            // Column 2 -> WORLD CHUNK MAP
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Toggle("World Chunk Map", WorldChunkMap.chunkMapInitialized);
            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();


        #endregion ==============================================================


        EditorGUILayout.Space();

        // ================================================= >>

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }
    }


    private void OnSceneGUI()
    {
        DrawWorldMap();
    }

    #region == DRAW WORLD MAP ============================================== >>>>
    private void DrawWorldMap()
    {
        if (WorldCoordinateMap.coordMapInitialized == false || WorldChunkMap.chunkMapInitialized == false) { return; }

        // Start by defining a GUIStyle for your labels
        GUIStyle coordinatelabelStyle = new GUIStyle();
        coordinatelabelStyle.fontSize = 10; // Adjust font size
        coordinatelabelStyle.normal.textColor = Color.white; // Text color
        coordinatelabelStyle.alignment = TextAnchor.MiddleCenter; // Center the text
        coordinatelabelStyle.fontStyle = FontStyle.Bold;

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
                    break;
                case WorldCoordinate.TYPE.ZONE:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    break;
                case WorldCoordinate.TYPE.EXIT:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    break;
            }

            Handles.Label(chunk.groundPosition,
                new GUIContent($"{chunk.worldCoordinate.type}" +
                $"\nheight: {chunk.groundHeight}"),
                coordinatelabelStyle);
        }
    }
    private void DrawGUIWorldMap()
    {

        // Control the size of each box representing a coordinate
        float mapGUIBoxSize = 20f;
        int mapWidth = WorldGeneration.GetFullWorldArea().x;
        int mapHeight = WorldGeneration.GetFullWorldArea().y;

        // Begin a scroll view to handle maps that won't fit in the inspector window
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mapWidth * (mapGUIBoxSize * 1.15f)), GUILayout.Height(mapHeight * (mapGUIBoxSize * 1.15f)));

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
                        /*
                        // Draw a box for each type of coordinate with different colors
                        Color color = worldCoord.debugColor;
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
                        EditorGUI.DrawRect(rect, color);
                        */

                        // Make the color brighter by interpolating towards white
                        GUI.backgroundColor = worldCoord.debugColor;

                        if (GUILayout.Button("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize)))
                        {
                            WorldCoordinate worldCoordinate = WorldCoordinateMap.GetCoordinateAt(new Vector2Int(x, y));
                            Debug.Log($"Clicked Coordinate: {worldCoordinate.Coordinate} >> TYPE {worldCoordinate.type}");
                        }

                        GUI.backgroundColor = Color.white; // Reset color to default

                        continue; // continue to next coordinate
                    }
                }

                // Draw a default button for null coordinates
                if (GUILayout.Button("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize)))
                {
                    Debug.Log($"Clicked Empty Coordinate: {x}, {y}");
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
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
    #endregion


    void CreateIntegerControl(string title, int currentValue, int minValue, int maxValue, System.Action<int> setValue)
    {
        WorldMap worldMap = (WorldMap)target;

        GUIStyle controlStyle = new GUIStyle();
        controlStyle.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
        controlStyle.alignment = TextAnchor.UpperCenter;
        controlStyle.margin = new RectOffset(20, 20, 20, 20);
        controlStyle.fixedWidth = 120;

        EditorGUILayout.BeginVertical(controlStyle);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(title, centeredStyle, GUILayout.MaxWidth(100));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // +/- Buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("-", GUILayout.MaxWidth(20))) { 
            setValue(Mathf.Max(minValue, currentValue - 1));
            worldMap.ResetWorldMap();
        }
        EditorGUILayout.LabelField($"{currentValue}", centeredStyle, GUILayout.MaxWidth(50));
        if (GUILayout.Button("+", GUILayout.MaxWidth(20))) { 
            setValue(Mathf.Min(maxValue, currentValue + 1));
            worldMap.ResetWorldMap();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}
#endif


