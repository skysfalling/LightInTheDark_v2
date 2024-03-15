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
	public class Traveler : Game.Movement.Player8DirMovement
	{
		public CoordinateMap coordinateMap;
		public bool Active = false;
		public RegionBuilder ParentRegion { get; private set; }
		public Chunk CurrentChunk { get; private set; }
		public CoordinateMap CurrentCoordinateMap { get; private set; }
		public Coordinate CurrentCoordinate { get; private set; }
		[SerializeField] public WorldSpawnUnit worldSpawnUnit;

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
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Traveler))]
	public class TravelerEditor : UnityEditor.Editor
	{
		Traveler _travelerScript;
		WorldSpawnUnit _worldSpawnUnit;


		void OnEnable()
		{
			_travelerScript = (Traveler)target;
			_worldSpawnUnit = _travelerScript.worldSpawnUnit;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			DrawDefaultInspector();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical();
			UnityEditor.Editor editor = CreateEditor(_worldSpawnUnit.modelPrefab);
			editor.OnInspectorGUI();

			EditorGUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);
				Repaint();
			}

		}
	}
#endif
}