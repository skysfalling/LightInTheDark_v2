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
        foreach (Vector2Int position in coordinateMap.allPositions)
        {
            Coordinate coordinate = coordinateMap.GetCoordinateAt(position);
            WorldChunk newChunk = new WorldChunk(this, coordinate);
            _chunks.Add(newChunk);
            _chunkMap[coordinate.localPosition] = newChunk;
        }

        initialized = true;


        // Create Chunk Meshes
        foreach (WorldChunk chunk in _chunks)
        {
            chunk.CreateChunkMesh();
        }
    }


    #region == GET CHUNKS ======================================== ////

    public WorldChunk GetChunkAt(Vector2Int position)
    {
        if (!initialized || !coordinateMap.allPositions.Contains(position)) { return null; }
        return _chunkMap[position];
    }

    public WorldChunk GetChunkAt(Coordinate worldCoord)
    {
        if (!initialized || worldCoord == null) { return null; }
        return GetChunkAt(worldCoord.localPosition);
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
