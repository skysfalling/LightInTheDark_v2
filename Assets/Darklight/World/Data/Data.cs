using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.World.Data
{
	using Settings;
	using Builder;

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

