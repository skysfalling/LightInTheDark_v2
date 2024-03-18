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
	using Darklight.Game.Movement;
	using Darklight.UniversalInput;
	using Darklight.World.Map;
	using static UnityEngine.InputSystem.InputAction;

	[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Player8DirMovement))]
	public class Traveler : MonoBehaviour
	{
		private GameObject _modelObject;
		public CoordinateMap coordinateMap;
		public Player8DirMovement playerMovement;
		public bool Active = false;
		public RegionBuilder CurrentRegion { get; private set; }
		public ChunkData CurrentChunk { get; private set; }
		public CoordinateMap CurrentCoordinateMap { get; private set; }
		public Coordinate CurrentCoordinate { get; private set; }
		public Cell CurrentCell { get; private set; }
		[SerializeField] public CoordinateMap coordinateMapParent;
		[SerializeField] public WorldSpawnUnit worldSpawnUnit;

		public Vector3 OriginPosition => transform.position;
		public Vector3 CenterPosition => transform.position + (Vector3Int.up * worldSpawnUnit.dimensions * WorldBuilder.Settings.CellSize_inGameUnits);

		private WorldDirection? _continuousMoveDirection = null;
		private float _inputHoldDuration = 0f;
		private bool _isContinuouslyMoving = false;
		private float _continuousMoveThreshold = 0.1f; // Time in seconds to trigger continuous movement
		private float _continuousMoveInterval = 0.1f; // Time interval between continuous moves
		private float _timeSinceLastMove = 0f;


		private void Awake()
		{
			playerMovement = GetComponent<Game.Movement.Player8DirMovement>();
			if (UniversalInputManager.Instance != null)
			{
				UniversalInputManager.Instance.moveInput.performed += context => HandleMoveInput(context.ReadValue<Vector2>());
				UniversalInputManager.Instance.moveInput.canceled += context => HandleMoveInput(Vector2.zero);

			}
		}
		private void OnDestroy()
		{
			if (UniversalInputManager.Instance != null)
			{
				UniversalInputManager.Instance.moveInputEvent.RemoveListener(HandleMoveInput);
			}
		}
		// [[ INSPECTOR VARIABLES ]]
		public void InitializeAtCell(Cell cell)
		{
			CurrentChunk = cell.ChunkParent;
			CurrentRegion = CurrentChunk.ChunkBuilderParent.RegionParent;
			CurrentCoordinate = CurrentChunk.GetCoordinateAtCell(cell);
			CurrentCoordinateMap = CurrentChunk.CoordinateMap;
			CurrentCell = cell;

			Debug.Log($"Traveler Initialized at Cell: {cell.Position} {cell.FaceType}");
			Debug.Log($"Traveler Initialized at Coordinate: {CurrentCoordinate.ValueKey}");

			Active = true;

			SpawnModelAtPosition();
		}

		private void Update()
		{
			if (_continuousMoveDirection.HasValue)
			{
				_inputHoldDuration += Time.deltaTime;
				_timeSinceLastMove += Time.deltaTime;

				if (_inputHoldDuration >= _continuousMoveThreshold && !_isContinuouslyMoving)
				{
					_isContinuouslyMoving = true;
					_timeSinceLastMove = 0f; // Reset to immediately trigger the first continuous move
				}

				if (_isContinuouslyMoving && _timeSinceLastMove >= _continuousMoveInterval)
				{
					MoveToDirection(_continuousMoveDirection.Value);
					_timeSinceLastMove = 0f;
				}
			}
		}

		private void HandleMoveInput(Vector2 input)
		{
			if (!Active || CurrentCoordinate == null) return;
			if (input == Vector2.zero)
			{
				_continuousMoveDirection = null;
				_isContinuouslyMoving = false;
				_inputHoldDuration = 0f;
				return;
			}

			// Convert the Vector2 input to a WorldDirection
			WorldDirection? direction = CoordinateMap.GetEnumFromDirectionVector(new Vector2Int((int)input.x, (int)input.y));
			if (direction == null) return;
			if (direction.HasValue && (_continuousMoveDirection != direction || !_isContinuouslyMoving))
			{
				_continuousMoveDirection = direction;
				_isContinuouslyMoving = false;
				_inputHoldDuration = 0f;
				MoveToDirection(direction.Value);
			}
		}
		void MoveToDirection(WorldDirection direction)
		{
			bool validCellFound = MoveToCellInDirection(direction);
			if (!validCellFound)
			{
				MoveToChunkInDirection(direction);
			}
		}
		public bool MoveToChunkInDirection(WorldDirection worldDirection)
		{
			ChunkData neighborChunk = CurrentChunk.GetNeighborInDirection(worldDirection);
			if (neighborChunk == null) return false;

			BorderDirection? borderDirection = CoordinateMap.GetBorderDirection(worldDirection);
			if (borderDirection == null) return false;

			BorderDirection? neighborBorderDirection = CoordinateMap.GetOppositeBorder(borderDirection.Value);
			if (neighborBorderDirection == null) return false;

			Coordinate neighborBorderCoordinate = null;
			switch (neighborBorderDirection)
			{
				case BorderDirection.SOUTH:
					neighborBorderCoordinate = neighborChunk.CoordinateMap.GetCoordinateAt(new Vector2Int(CurrentCoordinate.ValueKey.x, 0));
					break;
				case BorderDirection.NORTH:
					neighborBorderCoordinate = neighborChunk.CoordinateMap.GetCoordinateAt(new Vector2Int(CurrentCoordinate.ValueKey.x, neighborChunk.CoordinateMap.MaxCoordinateValue - 1));
					break;
				case BorderDirection.WEST:
					neighborBorderCoordinate = neighborChunk.CoordinateMap.GetCoordinateAt(new Vector2Int(0, CurrentCoordinate.ValueKey.y));
					break;
				case BorderDirection.EAST:
					neighborBorderCoordinate = neighborChunk.CoordinateMap.GetCoordinateAt(new Vector2Int(neighborChunk.CoordinateMap.MaxCoordinateValue - 1, CurrentCoordinate.ValueKey.y));
					break;
			}
			if (neighborBorderCoordinate == null) return false;
			Cell cell = neighborChunk.GetCellAtCoordinate(neighborBorderCoordinate);

			if (cell == null) return false;
			Debug.Log("Move to Chunk in Direction: " + worldDirection + " Neighbor Border: " + neighborBorderDirection.Value);

			return MoveToCell(cell);
		}



		public bool MoveToCellInDirection(WorldDirection worldDirection)
		{
			Coordinate coordinateInDirection = CurrentCoordinate.GetNeighborInDirection(worldDirection);
			if (coordinateInDirection == null)
			{
				//Debug.LogError("Traveler has no current coordinate");
				return false;
			}

			Cell cellInDirection = CurrentChunk.GetCellAtCoordinate(coordinateInDirection);
			return MoveToCell(cellInDirection);
		}

		public bool MoveToCell(Cell cell)
		{
			if (cell == null)
			{
				Debug.LogError("Traveler has no current cell");
				return false;
			}

			CurrentCell = cell;
			CurrentCoordinate = cell.Coordinate;
			CurrentCoordinateMap = cell.Coordinate.ParentMap;
			CurrentChunk = cell.ChunkParent;
			CurrentRegion = CurrentChunk.ChunkBuilderParent.RegionParent;

			//Debug.Log("Traveler Moved to Coordinate: " + CurrentCoordinate.ValueKey);

			playerMovement.targetPosition = cell.Position;
			return true;
		}



		public void InitializeAtCoordinate(CoordinateMap map, Vector2Int value)
		{
			Debug.Log("Traveler Initialized at Coordinate: " + value);
			CurrentCoordinateMap = map;
			CurrentCoordinate = map.GetCoordinateAt(value);
		}

		public void SpawnModelAtPosition()
		{
			_modelObject = Instantiate(worldSpawnUnit.modelPrefab, transform);
			_modelObject.transform.position = CenterPosition;
			_modelObject.transform.localScale = Vector3.one * worldSpawnUnit.modelScale * WorldBuilder.Settings.CellSize_inGameUnits;
		}




		public void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(CenterPosition, worldSpawnUnit.dimensions * WorldBuilder.Settings.CellSize_inGameUnits);

			if (CurrentCell != null)
			{
				CustomGizmos.DrawFilledSquareAt(CurrentCell.Position, WorldBuilder.Settings.CellSize_inGameUnits, Vector3.up, Color.red);

				foreach (Coordinate neighbor in CurrentCoordinate.GetValidNaturalNeighbors())
				{
					Handles.color = Color.black;
					Cell cellAtNeighbor = CurrentChunk.GetCellAtCoordinate(neighbor);
					Handles.DrawWireCube(cellAtNeighbor.Position, Vector3.one * 0.5f);
					CustomGizmos.DrawLabel(CurrentCoordinate.GetWorldDirectionOfNeighbor(neighbor).ToString(), neighbor.ScenePosition, CustomInspectorGUI.CenteredStyle);
				}

				foreach (Coordinate coordinate in CurrentChunk.CoordinateMap.AllCoordinates)
				{
					if (coordinate == null) continue;
					Cell cell = CurrentChunk.GetCellAtCoordinate(coordinate);
					if (cell == null) continue;
					if (coordinate == CurrentCoordinate) continue;
					CustomGizmos.DrawWireSquare(cell.Position, WorldBuilder.Settings.CellSize_inGameUnits, Color.black, Vector3.up);
				}
			}
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

			DrawWorldSpawnUnitInspector();

			if (GUILayout.Button("Spawn Model"))
			{
				((Traveler)target).SpawnModelAtPosition();
			}

			serializedObject.ApplyModifiedProperties();
		}

		public void OnSceneGUI()
		{
			Traveler traveler = (Traveler)target;


		}

		private void OnDisable()
		{
			// Clean up to avoid memory leaks.
			if (worldSpawnUnitEditor != null) DestroyImmediate(worldSpawnUnitEditor);
		}

		private void DrawWorldSpawnUnitInspector()
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