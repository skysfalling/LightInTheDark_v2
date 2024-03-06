using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.ThirdDimensional.World.Editor
{
    using Editor = UnityEditor.Editor;
    using DarklightCustomEditor = Darklight.CustomEditorLibrary;

    [CustomEditor(typeof(WorldGeneration))]
    public class WorldGenerationEditor : UnityEditor.Editor
    {
        private SerializedObject serializedWorldGen;

        static bool showGenerationSettingsFoldout = false;

        static List<Coordinate.TYPE> visibleCoordinateTypes = new List<Coordinate.TYPE> { Coordinate.TYPE.CLOSED, Coordinate.TYPE.EXIT };

        Region selectedRegion = null;
        Chunk selectedChunk = null;
        Cell selectedCell = null;

        private void OnEnable()
        {
            // Cache the SerializedObject
            serializedWorldGen = new SerializedObject(target);
            WorldGeneration.InitializeSeedRandom();

            WorldGeneration worldGen = (WorldGeneration)target;
            worldGen.Reset(); // Reset the generation on editor refresh
        }

        public override void OnInspectorGUI()
        {
            serializedWorldGen.Update(); // Always start with this call

            EditorGUI.BeginChangeCheck();

            // ----------------------------------------------------------------
            // WORLD GENERATION SETTINGS
            // ----------------------------------------------------------------
            WorldGeneration worldGen = (WorldGeneration)target;

            SerializedProperty materialLibraryProperty = serializedWorldGen.FindProperty("materialLibrary");
            EditorGUILayout.PropertyField(materialLibraryProperty);

            SerializedProperty customWorldGenSettingsProperty = serializedWorldGen.FindProperty("customWorldGenSettings");
            EditorGUILayout.PropertyField(customWorldGenSettingsProperty);

            if (worldGen.customWorldGenSettings != null)
            {
                // Override World Gen Settings with custom settings
                worldGen.OverrideSettings((CustomWorldGenerationSettings)customWorldGenSettingsProperty.objectReferenceValue);

                // >>>> foldout
                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Custom World Generation Settings", true);
                if (showGenerationSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();

                    Editor editor = CreateEditor(worldGen.customWorldGenSettings);
                    editor.OnInspectorGUI(); // Draw the editor for the ScriptableObject

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }
            }
            else
            {
                worldGen.OverrideSettings(null); // Set World Generation Settings to null

                // >>>> foldout
                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Default World Generation Settings", true);
                if (showGenerationSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    DarklightCustomEditor.CreateSettingsLabel("Seed", WorldGeneration.Settings.Seed);
                    DarklightCustomEditor.CreateSettingsLabel("Cell Width In World Space", $"{WorldGeneration.Settings.CellSize_inGameUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("Chunk Width In Cells", $"{WorldGeneration.Settings.ChunkDepth_inCellUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldGeneration.Settings.ChunkDepth_inCellUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Max Chunk Height", $"{WorldGeneration.Settings.ChunkMaxHeight_inCellUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldGeneration.Settings.RegionWidth_inChunkUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Boundary Wall Count", $"{WorldGeneration.Settings.RegionBoundaryOffset_inChunkUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("World Width In Regions", $"{WorldGeneration.Settings.WorldWidth_inRegionUnits}");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
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
                    bool isSelected = visibleCoordinateTypes.Contains(type);
                    bool newIsSelected = EditorGUILayout.ToggleLeft(type.ToString(), isSelected);

                    if (newIsSelected != isSelected)
                    {
                        if (newIsSelected)
                        {
                            visibleCoordinateTypes.Add(type);
                        }
                        else
                        {
                            visibleCoordinateTypes.Remove(type);
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
            WorldGeneration worldGen = (WorldGeneration)target;
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

                        CustomGizmoLibrary.DrawButtonHandle(region.Position, Vector3.right * 90, WorldGeneration.Settings.RegionFullWidth_inGameUnits * 0.5f, Color.black, () =>
                        {
                            SelectRegion(region);
                            DarklightCustomEditor.FocusSceneView(region.Position, WorldGeneration.Settings.RegionFullWidth_inGameUnits);

                        });

                        CustomGizmoLibrary.DrawWireSquare_withLabel($"World Region {region.Coordinate.Value}", region.Position, WorldGeneration.Settings.RegionFullWidth_inGameUnits, Color.blue, labelStyle);

                        List<Coordinate> allCoordinates = region.CoordinateMap.AllCoordinates;
                        for (int i = 0; i < allCoordinates.Count; i++)
                        {
                            Coordinate coordinate = allCoordinates[i];

                            // Check if this coordinate's type is in the selected types (showCoordinateType)
                            if (visibleCoordinateTypes.Contains(coordinate.Type)) // This line checks for matching types
                            {
                                CustomGizmoLibrary.DrawWireSquare_withLabel($"{coordinate.Type}", coordinate.Position,
                                    WorldGeneration.Settings.ChunkWidth_inGameUnits, coordinate.TypeColor, labelStyle);
                            }
                        }
                    }
                }
            }
            else
            {
                CustomGizmoLibrary.DrawWireSquare_withLabel("World Generation Size", worldGen.CenterPosition, WorldGeneration.Settings.WorldWidth_inGameUnits, Color.black, labelStyle);
                CustomGizmoLibrary.DrawWireSquare_withLabel("Origin Region", worldGen.OriginPosition, WorldGeneration.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);

                CustomGizmoLibrary.DrawWireSquare_withLabel("World Chunk Size", worldGen.OriginPosition, WorldGeneration.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
                CustomGizmoLibrary.DrawWireSquare_withLabel("World Cell Size", worldGen.OriginPosition, WorldGeneration.Settings.CellSize_inGameUnits, Color.black, labelStyle);
            }

            DrawCoordinates();
        }

        void DrawCoordinates()
        {
            WorldGeneration worldGen = (WorldGeneration)target;
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
                    CustomGizmoLibrary.DrawWireSquare(coordinate.Position, WorldGeneration.Settings.CellSize_inGameUnits, coordinate.TypeColor);
                    CustomGizmoLibrary.DrawLabel($"{coordinate.Type}", coordinate.Position - (Vector3.forward * WorldGeneration.Settings.CellSize_inGameUnits), coordLabelStyle);
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
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.25f;

                    CustomGizmoLibrary.DrawArrow(coordinate.Position, direction, Color.red);
                }

                List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
                foreach (Coordinate neighbor in diagonal_neighbors)
                {
                    WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                    Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.25f;

                    CustomGizmoLibrary.DrawArrow(coordinate.Position, direction, Color.yellow);
                }
            }
        }

        void SelectRegion(Region region)
        {
            Debug.Log($"Selected Region {region.Coordinate.Value}");
            selectedRegion = region;
            selectedChunk = null;
            selectedCell = null;
        }

        void SelectChunk(Chunk chunk)
        {
            Debug.Log($"Selected Chunk {chunk.Coordinate.Value}");
            selectedRegion = null;
            selectedChunk = chunk;
            selectedCell = null;
        }

        void SelectCell(Cell cell)
        {
            Debug.Log($"Selected Cell {cell.Position}");
            selectedRegion = null;
            selectedChunk = null;
            selectedCell = cell;
        }
    }
}