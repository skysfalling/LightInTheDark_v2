using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
	    using UnityEditor;
#endif

namespace Darklight.World.Generation.Entity.Spawner
{
    public class EntitySpawner : AsyncTaskQueen, ITaskQueen
    {
        public static EntitySpawner Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) { Destroy(this); }
            else { Instance = this; }
        }
        WorldBuilder _worldBuilder => WorldBuilder.Instance;
        Dictionary<Vector2Int, RegionBuilder> _regionMap => _worldBuilder.RegionMap;
        Dictionary<Vector2Int, List<Zone>> _regionZoneMap = new();

        public GameObject travelerPrefab;

        public bool initializeOnStart;

        private void Start()
        {
            if (initializeOnStart == true) { Initialize(); }
        }

        public override void Initialize(string name = "SpawnerAsyncTaskQueen")
        {
            base.Initialize(name);
            _ = InitializeAsync();
        }

        async Task InitializeAsync()
        {
            asyncTaskConsole.Log(this, $"Waiting for WorldBuilder to be Initialized => status : {_worldBuilder.Initialized}");
            while (_worldBuilder.Initialized == false)
            {
                await Task.Yield();
            }

            asyncTaskConsole.Log(this, $"WorldBuilder Initialized => status : {_worldBuilder.Initialized}");

            base.NewTaskBot("Spawn Player", async () =>
            {
                SpawnTravelerInRandomValidZone(travelerPrefab);
                await Task.Yield();

            });
            await ExecuteAllBotsInQueue();
            await Task.Yield();

        }

	    public void SpawnTravelerInRandomValidZone(GameObject travelerPrefab)
        {
            foreach (RegionBuilder region in _regionMap.Values)
            {
                if (region.CoordinateMap.Zones.Count > 0)
                {
                    Coordinate spawnCoordinate = region.CoordinateMap.Zones[0].CenterCoordinate;
                    Chunk spawnChunk = region.ChunkGeneration.GetChunkAt(spawnCoordinate);


                    GameObject travelerObject = Instantiate(travelerPrefab, spawnChunk.GroundPosition, Quaternion.identity);
                    travelerObject.transform.parent = transform;
                    Traveler traveler = travelerObject.GetComponent<Traveler>();
                    traveler.InitializeAt(region, spawnChunk);


                    Debug.Log("Spawning entity at " + spawnCoordinate.ValueKey.ToString());
                    return;
                }
            }
        }

    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(EntitySpawner))]
    public class EntitySpawnerEditor : AsyncTaskQueen.AsyncTaskQueenEditor
    {
        private EntitySpawner _entitySpawner;
        private void OnEnable()
        {
            _entitySpawner = (EntitySpawner)target;
        }

        public override void OnInspectorGUI()
        {

			DrawDefaultInspector();

            base.OnInspectorGUI();

        }
    }
    #endif
}

