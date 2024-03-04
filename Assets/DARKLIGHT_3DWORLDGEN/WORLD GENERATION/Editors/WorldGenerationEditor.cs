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

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Generation Seed", DarklightEditor.Header2Style);
        EditorGUILayout.Space(20);




        EditorGUILayout.Space(40);


        // ----------------------------------------------------------------
        // WORLD GENERATION SETTINGS
        // ----------------------------------------------------------------
        WorldGeneration worldGen = (WorldGeneration)target;
        EditorGUILayout.LabelField("Generation Settings", DarklightEditor.Header2Style);
        EditorGUILayout.Space(20);
        SerializedProperty worldSettings = serializedWorldGen.FindProperty("worldSettings");
        EditorGUILayout.PropertyField(worldSettings);

        // Add a custom UI for the WorldGenerationSettings
        WorldGenerationSettings settings = worldGen.worldSettings;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("World Generation Parameters", EditorStyles.boldLabel);

        DarklightEditor.CreateSettingsLabel("Seed", WorldGeneration.Seed);
        DarklightEditor.CreateSettingsLabel("Cell Width In World Space", $"{WorldGeneration.CellWidth_inWorldSpace}");
        DarklightEditor.CreateSettingsLabel("Chunk Width In Cells", $"{WorldGeneration.ChunkWidth_inCells}");
        DarklightEditor.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldGeneration.ChunkDepth_inCells}");
        DarklightEditor.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldGeneration.PlayRegionWidth_inChunks}");
        DarklightEditor.CreateSettingsLabel("Boundary Wall Count", $"{WorldGeneration.BoundaryWallCount}");
        DarklightEditor.CreateSettingsLabel("Max Chunk Height", $"{WorldGeneration.MaxChunkHeight}");
        DarklightEditor.CreateSettingsLabel("World Width In Regions", $"{WorldGeneration.WorldWidth_inRegions}");

        EditorGUILayout.Space();

        if (worldGen.worldSettings != null)
        {
            if (GUILayout.Button("Load Settings"))
            {
                WorldGeneration.LoadWorldGenerationSettings(worldGen.worldSettings);
            }
        }

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