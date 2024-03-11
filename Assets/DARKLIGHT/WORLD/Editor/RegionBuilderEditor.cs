using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Darklight.World.Generation.Editor
{
    using DarklightCustomEditor = Darklight.Unity.CustomInspectorGUI;
    using Backend = Darklight.Unity.Backend;

    [UnityEditor.CustomEditor(typeof(RegionBuilder))]
    public class RegionBuilderEditor : Backend.AsyncTaskQueen.AsyncTaskQueenEditor
    {
        private SerializedObject _serializedRegionBuilderObject;
        private RegionBuilder _regionBuilderScript;

        static bool showGenerationSettingsFoldout = false;

        private void OnEnable()
        {
            // Cache the SerializedObject
            _serializedRegionBuilderObject = new SerializedObject(target);
            RegionBuilder.InitializeSeedRandom();

            _regionBuilderScript = (RegionBuilder)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedRegionBuilderObject.Update(); // Always start with this call

            // Draw the console window
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            if (!_regionBuilderScript.Initialized && GUILayout.Button("Initialize"))
            {
                _regionBuilderScript.Initialize();
            }
            else if (_regionBuilderScript.Initialized && GUILayout.Button("Reset"))
            {
                _regionBuilderScript.Reset();
            }

            DrawCustomGenerationSettings();

            if (EditorGUI.EndChangeCheck())
            {
                // Apply changes to the serialized object
                _serializedRegionBuilderObject.ApplyModifiedProperties();

                // Optionally, mark the target object as dirty to ensure the changes are saved
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawCustomGenerationSettings()
        {
            SerializedProperty customWorldGenSettingsProperty = _serializedRegionBuilderObject.FindProperty("customRegionSettings");
            if (_regionBuilderScript.customRegionSettings != null)
            {
                _regionBuilderScript.OverrideSettings((CustomGenerationSettings)customWorldGenSettingsProperty.objectReferenceValue);

                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "CustomGenerationSettings", true);
                if (showGenerationSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();

                    UnityEditor.Editor editor = CreateEditor(_regionBuilderScript.customRegionSettings);
                    editor.OnInspectorGUI();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                _regionBuilderScript.OverrideSettings(null);

                showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "DefaultGenerationSettings", true);
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
        }
    }
}