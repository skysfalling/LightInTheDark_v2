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
		Dictionary<Direction, Vector2Int> _neighborDirectionMap = new();
		HashSet<Vector2Int> _neighborValues { get { return _neighborDirectionMap.Values.ToHashSet(); } }

		// [[ PUBLIC REFERENCE VARIABLES ]]
		public UnitSpace Space { get; private set; }
		public CoordinateMap ParentMap { get; private set; }
		public TYPE Type => _type;
		public Vector2Int ValueKey => _value;
		public Vector3 ScenePosition { get; private set; }
		public bool Initialized { get; private set; }
		public Color TypeColor { get; private set; } = Color.black;
		public Dictionary<Direction, Vector2Int> NeighborDirectionMap => _neighborDirectionMap;

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
			foreach (Direction direction in Enum.GetValues(typeof(Direction)))
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

		public Coordinate GetNeighborInDirection(Direction direction)
		{
			Vector2Int neighborValue = _neighborDirectionMap[direction];
			return ParentMap.GetCoordinateAt(neighborValue);
		}

		public Direction? GetWorldDirectionOfNeighbor(Coordinate neighbor)
		{
			if (!Initialized || !_neighborValues.Contains(neighbor.ValueKey)) return null;

			// Get Offset
			Vector2Int offset = neighbor.ValueKey - this.ValueKey;
			return CoordinateMap.GetEnumFromDirectionVector(offset);
		}

		public List<Vector2Int> GetNaturalNeighborValues()
		{
			List<Vector2Int> neighbors = new List<Vector2Int> {
				_neighborDirectionMap[Direction.WEST],
				_neighborDirectionMap[Direction.EAST],
				_neighborDirectionMap[Direction.NORTH],
				_neighborDirectionMap[Direction.SOUTH],
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Vector2Int> GetDiagonalNeighborValues()
		{
			List<Vector2Int> neighbors = new List<Vector2Int> {
				_neighborDirectionMap[Direction.NORTHWEST],
				_neighborDirectionMap[Direction.NORTHEAST],
				_neighborDirectionMap[Direction.SOUTHWEST],
				_neighborDirectionMap[Direction.SOUTHEAST],
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Coordinate> GetValidNaturalNeighbors()
		{
			if (!Initialized) return new();

			List<Coordinate> neighbors = new List<Coordinate> {
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.WEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.EAST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.NORTH]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.SOUTH])
			};
			neighbors.RemoveAll(item => item == null);
			return neighbors;
		}

		public List<Coordinate> GetValidDiagonalNeighbors()
		{
			if (!Initialized) return new();

			List<Coordinate> neighbors = new List<Coordinate> {
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.NORTHWEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.NORTHEAST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.SOUTHWEST]),
				ParentMap.GetCoordinateAt(_neighborDirectionMap[Direction.SOUTHEAST])
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

		public Coordinate GetNeighborInOppositeDirection(Direction direction)
		{
			if (!Initialized) return null;

			switch (direction)
			{
				case Direction.WEST:
					return GetNeighborInDirection(Direction.EAST);
				case Direction.EAST:
					return GetNeighborInDirection(Direction.WEST);
				case Direction.NORTH:
					return GetNeighborInDirection(Direction.SOUTH);
				case Direction.SOUTH:
					return GetNeighborInDirection(Direction.NORTH);
				case Direction.NORTHWEST:
					return GetNeighborInDirection(Direction.SOUTHEAST);
				case Direction.NORTHEAST:
					return GetNeighborInDirection(Direction.SOUTHWEST);
				case Direction.SOUTHWEST:
					return GetNeighborInDirection(Direction.NORTHEAST);
				case Direction.SOUTHEAST:
					return GetNeighborInDirection(Direction.NORTHWEST);
			}

			return null;
		}

		#endregion
	}
}

