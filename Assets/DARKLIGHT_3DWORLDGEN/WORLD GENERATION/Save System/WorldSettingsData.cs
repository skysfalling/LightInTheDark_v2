using System;
using UnityEngine;

[System.Serializable]
public class WorldSettingsData
{
    public string gameSeed;
    public int cellWidthInWorldSpace;
    public int chunkWidthInCells;
    public int chunkDepthInCells;
    public int playRegionWidthInChunks;
    public int boundaryWallCount;
    public int maxChunkHeight;
    public int worldWidthInRegions;

    public WorldSettingsData() { }

    public WorldSettingsData(WorldGenerationSettings settings)
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

[System.Serializable]
public class CoordinateData
{
    public int typeID; // Coordinate Type
    public int X;
    public int Y;

    public CoordinateData(Coordinate coord)
    {
        typeID = (int)coord.type;
        X = coord.Value.x;
        Y = coord.Value.y;
    }
}

[System.Serializable]
public class CoordinateMapData
{
    public CoordinateData[][] CoordinateMap;

    public CoordinateMapData(CoordinateMap map)
    {

    }
}




[System.Serializable]
public class WorldPathData
{
    public Vector2Int[] path;

    public WorldPathData(WorldPath path)
    {
        this.path = path.positions.ToArray();
    }
}