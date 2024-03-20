namespace Darklight.World.Settings
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine.UIElements;
	using UnityEditor.UIElements;
	using Unity.Android.Gradle;
#endif

	[CreateAssetMenu(fileName = "New Generation Settings", menuName = "World/New Generation Settings", order = 1)]
	public class CustomGenerationSettings : ScriptableObject
	{
		[SerializeField] private string _seed = "Default Game Seed";
		[SerializeField] private int _cellSize = 2; // in Units
		[SerializeField] private int _chunkWidth = 10; // in Cells
		[SerializeField] private int _chunkDepth = 10; // in Cells
		[SerializeField] private int _chunkMaxHeight = 25; // in Cells
		[SerializeField] private int _regionWidth = 7; // in Chunks
		[SerializeField] private int _regionBoundaryOffset = 0; // in Chunks
		[SerializeField] private int _worldWidth = 5; // in Regions
		[SerializeField] private float _pathRandomness = 0.5f;
		[SerializeField] private float _perlinMultiplier = 1.0f;


		// >> Public Accessors
		public string Seed => _seed;
		public int CellSize => _cellSize;
		public int ChunkWidth => _chunkWidth;
		public int ChunkDepth => _chunkDepth;
		public int ChunkMaxHeight => _chunkMaxHeight;
		public int RegionWidth => _regionWidth;
		public int RegionBoundaryOffset => _regionBoundaryOffset;
		public int WorldWidth => _worldWidth;
		public float PathRandomness => _pathRandomness;
		public float PerlinMultiplier => _perlinMultiplier;
	}


	/*
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
	*/
}
