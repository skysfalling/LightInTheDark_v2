namespace Darklight.World.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Generation;
	using Builder;
	using Map;
	using Settings;
	using Darklight.Bot.Editor;

#if UNITY_EDITOR
	[CustomEditor(typeof(RegionBuilder))]
	public class RegionBuilderEditor : TaskQueenEditor
	{
		private SerializedObject _serializedRegionBuilderObject;
		private RegionBuilder _regionBuilderScript;
		static bool showGenerationSettingsFoldout = false;

		public override void OnEnable()
		{
			base.OnEnable();

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
				_ = _regionBuilderScript.Initialize();
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
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Seed", RegionBuilder.Settings.Seed);
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Cell Width In World Space", $"{RegionBuilder.Settings.CellSize_inGameUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("Chunk Width In Cells", $"{RegionBuilder.Settings.ChunkDepth_inCellUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Chunk Depth In Cells", $"{RegionBuilder.Settings.ChunkDepth_inCellUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Max Chunk Height", $"{RegionBuilder.Settings.ChunkMaxHeight_inCellUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldBuilder.Settings.RegionWidth_inChunkUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Boundary Wall Count", $"{RegionBuilder.Settings.RegionBoundaryOffset_inChunkUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("World Width In Regions", $"{RegionBuilder.Settings.WorldWidth_inRegionUnits}");
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
#endif
}