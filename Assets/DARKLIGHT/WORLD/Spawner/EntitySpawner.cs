using System.Collections.Generic;
using UnityEngine;
using Darklight.World.Builder;
using Darklight.World.Generation;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Spawner
{
    public class EntitySpawner
    {
        public static EntitySpawner Instance { get; private set; }
        WorldBuilder _worldBuilder => WorldBuilder.Instance;
        Dictionary<Vector2Int, RegionBuilder> _regionMap => _worldBuilder.RegionMap;
        Dictionary<Vector2Int, List<Zone>> _regionZoneMap = new();

        public GameObject travelerPrefab;

        public bool initializeOnStart;

        /*

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
                */

    }
}

