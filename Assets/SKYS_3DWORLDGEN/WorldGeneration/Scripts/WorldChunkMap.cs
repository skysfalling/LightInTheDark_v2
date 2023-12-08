 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldChunkDebug))]
public class WorldChunkMap : MonoBehaviour
{
    public bool initialized = false;
    WorldGeneration _worldGeneration;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    Dictionary<WorldChunk, List<WorldChunk>> _chunkNeighborMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    public void InitializeChunkMap()
    {
        initialized = false;

        _worldGeneration = GetComponentInParent<WorldGeneration>();
        _worldChunks = _worldGeneration.GetChunks();
        _chunkNeighborMap.Clear();

        // << SET CHUNK NEIGHBORS >>
        foreach (WorldChunk chunk in _worldChunks)
        {
            List<WorldChunk> neighbors = SetChunkNeighbors(chunk);
            _chunkNeighborMap[chunk] = neighbors;
        }

        foreach (WorldChunk chunk in _worldChunks)
        {
            chunk.SetChunkType();
        }

        initialized = true;
    }


    public void Reset()
    {
        _worldChunks.Clear();
        _chunkNeighborMap.Clear();
        initialized = false;
    }

    // =========================================================================================
    private List<WorldChunk> SetChunkNeighbors(WorldChunk chunk)
    {
        List<WorldChunk> neighbors = new List<WorldChunk>(new WorldChunk[4]);
        float chunkSize = _worldGeneration.fullsize_chunkDimensions.x;

        // Calculate neighbor positions
        Vector3 leftPosition = chunk.position + new Vector3(-chunkSize, 0, 0);
        Vector3 rightPosition = chunk.position + new Vector3(chunkSize, 0, 0);
        Vector3 forwardPosition = chunk.position + new Vector3(0, 0, chunkSize);
        Vector3 backwardPosition = chunk.position + new Vector3(0, 0, -chunkSize);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldChunks.Find(c => c.position == leftPosition);     // Left
        neighbors[1] = _worldChunks.Find(c => c.position == rightPosition);    // Right
        neighbors[2] = _worldChunks.Find(c => c.position == forwardPosition);  // Forward
        neighbors[3] = _worldChunks.Find(c => c.position == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public List<WorldChunk> GetChunkNeighbors(WorldChunk chunk) 
    {
        return _chunkNeighborMap[chunk];
    }

    public WorldChunk FindClosestChunk(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldChunk closestChunk = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            float distance = Vector3.Distance(position, chunk.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestChunk = chunk;
            }
        }

        if (closestChunk != null)
        {
            return closestChunk;
        }

        return null;
    }
}
