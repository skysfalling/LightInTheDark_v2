using System;
using UnityEngine;

[System.Serializable]
public class WorldSaveData
{
    public string gameSeed;
    public int cellWidthInWorldSpace;
    public int chunkWidthInCells;
    public int chunkDepthInCells;
    public int playRegionWidthInChunks;
    public int boundaryWallCount;
    public int maxChunkHeight;
    public int worldWidthInRegions;

    public WorldSaveData() { }

    public WorldSaveData(WorldGenerationSettings settings)
    {
        this.gameSeed = settings.Seed;
        this.cellWidthInWorldSpace = settings.CellWidthInWorldSpace;
        this.chunkWidthInCells = settings.ChunkWidthInCells;
        this.chunkDepthInCells = settings.ChunkDepthInCells;
        this.playRegionWidthInChunks = settings.PlayRegionWidthInChunks;
        this.boundaryWallCount = settings.BoundaryWallCount;
        this.maxChunkHeight = settings.MaxChunkHeight;
        this.worldWidthInRegions = settings.WorldWidthInRegions;
    }
}
