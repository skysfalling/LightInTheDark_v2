using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldChunkMap
{
    HashSet<WorldChunk> _chunks = new();
    Dictionary<Vector2Int, WorldChunk> _chunkMap = new();

    public bool Initialized { get; private set; }
    public WorldRegion WorldRegion { get; private set; }
    public CoordinateMap CoordinateMap { get; private set; }
    public HashSet<WorldChunk> AllChunks { get { return _chunks; } private set { } }

    public WorldChunkMap(WorldRegion worldRegion, CoordinateMap coordinateMap)
    {
        this.WorldRegion = worldRegion;
        this.CoordinateMap = coordinateMap;
        foreach (Vector2Int position in coordinateMap.allPositions)
        {
            Coordinate coordinate = coordinateMap.GetCoordinateAt(position);
            WorldChunk newChunk = new WorldChunk(this, coordinate);
            _chunks.Add(newChunk);
            _chunkMap[coordinate.Value] = newChunk;

            Coordinate.TYPE type = (Coordinate.TYPE)coordinateMap.GetCoordinateTypeAt(position);
            switch(type)
            {
                case Coordinate.TYPE.EXIT:
                    // try to make sure that all exits are accessible
                    int exitMin = Mathf.Abs(WorldGeneration.MaxChunkHeight - WorldGeneration.PlayRegionWidth_inChunks);
                    int exitMax = Mathf.Abs(WorldGeneration.MaxChunkHeight - 1);


                    newChunk.SetGroundHeight(Random.Range(exitMin, exitMax)); break;

                case Coordinate.TYPE.CLOSED:
                    newChunk.SetGroundHeight(WorldGeneration.MaxChunkHeight); break;
            }
        }

        Initialized = true;

        // Apply heights

        // Create Chunk Meshes
        foreach (WorldChunk chunk in _chunks)
        {
            chunk.CreateChunkMesh();
        }
    }

    public WorldChunk GetChunkAt(Vector2Int position)
    {
        if (!Initialized || !CoordinateMap.allPositions.Contains(position)) { return null; }
        return _chunkMap[position];
    }

    public WorldChunk GetChunkAt(Coordinate worldCoord)
    {
        if (!Initialized || worldCoord == null) { return null; }
        return GetChunkAt(worldCoord.Value);
    }

    public List<WorldChunk> GetChunksAtCoordinates(List<Coordinate> worldCoords)
    {
        if (!Initialized) { return new List<WorldChunk>(); }

        List<WorldChunk> chunks = new List<WorldChunk>();
        foreach (Coordinate worldCoord in worldCoords)
        {
            chunks.Add(GetChunkAt(worldCoord));
        }

        return chunks;
    }

    public void ResetAllChunkHeights()
    {
        foreach (WorldChunk chunk in AllChunks)
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

    public void SetChunksToHeightFromPositions(List<Vector2Int> positions, int chunkHeight)
    {
        foreach (Vector2Int pos in positions)
        {
            WorldChunk chunk = GetChunkAt(pos);
            if (chunk != null)
            {
                chunk.SetGroundHeight(chunkHeight);
            }
        }
    }

    public void SetChunksToHeightFromPath(WorldPath path, float heightAdjustChance = 1f)
    {
        int startHeight = GetChunkAt(path.start).groundHeight;
        int endHeight = GetChunkAt(path.end).groundHeight;

        // Calculate height difference
        int endpointHeightDifference = endHeight - startHeight;
        int currHeightLevel = startHeight; // current height level starting from the startHeight
        int heightLeft = endpointHeightDifference; // initialize height left

        // Iterate through the chunks
        for (int i = 0; i < path.positions.Count; i++)
        {
            WorldChunk currentChunk = GetChunkAt(path.positions[i]);

            // Assign start/end chunk heights & CONTINUE
            if (i == 0) { currentChunk.SetGroundHeight(startHeight); continue; }
            else if (i == path.positions.Count - 1) { currentChunk.SetGroundHeight(endHeight); continue; }
            else
            {
                // Determine heightOffset 
                int heightOffset = 0;

                // Determine the direction of the last & next chunk in path
                WorldChunk previousChunk = GetChunkAt(path.positions[i - 1]);
                WorldChunk nextChunk = GetChunkAt(path.positions[i + 1]);
                WorldDirection? lastChunkDirection = currentChunk.coordinate.GetWorldDirectionOfNeighbor(previousChunk.coordinate);
                WorldDirection? nextChunkDirection = currentChunk.coordinate.GetWorldDirectionOfNeighbor(nextChunk.coordinate);
                if (lastChunkDirection != null && nextChunkDirection != null)
                {
                    // if previous chunk is direct opposite of next chunk, allow for change in the current chunk
                    if (currentChunk.coordinate.GetNeighborInOppositeDirection((WorldDirection)nextChunkDirection) == previousChunk.coordinate)
                    {
                        // Valid transition chunk
                        if (heightLeft > 0) { heightOffset = 1; } // if height left is greater
                        else if (heightLeft < 0) { heightOffset = -1; } // if height left is less than 0
                        else { heightOffset = 0; } // if height left is equal to 0
                    }

                }

                // Set the new height level
                currHeightLevel += heightOffset;
                currentChunk.SetGroundHeight(currHeightLevel);

                // Recalculate heightLeft with the new current height level
                heightLeft = endHeight - currHeightLevel;
            }
        }
    }
}
