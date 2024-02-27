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

    [Header("Scale")]
    public float minScale = 1f;
    public float maxScale = 1f;
    public float GetRandomScaleMultiplier()
    {
        return UnityEngine.Random.Range(minScale, maxScale);
    }


    [Range(0, 1)]
    public float spawnChance = 0.5f;
}
public class WorldEnvironment : MonoBehaviour
{
    public static WorldEnvironment Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }
    string prefix = "{ WORLD ENVIRONMENT } ";
    public bool generation_finished = false;
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;

    string parentObjectPrefix = "env_parent :: ";
    Dictionary<WorldChunk, Transform> _worldChunkEnvParentMap = new Dictionary<WorldChunk, Transform>();
    Dictionary<WorldChunk, Transform> _borderChunkEnvParentMap = new Dictionary<WorldChunk, Transform>();

    [Header("PLAYER")]
    public GameObject playerPrefab;
    [HideInInspector] public WorldCell playerSpawnCell;
    [HideInInspector] public GameObject instantiatedPlayer;

    [Header("WALLS")]
    public GameObject wall_0;

    [Header("ENVIRONMENT")]
    public List<EnvironmentObject> environmentObjects = new List<EnvironmentObject>();
    public List<EnvironmentObject> borderObjects = new List<EnvironmentObject>();

    public void StartEnvironmentGeneration()
    {
        generation_finished = false;

        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();

        Destroy(instantiatedPlayer);
        instantiatedPlayer = null;
        _worldChunkEnvParentMap.Clear();
        _borderChunkEnvParentMap.Clear();

        /*
        // << CREATE ENV PARENTS >>
        if (environmentObjects.Count == 0) { return; }
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "chunk" + chunk.coordinate);
            newParent.transform.parent = transform;
            _worldChunkEnvParentMap[chunk] = newParent.transform;
        }

        // << CREATE BORDER PARENTS >>
        if (environmentObjects.Count == 0) { return; }
        foreach (WorldChunk chunk in _worldGeneration.GetBorderChunks())
        {
            GameObject newParent = new GameObject(parentObjectPrefix + "BORDER chunk" + chunk.coordinate);
            newParent.transform.parent = transform;
            _borderChunkEnvParentMap[chunk] = newParent.transform;
        }

        // Set Player Spawn Point
        playerSpawnCell = _worldGeneration.GetChunks()[0].GetRandomCellOfType(WorldCell.TYPE.EMPTY);
        playerSpawnCell.SetCellType(WorldCell.TYPE.SPAWN_POINT);
        if (playerPrefab != null) {
            instantiatedPlayer = SpawnPrefab(playerPrefab, playerSpawnCell, _worldGeneration.transform);
        }

        // Create Chunk Environment for each Chunk
        foreach (WorldChunk chunk in _worldGeneration.GetChunks())
        {
            CreateChunkEnvironment(chunk, environmentObjects, _worldChunkEnvParentMap);
        }

        // Create Chunk Environment for each BORDER Chunk
        foreach (WorldChunk borderChunk in _worldGeneration.GetBorderChunks())
        {
            borderChunk.Initialize();
            CreateChunkEnvironment(borderChunk, borderObjects, _borderChunkEnvParentMap);
        }
        */

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

        foreach (Transform parent in _borderChunkEnvParentMap.Values)
        {
            Destroy(parent.gameObject);
        }
        _borderChunkEnvParentMap.Clear();

        Destroy(instantiatedPlayer.gameObject);
    }

    // ======================= CREATE CHUNK ENVIRONMENT =========================================
    private void CreateChunkEnvironment(WorldChunk chunk, List<EnvironmentObject> envObjects, Dictionary<WorldChunk, Transform> parentMap)
    {
        if (envObjects.Count == 0) {  return; }
        EnvironmentObject envObj = envObjects[Random.Range(0, envObjects.Count)];

        foreach (WorldCell cell in chunk.localCells)
        {
            if (cell.type == WorldCell.TYPE.EDGE || cell.type == WorldCell.TYPE.CORNER)
            {
                SpawnPrefab(wall_0, cell, parentMap[cell.GetChunk()], WorldGeneration.CellWidth_inWorldSpace);
            }

            // TRY TO SPAWN ENV OBJECT
            else
            {
                // Random Spawn Chance
                if (Random.Range(0f, 1f) > envObj.spawnChance) { continue; }

                // Get Required Space Area
                int req_spaceArea = envObj.space.x * envObj.space.y;
                List<WorldCell> foundSpace = chunk.FindSpace(envObj);

                if (foundSpace == null) { continue; }
                if (foundSpace.Count == req_spaceArea)
                {
                    SpawnEnvObject(envObj, parentMap[cell.GetChunk()], foundSpace);
                }
            }
        }
    }

    private GameObject SpawnEnvObject(EnvironmentObject envObj, Transform envParent, List<WorldCell> spawnArea)
    {
        WorldCell startCell = spawnArea[0]; // start cell ( top left )

        // spawn object in center of area
        GameObject newObject = SpawnPrefab(envObj.prefab, startCell, envParent, envObj.GetRandomScaleMultiplier());

        /*
        Debug.Log($"{prefix} SpawnEnvObject{envObj.prefab.name}\n" +
            $"\tStart Cell : {startCell.position}\n");
        */

        // Mark Cell Area
        startCell.GetChunk().MarkArea(spawnArea, envObj.cellTypeConversion);

        return newObject;
    }

    private GameObject SpawnPrefab(GameObject prefab, WorldCell cell, Transform parent, float scaleMultiplier = 1)
    {
        GameObject newObject = Instantiate(prefab, cell.position, Quaternion.identity);
        newObject.transform.parent = parent;
        newObject.transform.position = cell.position;
        newObject.transform.localScale *= scaleMultiplier;
        return newObject;
    }

}