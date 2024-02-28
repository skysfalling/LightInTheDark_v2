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
    GUIStyle h1Style;
    GUIStyle h2Style;
    GUIStyle pStyle;


    WorldSpace editorViewSpace = WorldSpace.Region;

    // Coordinate Map
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

        h1Style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40,
        };

        h2Style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
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

        // [[ CHUNK COORDINATE MAP ]]
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Chunk Coordinate Map", h1Style);
        EditorGUILayout.Space(10);
        if (region.coordinateMap != null)
        {
            CoordinateMap coordinateMap = region.coordinateMap;
            EditorGUILayout.LabelField($"Coordinate Map Initialized: {coordinateMap.coordMapInitialized}");
            EditorGUILayout.LabelField($"Coordinate Neighbors Initialized: {coordinateMap.coordNeighborsInitialized}");
            EditorGUILayout.LabelField($"Coordinate Count: {coordinateMap.coordinates.Count}");

            EditorGUILayout.LabelField("Selected Coordinate", h1Style);
            EditorGUILayout.Space(10);
            if (selectedCoordinate != null)
            {
                EditorGUILayout.LabelField($"Space:  {selectedCoordinate.Space}");
                EditorGUILayout.LabelField($"Local Coordinate: {selectedCoordinate.LocalCoordinate}");
                EditorGUILayout.LabelField($"World Position: {selectedCoordinate.WorldPosition}");
                EditorGUILayout.LabelField($"Neighbor Count: {selectedCoordinate.GetAllValidNeighbors().Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Please Select a Coordinate in the Scene View");
            }


        }
        else
        {
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
        if (region.coordinateMap.coordinates != null)
        {
            foreach (Coordinate coord in region.coordinateMap.coordinates)
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

                DarklightGizmos.DrawWireSquare(coord.WorldPosition, WorldGeneration.CellWidth_inWorldSpace, Color.blue);
                DarklightGizmos.DrawLabel($"{coord.LocalCoordinate}", coord.WorldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
            }
        }
        else
        {
            DarklightGizmos.DrawWireSquare_withLabel($"region origin", region.originCoordinatePosition, 10, Color.red, coordLabelStyle);
        }
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

}
