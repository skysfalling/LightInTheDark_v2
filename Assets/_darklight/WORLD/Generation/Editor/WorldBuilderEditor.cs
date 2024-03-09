using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.World.Generation.CustomEditor
{
    using DarklightCustomEditor = Darklight.Unity.CustomInspectorGUI;
    using Darklight.Unity.Backend;

    [UnityEditor.CustomEditor(typeof(WorldBuilder))]
    public class WorldBuilderEditor : UnityEditor.Editor
    {
        private SerializedObject _serializedWorldGenObject;
        private WorldBuilder _worldBuilderScript;

        static bool showGenerationSettingsFoldout = false;
        static bool showAsyncTaskBotQueen = false;

        private void OnEnable()
        {
            // Cache the SerializedObject
            _serializedWorldGenObject = new SerializedObject(target);
            WorldBuilder.InitializeSeedRandom();

            _worldBuilderScript = (WorldBuilder)target;
        }

        public override async void OnInspectorGUI()
        {
            _serializedWorldGenObject.Update(); // Always start with this call

            EditorGUI.BeginChangeCheck();

            // ----------------------------------------------------------------
            // CUSTOM GENERATION SETTINGS
            // ----------------------------------------------------------------
            SerializedProperty customWorldGenSettingsProperty = _serializedWorldGenObject.FindProperty("customWorldGenSettings");
            EditorGUILayout.PropertyField(customWorldGenSettingsProperty);
            if (_worldBuilderScript.customWorldGenSettings != null)
            {
                // Override World Gen Settings with custom settings
                _worldBuilderScript.OverrideSettings((CustomWorldGenerationSettings)customWorldGenSettingsProperty.objectReferenceValue);

                // >>>> foldout
                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Custom World Generation Settings", true);
                if (showGenerationSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();

                    Editor editor = CreateEditor(_worldBuilderScript.customWorldGenSettings);
                    editor.OnInspectorGUI(); // Draw the editor for the ScriptableObject

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                _worldBuilderScript.OverrideSettings(null); // Set World Generation Settings to null

                // >>>> foldout
                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Default World Generation Settings", true);
                if (showGenerationSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    DarklightCustomEditor.CreateSettingsLabel("Seed", WorldBuilder.Settings.Seed);
                    DarklightCustomEditor.CreateSettingsLabel("Cell Width In World Space", $"{WorldBuilder.Settings.CellSize_inGameUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("Chunk Width In Cells", $"{WorldBuilder.Settings.ChunkDepth_inCellUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldBuilder.Settings.ChunkDepth_inCellUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Max Chunk Height", $"{WorldBuilder.Settings.ChunkMaxHeight_inCellUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldBuilder.Settings.RegionWidth_inChunkUnits}");
                    DarklightCustomEditor.CreateSettingsLabel("Boundary Wall Count", $"{WorldBuilder.Settings.RegionBoundaryOffset_inChunkUnits}");

                    DarklightCustomEditor.CreateSettingsLabel("World Width In Regions", $"{WorldBuilder.Settings.WorldWidth_inRegionUnits}");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            // ----------------------------------------------------------------
            // Buttons
            // ----------------------------------------------------------------

            if (_worldBuilderScript.AllRegions.Count == 0)
            {
                if (GUILayout.Button("Initialize"))
                {
                    await _worldBuilderScript.InitializeAsync();
                }
            }
            else
            {
                if (GUILayout.Button("Start Generation"))
                {
                    _worldBuilderScript.StartGeneration();
                }

                if (GUILayout.Button("Reset"))
                {
                    _worldBuilderScript.ResetGeneration();
                }
            }

            // Check if any changes were made in the Inspector
            if (EditorGUI.EndChangeCheck())
            {
                // If there were changes, apply them to the serialized object
                _serializedWorldGenObject.ApplyModifiedProperties();

                // Optionally, mark the target object as dirty to ensure the changes are saved
                EditorUtility.SetDirty(target);
            }
        }


    }
}