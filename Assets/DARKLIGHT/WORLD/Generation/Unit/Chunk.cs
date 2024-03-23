using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Darklight;
using Darklight.Bot;
using Darklight.World.Generation;
using Darklight.World.Generation.System;
using Darklight.World.Generation.Unit;
using Darklight.World.Map;
using Darklight.World.Settings;
using static Darklight.World.Map.GridMap2D;

using UnityEngine;
using UnityEngine.UIElements;
using Darklight.UnityExt;
namespace Darklight.World.Generation.Unit
{
	public class Chunk : IGridMapData<Chunk>
	{
		/// <summary>
		/// Defines World Chunks based on wall count / location
		/// </summary>
		public enum TYPE
		{
			/// <summary>No walls present.</summary>
			EMPTY,
			/// <summary>One sidewall present.</summary>
			WALL,
			/// <summary>Two parallel walls present, forming a hallway.</summary>
			HALLWAY,
			/// <summary>Two perpendicular walls present, forming a corner.</summary>
			CORNER,
			/// <summary>Three walls present, forming a dead end.</summary>
			DEAD_END,
			/// <summary>Enclosed by walls on all four sides.</summary>
			CLOSED,
			/// <summary>Indicates a boundary limit, set by WorldCoordinateMap.</summary>
			BORDER,
			/// <summary>Indicates an exit point, set by WorldExit.</summary>
			EXIT
		}
		public enum FaceDirection { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM, SPLIT }

		// [[ PRIVATE VARIABLES ]]
		CellMap _cellMap;
		TYPE _type;
		int _groundHeight = 0;

		// [[ PUBLIC ACCESS VARIABLES ]] 
		public bool Initialized { get; private set; } = false;
		public WorldGenerationSystem WorldGenSys => WorldGenerationSystem.Instance;
		public GenerationSettings Settings => WorldGenSys.Settings;
		public int Width => WorldGenSys.Settings.ChunkWidth_inGameUnits;
		public ChunkGenerationSystem GenerationSys { get; private set; }
		public GameObject ChunkObject;
		public ChunkMesh ChunkMesh { get; private set; }
		public CellMap CellMap => _cellMap;
		public int GroundHeight => _groundHeight;
		public TYPE Type => _type;
		public Color TypeColor { get; private set; } = Color.white;
		public Vector2Int PositionKey { get; set; }
		public Coordinate CoordinateValue { get; set; }
		public GridMap2D<Chunk> GridMapParent { get; set; }
		public Vector3 CenterPosition => CoordinateValue.GetPositionInScene();
		public Vector3 OriginPosition
		{
			get
			{
				Vector3 origin = CenterPosition;
				origin -= WorldGenSys.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				origin += WorldGenSys.Settings.CellSize_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				return origin;
			}
		}
		public Vector3 GroundPosition
		{
			get
			{
				Vector3 groundPosition = CenterPosition;
				groundPosition += GroundHeight * WorldGenSys.Settings.CellSize_inGameUnits * Vector3Int.up;
				return groundPosition;
			}
		}
		public Vector3 ChunkMeshDimensions => WorldGenSys.Settings.ChunkVec3Dimensions_inCellUnits + new Vector3Int(0, GroundHeight, 0);

		public Task Initialize(GridMap2D<Chunk> parent, Vector2Int positionKey)
		{
			this.GridMapParent = parent;
			this.PositionKey = positionKey;
			this.CoordinateValue = GridMapParent.GetCoordinateAt(positionKey);
			Initialized = true;
			return Task.CompletedTask;
		}

		public ChunkMesh CreateChunkMesh()
		{
			UpdateChunkHeight();

			// Create chunkMesh
			ChunkMesh = new ChunkMesh(this);
			_cellMap = new CellMap(this, ChunkMesh);

			//DetermineChunkType();

			return ChunkMesh;
		}

		public void SetChunkHeight(int height)
		{
			this._groundHeight = height;
		}

		public void UpdateChunkHeight()
		{
			Coordinate.Flag type = CoordinateValue.CurrentFlag;
			Vector2Int perlinOffset = new Vector2Int((int)CenterPosition.x, (int)CenterPosition.z);
			this._groundHeight = Mathf.RoundToInt(PerlinNoise.CalculateHeightFromNoise(perlinOffset) * WorldGenSys.Settings.PerlinMultiplier);
			this._groundHeight = Mathf.Clamp(this._groundHeight, 0, WorldGenSys.Settings.ChunkMaxHeight_inCellUnits);
			this._groundHeight *= WorldGenSys.Settings.CellSize_inGameUnits; // Convert to game units

			// Set Zone Chunks to same height as centerE
			/*
			if (type == Coordinate.Flag.ZONE)
			{
				this._groundHeight = 0;
				Zone coordinateZone = _coordinateMap.GetZoneFromCoordinate(_coordinate.ValueKey);
				if (coordinateZone != null && coordinateZone.CenterCoordinate.ValueKey != _coordinate.ValueKey)
				{
					// Try to find center chunk of zone and set height to match
					Chunk centerChunk = ChunkBuilderParent.GetChunkAt(coordinateZone.CenterCoordinate);
					if (centerChunk != null)
					{
						this._groundHeight = centerChunk.GroundHeight;
					}
				}
			}
			*/
		}
	}
}


