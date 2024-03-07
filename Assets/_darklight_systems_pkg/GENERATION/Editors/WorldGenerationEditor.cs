using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.ThirdDimensional.Generation.Editor
{
    using Editor = UnityEditor.Editor;
    using DarklightCustomEditor = Darklight.DarklightEditor;

    [CustomEditor(typeof(WorldGeneration))]
    public class WorldGenerationEditor : UnityEditor.Editor
    {
        private SerializedObject serializedWorldGen;
        private WorldGeneration _worldGenerationScript;

        static bool showGenerationSettingsFoldout = false;
        static bool showGenerationProfilerFoldout = false;


        private void OnEnable()
        {
            // Cache the SerializedObject
            serializedWorldGen = new SerializedObject(target);
            WorldGeneration.InitializeSeedRandom();


        }

        public override async void OnInspectorGUI()
        {
            serializedWorldGen.Update(); // Always start with this call

            EditorGUI.BeginChangeCheck();

            // ----------------------------------------------------------------
            // CUSTOM GENERATION SETTINGS
            // ----------------------------------------------------------------
            WorldGeneration worldGen = (WorldGeneration)target;

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

            // ----------------------------------------------------------------
            // GENERATION PROFILER
            // ----------------------------------------------------------------
                // >>>> foldout
                showGenerationProfilerFoldout = EditorGUILayout.Foldout(showGenerationProfilerFoldout, "World Generation Profiler", true);
                if (showGenerationProfilerFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();

                    List<WorldGeneration.InitializationStage> initStages = _worldGenerationScript.InitStages;
                    foreach (WorldGeneration.InitializationStage stage in initStages)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Stage {stage.id} => {stage.name}");
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField($"{stage.time} milliseconds");
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

            // ----------------------------------------------------------------
            // Buttons
            // ----------------------------------------------------------------
            if (worldGen.AllRegions.Count == 0)
            {
                if (GUILayout.Button("Initialize"))
                {
                    await worldGen.InitializeAsync();
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
                    worldGen.ResetGeneration();
                }
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




    }
}