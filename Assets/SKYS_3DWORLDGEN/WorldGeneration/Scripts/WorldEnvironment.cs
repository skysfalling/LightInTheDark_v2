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
    List<GameObject> _envParents = new List<GameObject>();

    [Header("WALLS")]
    public EnvironmentObject wall_0;

    public void StartEnvironmentGeneration()
    {
        initialized = false;

        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();

        // << SPAWN WALLS >>
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "chunk" + chunk.position);
            newParent.transform.parent = transform;
            _envParents.Add(newParent);

            // SPAWN ASSETS
            foreach (WorldCell cell in chunk.cells)
            {
                if (cell.type != WorldCell.Type.EMPTY)
                {
                    GameObject newAsset = Instantiate(wall_0.prefab, cell.position, Quaternion.identity);
                    newAsset.transform.parent = newParent.transform;
                }
            }
        }

        initialized = true;
    }

    public void Reset()
    {
        initialized = false;

        foreach (GameObject parent in _envParents)
        {
            Destroy(parent);
        }
        _envParents.Clear();
    }
}