using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Total number of chunks
 * Count of each type of chunk
 * 
 */


public class WorldStatTracker : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    WorldSpawnMap _worldSpawnMap;
    int _currentGeneration = 0;

    public void UpdateStats()
    {
        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();
        _currentGeneration++;
    }

    public string GetWorldStats()
    {
        if (_worldGeneration == null) return "[ WORLD GENERATION ] is not available.";
        if (_worldSpawnMap == null || !_worldSpawnMap.initialized) return "[ WORLD GENERATION ] : WorldSpawnMap not initialized.";

        string str_out = $"[ WORLD GENERATION ] : #{_currentGeneration}\n";
        str_out += $"\t>> chunk_dimensions {WorldGeneration.ChunkVec3Dimensions_inCells()}\n";
        str_out += $"\t>> full_chunk_dimensions {WorldGeneration.GetChunkVec3Dimensions_inWorldSpace()}\n";
        str_out += $"\t    -- Empty Chunks {_worldSpawnMap.GetAllChunksOfType(WorldChunk.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Hallway Chunks {_worldSpawnMap.GetAllChunksOfType(WorldChunk.TYPE.HALLWAY).Count}\n";
        str_out += $"\t    -- Corner Chunks {_worldSpawnMap.GetAllChunksOfType(WorldChunk.TYPE.CORNER).Count}\n";
        str_out += $"\t    -- Deadend Chunks {_worldSpawnMap.GetAllChunksOfType(WorldChunk.TYPE.DEADEND).Count} \n";
        str_out += $"\t    -- Closed Chunks {_worldSpawnMap.GetAllChunksOfType(WorldChunk.TYPE.CLOSED).Count} \n";
                   
        return str_out;
    }
}
