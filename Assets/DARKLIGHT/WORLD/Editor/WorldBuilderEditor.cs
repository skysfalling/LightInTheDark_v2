namespace Darklight.World.Editor
{
	using UnityEditor;
	using Generation;
	using Builder;
	using Map;
	using Settings;
	using Darklight.Bot;

	[CustomEditor(typeof(WorldBuilder))]
	public class WorldBuilderEditor : TaskBotQueenEditor
	{
		private SerializedObject _serializedObject;
		private WorldBuilder _worldBuilderScript;

		static bool showGenerationSettingsFoldout = false;

		public override void OnEnable()
		{
			base.OnEnable();

			// Cache the SerializedObject
			_serializedObject = new SerializedObject(target);

			// Initialize the seed random
			WorldBuilder.InitializeSeedRandom();

			// Store the script
			_worldBuilderScript = (WorldBuilder)target;
		}

		public override void OnInspectorGUI()
		{
			_serializedObject.Update(); // Always start with this call

			// Draw the TaskConsole window
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();

			DrawCustomGenerationSettings();

			EditorGUILayout.Space();

			// Check if any changes were made in the Inspector
			if (EditorGUI.EndChangeCheck())
			{
				// If there were changes, apply them to the serialized object
				_serializedObject.ApplyModifiedProperties();

				// Optionally, mark the target object as dirty to ensure the changes are saved
				EditorUtility.SetDirty(target);
			}
		}

		private void DrawCustomGenerationSettings()
		{
			SerializedProperty customWorldGenSettingsProperty = _serializedObject.FindProperty("customWorldGenSettings");
			if (_worldBuilderScript.customWorldGenSettings != null)
			{
				WorldBuilder.OverrideSettings((CustomGenerationSettings)customWorldGenSettingsProperty.objectReferenceValue);

				showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Custom World Generation Settings", true);
				if (showGenerationSettingsFoldout)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical();

					UnityEditor.Editor editor = CreateEditor(_worldBuilderScript.customWorldGenSettings);
					editor.OnInspectorGUI();

					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				WorldBuilder.OverrideSettings(null);

				showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Default World Generation Settings", true);
				if (showGenerationSettingsFoldout)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical();
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Seed", WorldBuilder.Settings.Seed);
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Cell Width In World Space", $"{WorldBuilder.Settings.CellSize_inGameUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("Chunk Width In Cells", $"{WorldBuilder.Settings.ChunkDepth_inCellUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Chunk Depth In Cells", $"{WorldBuilder.Settings.ChunkDepth_inCellUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Max Chunk Height", $"{WorldBuilder.Settings.ChunkMaxHeight_inCellUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("Play Region Width In Chunks", $"{WorldBuilder.Settings.RegionWidth_inChunkUnits}");
					Darklight.CustomInspectorGUI.CreateSettingsLabel("Boundary Wall Count", $"{WorldBuilder.Settings.RegionBoundaryOffset_inChunkUnits}");

					Darklight.CustomInspectorGUI.CreateSettingsLabel("World Width In Regions", $"{WorldBuilder.Settings.WorldWidth_inRegionUnits}");
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
}