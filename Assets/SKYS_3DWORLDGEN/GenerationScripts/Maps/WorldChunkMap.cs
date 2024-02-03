 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    // >> SINGLETON INSTANCE ================== >>
    public static WorldChunkMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // == GAME INTIALIZATION ===============================================================
    [HideInInspector] public bool initialized = false;
    WorldGeneration _worldGen;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    Dictionary<WorldChunk, List<WorldChunk>> _chunkMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    public void InitializeChunkMap()
    {
        initialized = false;

        _worldGen = WorldGeneration.Instance;
        _worldChunks = _worldGen.GetChunks();
        _chunkMap.Clear();

        // << Initialize Chunks >>
        foreach (WorldChunk chunk in _worldChunks)
        {
            List<WorldChunk> neighbors = SetChunkNeighbors(chunk);
            _chunkMap[chunk] = neighbors;
            chunk.Initialize();
        }

        /*
        // << Find Golden Path >>
        if (_worldGen.worldExits.Count > 1)
        {
            List<WorldChunk> pathA = FindCoordinatePath(_worldGen.worldExits[0], _worldGen.worldExits[1]);
            if (pathA.Count > 1) { foreach (WorldChunk chunk in pathA) { chunk.isOnGoldenPath = true; } }
        }
        */
        initialized = true;
    }

    public void Reset()
    {
        _worldChunks.Clear();
        _chunkMap.Clear();
        initialized = false;
    }


    #region == INDIVIDUAL CHUNK NEIGHBORS ==============>>
    private List<WorldChunk> SetChunkNeighbors(WorldChunk chunk)
    {
        List<WorldChunk> neighbors = new List<WorldChunk>(new WorldChunk[4]);
        int chunkWidth = WorldGeneration.GetRealChunkAreaSize().x;
        int chunkLength = WorldGeneration.GetRealChunkAreaSize().y;

        // Calculate neighbor positions
        Vector2 leftPosition = chunk.coordinate + new Vector2(-chunkWidth, 0);
        Vector2 rightPosition = chunk.coordinate + new Vector2(chunkWidth, 0);
        Vector2 forwardPosition = chunk.coordinate + new Vector2(0, chunkLength);
        Vector2 backwardPosition = chunk.coordinate + new Vector2(0, -chunkLength);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldChunks.Find(c => c.coordinate == leftPosition);     // Left
        neighbors[1] = _worldChunks.Find(c => c.coordinate == rightPosition);    // Right
        neighbors[2] = _worldChunks.Find(c => c.coordinate == forwardPosition);  // Forward
        neighbors[3] = _worldChunks.Find(c => c.coordinate == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public List<WorldChunk> GetAllChunkNeighbors(WorldChunk chunk) 
    {
        if (!_chunkMap.ContainsKey(chunk)) { return new List<WorldChunk>(); }

        return _chunkMap[chunk];
    }
    #endregion

    // == HELPER FUNCTIONS ==============>>
    public WorldChunk FindClosestChunk(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldChunk closestChunk = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldChunk chunk in _worldGen.GetChunks())
        {
            float distance = Vector3.Distance(position, chunk.coordinate);

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

    public float GetDistanceBetweenChunks(WorldChunk chunkA, WorldChunk chunkB)
    {
        return Vector2.Distance(chunkA.coordinate, chunkB.coordinate);
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
        if (_worldGen == null) return "[ WORLD GENERATION ] is not available.";
        if (chunk == null) return "[ WORLD CHUNK ] is not available.";
        if (chunk.initialized == false) return "[ WORLD CHUNK ] is not initialized.";


        string str_out = $"[ WORLD CHUNK ] : {chunk.coordinate}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";

        return str_out;
    }
}
