using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldRegion))]
public class WorldRegionEditor : Editor
{
    private SerializedObject serializedObject;
    private WorldRegion region;

    GUIStyle titleHeaderStyle;
    GUIStyle centeredStyle;
    GUIStyle rightAlignedStyle;
    GUIStyle h1Style;
    GUIStyle h2Style;
    GUIStyle pStyle;


    WorldSpace editorViewSpace = WorldSpace.Region;

    // Coordinate Map
    enum CoordinateMapDebug { NONE, COORDINATE, TYPE }
    CoordinateMapDebug coordinateMapDebugType = CoordinateMapDebug.TYPE;
    Coordinate selectedCoordinate = null;

    // Chunk Map
    enum ChunkMapDebug { NONE, COORDINATE_TYPE, CHUNK_TYPE, CHUNK_HEIGHT }
    ChunkMapDebug chunkMapDebugType = ChunkMapDebug.COORDINATE_TYPE;
    WorldChunk selectedChunk = null;


    private void OnEnable()
    {
        serializedObject = new SerializedObject(target);
        region = (WorldRegion)target;

        // ================================================= >>
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        #region STYLES ======================= >>>>
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

        rightAlignedStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight
        };

        h1Style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40,
        };

        h2Style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40,
        };

        pStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 12,
            fontStyle = FontStyle.Normal,
        };
        pStyle.margin.left = 20;
        #endregion ================================== ////


        // [[ EDITOR VIEW ]]
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Editor View Space  =>");
        editorViewSpace = (WorldSpace)EditorGUILayout.EnumPopup(editorViewSpace);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // [[ REGION VIEW ]]
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Region", h1Style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Region Coordinate:", region.regionCoordinate.ToString());
        EditorGUILayout.LabelField("Center Position:", region.centerPosition.ToString());
        EditorGUILayout.LabelField("Origin Coordinate Position:", region.originCoordinatePosition.ToString());
        EditorGUILayout.LabelField("Region Initialized:", region.IsInitialized().ToString());
        EditorGUILayout.EndVertical();

        switch(editorViewSpace)
        {
            case WorldSpace.Chunk:
                DrawChunkMapInspector();
                break;
            case WorldSpace.Region:
                DrawCoordinateMapInspector();
                break;
        }

        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    void DrawChunkMapInspector()
    {
        WorldChunkMap chunkMap = region.worldChunkMap;

        EditorGUILayout.LabelField("Region Chunk Map", h2Style);
        EditorGUILayout.Space(10);

        // >> initialize button
        if (region.worldChunkMap == null)
        {
            if (GUILayout.Button("Initialize Chunk Map"))
            {
                region.CreateChunkMap();
            }
            return;
        };

        // << DEBUG VIEW >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Debug View  =>");
        chunkMapDebugType = (ChunkMapDebug)EditorGUILayout.EnumPopup(chunkMapDebugType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // << SELECTED CHUNK >>
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Selected Chunk", h2Style);
        EditorGUILayout.Space(10);
        if (selectedChunk != null && selectedChunk.coordinate != null)
        {
            EditorGUILayout.LabelField($"Coordinate Local Position:  {selectedChunk.coordinate.localPosition}");
            EditorGUILayout.LabelField($"Chunk Ground Height:  {selectedChunk.groundHeight}");
            EditorGUILayout.LabelField($"Chunk Type:  {selectedChunk.type}");
        }
        else
        {
            EditorGUILayout.LabelField($"Please Select a Chunk in the Scene View");
        }
        EditorGUILayout.EndVertical();

    }


    void DrawCoordinateMapInspector()
    {
        // [[ COORDINATE MAP ]]
        CoordinateMap coordinateMap = region.coordinateMap;

        EditorGUILayout.LabelField("Region Coordinate Map", h2Style);
        EditorGUILayout.Space(10);

        // >> initialize button
        if (region.coordinateMap == null)
        {
            if (GUILayout.Button("Initialize Coordinate Map"))
            {
                region.Initialize(region.regionCoordinate);
            }
            return;
        }
        else if (GUILayout.Button("Reset Coordinate Map"))
        {
            region.coordinateMap = new CoordinateMap(region);
        }

        // << DEBUG VIEW >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Debug View  =>");
        coordinateMapDebugType = (CoordinateMapDebug)EditorGUILayout.EnumPopup(coordinateMapDebugType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // >> show coordinate type data
        if (coordinateMapDebugType == CoordinateMapDebug.TYPE)
        {
            EditorGUILayout.LabelField($"NULL TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.NULL).Count}", rightAlignedStyle);
            EditorGUILayout.LabelField($"BORDER TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.BORDER).Count}", rightAlignedStyle);
            EditorGUILayout.LabelField($"CLOSED TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.CLOSED).Count}", rightAlignedStyle);
            EditorGUILayout.LabelField($"EXIT TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.EXIT).Count}", rightAlignedStyle);
        }

        // << SELECTED COORDINATE >>
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Selected Coordinate", h2Style);
        EditorGUILayout.Space(10);
        if (selectedCoordinate != null)
        {
            EditorGUILayout.LabelField($"Space:  {selectedCoordinate.worldSpace}");
            EditorGUILayout.LabelField($"Local Coordinate: {selectedCoordinate.localPosition}");
            EditorGUILayout.LabelField($"Type: {selectedCoordinate.type}");
            EditorGUILayout.LabelField($"World Position: {selectedCoordinate.worldPosition}");
            EditorGUILayout.LabelField($"Neighbor Count: {selectedCoordinate.GetAllValidNeighbors().Count}");
        }
        else
        {
            EditorGUILayout.LabelField($"Please Select a Coordinate in the Scene View");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        // << EXITS >>
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Exits", h2Style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Exit Count: {coordinateMap.exitPositions.Count}");

        if (coordinateMap.exitPositions.Count > 0)
        {
            foreach (Vector2Int pos in coordinateMap.exitPositions)
            {
                EditorGUILayout.LabelField($">> Exit At: {pos}");
            }
        }

        // >>>> convert selected to exit
        if (selectedCoordinate != null && GUILayout.Button("Convert Selected Coordinate to EXIT"))
        {
            coordinateMap.ConvertCoordinateToExit(selectedCoordinate);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // << PATHS >>
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Paths", h2Style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Path Count: {coordinateMap.worldPaths.Count}");

        // >>>> create paths
        if (coordinateMap.exitPositions.Count > 1)
        {
            if (GUILayout.Button("Create Path"))
            {
                coordinateMap.CreatePathFrom(coordinateMap.exitPositions[0], coordinateMap.exitPositions[1]);
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
    }


    // ==================== SCENE GUI =================================================== ////


    protected void OnSceneGUI()
    {
        WorldGeneration worldGen = region.GetComponentInParent<WorldGeneration>();
        Transform transform = region.transform;

        GUIStyle labelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 12, // Example font size
        };

        DarklightGizmos.DrawWireSquare_withLabel("World Generation", worldGen.transform.position, WorldGeneration.GetWorldWidth_inWorldSpace(), Color.black, labelStyle);
        DarklightGizmos.DrawWireSquare_withLabel("World Region", transform.position, WorldGeneration.GetFullRegionWidth_inWorldSpace(), Color.blue, labelStyle);

        switch(editorViewSpace)
        {
            case WorldSpace.World:
                DrawWorldView();
                break;
            case WorldSpace.Region:
                DrawRegionView(); 
                break;
            case WorldSpace.Chunk:
                DrawChunkView();
                break;
            case WorldSpace.Cell:
                DrawCellView();
                break;
        }
    }


    void DrawWorldView()
    {

    }

    void DrawRegionView()
    {
        if (region == null || region.coordinateMap == null) { return; }

        GUIStyle coordLabelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 12, // Example font size
            normal = new GUIStyleState { textColor = Color.blue } // Set the text color
        };

        // Draw Coordinates
        CoordinateMap coordinateMap = region.coordinateMap;
        if (coordinateMap.IsInitialized() && coordinateMap.allPositions.Count > 0)
        {   
            foreach (Vector2Int position in coordinateMap.allPositions)
            {
                Coordinate coordinate = coordinateMap.GetCoordinateAt(position);

                if (selectedCoordinate != null && position == selectedCoordinate.localPosition)
                {
                    DrawCoordinateNeighbors(coordinateMap.GetCoordinateAt(position));
                    continue;
                }

                DarklightGizmos.DrawButtonHandle(coordinate.worldPosition, Vector3.right * 90, WorldGeneration.CellWidth_inWorldSpace * 0.5f, Color.black, () =>
                {
                    SelectCoordinate(coordinate);
                });

                switch (coordinateMapDebugType)
                {
                    case CoordinateMapDebug.COORDINATE:
                        DarklightGizmos.DrawWireSquare(coordinate.worldPosition, WorldGeneration.CellWidth_inWorldSpace, Color.blue);
                        DarklightGizmos.DrawLabel($"{coordinate.localPosition}", coordinate.worldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
                        break;
                    case CoordinateMapDebug.TYPE:
                        coordLabelStyle.normal.textColor = coordinate.debugColor;
                        DarklightGizmos.DrawWireSquare(coordinate.worldPosition, WorldGeneration.CellWidth_inWorldSpace, coordinate.debugColor);
                        DarklightGizmos.DrawLabel($"{coordinate.type}", coordinate.worldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
                        break;
                }
            }
        }
        else
        {
            DarklightGizmos.DrawWireSquare_withLabel($"region origin", region.originCoordinatePosition, 10, Color.red, coordLabelStyle);
        }
    }

    void DrawChunkView()
    {
        if (region == null || region.worldChunkMap == null) { return; }
        GUIStyle chunkLabelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 12, // Example font size
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState { textColor = Color.black } // Set the text color
        };

        WorldChunkMap chunkMap = region.worldChunkMap;
        if (chunkMap.initialized)
        {
            foreach (WorldChunk chunk in chunkMap.allChunks)
            {
                Color chunkDebugColor = Color.green;
                string chunkDebugString = "Chunk";

                switch(chunkMapDebugType)
                {
                    case ChunkMapDebug.COORDINATE_TYPE:
                        chunkDebugColor = chunk.coordinate.debugColor;
                        chunkDebugString = $"{chunk.coordinate.type}";
                        break;
                    case ChunkMapDebug.CHUNK_TYPE:
                        chunkDebugColor = chunk.debugColor;
                        chunkDebugString = $"{chunk.type}";
                        break;
                    case ChunkMapDebug.CHUNK_HEIGHT:
                        chunkDebugColor = Color.Lerp(Color.black, Color.white, (float)chunk.groundHeight / (float)WorldGeneration.RegionMaxGroundHeight);
                        chunkDebugString = $"{chunk.groundHeight}";
                        break;
                }


                DarklightGizmos.DrawFilledSquareAt(chunk.groundPosition, WorldGeneration.GetChunkWidth_inWorldSpace() * 0.75f, Vector3.up, chunkDebugColor);
                DarklightGizmos.DrawLabel(chunkDebugString, chunk.groundPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace * 2), chunkLabelStyle);

                DarklightGizmos.DrawButtonHandle(chunk.groundPosition, Vector3.right * 90, WorldGeneration.CellWidth_inWorldSpace * 0.5f, Color.black, () =>
                {
                    SelectChunk(chunk);
                });

            }
        }
    }

    void DrawCellView()
    {
        if (region == null || region.worldChunkMap == null) { return; }
        GUIStyle cellLabelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 8, // Example font size
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState { textColor = Color.black } // Set the text color
        };

        WorldChunkMap chunkMap = region.worldChunkMap;
        if (chunkMap.initialized)
        {
            foreach (WorldChunk chunk in chunkMap.allChunks)
            {
                Color chunkDebugColor = Color.white;
                string chunkDebugString = "";

                DarklightGizmos.DrawButtonHandle(chunk.groundPosition, Vector3.right * 90, WorldGeneration.CellWidth_inWorldSpace * 0.5f, Color.black, () =>
                {
                    SelectChunk(chunk);
                });

            }

            if (selectedChunk != null)
            {
                foreach (Coordinate coordinate in selectedChunk.coordinateMap.allCoordinates)
                {
                    DarklightGizmos.DrawFilledSquareAt(coordinate.worldPosition, WorldGeneration.CellWidth_inWorldSpace * 0.75f, Vector3.up, Color.white);
                    DarklightGizmos.DrawLabel($"{coordinate.localPosition}", coordinate.worldPosition, cellLabelStyle);
                }
            }
        }
    }

    void SelectCoordinate(Coordinate coordinate)
    {
        selectedCoordinate = coordinate;

        Repaint();
    }

    void SelectChunk(WorldChunk chunk)
    {
        selectedChunk = chunk;
        Repaint();
    }


    void DrawCoordinateNeighbors(Coordinate coordinate)
    {
        if (coordinate.initialized)
        {
            List<Coordinate> natural_neighbors = coordinate.GetValidNaturalNeighbors();

            foreach (Coordinate neighbor in natural_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.worldPosition, direction, Color.red);
            }

            List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
            foreach (Coordinate neighbor in diagonal_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.worldPosition, direction, Color.yellow);
            }
        }
    }

}
