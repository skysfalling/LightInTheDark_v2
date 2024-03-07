using Darklight.ThirdDimensional.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class WorldSpawnSystem : MonoBehaviour
    {
        public WorldGeneration GenerationParent => GetComponent<WorldGeneration>();

        public GameObject playerPrefab;

        [EasyButtons.Button]
        public void SpawnPlayer(){

            foreach(Region region in GenerationParent.AllRegions){
                if (region.CoordinateMap.Zones.Count > 0)
                {
                    Coordinate spawnCoordinate = region.CoordinateMap.Zones[0].CenterCoordinate;

                    GameObject.Instantiate(playerPrefab, spawnCoordinate.ScenePosition, Quaternion.identity);

                    Debug.Log("Spawning player at " + spawnCoordinate.Value.ToString());
                    return;
                }
            }
        }


    }
}


