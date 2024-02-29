using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UIElements;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldRegionMap : MonoBehaviour
{
    public static bool worldMapInitialized { get; private set; }

    /*
    public void UpdateRegionMap()
    {
        CoordinateMap worldCoordinateMap = GetComponent<CoordinateMap>();
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
        worldCellMap.UpdateCellMap();
    }

    public void ResetWorldMap()
    {
        CoordinateMap worldCoordinateMap = GetComponent<CoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
        WorldCellMap worldCellMap = GetComponent<WorldCellMap>();

        worldCoordinateMap.DestroyCoordinateMap();
        worldChunkMap.DestroyChunkMap();
        worldCellMap.Reset();

        worldMapInitialized = false;
    }
    */
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldRegionMap))]
public class WorldRegionMapEditor : Editor
{
    bool _showDebugSettingsFoldout = false;
    bool _showSelectedChunkFoldout = false;
    bool _showInitializationFoldout = false;
    Vector2 scrollPosition;

    GUIStyle titleHeaderStyle;
    GUIStyle centeredStyle;

    bool _showWorldMapBoundaries = true;

    int _labelWidth = 125;

    enum WorldCoordinateMapDebug { NONE, COORDINATE, TYPE, CHUNK_HEIGHT }
    WorldCoordinateMapDebug worldCoordinateMapDebug = WorldCoordinateMapDebug.COORDINATE;

    enum WorldChunkMapDebug { NONE, ALL_CHUNKS }
    WorldChunkMapDebug worldChunkMapDebug = WorldChunkMapDebug.NONE;

    WorldChunk selectedChunk;

    Color transparentWhite = new Color(255, 255, 255, 0.5f);


    public void OnEnable()
    {

    }

    private void OnSceneGUI()
    {


        /*
        WorldRegionMap regionMap = (WorldRegionMap)target;

        if (_showWorldMapBoundaries)
        {
            WorldRegion region = regionMap.GetComponent<WorldRegion>();
            DarklightEditor.DrawWireRectangle_withLabel($"Region {region.regionCoordinate}", region.centerPosition, WorldGeneration.GetFullRegionWidth_inWorldSpace());
        }

        if (WorldRegionMap.worldMapInitialized)
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

        DarklightEditor.DrawWireRectangle_withLabel("World Generation", Vector3.zero, WorldGeneration.GetWorldWidth_inWorldSpace());
        */

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WorldRegionMap worldMap = (WorldRegionMap)target;
        CoordinateMap worldCoordinateMap = worldMap.GetComponent<CoordinateMap>();
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

        EditorGUILayout.LabelField("Region Map", titleHeaderStyle, GUILayout.Height(25));
        EditorGUILayout.Space();


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

        #endregion ===================================================================

        EditorGUILayout.Space(25);

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

            //CreateToggle("Coordinate Map", CoordinateMap.coordMapInitialized);
            CreateToggle("Chunk Map", WorldChunkMap.chunkMapInitialized);
            CreateToggle("Cell Map", WorldCellMap.cellMapInitialized);

            //CreateToggle("Coordinate Neighbors", CoordinateMap.coordNeighborsInitialized);
            //CreateToggle("Zones", CoordinateMap.zonesInitialized);
            //CreateToggle("Exit Paths", CoordinateMap.exitPathsInitialized);
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
                Coordinate coord = selectedChunk.worldCoordinate;
                try
                {
                    string chunkParameters =
                        $"Coordinate => {coord.localPosition}" +
                        $"\nCoordinate Type => {coord.type}" +
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
                catch
                {
                    Debug.LogWarning("Could not access selected chunk, setting to null");
                    selectedChunk = null;
                }



            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        // ======================================= //// 

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        #endregion ================================================================


        EditorGUILayout.EndVertical();

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
        //if (CoordinateMap.coordMapInitialized == false || WorldChunkMap.chunkMapInitialized == false) { return; }

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

                    //DrawRectangleAtChunkGround(chunk, chunk.worldCoordinate.debugColor);
                    break;
            }
        }

        /*
        // << DRAW BASE COORDINATE MAP >>
        List<Coordinate> coordList = CoordinateMap.CoordinateList;
        foreach (Coordinate coord in coordList)
        {
            switch(worldCoordinateMapDebug)
            {
                case WorldCoordinateMapDebug.COORDINATE:
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.LocalCoordinate}"), coordinatelabelStyle);
                    break;
                case WorldCoordinateMapDebug.TYPE:
                    Handles.Label(coord.WorldPosition, new GUIContent($"{coord.type}"), coordinatelabelStyle);
                    break;
                case WorldCoordinateMapDebug.CHUNK_HEIGHT:
                    WorldChunk chunk = WorldChunkMap.GetChunkAt(coord);
                    Handles.Label(coord.WorldPosition, new GUIContent($"{chunk.groundHeight}"), coordinatelabelStyle);
                    break;
            }
        }
        */



    }

    private void DrawGUIWorldMap()
    {

        // Control the size of each box representing a coordinate
        float mapGUIBoxSize = 10f;
        int mapWidth = WorldGeneration.GetFullRegionWidth_inChunks();
        int mapHeight = WorldGeneration.GetFullRegionWidth_inChunks();

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
                Coordinate worldCoord = null;// CoordinateMap.GetCoordinateAt(new Vector2Int(x, y));
                WorldChunk worldChunk = WorldChunkMap.GetChunkAt(worldCoord);
                if (worldCoord != null)
                {

                    // << SET WORLD CHUNK COLOR >>
                    GUI.backgroundColor = worldCoord.debugColor;

                    // << SET SELECTED CHUNK COLOR >>
                    if (selectedChunk != null)
                    {
                        // Make the color brighter by interpolating towards white
                        if (selectedChunk == worldChunk)
                        {
                            GUI.backgroundColor = Color.Lerp(worldCoord.debugColor, Color.white, 0.75f);
                        }
                        else
                        {
                            List<Vector2Int> naturalNeighbors = selectedChunk.worldCoordinate.GetValidNaturalNeighborCoordinates();
                            List<Vector2Int> diagonalNeighbors = selectedChunk.worldCoordinate.GetValidDiagonalNeighborCoordinates();

                            // Draw Natural Neighbors
                            if (naturalNeighbors.Contains(worldChunk.worldCoordinate.localPosition))
                            {
                                GUI.backgroundColor = Color.Lerp(worldCoord.debugColor, Color.white, 0.5f);
                            }

                            // Draw Diagonal Neighbors
                            else if (diagonalNeighbors.Contains(worldChunk.worldCoordinate.localPosition))
                            {
                                GUI.backgroundColor = Color.Lerp(worldCoord.debugColor, Color.white, 0.25f);
                            }

                        }
                    }


                    // Create Button
                    if (GUILayout.Button("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize)))
                    {
                        SelectChunk(worldChunk);
                    }

                    GUI.backgroundColor = Color.white; // Reset color to default

                    continue; // continue to next coordinate
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void SelectChunk(WorldChunk worldChunk)
    {
        selectedChunk = worldChunk;
    }

    #endregion


    // ================ HELPER FUNCTIONS ========================================================= ////

    private void CreateToggle(string label, bool currentValue)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
        EditorGUILayout.Toggle("", currentValue);
        EditorGUILayout.EndHorizontal();
    }


}
#endif


