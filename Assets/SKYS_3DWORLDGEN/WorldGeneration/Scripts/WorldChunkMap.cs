 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    Dictionary<WorldChunk, List<WorldChunk>> worldChunkMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    // Start is called before the first frame update
    void Awake()
    {
        _worldGeneration = GetComponentInParent<WorldGeneration>();
    }

    public void InitializeChunkMap()
    {
        _worldChunks = _worldGeneration.GetChunks();
        worldChunkMap.Clear();

        // SET CHUNK NEIGHBORS
        foreach (WorldChunk chunk in _worldChunks)
        {
            List<WorldChunk> neighbors = GetChunkNeighbors(chunk);
            worldChunkMap[chunk] = neighbors;
        }
    }

    private List<WorldChunk> GetChunkNeighbors(WorldChunk chunk)
    {
        List<WorldChunk> neighbors = new List<WorldChunk>(new WorldChunk[4]);
        float cellSize = _worldGeneration.cellSize; // Assuming 'cellSize' is a public field in WorldGeneration

        // Calculate neighbor positions
        Vector3 leftPosition = chunk.position + new Vector3(-cellSize, 0, 0);
        Vector3 rightPosition = chunk.position + new Vector3(cellSize, 0, 0);
        Vector3 forwardPosition = chunk.position + new Vector3(0, 0, cellSize);
        Vector3 backwardPosition = chunk.position + new Vector3(0, 0, -cellSize);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldChunks.Find(c => c.position == leftPosition);     // Left
        neighbors[1] = _worldChunks.Find(c => c.position == rightPosition);    // Right
        neighbors[2] = _worldChunks.Find(c => c.position == forwardPosition);  // Forward
        neighbors[3] = _worldChunks.Find(c => c.position == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    private void OnDrawGizmosSelected()
    {
        if (_worldChunks.Count == 0) { return; }
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(_worldChunks[0].position, _worldGeneration.fullsize_chunkDimensions);
    }
}