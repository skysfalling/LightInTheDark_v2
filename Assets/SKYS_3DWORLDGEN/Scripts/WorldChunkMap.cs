 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    public static WorldChunkMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

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

        // << Initialize Chunks >>
        foreach (WorldChunk chunk in _worldChunks)
        {
            // Set Neighbors
            List<WorldChunk> neighbors = MapChunkNeighbors(chunk);
            _chunkNeighborMap[chunk] = neighbors;

            // Initialize
            chunk.Initialize();
        }

        initialized = true;
    }

    public void Reset()
    {
        _worldChunks.Clear();
        _chunkNeighborMap.Clear();
        initialized = false;
    }

    // == MAP CHUNK NEIGHBORS ==============>>
    private List<WorldChunk> MapChunkNeighbors(WorldChunk chunk)
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
        if (!_chunkNeighborMap.ContainsKey(chunk)) { return new List<WorldChunk>(); }

        return _chunkNeighborMap[chunk];
    }


    // == HELPER FUNCTIONS ==============>>
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


    // == DEBUG FUNCTIONS ==============>>
    public void ShowChunkCells(WorldChunk chunk)
    {
        if (chunk == null) { return; }
        foreach (WorldCell cell in chunk.localCells)
        {
            cell.ShowDebugCube();
        }
    }

    public void HideChunkCells(WorldChunk chunk)
    {
        if (chunk == null) { return; }
        foreach (WorldCell cell in chunk.localCells)
        {
            //cell.HideDebugCube();
            Destroy(cell.GetDebugCube()); // Destroy the debug cube for efficiency
        }
    }

    public string GetChunkStats(WorldChunk chunk)
    {
        if (_worldGeneration == null || !_worldGeneration.generation_finished) return "[ WORLD GENERATION ] is not available.";
        if (chunk == null) return "[ WORLD CHUNK ] is not available.";
        if (chunk.initialized == false) return "[ WORLD CHUNK ] is not initialized.";


        string str_out = $"[ WORLD CHUNK ] : {chunk.position}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";


        return str_out;
    }
}
