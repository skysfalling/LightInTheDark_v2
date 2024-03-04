using System;
using UnityEngine;


[Serializable, CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "WorldGeneration/Settings", order = 0)]
public class WorldGenerationSettings : ScriptableObject
{
    [SerializeField] private string _gameSeed;
    [SerializeField] private int _cellWidthInWorldSpace;
    [SerializeField] private int _chunkWidthInCells;
    [SerializeField] private int _chunkDepthInCells;
    [SerializeField] private int _playRegionWidthInChunks;
    [SerializeField] private int _boundaryWallCount;
    [SerializeField] private int _maxChunkHeight;
    [SerializeField] private int _worldWidthInRegions;

    public string Seed => _gameSeed;
    public int CellWidthInWorldSpace => _cellWidthInWorldSpace;
    public int ChunkWidthInCells => _chunkWidthInCells;
    public int ChunkDepthInCells => _chunkDepthInCells;
    public int PlayRegionWidthInChunks => _playRegionWidthInChunks;
    public int BoundaryWallCount => _boundaryWallCount;
    public int MaxChunkHeight => _maxChunkHeight;
    public int WorldWidthInRegions => _worldWidthInRegions;
}
