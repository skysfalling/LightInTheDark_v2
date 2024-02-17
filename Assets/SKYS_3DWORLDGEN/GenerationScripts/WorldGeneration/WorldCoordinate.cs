using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldCoordinate
{
    public bool foundNeighbors = false;

    public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }
    public TYPE type;
    public WorldDirection borderEdgeDirection;
    public Color debugColor = Color.clear;

    public Vector2Int Coordinate { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector3 WorldPosition
    {
        get
        {
            return new Vector3(Position.x, 0, Position.y);
        }
        private set { }
    }

    public Dictionary<WorldDirection, Vector2Int> NeighborCoordinateMap { get; private set; }

    public WorldCoordinate(Vector2Int coord)
    {
        Coordinate = coord;

        // Calculate position
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkArea();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2 half_FullWorldSize = realFullWorldSize * 0.5f;
        Vector2 newPos = new Vector2(coord.x * realChunkAreaSize.x, coord.y * realChunkAreaSize.y);
        newPos -= Vector2.one * half_FullWorldSize;
        newPos += Vector2.one * realChunkAreaSize * 0.5f;

        Position = newPos;
        WorldPosition = new Vector3(Position.x, 0, Position.y);
    }

    public void InitializeNeighborMap()
    {
        // Set Neighbors
        NeighborCoordinateMap = new Dictionary<WorldDirection, Vector2Int>();

        foreach (WorldDirection direction in Enum.GetValues(typeof(WorldDirection)))
        {
            // Get neighbor in direction
            NeighborCoordinateMap[direction] = Coordinate + GetDirectionVector(direction);
        }

        foundNeighbors = true;
    }

    static Vector2Int GetDirectionVector(WorldDirection direction)
    {
        Vector2Int directionVector = new Vector2Int(0, 0);
        switch (direction)
        {
            case WorldDirection.NORTH: directionVector = Vector2Int.up; break;
            case WorldDirection.SOUTH: directionVector = Vector2Int.down; break;
            case WorldDirection.EAST: directionVector = Vector2Int.right; break;
            case WorldDirection.WEST: directionVector = Vector2Int.left; break;
            case WorldDirection.NORTHEAST: directionVector = new Vector2Int(1, 1); break;
            case WorldDirection.NORTHWEST: directionVector = new Vector2Int(-1, 1); break;
            case WorldDirection.SOUTHEAST: directionVector = new Vector2Int(1, -1); break;
            case WorldDirection.SOUTHWEST: directionVector = new Vector2Int(-1, -1); break;
        }
        return directionVector;
    }

    public WorldCoordinate GetNeighborInDirection(WorldDirection direction)
    {
        if (!foundNeighbors) return null;
        return WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[direction]);
    }

    public WorldDirection? GetDirectionOfNeighbor(WorldCoordinate neighbor)
    {
        if (!foundNeighbors) return null;

        // Iterate through each entry in the NeighborCoordinateMap
        foreach (var entry in NeighborCoordinateMap)
        {
            // Check if the neighbor's Coordinate matches the entry's value
            if (entry.Value == neighbor.Coordinate)
            {
                // If so, return the direction
                return entry.Key;
            }
        }

        // If no matching neighbor is found, return null
        return null;
    }

    public List<WorldCoordinate> GetValidNaturalNeighbors()
    {
        if (!foundNeighbors) return new();

        List<WorldCoordinate> neighbors = new List<WorldCoordinate> {
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.WEST]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.EAST]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTH]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTH])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<WorldCoordinate> GetValidDiagonalNeighbors()
    {
        if (!foundNeighbors) return new();

        List<WorldCoordinate> neighbors = new List<WorldCoordinate> {
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTHWEST]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTHEAST]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTHWEST]),
            WorldCoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTHEAST])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<WorldCoordinate> GetAllValidNeighbors()
    {
        if (!foundNeighbors) return new();

        List<WorldCoordinate> neighbors = GetValidNaturalNeighbors();
        neighbors.AddRange(GetValidDiagonalNeighbors());
        return neighbors;
    }

    public WorldCoordinate GetNeighborInOppositeDirection(WorldDirection direction)
    {
        if (!foundNeighbors) return null;

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

    public List<Vector2Int> GetValidNaturalNeighborCoordinates()
    {
        if (!foundNeighbors) return new();

        List<Vector2Int> neighbors = new List<Vector2Int> {
            NeighborCoordinateMap[WorldDirection.WEST],
            NeighborCoordinateMap[WorldDirection.EAST],
            NeighborCoordinateMap[WorldDirection.NORTH],
            NeighborCoordinateMap[WorldDirection.SOUTH],
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Vector2Int> GetValidDiagonalNeighborCoordinates()
    {
        if (!foundNeighbors) return new();

        List<Vector2Int> neighbors = new List<Vector2Int> {
            NeighborCoordinateMap[WorldDirection.NORTHWEST],
            NeighborCoordinateMap[WorldDirection.NORTHEAST],
            NeighborCoordinateMap[WorldDirection.SOUTHWEST],
            NeighborCoordinateMap[WorldDirection.SOUTHEAST],
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public bool IsCoordinateInNaturalNeighbors(Vector2Int coordinate)
    {
        return GetValidDiagonalNeighborCoordinates().Contains(coordinate);
    }
}

