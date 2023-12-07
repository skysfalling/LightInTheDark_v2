 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldChunkDebug))]
public class WorldChunkMap : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    public Dictionary<WorldChunk, List<WorldChunk>> neighborMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    // Start is called before the first frame update
    void Awake()
    {
        _worldGeneration = GetComponentInParent<WorldGeneration>();
    }

    public void InitializeChunkMap()
    {
        _worldChunks = _worldGeneration.GetChunks();
        neighborMap.Clear();

        // SET CHUNK NEIGHBORS
        foreach (WorldChunk chunk in _worldChunks)
        {
            List<WorldChunk> neighbors = GetChunkNeighbors(chunk);
            neighborMap[chunk] = neighbors;
        }
    }

    private List<WorldChunk> GetChunkNeighbors(WorldChunk chunk)
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
