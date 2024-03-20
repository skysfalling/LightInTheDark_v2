using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Darklight.World.Generation
{
	using Map;
	using Builder;
	public class Coordinate
	{
		public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }

		// [[ PRIVATE VARIABLES ]]
		TYPE _type;
		Vector2Int _value;
		Dictionary<WorldDirection, Vector2Int> _neighborDirectionMap = new();
		HashSet<Vector2Int> _neighborValues { get { return _neighborDirectionMap.Values.ToHashSet(); } }

		// [[ PUBLIC REFERENCE VARIABLES ]]
		public UnitSpace Space { get; private set; }
		public CoordinateMap ParentMap { get; private set; }
		public TYPE Type => _type;
		public Vector2Int ValueKey => _value;
		public Vector3 ScenePosition { get; private set; }
		public bool Initialized { get; private set; }
		public Color TypeColor { get; private set; } = Color.black;
		public Dictionary<WorldDirection, Vector2Int> NeighborDirectionMap => _neighborDirectionMap;

		// [[ CONSTRUCTOR ]]

		// This is for single unit types, like single region generation
		public Coordinate(Vector2Int value, UnitSpace space)
		{
			this._value = value;
			this.Space = space;
		}

		public Coordinate(CoordinateMap mapParent, Vector3 mapOriginPosition, Vector2Int value, int size)
		{
			this.ParentMap = mapParent;
			this._value = value;

			// Calculate Coordinate Position in game world
			this.ScenePosition = mapOriginPosition + (new Vector3(value.x, 0, value.y) * size);

			// Assign Neighbor Values
			_neighborDirectionMap = new();
			foreach (WorldDirection direction in Enum.GetValues(typeof(WorldDirection)))
			{
				// Get neighbor value in direction
				Vector2Int neighborPosition = this.ValueKey + CoordinateMap.GetDirectionVector(direction);
				_neighborDirectionMap[direction] = neighborPosition;
			}
			this.Initialized = true;
		}

		public void SetType(TYPE newType)
		{
			_type = newType;
			switch (newType)
			{
				case TYPE.CLOSED: TypeColor = Color.black; break;
				case TYPE.BORDER: TypeColor = Color.grey; break;
				case TYPE.NULL: TypeColor = Color.grey; break;
				case TYPE.EXIT: TypeColor = Color.red; break;
				case TYPE.PATH: TypeColor = Color.white; break;
				case TYPE.ZONE: TypeColor = Color.green; break;
			}
		}

		#region =================== Get Neighbors ====================== >>>> 

		public Coordinate GetNeighborInDirection(WorldDirection direction)
		{
			Vector2Int neighborValue = _neighborDirectionMap[direction];
			return ParentMap.GetCoordinateAt(neighborValue);
		}

		public WorldDirection? GetWorldDirectionOfNeighbor(Coordinate neighbor)
		{
			if (!Initialized || !_neighborValues.Contains(neighbor.ValueKey)) return null;

			// Get Offset
			Vector2Int offset = neighbor.ValueKey - this.ValueKey;
			return CoordinateMap.GetEnumFromDirectionVector(offset);
		}

		public List<Vector2Int> GetNaturalNeighborValues()
		{
			List<Vector2Int> neighbors = new List<Vector2Int> {
				_neighborDirectionMap[WorldDirection.WEST],
				_neighborDirectionMap[WorldDirection.EAST],
				_neighborDirectionMap[WorldDirection.NORTH],
				_neighborDirectionMap[WorldDirection.SOUTH],
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Vector2Int> GetDiagonalNeighborValues()
		{
			List<Vector2Int> neighbors = new List<Vector2Int> {
				_neighborDirectionMap[WorldDirection.NORTHWEST],
				_neighborDirectionMap[WorldDirection.NORTHEAST],
				_neighborDirectionMap[WorldDirection.SOUTHWEST],
				_neighborDirectionMap[WorldDirection.SOUTHEAST],
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Coordinate> GetValidNaturalNeighbors()
		{
			if (!Initialized) return new();

			List<Coordinate> neighbors = new List<Coordinate> {
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.WEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.EAST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTH]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTH])
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Coordinate> GetValidDiagonalNeighbors()
		{
			if (!Initialized) return new();

			List<Coordinate> neighbors = new List<Coordinate> {
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTHWEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTHEAST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTHWEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTHEAST])
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Coordinate> GetAllValidNeighbors()
		{
			if (!Initialized) return new();

			List<Coordinate> neighbors = GetValidNaturalNeighbors();
			neighbors.AddRange(GetValidDiagonalNeighbors());
			return neighbors;
		}

		public Coordinate GetNeighborInOppositeDirection(WorldDirection direction)
		{
			if (!Initialized) return null;

			switch (direction)
			{
				case WorldDirection.WEST:
					return GetNeighborInDirection(WorldDirection.EAST);
				case WorldDirection.EAST:
					return GetNeighborInDirection(WorldDirection.WEST);
				case WorldDirection.NORTH:
					return GetNeighborInDirection(WorldDirection.SOUTH);
				case WorldDirection.SOUTH:
					return GetNeighborInDirection(WorldDirection.NORTH);
				case WorldDirection.NORTHWEST:
					return GetNeighborInDirection(WorldDirection.SOUTHEAST);
				case WorldDirection.NORTHEAST:
					return GetNeighborInDirection(WorldDirection.SOUTHWEST);
				case WorldDirection.SOUTHWEST:
					return GetNeighborInDirection(WorldDirection.NORTHEAST);
				case WorldDirection.SOUTHEAST:
					return GetNeighborInDirection(WorldDirection.NORTHWEST);
			}

			return null;
		}

		#endregion
	}
}

