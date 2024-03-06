using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    [CreateAssetMenu(fileName = "NewGenerationSettings", menuName = "WorldGeneration/Settings", order = 1)]
    public class CustomWorldGenerationSettings : ScriptableObject
    {
        [SerializeField] private string _seed = "Default Game Seed";
        [SerializeField] private int _cellSize = 2; // in Units
        [SerializeField] private int _chunkWidth = 10; // in Cells
        [SerializeField] private int _chunkDepth = 10; // in Cells
        [SerializeField] private int _chunkMaxHeight = 25; // in Cells
        [SerializeField] private int _regionWidth = 7; // in Chunks
        [SerializeField] private int _regionBoundaryOffset = 0; // in Chunks
        [SerializeField] private int _worldWidth = 5; // in Regions

        // >> Public Accessors
        public string Seed => _seed;
        public int CellSize => _cellSize;
        public int ChunkWidth => _chunkWidth;
        public int ChunkDepth => _chunkDepth;
        public int ChunkMaxHeight => _chunkMaxHeight;
        public int RegionWidth=> _regionWidth;
        public int RegionBoundaryOffset => _regionBoundaryOffset;
        public int WorldWidth => _worldWidth;
    }
}