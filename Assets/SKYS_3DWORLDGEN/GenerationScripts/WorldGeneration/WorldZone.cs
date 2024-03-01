using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[System.Serializable]
public class WorldZone
{

    bool _initialized;
    CoordinateMap _coordinateMap;
    Coordinate _centerCoordinate;
    TYPE _zoneType;
    int _zoneHeight;

    public enum TYPE { FULL, NATURAL_CROSS, DIAGONAL_CROSS, HORIZONTAL, VERTICAL }
    public List<Coordinate> coordinates { get; private set; }
    public List<Vector2Int> positions { get; private set; }

    public WorldZone(CoordinateMap coordinateMap, Coordinate centerCoordinate, TYPE zoneType, int zoneHeight )
    {
        this._coordinateMap = coordinateMap;
        this._centerCoordinate = centerCoordinate;
        this._zoneType = zoneType;
        this._zoneHeight = zoneHeight;

        // Get affected neighbors
        List<Coordinate> neighborsInZone = new();
        switch (_zoneType)
        {
            case TYPE.FULL:
                neighborsInZone = _centerCoordinate.GetAllValidNeighbors();
                break;
            case TYPE.NATURAL_CROSS:
                neighborsInZone = _centerCoordinate.GetValidNaturalNeighbors();
                break;
            case TYPE.DIAGONAL_CROSS:
                neighborsInZone = _centerCoordinate.GetValidDiagonalNeighbors();
                break;
            case TYPE.HORIZONTAL:
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.WEST));
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.EAST));
                break;
            case TYPE.VERTICAL:
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.NORTH));
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.SOUTH));
                break;
        }

        // Assign Zone Coordinates
        coordinates = new List<Coordinate> { _centerCoordinate };
        coordinates.AddRange(neighborsInZone);

        // Extract positions
        positions = new();
        for (int i = 0; i < coordinates.Count; i++)
        {
            positions.Add(coordinates[i].localPosition);
        }

        // Leave uninitialized if positions are invalid
        HashSet<Vector2Int> validPositions = coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.NULL);
        foreach(Vector2Int position in validPositions)
        {
            if (!validPositions.Contains(position)) { return; }
        }


        _initialized = true;
        //Debug.Log($"Initialized WORLD ZONE : {_coordinateVector} : height {zoneHeight}");
    }

    public void Initialize()
    {
        if (_initialized) { return; }
        _initialized = false;

        // Update private variables

    }

    public void Reset()
    {
        _initialized = false;
    }

    public List<Coordinate> GetZoneCoordinates() { return coordinates; }

}
