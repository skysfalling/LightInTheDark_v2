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
}
