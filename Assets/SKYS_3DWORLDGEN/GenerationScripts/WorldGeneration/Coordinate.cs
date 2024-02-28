using System;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    public bool foundNeighbors = false;

    public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }
    public TYPE type;
    public WorldDirection borderEdgeDirection;
    public Color debugColor = Color.clear;

    public CoordinateMap CoordinateMap { get; private set; }
    public WorldSpace Space { get; private set; }
    public Vector2Int LocalCoordinate { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    public Dictionary<WorldDirection, Vector2Int> NeighborCoordinateMap { get; private set; }

    public Coordinate(CoordinateMap coordinateMap, Vector2Int coord, WorldRegion region)
    {
        this.CoordinateMap = coordinateMap;
        LocalCoordinate = coord;
        Space = WorldSpace.Region;

        // Calculate position
        int chunkWidth = WorldGeneration.GetChunkWidth_inWorldSpace();

        // Calculate local position
        Vector2 worldPosition = new Vector2(region.originCoordinatePosition.x, region.originCoordinatePosition.z);
        worldPosition += new Vector2(coord.x, coord.y) * chunkWidth;

        WorldPosition = new Vector3(worldPosition.x, 0, worldPosition.y);

        InitializeNeighborMap();
    }

    // =================== NEIGHBOR MAP ====================== >>>> 
    public void InitializeNeighborMap()
    {
        // Set Neighbors
        NeighborCoordinateMap = new Dictionary<WorldDirection, Vector2Int>();

        foreach (WorldDirection direction in Enum.GetValues(typeof(WorldDirection)))
        {
            // Get neighbor in direction
            NeighborCoordinateMap[direction] = LocalCoordinate + GetDirectionVector(direction);
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

    public Coordinate GetNeighborInDirection(WorldDirection direction)
    {
        if (!foundNeighbors) return null;
        return CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[direction]);
    }

    public WorldDirection? GetDirectionOfNeighbor(Coordinate neighbor)
    {
        if (!foundNeighbors) return null;

        // Iterate through each entry in the NeighborCoordinateMap
        foreach (var entry in NeighborCoordinateMap)
        {
            // Check if the neighbor's Coordinate matches the entry's value
            if (entry.Value == neighbor.LocalCoordinate)
            {
                // If so, return the direction
                return entry.Key;
            }
        }

        // If no matching neighbor is found, return null
        return null;
    }

    public List<Coordinate> GetValidNaturalNeighbors()
    {
        if (!foundNeighbors) return new();

        List<Coordinate> neighbors = new List<Coordinate> {
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.WEST]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.EAST]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTH]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTH])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Coordinate> GetValidDiagonalNeighbors()
    {
        if (!foundNeighbors) return new();

        List<Coordinate> neighbors = new List<Coordinate> {
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTHWEST]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.NORTHEAST]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTHWEST]),
            CoordinateMap.GetCoordinateAt(NeighborCoordinateMap[WorldDirection.SOUTHEAST])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Coordinate> GetAllValidNeighbors()
    {
        if (!foundNeighbors) return new();

        List<Coordinate> neighbors = GetValidNaturalNeighbors();
        neighbors.AddRange(GetValidDiagonalNeighbors());
        return neighbors;
    }

    public Coordinate GetNeighborInOppositeDirection(WorldDirection direction)
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
