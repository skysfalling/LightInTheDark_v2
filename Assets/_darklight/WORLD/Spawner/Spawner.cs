using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.World.Generation.Entity.Spawner
{
    public class EntitySpawner : EventTaskQueen
    {
        public static EntitySpawner Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) { Destroy(this); }
            else { Instance = this; }
        }
        WorldBuilder _worldBuilder => WorldBuilder.Instance;
        Dictionary<Vector2Int, Region> _regionMap => _worldBuilder.RegionMap;
        Dictionary<Vector2Int, List<Zone>> _regionZoneMap = new();

        // [[ PUBLIC ACCESS VARIABLES ]] ===== >>


        // [[ PUBLIC INSPECTOR VARIABLES ]] ===== >>
        [Range(0, 1)] public float tickSpeed = 0.5f;

        [Header("Entity Library")]
        public GameObject playerPrefab;
        public GameObject testEntityPrefab;

        void Start()
        {
            _ = InitializeAsync();
        }

        async Task InitializeAsync()
        {
            while (_worldBuilder.Initialized == false)
            {
                await Task.Yield();
            }

            await Task.Delay(1000);


            base.ExecuteAllBotsInQueue();
            await Task.Yield();
        }

        public void SpawnTravelerInRandomValidZone(GameObject travelerPrefab)
        {
            foreach (Region region in _regionMap.Values)
            {
                if (region.CoordinateMap.Zones.Count > 0)
                {
                    Coordinate spawnCoordinate = region.CoordinateMap.Zones[0].CenterCoordinate;
                    Chunk spawnChunk = region.ChunkMap.GetChunkAt(spawnCoordinate);


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
}


