using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnvironmentObject
{
    public GameObject prefab;
    public Vector2Int space = new Vector2Int(1, 1);
    public List<WorldCell.TYPE> spawnCellTypeRequirements = new List<WorldCell.TYPE>();
    public WorldCell.TYPE cellTypeConversion;
}
public class WorldEnvironment : MonoBehaviour
{
    string prefix = "{ WORLD ENVIRONMENT } ";
    public bool generation_finished = false;
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;

    string parentObjectPrefix = "env_parent :: ";
    Dictionary<WorldChunk, Transform> _worldChunkEnvParentMap = new Dictionary<WorldChunk, Transform>();

    [Header("PLAYER")]
    public GameObject playerPrefab;

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
        if (environmentObjects.Count == 0 ) { return; }
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "chunk" + chunk.position);
            newParent.transform.parent = transform;
            _worldChunkEnvParentMap[chunk] = newParent.transform;

        }

        // Set Player Spawn Point
        WorldCell playerSpawnCell = _worldGeneration.GetChunks()[0].GetRandomCellOfType(WorldCell.TYPE.EMPTY);
        SpawnPrefab(playerPrefab, playerSpawnCell);
        playerSpawnCell.SetCellType(WorldCell.TYPE.SPAWN_POINT);

        // Create Chunk Environment for each Chunk
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
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
    private void CreateChunkEnvironment(WorldChunk chunk, List<EnvironmentObject> envObjects)
    {
        if (envObjects.Count == 0) {  return; }
        EnvironmentObject envObj = envObjects[Random.Range(0, envObjects.Count)];

        foreach (WorldCell cell in chunk.localCells)
        {
            if (cell.type == WorldCell.TYPE.EDGE || cell.type == WorldCell.TYPE.CORNER)
            {
                SpawnPrefab(wall_0, cell);
            }

            // TRY TO SPAWN ENV OBJECT
            else
            {
                int req_spaceArea = envObj.space.x * envObj.space.y;
                List<WorldCell> foundSpace = chunk.FindSpace(envObj);

                if (foundSpace == null) { continue; }
                if (foundSpace.Count == req_spaceArea)
                {
                    SpawnEnvObject(envObj, foundSpace);
                }
            }
        }
    }

    private GameObject SpawnEnvObject(EnvironmentObject envObj, List<WorldCell> spawnArea)
    {
        WorldCell startCell = spawnArea[0]; // start cell ( top left )

        // spawn object in center of area
        GameObject newObject = SpawnPrefab(envObj.prefab, startCell);

        Debug.Log($"{prefix} SpawnEnvObject{envObj.prefab.name}\n" +
            $"\tStart Cell : {startCell.position}\n");
        // Mark Cell Area
        startCell.GetChunk().MarkArea(spawnArea, envObj.cellTypeConversion);

        return newObject;
    }
    private GameObject SpawnPrefab(GameObject prefab, WorldCell cell)
    {
        GameObject newObject = Instantiate(prefab, cell.position, Quaternion.identity);
        newObject.transform.parent = _worldChunkEnvParentMap[cell.GetChunk()];
        newObject.transform.position = cell.position;
        return newObject;
    }


}