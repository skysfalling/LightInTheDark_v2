namespace Darklight.World.Settings
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

	[System.Serializable]
	public class GenerationSettings
	{
		// [[ STORED SETTINGS DATA ]] 
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

		public string Seed => _seed;

		#region [[ WORLD UNIT SIZES ]] ========
		// >>>> WORLD CELL
		public int CellSize_inGameUnits => _cellSize;

		// >>>> WORLD CHUNK
		public int ChunkWidth_inCellUnits => _chunkWidth;
		public int ChunkDepth_inCellUnits => _chunkDepth;
		public int ChunkMaxHeight_inCellUnits => _chunkMaxHeight;
		public Vector3Int ChunkVec3Dimensions_inCellUnits => new Vector3Int(_chunkWidth, _chunkDepth, _chunkWidth);
		public int ChunkWidth_inGameUnits => _chunkWidth * _cellSize;
		public int ChunkDepth_inGameUnits => _chunkDepth * _cellSize;
		public int ChunkMaxHeight_inGameUnits => _chunkMaxHeight * _cellSize;
		public Vector3Int ChunkVec3Dimensions_inGameUnits => ChunkVec3Dimensions_inCellUnits * _cellSize;

		// >>>> WORLD REGION
		public int RegionWidth_inChunkUnits => _regionWidth;
		public int RegionWidth_inCellUnits => _regionWidth * _chunkWidth;
		public int RegionWidth_inGameUnits => RegionWidth_inCellUnits * _cellSize;
		public int RegionBoundaryOffset_inChunkUnits => _regionBoundaryOffset;
		public int RegionFullWidth_inChunkUnits => RegionWidth_inChunkUnits + (_regionBoundaryOffset * 2);
		public int RegionFullWidth_inCellUnits => RegionFullWidth_inChunkUnits * _chunkWidth;
		public int RegionFullWidth_inGameUnits => RegionFullWidth_inCellUnits * _cellSize;

		// >>>> WORLD GENERATION
		public int WorldWidth_inRegionUnits => _worldWidth;
		public int WorldWidth_inChunkUnits => _worldWidth * RegionFullWidth_inChunkUnits;
		public int WorldWidth_inCellUnits => _worldWidth * RegionFullWidth_inCellUnits;
		public int WorldWidth_inGameUnits => WorldWidth_inCellUnits * _cellSize;
		#endregion

		// >>>> WORLD GENERATION PARAMETERS
		public float PathRandomness => _pathRandomness;
		public float PerlinMultiplier => _perlinMultiplier;

		public GenerationSettings() { }
		public GenerationSettings(CustomGenerationSettings worldGenSettings)
		{
			_seed = worldGenSettings.Seed;
			_cellSize = worldGenSettings.CellSize;
			_chunkWidth = worldGenSettings.ChunkWidth;
			_chunkDepth = worldGenSettings.ChunkDepth;
			_chunkMaxHeight = worldGenSettings.ChunkMaxHeight;
			_regionWidth = worldGenSettings.RegionWidth;
			_regionBoundaryOffset = worldGenSettings.RegionBoundaryOffset;
			_worldWidth = worldGenSettings.WorldWidth;

			_pathRandomness = worldGenSettings.PathRandomness;
			_perlinMultiplier = worldGenSettings.PerlinMultiplier;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(GenerationSettings), true)]
	public class GenerationSettingsDrawer : PropertyDrawer
	{
		bool showGenerationSettingsFoldout = true;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty seedProp = property.FindPropertyRelative("_seed");
			SerializedProperty cellSizeProp = property.FindPropertyRelative("_cellSize");
			SerializedProperty chunkWidthProp = property.FindPropertyRelative("_chunkWidth");
			SerializedProperty chunkDepthProp = property.FindPropertyRelative("_chunkDepth");
			SerializedProperty chunkMaxHeightProp = property.FindPropertyRelative("_chunkMaxHeight");
			SerializedProperty regionWidthProp = property.FindPropertyRelative("_regionWidth");
			SerializedProperty regionBoundaryOffsetProp = property.FindPropertyRelative("_regionBoundaryOffset");
			SerializedProperty worldWidthProp = property.FindPropertyRelative("_worldWidth");
			SerializedProperty pathRandomnessProp = property.FindPropertyRelative("_pathRandomness");
			SerializedProperty perlinMultiplierProp = property.FindPropertyRelative("_perlinMultiplier");

			EditorGUI.BeginProperty(position, label, property);

			// >> Foldout
			showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "CustomGenerationSettings");
			if (showGenerationSettingsFoldout)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();

				EditorGUILayout.PropertyField(seedProp);
				EditorGUILayout.PropertyField(cellSizeProp);
				EditorGUILayout.PropertyField(chunkWidthProp);
				EditorGUILayout.PropertyField(chunkDepthProp);
				EditorGUILayout.PropertyField(chunkMaxHeightProp);
				EditorGUILayout.PropertyField(regionWidthProp);
				EditorGUILayout.PropertyField(regionBoundaryOffsetProp);
				EditorGUILayout.PropertyField(worldWidthProp);
				EditorGUILayout.PropertyField(pathRandomnessProp);
				EditorGUILayout.PropertyField(perlinMultiplierProp);

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}

			EditorGUI.EndProperty();
		}


	}
#endif
}