public class ChunkMonoOperator : MonoBehaviour, IUnityEditorListener
{
	public WorldGenerationSystem WorldGen => WorldGenerationSystem.Instance;
	public Chunk Chunk { get; private set; }
	public GridMap2D<Chunk> ParentGridMap { get; private set; }

	public void OnEditorReloaded()
	{
		Destroy(this.gameObject);
	}
}


//

/*
		public Chunk(ChunkBuilder chunkGeneration, Coordinate coordinate)
		{
			this.ChunkBuilderParent = chunkGeneration;
			this._coordinate = coordinate;

			// Create coordinate map
			this._coordinateMap = new CoordinateMap(this);
			_ = this._coordinateMap.InitializeDefaultMap();

			UpdateChunkHeight();
		}
		*/
/*
		

public void DetermineChunkType()
{
	// [[ ITERATE THROUGH CHUNK NEIGHBORS ]] 
	Dictionary<Direction, Chunk> naturalNeighborMap = GetNaturalNeighborMap();
	foreach (Direction direction in naturalNeighborMap.Keys.ToList())
	{
		Chunk neighborChunk = naturalNeighborMap[direction];
		EdgeDirection? neighborBorder = CoordinateMap.GetBorderDirection(direction); // get chunk border

		// << CLOSE BORDER WITH NULL NEIGHBOR OR NEIGHBOR WITH HEIGHT OFFSET >>
		if (neighborChunk == null || neighborChunk.GroundHeight != GroundHeight)
		{
			if (neighborBorder == null) continue;

			CoordinateMap.CloseMapBorder((EdgeDirection)neighborBorder); // close the chunk border
		}
	}

	// ========================================================

	// [[ DETERMINE TYPE FROM BORDERS ]]
	Dictionary<EdgeDirection, bool> activeBorderMap = CoordinateMap.ActiveBorderMap;

	// Count active borders directly from the dictionary
	int activeBorderCount = activeBorderMap.Count(kv => kv.Value == true);

	// Determine type based on active edge count and their positions
	switch (activeBorderCount)
	{
		case 4:
			SetType(TYPE.CLOSED); break;
		case 3:
			SetType(TYPE.DEAD_END); break;
		case 2:
			// Check for parallel edges
			if (activeBorderMap[EdgeDirection.NORTH] && activeBorderMap[EdgeDirection.SOUTH])
			{ SetType(TYPE.HALLWAY); break; }
			if (activeBorderMap[EdgeDirection.EAST] && activeBorderMap[EdgeDirection.WEST])
			{ SetType(TYPE.HALLWAY); break; }

			// Otherwise chunk is in corner
			SetType(TYPE.CORNER); break;
		case 1:
			SetType(TYPE.WALL); break;
		case 0:
			SetType(TYPE.EMPTY); break;
	}
}

public void SetType(TYPE newType)
{
	_type = newType;
	switch (newType)
	{
		case TYPE.CLOSED: TypeColor = Color.black; break;
		case TYPE.DEAD_END: TypeColor = Color.red; break;
		case TYPE.HALLWAY: TypeColor = Color.yellow; break;
		case TYPE.CORNER: TypeColor = Color.blue; break;
		case TYPE.WALL: TypeColor = Color.green; break;
		case TYPE.EMPTY: TypeColor = Color.white; break;
	}
}

public Dictionary<Direction, Chunk> GetNaturalNeighborMap()
{
	Dictionary<Direction, Chunk> neighborMap = new Dictionary<Direction, Chunk>();

	List<Direction> naturalNeighborDirections = new List<Direction> { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };
	foreach (Direction direction in naturalNeighborDirections)
	{
		Vector2Int neighborCoordinateValue = CoordinateMap.CalculateNeighborCoordinateValue(Coordinate.ValueKey, direction);
		neighborMap[direction] = ChunkBuilderParent.GetChunkAt(neighborCoordinateValue);
	}

	return neighborMap;
}

public Chunk GetNeighborInDirection(Direction direction)
{
	return GetNaturalNeighborMap()[direction];
}

public Coordinate GetCoordinateAtCell(Cell cell)
{
	return _coordinateMap.GetClosestCoordinateAt(cell.Position);
}

public Cell GetCellAtCoordinate(Coordinate coordinate)
{
	return _cellMap.GetCellAtCoordinate(coordinate);
}
				*/