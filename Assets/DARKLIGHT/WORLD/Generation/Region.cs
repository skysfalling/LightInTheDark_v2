using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Bot;
using Darklight.World.Builder;
using Darklight.World.Map;
using UnityEngine;

namespace Darklight.World.Generation
{
	using WorldGenSystem = WorldGenerationSystem;
	public class Region : IGridMapData
	{
		public GridMap2D gridMapParent { get; set; }
		public Vector2Int positionKey { get; set; }
		public GridMap2D.Coordinate coordinateValue { get; set; }
		public bool isInitialized { get; private set; }
		public Builder.RegionBuilder builder { get; private set; }
		public string prefix => "[ REGION ]";
		public Region() { }
		public Region(GridMap2D<Region> parent, Vector2Int positionKey)
		{
			_ = Initialize(parent, positionKey);
		}
		public Task Initialize(GridMap2D parent, Vector2Int positionKey)
		{
			this.gridMapParent = parent;
			this.positionKey = positionKey;
			this.coordinateValue = gridMapParent.GetCoordinateAt(positionKey);
			isInitialized = true;
			return Task.CompletedTask;
		}
	}
}

