using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Zone
    {
        public enum TYPE { FULL, NATURAL_CROSS, DIAGONAL_CROSS, HORIZONTAL, VERTICAL }

        // [[ PRIVATE VARIABLES ]]
        Coordinate _coordinate;
        TYPE _type;
        int _height;
        List<Vector2Int> _positions = new();

        public TYPE Type => _type;
        public List<Vector2Int> AllPositions => _positions;
        public Zone(Coordinate coordinate, TYPE zoneType, int zoneHeight)
        {
            this._coordinate = coordinate;
            this._type = zoneType;
            this._height = zoneHeight;

            // Get affected neighbors
            List<Coordinate> neighborsInZone = new();
            switch (_type)
            {
                case TYPE.FULL:
                    neighborsInZone = _coordinate.GetAllValidNeighbors();
                    break;
                case TYPE.NATURAL_CROSS:
                    neighborsInZone = _coordinate.GetValidNaturalNeighbors();
                    break;
                case TYPE.DIAGONAL_CROSS:
                    neighborsInZone = _coordinate.GetValidDiagonalNeighbors();
                    break;
                case TYPE.HORIZONTAL:
                    neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.WEST));
                    neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.EAST));
                    break;
                case TYPE.VERTICAL:
                    neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.NORTH));
                    neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.SOUTH));
                    break;
            }

            // Assign Zone Coordinates
            List<Coordinate> zoneCoordinates = new List<Coordinate> { _coordinate };
            zoneCoordinates.AddRange(neighborsInZone);

            // Extract positions
            _positions = new();
            foreach (Coordinate coord in zoneCoordinates) { 

                if (coord.Type == Coordinate.TYPE.NULL)
                {
                    _positions.Add(coord.Value);
                }
            }
        }
    }
}

