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
    public bool generation_finished = false;
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;

    string parentObjectPrefix = "env_parent :: ";
    Dictionary<WorldChunk, Transform> _worldChunkEnvParentMap = new Dictionary<WorldChunk, Transform>();

    [Header("WALLS")]
    public GameObject wall_0;

    [Header("ENVIRONMENT")]
    public List<EnvironmentObject> environmentObjects = new List<EnvironmentObject>();

    public void StartEnvironmentGeneration()
    {
        generation_finished = false;

        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();

        _worldChunkEnvParentMap.Clear();

        // << CREATE ENV PARENTS >>
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "chunk" + chunk.position);
            newParent.transform.parent = transform;
            _worldChunkEnvParentMap[chunk] = newParent.transform;

            CreateChunkEnvironment(chunk, environmentObjects);
        }



        generation_finished = true;
    }

    public void Reset()
    {
        generation_finished = false;
        foreach (Transform parent in _worldChunkEnvParentMap.Values)
        {
            Destroy(parent.gameObject);
        }
        _worldChunkEnvParentMap.Clear();
    }

    // ======================= CREATE CHUNK ENVIRONMENT =========================================

    private void CreateChunkEnvironment(WorldChunk chunk, List<EnvironmentObject> prefabs)
    {
        foreach (WorldCell cell in chunk.localCells)
        {
            switch (cell.type)
            {
                case WorldCell.TYPE.EMPTY:
                    if (environmentObjects == null || environmentObjects.Count == 0) return;
                    SpawnObjectAtCell(environmentObjects[0].prefab, cell);
                    break;
                case WorldCell.TYPE.EDGE:
                case WorldCell.TYPE.CORNER:
                    SpawnObjectAtCell(wall_0, cell);
                    break;
                default:
                    break;
            }
        }
    }

    // ======================= SPAWN OBJECTS =========================================

    private GameObject SpawnObjectAtCell(GameObject prefab, WorldCell cell)
    {
        GameObject newEnvObject = Instantiate(prefab, cell.position, Quaternion.identity);
        newEnvObject.transform.parent = _worldChunkEnvParentMap[cell.GetChunk()];
        newEnvObject.transform.position = cell.position;

        return newEnvObject;
    }



}