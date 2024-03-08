using Darklight.ThirdDimensional.Generation;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation
{
    public class EntitySpawnHandler : MonoBehaviour
    {
        WorldGeneration _worldGeneration => WorldGeneration.Instance;
        Dictionary<Vector2Int, Region> _regionMap => _worldGeneration.RegionMap;
        Dictionary<Vector2Int, List<Zone>> _regionZoneMap = new();




        // [[ PUBLIC ACCESS VARIABLES ]] ===== >>

        // [[ PUBLIC INSPECTOR VARIABLES ]] ===== >>
        [Range(0, 1)] public float tickSpeed = 0.5f;
        public GameObject entityPrefab;

        [EasyButtons.Button]
        public void SpawnEntity(){

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

        public Entity CreateNewEntity(string name, GameObject modelPrefab, Region regionParent, Chunk chunk )
        {
            GameObject entityObject = new GameObject($"_entity({name})");
            entityObject.transform.parent = this.transform;
            Entity newEntity = entityObject.AddComponent<Entity>();
            newEntity.Initialize(name, modelPrefab, regionParent, chunk);
            return newEntity;
        }
    }
}


