using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.ThirdDimensional.World.Editor
{
    using WorldGen = Generation;

    [CustomEditor(typeof(Generation))]
    public class WorldGenerationEditor : UnityEditor.Editor
    {
        private SerializedObject serializedWorldGen;
        static Coordinate.TYPE showCoordinateType = Coordinate.TYPE.EXIT;

        GUIStyle centeredStyle;

        private void OnEnable()
        {
            // Cache the SerializedObject
            serializedWorldGen = new SerializedObject(target);
            WorldGen.InitializeSeedRandom();

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
            Generation worldGen = (WorldGen)target;
            EditorGUILayout.LabelField("Generation Settings", DarklightEditor.Header2Style);
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("World Generation Parameters", EditorStyles.boldLabel);

            DarklightEditor.CreateSettingsLabel("Seed", WorldGen.Settings.Seed);
            DarklightEditor.CreateSettingsLabel("Cell Width In World Space", $"{WorldGen.Settings.CellSize_inGameUnits}");

            DarklightEditor.CreateSettingsLabel("Chunk Width In Cells", $"{WorldGen.Settings.ChunkDepth_inCellUnits}");
            DarklightEditor.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldGen.Settings.ChunkDepth_inCellUnits}");
            DarklightEditor.CreateSettingsLabel("Max Chunk Height", $"{WorldGen.Settings.ChunkMaxHeight_inCellUnits}");

            DarklightEditor.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldGen.Settings.RegionWidth_inChunkUnits}");
            DarklightEditor.CreateSettingsLabel("Boundary Wall Count", $"{WorldGen.Settings.RegionBoundaryOffset_inChunkUnits}");

            DarklightEditor.CreateSettingsLabel("World Width In Regions", $"{WorldGen.Settings.WorldWidth_inRegionUnits}");

            EditorGUILayout.Space();

            if (worldGen.AllRegions.Count == 0)
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
            WorldGen worldGen = (WorldGen)target;
            Transform transform = worldGen.transform;

            GUIStyle labelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 12, // Example font size
            };

            DarklightGizmos.DrawWireSquare_withLabel("World Generation Size", worldGen.CenterPosition, WorldGen.Settings.WorldWidth_inGameUnits, Color.black, labelStyle);

            if (worldGen.Initialized)
            {
                DarklightGizmos.DrawWireSquare_withLabel("Origin Region", worldGen.OriginPosition, WorldGen.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
            }

            DarklightGizmos.DrawWireSquare_withLabel("World Chunk Size", worldGen.CenterPosition, WorldGen.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
            DarklightGizmos.DrawWireSquare_withLabel("World Cell Size", worldGen.CenterPosition, WorldGen.Settings.CellSize_inGameUnits, Color.black, labelStyle);


            if (worldGen.Initialized && worldGen.AllRegions.Count > 0)
            {
                foreach (Region region in worldGen.AllRegions)
                {

                    List<Coordinate> regionNeighbors = region.Coordinate.GetAllValidNeighbors();

                    if (region != null && region.Initialized)
                    {
                        DarklightGizmos.DrawWireSquare_withLabel($"World Region {region.Coordinate.Value}" +
                            $"\n neighbors : {regionNeighbors.Count}", region.CenterPosition, WorldGen.Settings.RegionFullWidth_inGameUnits, Color.blue, labelStyle);

                        List<Vector2Int> coordinatesOfType = region.CoordinateMap.GetAllPositionsOfType(showCoordinateType).ToList();
                        for (int i = 0; i < coordinatesOfType.Count; i++)
                        {
                            Coordinate coordinate = region.CoordinateMap.GetCoordinateAt(coordinatesOfType[i]);

                            DarklightGizmos.DrawWireSquare_withLabel($"{showCoordinateType}", coordinate.Position,
                                WorldGen.Settings.ChunkWidth_inGameUnits, coordinate.typeColor, labelStyle);
                        }

                    }

                    DrawCoordinateNeighbors(region.Coordinate);
                }
            }

            DrawCoordinates();
        }

        void DrawCoordinates()
        {
            Generation worldGen = (WorldGen)target;
            if (worldGen.CoordinateMap == null) { return; }

            GUIStyle coordLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 12, // Example font size
                normal = new GUIStyleState { textColor = Color.blue } // Set the text color
            };
            // Draw Coordinates
            CoordinateMap coordinateMap = worldGen.CoordinateMap;
            if (coordinateMap.Initialized && coordinateMap.AllPositions.Count > 0)
            {
                foreach (Vector2Int position in coordinateMap.AllPositions)
                {
                    Coordinate coordinate = coordinateMap.GetCoordinateAt(position);
                    DarklightGizmos.DrawWireSquare(coordinate.Position, WorldGen.Settings.CellSize_inGameUnits, coordinate.typeColor);
                    DarklightGizmos.DrawLabel($"{coordinate.type}", coordinate.Position - (Vector3.forward * WorldGen.Settings.CellSize_inGameUnits), coordLabelStyle);
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
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGen.Settings.ChunkWidth_inGameUnits * 0.25f;

                    DarklightGizmos.DrawArrow(coordinate.Position, direction, Color.red);
                }

                List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
                foreach (Coordinate neighbor in diagonal_neighbors)
                {
                    WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                    Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGen.Settings.ChunkWidth_inGameUnits * 0.25f;

                    DarklightGizmos.DrawArrow(coordinate.Position, direction, Color.yellow);
                }
            }
        }

    }
}