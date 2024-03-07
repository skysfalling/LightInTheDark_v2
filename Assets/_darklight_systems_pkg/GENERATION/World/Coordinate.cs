using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Coordinate
    {
        public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }

        // [[ PRIVATE VARIABLES ]]
        TYPE _type;
        Vector2Int _value;
        HashSet<Vector2Int> _neighborPositions = new();
        Dictionary<WorldDirection, Vector2Int> _neighborDirectionMap = new();

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public CoordinateMap CoordinateMapParent { get; private set; }
        public TYPE Type => _type;
        public Vector2Int Value => _value;
        public Vector3 ScenePosition { get; private set; }
        public bool Initialized { get; private set; }
        public Color TypeColor { get; private set; } = Color.black;
        public Dictionary<WorldDirection, Vector2Int> NeighborDirectionMap => _neighborDirectionMap;

        // [[ CONSTRUCTOR ]]
        public Coordinate(CoordinateMap mapParent, Vector3 mapOriginPosition, Vector2Int value, int size)
        {
            this.CoordinateMapParent = mapParent;
            this._value = value;

            // Calculate Coordinate Position in game world
            this.ScenePosition = mapOriginPosition + (new Vector3(value.x, 0, value.y) * size);

            SetNeighbors();

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

        void SetNeighbors()
        {
            _neighborPositions = new();
            _neighborDirectionMap = new();
            foreach (WorldDirection direction in Enum.GetValues(typeof(WorldDirection)))
            {
                // Get neighbor in direction
                Vector2Int neighborPosition = Value + CoordinateMap.GetDirectionVector(direction);
                _neighborPositions.Add(neighborPosition);
                _neighborDirectionMap[direction] = neighborPosition;
            }
        }

        #region =================== Get Neighbors ====================== >>>> 

        public Coordinate GetNeighborInDirection(WorldDirection direction)
        {
            if (!Initialized) return null;
            return CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[direction]);
        }

        public WorldDirection? GetWorldDirectionOfNeighbor(Coordinate neighbor)
        {
            if (!Initialized || !_neighborPositions.Contains(neighbor.Value)) return null;

            // Get Offset
            Vector2Int offset = neighbor.Value - this.Value;
            return CoordinateMap.GetEnumFromDirectionVector(offset);
        }

        public List<Coordinate> GetValidNaturalNeighbors()
        {
            if (!Initialized) return new();

            List<Coordinate> neighbors = new List<Coordinate> {
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.WEST]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.EAST]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTH]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTH])
            };
            neighbors.RemoveAll(item => item == null);
            return neighbors;
        }

        public List<Coordinate> GetValidDiagonalNeighbors()
        {
            if (!Initialized) return new();

            List<Coordinate> neighbors = new List<Coordinate> {
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTHWEST]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.NORTHEAST]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTHWEST]),
                CoordinateMapParent.GetCoordinateAt(_neighborDirectionMap[WorldDirection.SOUTHEAST])
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

        public List<Vector2Int> GetNaturalNeighborCoordinateValues()
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

        public List<Vector2Int> GetDiagonalNeighborCoordinateValues()
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
        #endregion
    }
}

