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

	[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Game.Movement.Player8DirMovement))]
	public class Traveler : MonoBehaviour
	{
		public CoordinateMap coordinateMap;
		public bool Active = false;
		public RegionBuilder CurrentRegion { get; private set; }
		public Chunk CurrentChunk { get; private set; }
		public CoordinateMap CurrentCoordinateMap { get; private set; }
		public Coordinate CurrentCoordinate { get; private set; }
		public Cell CurrentCell { get; private set; }
		[SerializeField] public CoordinateMap coordinateMapParent;
		[SerializeField] public WorldSpawnUnit worldSpawnUnit;

		public Vector3 OriginPosition => transform.position;
		public Vector3 CenterPosition => transform.position + (Vector3Int.up * worldSpawnUnit.dimensions * WorldBuilder.Settings.CellSize_inGameUnits) / 2;

		// [[ INSPECTOR VARIABLES ]]
		public void InitializeAtChunk(RegionBuilder region, Chunk chunk)
		{
			CurrentRegion = region;
			CurrentChunk = chunk;
			CurrentCoordinateMap = region.CoordinateMap;
			CurrentCoordinate = chunk.Coordinate;
			Active = true;

			SpawnModelAtPosition();
		}

		public void InitializeAtCell(Chunk chunk, Cell cell)
		{
			CurrentChunk = cell.ChunkParent;
			CurrentRegion = CurrentChunk.ChunkBuilderParent.RegionParent;

			CurrentCoordinateMap = CurrentChunk.CoordinateMap;
			CurrentCell = cell;

			Debug.Log($"Traveler Initialized at Cell: {cell.Position} {cell.FaceType}");
			Active = true;

			SpawnModelAtPosition();
		}

		public void InitializeAtCoordinate(CoordinateMap map, Vector2Int value)
		{
			Debug.Log("Traveler Initialized at Coordinate: " + value);
			CurrentCoordinateMap = map;
			CurrentCoordinate = map.GetCoordinateAt(value);
			transform.position = CurrentCoordinate.ScenePosition;
		}

		public void SpawnModelAtPosition()
		{
			GameObject model = Instantiate(worldSpawnUnit.modelPrefab, transform);
			model.transform.position = CenterPosition;
			model.transform.localScale = Vector3.one * worldSpawnUnit.modelScale;
		}

		public void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(OriginPosition, worldSpawnUnit.dimensions * WorldBuilder.Settings.CellSize_inGameUnits);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Traveler))]
	public class TravelerEditor : UnityEditor.Editor
	{
		SerializedProperty worldSpawnUnitProp;
		UnityEditor.Editor worldSpawnUnitEditor;

		void OnEnable()
		{
			// Use serialized properties for better handling of undo, prefab overrides, etc.
			worldSpawnUnitProp = serializedObject.FindProperty("worldSpawnUnit");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			// Display the default inspector view, which now includes the WorldSpawnUnit field automatically.
			DrawDefaultInspector();

			DrawWorldSpawnUnit();

			if (GUILayout.Button("Spawn Model"))
			{
				((Traveler)target).SpawnModelAtPosition();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void OnDisable()
		{
			// Clean up to avoid memory leaks.
			if (worldSpawnUnitEditor != null) DestroyImmediate(worldSpawnUnitEditor);
		}

		private void DrawWorldSpawnUnit()
		{
			// Only create a new editor if the property has a reference and it has changed.
			if (worldSpawnUnitProp.objectReferenceValue != null &&
				(worldSpawnUnitEditor == null || worldSpawnUnitEditor.target != worldSpawnUnitProp.objectReferenceValue))
			{
				// Clean up the previous editor if it exists.
				if (worldSpawnUnitEditor != null) DestroyImmediate(worldSpawnUnitEditor);
				worldSpawnUnitEditor = CreateEditor(worldSpawnUnitProp.objectReferenceValue);
			}

			// If the editor was successfully created, draw it.
			if (worldSpawnUnitEditor != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("World Spawn Unit Editor", EditorStyles.boldLabel);
				worldSpawnUnitEditor.OnInspectorGUI();
			}
		}
	}
#endif
}