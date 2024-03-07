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


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


