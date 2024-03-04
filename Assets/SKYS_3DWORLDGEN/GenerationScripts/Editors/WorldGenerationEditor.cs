using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    private SerializedObject serializedWorldGen;
    static Coordinate.TYPE showCoordinateType = Coordinate.TYPE.EXIT;


    GUIStyle centeredStyle;

    private void OnEnable()
    {
        // Cache the SerializedObject
        serializedWorldGen = new SerializedObject(target);
        WorldGeneration.InitializeRandomSeed();

    }

    public override void OnInspectorGUI()
    {
        serializedWorldGen.Update(); // Always start with this call

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        #region GENERATION PARAMETERS ===========================================
        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        SerializedProperty gameSeedProperty = serializedWorldGen.FindProperty("gameSeed");
        EditorGUILayout.PropertyField(gameSeedProperty);
        EditorGUILayout.LabelField("Encoded Seed", WorldGeneration.CurrentSeed.ToString());
        EditorGUILayout.LabelField("============///");


        // [[ WORLD GENERATION PARAMETER EDITOR ]]

        /*
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("World Generation", DarklightEditor.TitleHeaderStyle);
        EditorGUILayout.Space(20);
        DarklightEditor.CreateIntegerControl("World Width In Regions", WorldGeneration.WorldWidth_inRegions, 1, 16, WorldGeneration.WorldWidth_inRegions);


        EditorGUILayout.LabelField("World Region", DarklightEditor.Header2Style);
        EditorGUILayout.Space(20);
        DarklightEditor.CreateIntegerControl("Playable Area In Chunks", WorldGeneration.PlayRegionWidth_inChunks, 1, 16, WorldGeneration.PlayRegionWidth_inChunks);
        DarklightEditor.CreateIntegerControl("Boundary Wall Count", WorldGeneration.BoundaryWallCount, 0, 3, WorldGeneration.BoundaryWallCount);

        EditorGUILayout.LabelField("World Chunk", DarklightEditor.Header2Style);
        EditorGUILayout.Space(20);
        DarklightEditor.CreateIntegerControl("Chunk Width in Cells", WorldGeneration.ChunkWidth_inCells, 1, 16, WorldGeneration.ChunkWidth_inCells);
        DarklightEditor.CreateIntegerControl("Chunk Depth in Cells", WorldGeneration.ChunkDepth_inCells, 1, 16, WorldGeneration.ChunkDepth_inCells);
        DarklightEditor.CreateIntegerControl("Max Ground Height", WorldGeneration.MaxChunkHeight, 4, 32, WorldGeneration.MaxChunkHeight);

        EditorGUILayout.LabelField("World Cell", DarklightEditor.Header2Style);
        EditorGUILayout.Space(20);
        DarklightEditor.CreateIntegerControl("Cell Size", WorldGeneration.CellWidth_inWorldSpace, 1, 16, WorldGeneration.CellWidth_inWorldSpace);

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        */

        #endregion

        WorldGeneration worldGen = (WorldGeneration)target;

        if (worldGen.worldRegions.Count == 0)
        {
            if (GUILayout.Button("Initialize"))
            {
                worldGen.Initialize();
            }
        }
        else
        {
            if (GUILayout.Button("Start Generation"))
            {
                worldGen.StartGeneration();
            }

            if (GUILayout.Button("Reset"))
            {
                worldGen.Reset();
            }

            // >> select debug view
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug View  =>");
            showCoordinateType = (Coordinate.TYPE)EditorGUILayout.EnumPopup(showCoordinateType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }




        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedWorldGen.ApplyModifiedProperties();

            WorldGeneration.InitializeRandomSeed(gameSeedProperty.stringValue);

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        Transform transform = worldGen.transform;

        GUIStyle labelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 12, // Example font size
        };

        DarklightGizmos.DrawWireSquare_withLabel("World Generation Size", worldGen.centerPosition_inWorldSpace, WorldGeneration.GetWorldWidth_inWorldSpace(), Color.black, labelStyle);
        
        if (worldGen.Initialized)
        {
            DarklightGizmos.DrawWireSquare_withLabel("Origin Region", worldGen.originPosition_inWorldSpace, WorldGeneration.GetChunkWidth_inWorldSpace(), Color.red, labelStyle);
        }

        DarklightGizmos.DrawWireSquare_withLabel("World Chunk Size", worldGen.centerPosition_inWorldSpace, WorldGeneration.GetChunkWidth_inWorldSpace(), Color.black, labelStyle);
        DarklightGizmos.DrawWireSquare_withLabel("World Cell Size", worldGen.centerPosition_inWorldSpace, WorldGeneration.CellWidth_inWorldSpace, Color.black, labelStyle);


        if (worldGen.Initialized && worldGen.worldRegions.Count > 0)
        {
            foreach (WorldRegion region in worldGen.worldRegions)
            {

                List<Coordinate> regionNeighbors = region.coordinate.GetAllValidNeighbors();

                if (region != null && region.Initialized)
                {
                    DarklightGizmos.DrawWireSquare_withLabel($"World Region {region.localCoordinatePosition}" +
                        $"\n neighbors : {regionNeighbors.Count}", region.centerPosition_inWorldSpace, WorldGeneration.GetFullRegionWidth_inWorldSpace(), Color.blue, labelStyle);

                    List<Vector2Int> coordinatesOfType = region.coordinateMap.GetAllPositionsOfType(showCoordinateType).ToList();
                    for (int i = 0; i < coordinatesOfType.Count; i++)
                    {
                        Coordinate coordinate = region.coordinateMap.GetCoordinateAt(coordinatesOfType[i]);

                        DarklightGizmos.DrawWireSquare_withLabel($"{showCoordinateType}", coordinate.WorldPosition, 
                            WorldGeneration.GetChunkWidth_inWorldSpace(), coordinate.debugColor, labelStyle);
                    }
                
                }

                DrawCoordinateNeighbors(region.coordinate);
            }
        }

        DrawCoordinates();
    }

    void DrawCoordinates()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        if (worldGen.coordinateRegionMap == null) { return; }

        GUIStyle coordLabelStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, // Example style
            fontSize = 12, // Example font size
            normal = new GUIStyleState { textColor = Color.blue } // Set the text color
        };
        // Draw Coordinates
        CoordinateMap coordinateMap = worldGen.coordinateRegionMap;
        if (coordinateMap.Initialized && coordinateMap.allPositions.Count > 0)
        {
            foreach (Vector2Int position in coordinateMap.allPositions)
            {
                Coordinate coordinate = coordinateMap.GetCoordinateAt(position);
                DarklightGizmos.DrawWireSquare(coordinate.WorldPosition, WorldGeneration.CellWidth_inWorldSpace, coordinate.debugColor);
                DarklightGizmos.DrawLabel($"{coordinate.type}", coordinate.WorldPosition - (Vector3.forward * WorldGeneration.CellWidth_inWorldSpace), coordLabelStyle);
            }
        }
    }

    void DrawCoordinateNeighbors(Coordinate coordinate)
    {
        if (coordinate.Initialized)
        {
            List<Coordinate> natural_neighbors = coordinate.GetValidNaturalNeighbors();

            foreach (Coordinate neighbor in natural_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.WorldPosition, direction, Color.red);
            }

            List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
            foreach (Coordinate neighbor in diagonal_neighbors)
            {
                WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.GetChunkWidth_inWorldSpace() * 0.25f;

                DarklightGizmos.DrawArrow(coordinate.WorldPosition, direction, Color.yellow);
            }
        }
    }

}