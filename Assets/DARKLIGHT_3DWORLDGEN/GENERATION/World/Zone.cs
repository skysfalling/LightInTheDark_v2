using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Zone
    {
        public enum TYPE { FULL, NATURAL_CROSS, DIAGONAL_CROSS, HORIZONTAL, VERTICAL }

        // [[ PRIVATE VARIABLES ]]
        Coordinate _coordinate;
        TYPE _zoneType;
        int _zoneHeight;
        List<Vector2Int> _zonePositions = new();

        public TYPE Type => _zoneType;
        public List<Vector2Int> AllPositions { get; private set; }

        public Zone(Coordinate coordinate, TYPE zoneType, int zoneHeight)
        {
            this._coordinate = coordinate;
            this._zoneType = zoneType;
            this._zoneHeight = zoneHeight;

            // Get affected neighbors
            List<Coordinate> neighborsInZone = new();
            switch (_zoneType)
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
            _zonePositions = new();
            foreach (Coordinate coord in zoneCoordinates) { 

                if (coord.Type == Coordinate.TYPE.NULL)
                {
                    _zonePositions.Add(coord.Value);
                }
            }
        }
    }
}

