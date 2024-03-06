using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.ThirdDimensional.World.Editor
{
    using WorldGen = WorldGeneration;
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(WorldGeneration))]
    public class WorldGenerationEditor : UnityEditor.Editor
    {
        private SerializedObject serializedWorldGen;

        static List<Coordinate.TYPE> selectedCoordinateTypes = new();

        private void OnEnable()
        {
            // Cache the SerializedObject
            serializedWorldGen = new SerializedObject(target);
            WorldGen.InitializeSeedRandom();
        }

        public override void OnInspectorGUI()
        {
            serializedWorldGen.Update(); // Always start with this call

            EditorGUI.BeginChangeCheck();

            // ----------------------------------------------------------------
            // WORLD GENERATION SETTINGS
            // ----------------------------------------------------------------
            WorldGeneration worldGen = (WorldGen)target;

            SerializedProperty materialLibraryProperty = serializedWorldGen.FindProperty("materialLibrary");
            EditorGUILayout.PropertyField(materialLibraryProperty);

            SerializedProperty customWorldGenSettingsProperty = serializedWorldGen.FindProperty("customWorldGenSettings");
            EditorGUILayout.PropertyField(customWorldGenSettingsProperty);

            if (worldGen.customWorldGenSettings != null)
            {

                EditorGUILayout.LabelField("Custom World Generation Settings", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();

                Editor editor = CreateEditor(worldGen.customWorldGenSettings);
                editor.OnInspectorGUI(); // Draw the editor for the ScriptableObject

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                worldGen.OverrideSettings((CustomWorldGenerationSettings)customWorldGenSettingsProperty.objectReferenceValue);
            }
            else
            {
                worldGen.OverrideSettings(null);

                EditorGUILayout.LabelField("Default World Generation Settings", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                DarklightEditor.CreateSettingsLabel("Seed", WorldGen.Settings.Seed);
                DarklightEditor.CreateSettingsLabel("Cell Width In World Space", $"{WorldGen.Settings.CellSize_inGameUnits}");

                DarklightEditor.CreateSettingsLabel("Chunk Width In Cells", $"{WorldGen.Settings.ChunkDepth_inCellUnits}");
                DarklightEditor.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldGen.Settings.ChunkDepth_inCellUnits}");
                DarklightEditor.CreateSettingsLabel("Max Chunk Height", $"{WorldGen.Settings.ChunkMaxHeight_inCellUnits}");

                DarklightEditor.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldGen.Settings.RegionWidth_inChunkUnits}");
                DarklightEditor.CreateSettingsLabel("Boundary Wall Count", $"{WorldGen.Settings.RegionBoundaryOffset_inChunkUnits}");

                DarklightEditor.CreateSettingsLabel("World Width In Regions", $"{WorldGen.Settings.WorldWidth_inRegionUnits}");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

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

                // Assuming 'Coordinate.TYPE' is your enum
                foreach (Coordinate.TYPE type in Enum.GetValues(typeof(Coordinate.TYPE)))
                {
                    bool isSelected = selectedCoordinateTypes.Contains(type);
                    bool newIsSelected = EditorGUILayout.ToggleLeft(type.ToString(), isSelected);

                    if (newIsSelected != isSelected)
                    {
                        if (newIsSelected)
                        {
                            selectedCoordinateTypes.Add(type);
                        }
                        else
                        {
                            selectedCoordinateTypes.Remove(type);
                        }
                    }
                }

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

            if (worldGen.Initialized && worldGen.AllRegions.Count > 0)
            {
                foreach (Region region in worldGen.AllRegions)
                {

                    List<Coordinate> regionNeighbors = region.Coordinate.GetAllValidNeighbors();

                    if (region != null && region.Initialized)
                    {
                        DarklightGizmos.DrawWireSquare_withLabel($"World Region {region.Coordinate.Value}" +
                            $"\n neighbors : {regionNeighbors.Count}", region.CenterPosition, WorldGen.Settings.RegionFullWidth_inGameUnits, Color.blue, labelStyle);

                        List<Coordinate> allCoordinates = region.CoordinateMap.AllCoordinates;
                        for (int i = 0; i < allCoordinates.Count; i++)
                        {
                            Coordinate coordinate = allCoordinates[i];

                            // Check if this coordinate's type is in the selected types (showCoordinateType)
                            if (selectedCoordinateTypes.Contains(coordinate.Type)) // This line checks for matching types
                            {
                                DarklightGizmos.DrawWireSquare_withLabel($"{coordinate.Type}", coordinate.Position,
                                    WorldGen.Settings.ChunkWidth_inGameUnits, coordinate.TypeColor, labelStyle);
                            }

                        }

                    }

                    DrawCoordinateNeighbors(region.Coordinate);
                }
            }
            else
            {
                DarklightGizmos.DrawWireSquare_withLabel("World Generation Size", worldGen.CenterPosition, WorldGen.Settings.WorldWidth_inGameUnits, Color.black, labelStyle);

                if (worldGen.Initialized)
                {
                    DarklightGizmos.DrawWireSquare_withLabel("Origin Region", worldGen.OriginPosition, WorldGen.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
                }

                DarklightGizmos.DrawWireSquare_withLabel("World Chunk Size", worldGen.CenterPosition, WorldGen.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
                DarklightGizmos.DrawWireSquare_withLabel("World Cell Size", worldGen.CenterPosition, WorldGen.Settings.CellSize_inGameUnits, Color.black, labelStyle);
            }

            DrawCoordinates();
        }

        void DrawCoordinates()
        {
            WorldGeneration worldGen = (WorldGen)target;
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
                    DarklightGizmos.DrawWireSquare(coordinate.Position, WorldGen.Settings.CellSize_inGameUnits, coordinate.TypeColor);
                    DarklightGizmos.DrawLabel($"{coordinate.Type}", coordinate.Position - (Vector3.forward * WorldGen.Settings.CellSize_inGameUnits), coordLabelStyle);
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