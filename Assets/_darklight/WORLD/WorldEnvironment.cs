using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.World.Generation
{
    public class EnvironmentObject
    {
        public GameObject prefab;
        public Vector2Int space = new Vector2Int(1, 1);
        public List<Cell.TYPE> spawnCellTypeRequirements = new List<Cell.TYPE>();
        public Cell.TYPE cellTypeConversion;

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
        WorldBuilder _worldGeneration;
        ChunkGeneration _worldChunkMap;

        string parentObjectPrefix = "env_parent :: ";
        Dictionary<Chunk, Transform> _worldChunkEnvParentMap = new Dictionary<Chunk, Transform>();
        Dictionary<Chunk, Transform> _borderChunkEnvParentMap = new Dictionary<Chunk, Transform>();

        [Header("PLAYER")]
        public GameObject playerPrefab;
        [HideInInspector] public Cell playerSpawnCell;
        [HideInInspector] public GameObject instantiatedPlayer;

        [Header("WALLS")]
        public GameObject wall_0;

        [Header("ENVIRONMENT")]
        public List<EnvironmentObject> environmentObjects = new List<EnvironmentObject>();
        public List<EnvironmentObject> borderObjects = new List<EnvironmentObject>();

        public void StartEnvironmentGeneration()
        {
            generation_finished = false;

            _worldGeneration = FindObjectOfType<WorldBuilder>();

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
        private void CreateChunkEnvironment(Chunk chunk, List<EnvironmentObject> envObjects, Dictionary<Chunk, Transform> parentMap)
        {
            if (envObjects.Count == 0) { return; }
            EnvironmentObject envObj = envObjects[Random.Range(0, envObjects.Count)];

            /*
            foreach (Cell cell in chunk.localCells)
            {
                if (cell.Type == Cell.TYPE.EDGE || cell.Type == Cell.TYPE.CORNER)
                {
                    SpawnPrefab(wall_0, cell, parentMap[cell.ChunkParent], WorldGeneration.Settings.CellSize_inGameUnits);
                }

                // TRY TO SPAWN ENV OBJECT
                else
                {
                    // Random Spawn Chance
                    if (Random.Range(0f, 1f) > envObj.spawnChance) { continue; }

                    // Get Required Space Area
                    int req_spaceArea = envObj.space.x * envObj.space.y;
                    List<Cell> foundSpace = chunk.FindSpace(envObj);

                    if (foundSpace == null) { continue; }
                    if (foundSpace.Count == req_spaceArea)
                    {
                        SpawnEnvObject(envObj, parentMap[cell.ChunkParent], foundSpace);
                    }
                }
            }
            */
        }

        private GameObject SpawnEnvObject(EnvironmentObject envObj, Transform envParent, List<Cell> spawnArea)
        {
            Cell startCell = spawnArea[0]; // start cell ( top left )

            // spawn object in center of area
            GameObject newObject = SpawnPrefab(envObj.prefab, startCell, envParent, envObj.GetRandomScaleMultiplier());

            /*
            Debug.Log($"{prefix} SpawnEnvObject{envObj.prefab.name}\n" +
                $"\tStart Cell : {startCell.position}\n");
            */

            // Mark Cell Area
            startCell.ChunkParent.MarkArea(spawnArea, envObj.cellTypeConversion);

            return newObject;
        }

        private GameObject SpawnPrefab(GameObject prefab, Cell cell, Transform parent, float scaleMultiplier = 1)
        {
            GameObject newObject = Instantiate(prefab, cell.Position, Quaternion.identity);
            newObject.transform.parent = parent;
            newObject.transform.position = cell.Position;
            newObject.transform.localScale *= scaleMultiplier;
            return newObject;
        }

    }
}
