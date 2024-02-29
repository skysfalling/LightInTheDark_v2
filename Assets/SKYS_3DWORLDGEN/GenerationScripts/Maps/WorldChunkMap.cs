using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkMap
{
    HashSet<WorldChunk> _chunks = new();
    Dictionary<Vector2Int, WorldChunk> _chunkMap = new();

    public bool initialized { get; private set; }
    public CoordinateMap coordinateMap { get; private set; }
    public HashSet<WorldChunk> allChunks { get { return _chunks; } private set { } }

    public WorldChunkMap(CoordinateMap coordinateMap)
    {
        this.coordinateMap = coordinateMap;
        foreach (Coordinate coord in coordinateMap.allCoordinates)
        {
            WorldChunk newChunk = new WorldChunk(this, coord);
            _chunks.Add(newChunk);
            _chunkMap[coord.localPosition] = newChunk;
        }

        initialized = true;
    }


    #region == GET CHUNKS ======================================== ////

    public WorldChunk GetChunkAt(Vector2Int position)
    {
        if (!initialized) { return null; }

        // Use the dictionary for fast lookups
        if (_chunkMap.TryGetValue(position, out WorldChunk foundChunk))
        {
            return foundChunk;
        }
        return null;
    }

    public WorldChunk GetChunkAt(Coordinate worldCoord)
    {
        if (!initialized || worldCoord == null) { return null; }

        // Use the dictionary for fast lookups
        if (_chunkMap.TryGetValue(worldCoord.localPosition, out WorldChunk foundChunk))
        {
            return foundChunk;
        }
        return null;
    }

    public List<WorldChunk> GetChunksAtCoordinates(List<Coordinate> worldCoords)
    {
        if (!initialized) { return new List<WorldChunk>(); }

        List<WorldChunk> chunks = new List<WorldChunk>();
        foreach (Coordinate worldCoord in worldCoords)
        {
            chunks.Add(GetChunkAt(worldCoord));
        }

        return chunks;
    }
    #endregion


    #region == SET CHUNKS =====================================////
    public void ResetAllChunkHeights()
    {
        foreach (WorldChunk chunk in allChunks)
        {
            chunk.SetGroundHeight(0);
        }
    }


    public void SetChunksToHeight(List<WorldChunk> worldChunk, int chunkHeight)
    {
        foreach (WorldChunk chunk in worldChunk)
        {
            chunk.SetGroundHeight(chunkHeight);
        }
    }

    public void SetChunksToHeightFromCoordinates(List<Coordinate> worldCoords, int chunkHeight)
    {
        foreach (Coordinate coord in worldCoords)
        {
            WorldChunk chunk = GetChunkAt(coord);
            if (chunk != null)
            {
                chunk.SetGroundHeight(chunkHeight);
            }
        }
    }

    #endregion
}
