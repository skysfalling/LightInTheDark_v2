using System;
using UnityEngine;


[Serializable, CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "WorldGeneration/Settings", order = 0)]
public class WorldGenerationSettings : ScriptableObject
{
    // [[ Serialized Values ]]
    [SerializeField] private string _seed = "Default Game Seed";
    [SerializeField] private int _cellWidthInWorldSpace = 2;
    [SerializeField] private int _chunkWidthInCells = 10;
    [SerializeField] private int _chunkDepthInCells = 10;
    [SerializeField] private int _playRegionWidthInChunks = 7;
    [SerializeField] private int _boundaryWallCount = 0;
    [SerializeField] private int _maxChunkHeight = 25;
    [SerializeField] private int _worldWidthInRegions = 5;

    // [[ Public Accessors ]]
    public string Seed => _seed;
    public int CellWidthInWorldSpace => _cellWidthInWorldSpace;
    public int ChunkWidthInCells => _chunkWidthInCells;
    public int ChunkDepthInCells => _chunkDepthInCells;
    public int PlayRegionWidthInChunks => _playRegionWidthInChunks;
    public int BoundaryWallCount => _boundaryWallCount;
    public int MaxChunkHeight => _maxChunkHeight;
    public int WorldWidthInRegions => _worldWidthInRegions;
}
