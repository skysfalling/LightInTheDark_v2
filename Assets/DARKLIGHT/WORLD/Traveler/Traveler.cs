using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// This is to be used as a default accessor to the world generation values

namespace Darklight.World.Generation
{
	using Builder;
	using Darklight.World.Map;

	[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
	public class Traveler : Darklight.Game.Movement.Player8DirMovement
	{
		public CoordinateMap coordinateMap;
		public bool Active = false;
		public RegionBuilder ParentRegion { get; private set; }
		public Chunk CurrentChunk { get; private set; }
		public CoordinateMap CurrentCoordinateMap { get; private set; }
		public Coordinate CurrentCoordinate { get; private set; }

		// [[ INSPECTOR VARIABLES ]]
		public void InitializeAtChunk(RegionBuilder region, Chunk chunk)
		{
			ParentRegion = region;
			CurrentChunk = chunk;
			CurrentCoordinateMap = region.CoordinateMap;
			CurrentCoordinate = chunk.Coordinate;
			Active = true;
		}

		public void InitializeAtCoordinate(CoordinateMap map, Vector2Int value)
		{
			Debug.Log("Traveler Initialized at Coordinate: " + value);
			CurrentCoordinateMap = map;
			CurrentCoordinate = map.GetCoordinateAt(value);
			transform.position = CurrentCoordinate.ScenePosition;
		}

		private void OnDrawGizmos()
		{
			if (Active)
			{
				UnityEngine.Gizmos.color = Color.red;
				UnityEngine.Gizmos.DrawWireSphere(CurrentChunk.Coordinate.ScenePosition, 5f);

				foreach (Coordinate neighbor in CurrentChunk.Coordinate.GetAllValidNeighbors())
				{
					UnityEngine.Gizmos.color = Color.green;
					UnityEngine.Gizmos.DrawWireSphere(neighbor.ScenePosition, 5f);
				}
			}
		}

		public static void DestroyGameObject(GameObject gameObject)
		{
			// Check if we are running in the Unity Editor
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
			{
				// Use DestroyImmediate if in edit mode and not playing
				DestroyImmediate(gameObject);
				return;
			}
			else
#endif
			{
				// Use Destroy in play mode or in a build
				Destroy(gameObject);
			}
		}
	}
}