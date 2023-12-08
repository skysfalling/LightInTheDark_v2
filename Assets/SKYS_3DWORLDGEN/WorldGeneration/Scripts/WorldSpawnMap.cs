using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSpawnObject
{
    public int spawnRadius;
}

[RequireComponent(typeof(WorldSpawnDebug))]
public class WorldSpawnMap : MonoBehaviour
{
    public bool initialized = false;

    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;

    Dictionary<WorldChunk.TYPE, List<WorldChunk>> _chunkTypeMap = new Dictionary<WorldChunk.TYPE, List<WorldChunk>>();

    public void InitializeSpawnMap()
    {
        initialized = false;

        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();

        // Sort the chunks by type
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            WorldChunk.TYPE chunkType = chunk.type;

            // Check if this chunk type is already a key in the dictionary
            if (!_chunkTypeMap.ContainsKey(chunkType))
            {
                // If not, add it with a new list
                _chunkTypeMap[chunkType] = new List<WorldChunk>();
            }

            // Add the chunk to the appropriate list
            _chunkTypeMap[chunkType].Add(chunk);
        }

        initialized = true;
    }

    public void Reset()
    {
        initialized = false;
        _chunkTypeMap.Clear();
    }

    public List<WorldChunk> GetAllChunksOfType(WorldChunk.TYPE chunkType)
    {
        if (_chunkTypeMap.ContainsKey(chunkType))
        {
            return _chunkTypeMap[chunkType];
        }
        return new List<WorldChunk>();
    }
}
