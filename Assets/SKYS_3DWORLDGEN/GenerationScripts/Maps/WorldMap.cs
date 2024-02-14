using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UIElements;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WorldCoordinateMap), typeof(WorldChunkMap), typeof(WorldCellMap))]
public class WorldMap : MonoBehaviour
{
    public static bool worldMapInitialized { get; private set; }

    public void UpdateWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        worldCoordinateMap.UpdateCoordinateMap();
        worldChunkMap.UpdateChunkMap();

        worldMapInitialized = true;
    }

    public void GenerateWorldCellMap()
    {
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
        worldChunkMap.InitializeChunkMesh();

        WorldCellMap worldCellMap = GetComponent<WorldCellMap>();
        //worldCellMap.UpdateCellMap();
    }

    public void ResetWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
        WorldCellMap worldCellMap = GetComponent<WorldCellMap>();

        worldCoordinateMap.DestroyCoordinateMap();
        worldChunkMap.DestroyChunkMap();
        worldCellMap.Reset();

        worldMapInitialized = false;

        UpdateWorldMap();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    bool _showDebugSettingsFoldout = false;
    bool _showSelectedChunkFoldout = false;
    bool _showInitializationFoldout = false;
    Vector2 scrollPosition;

    GUIStyle titleHeaderStyle;
    GUIStyle centeredStyle;

    bool _showWorldMapBoundaries = false;

    int _labelWidth = 125;

    enum WorldCoordinateMapDebug { NONE, COORDINATE, TYPE }
    WorldCoordinateMapDebug worldCoordinateMapDebug = WorldCoordinateMapDebug.NONE;

    enum WorldChunkMapDebug { NONE, ALL_CHUNKS }
    WorldChunkMapDebug worldChunkMapDebug = WorldChunkMapDebug.ALL_CHUNKS;

    WorldChunk selectedChunk;

    Color transparentWhite = new Color(255, 255, 255, 0.5f);


    public void OnEnable()
    {
        WorldMap worldMap = (WorldMap)target;
        worldMap.ResetWorldMap();
    }

    private void OnSceneGUI()
    {
        WorldMap worldMap = (WorldMap)target;
        if (WorldMap.worldMapInitialized)
        {
            DrawWorldMap();

            if (selectedChunk != null)
            {
                if (selectedChunk.generation_finished)
                {
                    foreach (WorldCell cell in selectedChunk.localCells)
                    {
                        DrawRectangleAtCell(cell, transparentWhite, 0.75f);
                    }
                }
                else
                {
                    DrawRectangleAtChunkGround(selectedChunk, transparentWhite, 0.75f);
                }
            }
        }
        else
        {
            selectedChunk = null;
        }

        if (_showWorldMapBoundaries)
        {
            // >> DRAW BOUNDARY SQUARES
            Handles.color = Color.white;
            Handles.DrawWireCube(worldMap.transform.position, new Vector3(WorldGeneration.GetRealPlayAreaSize().x, 0, WorldGeneration.GetRealPlayAreaSize().y));

            Handles.color = Color.red;
            Handles.DrawWireCube(worldMap.transform.position, new Vector3(WorldGeneration.GetRealFullWorldSize().x, 0, WorldGeneration.GetRealFullWorldSize().y));
        }
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

        GUIStyle h1Style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            fixedHeight = 22
        };
        GUIStyle pStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 12,
            fontStyle = FontStyle.Normal,
        };
        pStyle.margin.left = 20;

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;
        foldoutStyle.margin.left = 5;
        foldoutStyle.margin.bottom = 5;

        EditorGUILayout.LabelField("World Map", titleHeaderStyle, GUILayout.Height(25));
        EditorGUILayout.Space();


        // [[[[[[[[[ BEGIN DOUBLE COLUMN ]] >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        // == COLUMN #1 =================================== ))

        #region DRAW GUI WORLD MAP ===============================================

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
            if (GUILayout.Button("Generate World"))
                worldGeneration.StartGeneration();
        }
        else { EditorGUILayout.HelpBox("World Generation requires Play Mode.", MessageType.Error); }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // RESET BUTTON >>>>>>>>>>>>>>>>>>>>
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate Cell Map"))
        {
            worldMap.GenerateWorldCellMap();
        }

        if (GUILayout.Button("Full Reset"))
        {
            worldMap.ResetWorldMap();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical(); // End the vertical group
        #endregion ===================================================================

        // == end COLUMN #1 ========================================== ))

        EditorGUILayout.Space(25);

        // == COLUMN #2 =================================== ))
        EditorGUILayout.BeginVertical();

        #region DRAW FOLDOUTS =================================================
        // << DEBUG TYPES >>
        int debugEnumWidth = 100;

        EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
        EditorGUILayout.BeginVertical();

        // >>>> Initialization Foldout ====================================== \\\\\\
        _showInitializationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showInitializationFoldout, new GUIContent("Initialization"), foldoutStyle);
        if (_showInitializationFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);

            CreateToggle("Coordinate Map", WorldCoordinateMap.coordMapInitialized);
            CreateToggle("Coordinate Neighbors", WorldCoordinateMap.coordNeighborsInitialized);
            CreateToggle("Zones", WorldCoordinateMap.zonesInitialized);
            CreateToggle("Exit Paths", WorldCoordinateMap.exitPathsInitialized);
            CreateToggle("World Chunk Map", WorldChunkMap.chunkMapInitialized);
            CreateToggle("World Chunk Mesh", WorldChunkMap.chunkMeshInitialized);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        // ======================================= //// 

        EditorGUILayout.Space(10);

        // >>>> Debug Settings Foldout ====================================== \\\\\
        _showDebugSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugSettingsFoldout, new GUIContent("Debug Settings"), foldoutStyle);
        if (_showDebugSettingsFoldout)
        {
            // World Map
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Show Boundaries =>", GUILayout.Width(_labelWidth));
            _showWorldMapBoundaries = EditorGUILayout.Toggle(_showWorldMapBoundaries); ;
            EditorGUILayout.EndHorizontal();

            // WorldCoordinateMap
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CoordinateMap  =>", GUILayout.Width(_labelWidth));
            worldCoordinateMapDebug = (WorldCoordinateMapDebug)EditorGUILayout.EnumPopup(worldCoordinateMapDebug, GUILayout.Width(debugEnumWidth));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // WorldChunkMap
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ChunkMap =>", GUILayout.Width(_labelWidth));
            worldChunkMapDebug = (WorldChunkMapDebug)EditorGUILayout.EnumPopup(worldChunkMapDebug, GUILayout.Width(debugEnumWidth));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        // ======================================= //// 

        EditorGUILayout.Space(10);

        // >>>> Selected Chunk Foldout ======================================== \\\\\
        _showSelectedChunkFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showSelectedChunkFoldout, new GUIContent("Selected Chunk"), foldoutStyle);
        if (_showSelectedChunkFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            if (selectedChunk == null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Selected chunk is NULL" +
                                         $"\nSelect chunk from map." +
                                         $"\n<---------------------", GUILayout.Height(50));
            }
            else
            {
                WorldCoordinate worldCoord = selectedChunk.worldCoordinate;

                string chunkParameters =
                    $"Coordinate => {worldCoord.Coordinate}" +
                    $"\nCoordinate Type => {worldCoord.type}" +
                    $"\nCoordinate Neighbors => {worldCoord.NeighborMap.Values.ToList().Count()}" +
                    $"\n" +
                    $"\nChunk GroundHeight => {selectedChunk.groundHeight}" +
                    $"\nChunk Mesh Dimensions => {selectedChunk.groundMeshDimensions}";

                if (WorldCellMap.cellMapInitialized)
                {
                    chunkParameters += "\n" +
                        $"\nChunk LocalCells => {selectedChunk.localCells.Count}";
                }
                else
                {
                    chunkParameters += "\n" +
                        "\nCell Map not initialized";
                }


                GUILayout.Box(chunkParameters, pStyle, GUILayout.Height(200));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        // ======================================= //// 

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        #endregion ================================================================

        EditorGUILayout.EndVertical();
        // == end COLUMN #2 ========================================== ))


        // [[[[[[[[[ END DOUBLE COLUMN ]] >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

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


        // ================================================= >>

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }
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

        // << DRAW CHUNK MAP >>
        foreach (WorldChunk chunk in WorldChunkMap.ChunkList)
        {
            switch (worldChunkMapDebug)
            {
                case WorldChunkMapDebug.ALL_CHUNKS:
                    DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    break;
            }
        }

        // << DRAW BASE COORDINATE MAP >>
        List<WorldCoordinate> coordList = WorldCoordinateMap.CoordinateList;
        foreach (WorldCoordinate coord in coordList)
        {
            switch(worldCoordinateMapDebug)
            {
                case WorldCoordinateMapDebug.COORDINATE:
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.Coordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinateMapDebug.TYPE:
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.type}"), coordinatelabelStyle);
                    break;
            }
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
                    WorldChunk worldChunk = WorldChunkMap.GetChunkAt(worldCoord);
                    if (worldCoord != null)
                    {
                        // Make the color brighter by interpolating towards white
                        GUI.backgroundColor = worldCoord.debugColor;
                        if (selectedChunk == worldChunk)
                        {
                            GUI.backgroundColor = Color.Lerp(worldCoord.debugColor, Color.white, 0.75f);
                        }

                        if (GUILayout.Button("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize)))
                        {
                            selectedChunk = worldChunk;
                        }

                        GUI.backgroundColor = Color.white; // Reset color to default

                        continue; // continue to next coordinate
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void DrawRectangleAtChunkGround(WorldChunk worldChunk, Color fillColor, float scaleMultiplier = 1)
    {
        if (WorldChunkMap.chunkMapInitialized == false || worldChunk == null) return;

        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(worldChunk.GetGroundWorldPosition(),
            (Vector2)WorldGeneration.GetRealChunkArea() * scaleMultiplier),
            fillColor, Color.clear);
    }

    private void DrawRectangleAtCell(WorldCell worldCell, Color fillColor, float scaleMultiplier = 1)
    {
        if (WorldChunkMap.chunkMapInitialized == false || worldCell == null) return;

        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(worldCell.position,
            Vector2.one * WorldGeneration.CellSize * scaleMultiplier),
            fillColor, Color.clear);
    }

    private void DrawRectangleAtWorldCoordinate(WorldCoordinate coord, Color fillColor)
    {
        if (WorldCoordinateMap.coordMapInitialized == false || coord == null) return;

        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(
            GetRectangleVertices(coord.WorldPosition,
            WorldGeneration.GetRealChunkArea()),
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


    // DRAW CHUNK


    // DRAW CELL



    // ================ HELPER FUNCTIONS ========================================================= ////
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

    private void CreateToggle(string label, bool currentValue)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
        EditorGUILayout.Toggle("", currentValue);
        EditorGUILayout.EndHorizontal();
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


