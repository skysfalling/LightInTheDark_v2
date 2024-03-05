using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Data
{
    [System.Serializable]
    public class WorldData
    {
        public WorldGeneration.GenerationSettings settings;

        // Default constructor for serialization/deserialization
        public WorldData() { }

        // Constructor used when creating new WorldData
        public WorldData(WorldGeneration worldGeneration)
        {
            settings = WorldGeneration.Settings;
        }
    }
}

