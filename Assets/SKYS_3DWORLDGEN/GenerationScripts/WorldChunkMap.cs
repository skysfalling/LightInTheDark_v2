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
        float chunkWidth = _worldGeneration.realWorldChunkSize.x;

        // Calculate neighbor positions
        Vector2 leftPosition = chunk.worldPosition + new Vector2(-chunkWidth, 0);
        Vector2 rightPosition = chunk.worldPosition + new Vector2(chunkWidth, 0);
        Vector2 forwardPosition = chunk.worldPosition + new Vector2(0, chunkWidth);
        Vector2 backwardPosition = chunk.worldPosition + new Vector2(0, -chunkWidth);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldChunks.Find(c => c.worldPosition == leftPosition);     // Left
        neighbors[1] = _worldChunks.Find(c => c.worldPosition == rightPosition);    // Right
        neighbors[2] = _worldChunks.Find(c => c.worldPosition == forwardPosition);  // Forward
        neighbors[3] = _worldChunks.Find(c => c.worldPosition == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public List<WorldChunk> GetChunkNeighbors(WorldChunk chunk) 
    {
        if (!_chunkNeighborMap.ContainsKey(chunk)) { return new List<WorldChunk>(); }

        return _chunkNeighborMap[chunk];
    }

    // == WORLD CHUNK EDGES
    // Method to retrieve an edge chunk
    public WorldChunk GetEdgeChunk(WorldDirection edgeDirection, int index)
    {
        // This list will hold the chunks on the specified edge
        List<WorldChunk> edgeChunks = new List<WorldChunk>();

        // Identify chunks on the specified edge
        foreach (WorldChunk chunk in WorldGeneration.Instance.GetBorderChunks())
        {
            if (IsOnSpecifiedEdge(chunk, edgeDirection))
            {
                edgeChunks.Add(chunk);
            }
        }

        // Ensure the index is within bounds
        if (index >= 0 && index < edgeChunks.Count)
        {
            return edgeChunks[index];
        }

        // Return null or throw an exception if no chunk is found at the index
        return null;
    }

    private bool IsOnSpecifiedEdge(WorldChunk chunk, WorldDirection edgeDirection)
    {
        WorldGeneration worldGen = WorldGeneration.Instance;
        Vector2 halfSize_playArea = (Vector2)worldGen.realWorldPlayAreaSize * 0.5f;


        // Define the boundaries for each direction
        float westBoundaryX = -halfSize_playArea.x;
        float eastBoundaryX = halfSize_playArea.x;
        float northBoundaryY = -halfSize_playArea.y;
        float southBoundaryY = halfSize_playArea.y;

        switch (edgeDirection)
        {
            case WorldDirection.West:
                return chunk.worldPosition.x == westBoundaryX;
            case WorldDirection.East:
                return chunk.worldPosition.x == eastBoundaryX;
            case WorldDirection.North:
                return chunk.worldPosition.y == northBoundaryY;
            case WorldDirection.South:
                return chunk.worldPosition.y == southBoundaryY;
            default:
                return false;
        }
    }

    // == HELPER FUNCTIONS ==============>>
    public WorldChunk FindClosestChunk(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldChunk closestChunk = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            float distance = Vector3.Distance(position, chunk.worldPosition);

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


        string str_out = $"[ WORLD CHUNK ] : {chunk.worldPosition}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";


        return str_out;
    }
}
