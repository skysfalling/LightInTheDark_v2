using System;
using System.Collections;
using System.Linq;
using Darklight.Bot;
using Darklight.World.Builder;
using Darklight.World.Map;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Darklight.World.Generation
{
	public class Region : IGridMapData
	{
		public GridMap2D gridMapParent { get; set; }
		public Vector2Int positionKey { get; set; }
		public bool isInitialized { get; private set; }
		public RegionBuilder builder { get; private set; }
		public string Prefix => "[ REGION ]";
		public WorldGenerationSystem WorldGen => WorldGenerationSystem.Instance;
		public Region() { }
		public Region(GridMap2D<Region> parent, Vector2Int positionKey)
		{
			Initialize(parent, positionKey);
		}
		public void Initialize(GridMap2D parent, Vector2Int positionKey)
		{
			this.gridMapParent = parent;
			this.positionKey = positionKey;
			isInitialized = true;

			//Debug.Log($"{Prefix} Initialized at {positionKey}");
		}

		public void InstantiateBuilder()
		{

			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject.name = "Region";
			gameObject.transform.parent = WorldGen.transform;

			GridMap2D.Coordinate coordinate = gridMapParent.FullMap[positionKey];
			gameObject.transform.position = coordinate.GetPositionInScene();

			this.builder = gameObject.AddComponent<RegionBuilder>();
		}


	}
}

