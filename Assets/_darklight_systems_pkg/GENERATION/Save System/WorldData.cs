using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation.Data
{
    [System.Serializable]
    public class WorldData
    {
        public GenerationSettings settings;

        // Default constructor for serialization/deserialization
        public WorldData() { }

        // Constructor used when creating new WorldData
        public WorldData(WorldGeneration worldGeneration)
        {
            settings = WorldGeneration.Settings;
        }
    }
}

