using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnvironmentObject
{
    public GameObject prefab;
    public int spawnRadius = 1;
}

public class WorldEnvironment : MonoBehaviour
{
    public bool initialized = false;
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;

    string parentObjectPrefix = "env_parent :: ";
    Dictionary<WorldChunk, Transform> _worldChunkEnvParentMap = new Dictionary<WorldChunk, Transform>();

    [Header("WALLS")]
    public GameObject wall_0;

    [Header("ENVIRONMENT")]
    public GameObject env_0;

    public void StartEnvironmentGeneration()
    {
        initialized = false;

        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();

        _worldChunkEnvParentMap.Clear();

        // << SPAWN BY CHUNK >>
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "chunk" + chunk.position);
            newParent.transform.parent = transform;
            _worldChunkEnvParentMap[chunk] = newParent.transform;

            // << SPAWN WALLS >>
            foreach (WorldCell cell in chunk.localCells)
            {
                if (cell.type != WorldCell.TYPE.EMPTY)
                {
                    SpawnObjectAtCell(wall_0, cell);

                }
            }

            // << SPAWN IN EMPTY CELLS >>
            if (chunk.type == WorldChunk.TYPE.EMPTY)
            {
                SpawnObjectAtCell(env_0, chunk.GetRandomCellOfType(WorldCell.TYPE.EMPTY));
            }
        }

        initialized = true;
    }

    public void Reset()
    {
        initialized = false;
        foreach (WorldChunk chunk in _worldChunkEnvParentMap.Keys)
        {
            Destroy(_worldChunkEnvParentMap[chunk].gameObject);
            _worldChunkEnvParentMap[chunk] = null;
        }
        _worldChunkEnvParentMap.Clear();
    }

    private GameObject SpawnObjectAtCell(GameObject prefab, WorldCell cell)
    {
        GameObject newEnvObject = Instantiate(prefab, cell.position, Quaternion.identity);
        newEnvObject.transform.parent = _worldChunkEnvParentMap[cell.GetChunk()];
        newEnvObject.transform.position = cell.position;

        return newEnvObject;
    }
}