using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.World.Generation.Data
{
    [System.Serializable]
    public class WorldData
    {
        public GenerationSettings settings;

        // Default constructor for serialization/deserialization
        public WorldData() { }

        // Constructor used when creating new WorldData
        public WorldData(WorldBuilder worldGeneration)
        {
            settings = WorldBuilder.Settings;
        }
    }
}

