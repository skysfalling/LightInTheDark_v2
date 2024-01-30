using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldSpawnDebug))]
public class WorldSpawnMap : MonoBehaviour
{
    public static WorldSpawnMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }
    public bool initialized = false;
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    Dictionary<WorldChunk.TYPE, List<WorldChunk>> _chunkTypeMap = new Dictionary<WorldChunk.TYPE, List<WorldChunk>>();

    List<EnemyAI> _activeEnemyAI = new List<EnemyAI>();

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

        List<EnemyAI> ref_activeEnemyAi = new List<EnemyAI>(_activeEnemyAI);
        for (int i = 0; i < ref_activeEnemyAi.Count; i++)
        {
            DestroyAi(ref_activeEnemyAi[i], 0);
        }

        _activeEnemyAI.Clear();
    }

    // =========================== REGISTER AI ===================== //

    public void RegisterAI(EnemyAI ai)
    {
        _activeEnemyAI.Add(ai);
    }

    public void DestroyAi(EnemyAI ai, float delay = 0)
    {
        _activeEnemyAI.Remove(ai);
        Destroy(ai.gameObject, delay);
    }

    // ======================================= HELPER FUNCTIONS ===================================================================

    public List<WorldChunk> GetAllChunksOfType(WorldChunk.TYPE chunkType)
    {
        if (_chunkTypeMap.ContainsKey(chunkType))
        {
            return _chunkTypeMap[chunkType];
        }
        return new List<WorldChunk>();
    }
}
