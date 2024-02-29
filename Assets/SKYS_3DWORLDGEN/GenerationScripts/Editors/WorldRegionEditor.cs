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
    enum CoordinateMapDebug { NONE, COORDINATE, TYPE, CHUNK_HEIGHT }
    CoordinateMapDebug coordinateMapDebugType = CoordinateMapDebug.COORDINATE;
    Coordinate selectedCoordinate = null;
    

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

        if (region.coordinateMap != null && GUILayout.Button("Reset Coordinate Map"))
        {
            region.coordinateMap = new CoordinateMap(region);
        }

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

        // [[ COORDINATE MAP ]]
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Coordinate Map", h2Style);
        EditorGUILayout.Space(10);
        if (region.coordinateMap != null)
        {
            CoordinateMap coordinateMap = region.coordinateMap;
            EditorGUILayout.LabelField($"Coordinate Map Initialized: {coordinateMap.IsInitialized()}");
            EditorGUILayout.LabelField($"Coordinate Count: {coordinateMap.allCoordinates.Count}");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug View  =>");
            coordinateMapDebugType = (CoordinateMapDebug)EditorGUILayout.EnumPopup(coordinateMapDebugType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (coordinateMapDebugType == CoordinateMapDebug.TYPE)
            {
                EditorGUILayout.LabelField($"NULL TYPE : {coordinateMap.GetAllCoordinatesOfType(Coordinate.TYPE.NULL).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"BORDER TYPE : {coordinateMap.GetAllCoordinatesOfType(Coordinate.TYPE.BORDER).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"CLOSED TYPE : {coordinateMap.GetAllCoordinatesOfType(Coordinate.TYPE.CLOSED).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"EXIT TYPE : {coordinateMap.GetAllCoordinatesOfType(Coordinate.TYPE.EXIT).Count}", rightAlignedStyle);

            }

            // << SELECTED COORDINATE >>
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10); // INDENTATION

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected Coordinate", h2Style);
            EditorGUILayout.Space(10);
            if (selectedCoordinate != null)
            {
                EditorGUILayout.LabelField($"Space:  {selectedCoordinate.Space}");
                EditorGUILayout.LabelField($"Local Coordinate: {selectedCoordinate.LocalPosition}");
                EditorGUILayout.LabelField($"Type: {selectedCoordinate.type}");
                EditorGUILayout.LabelField($"World Position: {selectedCoordinate.WorldPosition}");
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
            EditorGUILayout.Space(10); // INDENTATION

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Exits", h1Style);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Exit Count: {coordinateMap.worldPaths.Count}");

            // >>>> convert selected to exit
            if (selectedCoordinate != null && GUILayout.Button("Convert Selected Coordinate to EXIT"))
            {
                coordinateMap.SetCoordinateToType(selectedCoordinate, Coordinate.TYPE.EXIT);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // << PATHS >>
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10); // INDENTATION

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Paths", h1Style);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Path Count: {coordinateMap.worldPaths.Count}");

            // >>>> convert selected to exit
            if (selectedCoordinate != null && GUILayout.Button("Convert Selected Coordinate to EXIT"))
            {
                //coordinateMap.SetCoordinateToType(selectedCoordinate, Coordinate.TYPE.EXIT, Color.red);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        else
        {
            // >>>> initialize coordinate map
            EditorGUILayout.LabelField($"Coordinate Map Initialized: False");
            if (GUILayout.Button("Initialize Coordinate Map"))
            {
                region.Initialize(region.regionCoordinate);
            }
        }
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
        }

    }

    void SelectCoordinate (Coordinate coordinate)
    {
        selectedCoordinate = coordinate;

        Repaint();
    }

    // TODO
    private void OnDestroy()
    {
        
    }

    void DrawCoordinateNeighbors(Coordinate coordinate)
    {
        if (coordinate.foundNeighbors)
        {
            List<Coordinate> natural_neighbors = coordinate.GetValidNaturalNeighbors();

            foreach (Coordinate neighbor in natural_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = Coordinate.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.WorldPosition, direction, Color.red);
            }

            List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
            foreach (Coordinate neighbor in diagonal_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = Coordinate.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.WorldPosition, direction, Color.yellow);
            }

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
        if (region.coordinateMap.allCoordinates != null)
        {
            foreach (Coordinate coord in region.coordinateMap.allCoordinates)
            {

                if (coord == selectedCoordinate)
                {
                    DrawCoordinateNeighbors(coord);
                    continue;
                }

                DarklightGizmos.DrawButtonHandle(coord.WorldPosition, Vector3.right * 90, WorldGeneration.CellWidth_inWorldSpace * 0.5f, Color.black, () =>
                {
                    SelectCoordinate(coord);
                });


                switch (coordinateMapDebugType)
                {
                    case CoordinateMapDebug.COORDINATE:
                        DarklightGizmos.DrawWireSquare(coord.WorldPosition, WorldGeneration.CellWidth_inWorldSpace, Color.blue);
                        DarklightGizmos.DrawLabel($"{coord.LocalPosition}", coord.WorldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
                        break;
                    case CoordinateMapDebug.TYPE:

                        coordLabelStyle.normal.textColor = coord.debugColor;
                        DarklightGizmos.DrawWireSquare(coord.WorldPosition, WorldGeneration.CellWidth_inWorldSpace, coord.debugColor);
                        DarklightGizmos.DrawLabel($"{coord.type}", coord.WorldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
                        break;
                    case CoordinateMapDebug.CHUNK_HEIGHT:
                        //WorldChunk chunk = WorldChunkMap.GetChunkAt(coord);
                        //DarklightGizmos.DrawLabel($"{coord.}", coord.WorldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
                        break;
                }

            }
        }
        else
        {
            DarklightGizmos.DrawWireSquare_withLabel($"region origin", region.originCoordinatePosition, 10, Color.red, coordLabelStyle);
        }
    }


}
