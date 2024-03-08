using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.World.Generation.Entity.Spawner
{
    public class MainSpawner : MonoBehaviour
    {
        public static MainSpawner Instance { get; private set; }
        private void Awake() 
        {
            if (Instance != null) { Destroy(this); }
            else { Instance = this; }
        }
        WorldBuilder _worldGeneration => WorldBuilder.Instance;
        Dictionary<Vector2Int, Region> _regionMap => _worldGeneration.RegionMap;
        Dictionary<Vector2Int, List<Zone>> _regionZoneMap = new();

        // [[ PUBLIC ACCESS VARIABLES ]] ===== >>
        

        // [[ PUBLIC INSPECTOR VARIABLES ]] ===== >>
        [Range(0, 1)] public float tickSpeed = 0.5f;

        public void SpawnEntityInRandomValidZone(GameObject entityPrefab)
        {
            foreach(Region region in _regionMap.Values){
                if (region.CoordinateMap.Zones.Count > 0)
                {
                    Coordinate spawnCoordinate = region.CoordinateMap.Zones[0].CenterCoordinate;
                    Chunk spawnChunk = region.ChunkMap.GetChunkAt(spawnCoordinate);

                    CreateNewEntity("testEntity", entityPrefab, region, spawnChunk);

                    Debug.Log("Spawning entity at " + spawnCoordinate.ValueKey.ToString());
                    return;
                }
            }
        }

        public BaseEntity CreateNewEntity(string name, GameObject modelPrefab, Region regionParent, Chunk chunk )
        {
            GameObject entityObject = new GameObject($"_entity({name})");
            entityObject.transform.parent = this.transform;
            BaseEntity newEntity = entityObject.AddComponent<BaseEntity>();
            newEntity.Initialize(name, modelPrefab, regionParent, chunk);
            return newEntity;
        }
    }
}


