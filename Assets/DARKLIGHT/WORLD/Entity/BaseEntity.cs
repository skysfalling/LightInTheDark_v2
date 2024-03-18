using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.World.Entity
{
	using Generation;
	using Builder;
	using Map;

	public class BaseEntity : MonoBehaviour
	{
		public bool active;
		public Path currentPath;
		public RegionBuilder regionParent;
		public ChunkData currentChunk;
		public ChunkData targetChunk;
		public Coordinate currentCoordinate;
		public GameObject modelObject;

		public void Initialize(string name, GameObject modelPrefab, RegionBuilder region, ChunkData chunk)
		{
			this.gameObject.name = $"_entity({name})";
			regionParent = region;
			currentChunk = chunk;
			currentCoordinate = chunk.Coordinate;

			// create model as child transform
			modelObject = Instantiate(modelPrefab, transform);
			modelObject.SetActive(true);
			active = true;

			// set position to ground position of current chunk	
			this.transform.position = currentChunk.GroundPosition;

			currentChunk = DetermineNewTargetChunk();
			currentPath = new Path(regionParent.CoordinateMap, currentChunk.Coordinate.ValueKey, targetChunk.Coordinate.ValueKey, new List<Coordinate.TYPE>() { Coordinate.TYPE.NULL });
		}

		public ChunkData DetermineNewTargetChunk()
		{
			Vector2Int randomCoordinateValue = regionParent.CoordinateMap.GetRandomCoordinateValueOfType(Coordinate.TYPE.NULL);
			ChunkData randomChunk = regionParent.ChunkBuilder.GetChunkAt(randomCoordinateValue);
			return randomChunk;
		}
	}
}