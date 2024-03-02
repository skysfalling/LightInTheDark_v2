using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Coordinate
{

    public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }
    public TYPE type;
    public Color debugColor = Color.clear;

    public CoordinateMap coordinateMap { get; private set; }
    public WorldSpace worldSpace { get; private set; }
    public Vector2Int localPosition { get; private set; }
    public Vector3 worldPosition { get; private set; }
    public bool initialized { get; private set; }

    HashSet<Vector2Int> _neighborPositions = new();
    Dictionary<WorldDirection, Vector2Int> _neighborMap = new();
    Dictionary<Vector2Int, TYPE> _neighborTypeMap = new();

    public Coordinate(CoordinateMap coordinateMapParent, Vector2Int coord, WorldGeneration worldGeneration)
    {
        this.coordinateMap = coordinateMapParent;
        localPosition = coord;
        worldSpace = WorldSpace.Region; // The Coordinate is in REGION space because it determines where the REGIONS spawn in the parent WORLD

        // Calculate position
        int regionWidth = WorldGeneration.GetFullRegionWidth_inWorldSpace();

        // Calculate local position
        Vector2 worldPosition = new Vector2(worldGeneration.originPosition_inWorldSpace.x, worldGeneration.originPosition_inWorldSpace.z);
        worldPosition += new Vector2(coord.x, coord.y) * regionWidth;

        this.worldPosition = new Vector3(worldPosition.x, 0, worldPosition.y);

        SetNeighbors();

        initialized = true;
    }

    public Coordinate(CoordinateMap coordinateMapParent, Vector2Int coord, WorldRegion region)
    {
        this.coordinateMap = coordinateMapParent;
        localPosition = coord;
        worldSpace = WorldSpace.Chunk; // The Coordinate is in CHUNK space because it determines where the CHUNKS spawn in the parent REGION

        // Calculate position
        int chunkWidth = WorldGeneration.GetChunkWidth_inWorldSpace();

        // Calculate local position
        Vector2 worldPosition = new Vector2(region.originPosition_inWorldSpace.x, region.originPosition_inWorldSpace.z);
        worldPosition += new Vector2(coord.x, coord.y) * chunkWidth;

        this.worldPosition = new Vector3(worldPosition.x, 0, worldPosition.y);

        SetNeighbors();

        initialized = true;
    }

    public Coordinate(CoordinateMap coordinateMapParent, Vector2Int coord, WorldChunk chunk)
    {
        this.coordinateMap = coordinateMapParent;
        localPosition = coord;
        worldSpace = WorldSpace.Cell; // The Coordinate is in CELL space because it determines where the CELLS spawn in the parent CHUNK

        // Calculate position
        int cellWidth = WorldGeneration.CellWidth_inWorldSpace;

        // Calculate local position
        Vector2 worldPosition = new Vector2(chunk.originCoordinatePosition.x, chunk.originCoordinatePosition.z);
        worldPosition += new Vector2(coord.x, coord.y) * cellWidth;

        this.worldPosition = new Vector3(worldPosition.x, chunk.groundHeight, worldPosition.y);

        SetNeighbors();

        initialized = true;
    }

    void SetNeighbors()
    {
        _neighborPositions = new();
        _neighborMap = new();
        foreach (WorldDirection direction in Enum.GetValues(typeof(WorldDirection)))
        {
            // Get neighbor in direction
            Vector2Int neighborPosition = localPosition + CoordinateMap.GetDirectionVector(direction);
            _neighborPositions.Add(neighborPosition);
            _neighborMap[direction] = neighborPosition;
        }
    }

    

    #region =================== Get Neighbors ====================== >>>> 

    public HashSet<TYPE> GetNeighborTypes()
    {
        _neighborTypeMap = new(); // reset neighbor map
        List<Vector2Int> neighbors = _neighborPositions.ToList();
        HashSet<TYPE> types = new HashSet<TYPE>();

        for(int i = 0; i < neighbors.Count; i++)
        {
            TYPE neighborType = (TYPE)coordinateMap.GetCoordinateTypeAt(neighbors[i]);
            _neighborTypeMap[neighbors[i]] = neighborType;
            types.Add(neighborType);
        }

        return types;
    }

    public Coordinate GetNeighborInDirection(WorldDirection direction)
    {
        if (!initialized) return null;
        return coordinateMap.GetCoordinateAt(_neighborMap[direction]);
    }

    public WorldDirection? GetWorldDirectionOfNeighbor(Coordinate neighbor)
    {
        if (!initialized || !_neighborPositions.Contains(neighbor.localPosition)) return null;

        // Get Offset
        Vector2Int offset = neighbor.localPosition - this.localPosition;
        return CoordinateMap.GetDirectionEnum(offset);
    }

    public List<Coordinate> GetValidNaturalNeighbors()
    {
        if (!initialized) return new();

        List<Coordinate> neighbors = new List<Coordinate> {
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.WEST]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.EAST]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.NORTH]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.SOUTH])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Coordinate> GetValidDiagonalNeighbors()
    {
        if (!initialized) return new();

        List<Coordinate> neighbors = new List<Coordinate> {
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.NORTHWEST]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.NORTHEAST]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.SOUTHWEST]),
            coordinateMap.GetCoordinateAt(_neighborMap[WorldDirection.SOUTHEAST])
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Coordinate> GetAllValidNeighbors()
    {
        if (!initialized) return new();

        List<Coordinate> neighbors = GetValidNaturalNeighbors();
        neighbors.AddRange(GetValidDiagonalNeighbors());
        return neighbors;
    }

    public Coordinate GetNeighborInOppositeDirection(WorldDirection direction)
    {
        if (!initialized) return null;

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
        if (!initialized) return new();

        List<Vector2Int> neighbors = new List<Vector2Int> {
            _neighborMap[WorldDirection.WEST],
            _neighborMap[WorldDirection.EAST],
            _neighborMap[WorldDirection.NORTH],
            _neighborMap[WorldDirection.SOUTH],
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public List<Vector2Int> GetValidDiagonalNeighborCoordinates()
    {
        if (!initialized) return new();

        List<Vector2Int> neighbors = new List<Vector2Int> {
            _neighborMap[WorldDirection.NORTHWEST],
            _neighborMap[WorldDirection.NORTHEAST],
            _neighborMap[WorldDirection.SOUTHWEST],
            _neighborMap[WorldDirection.SOUTHEAST],
        };
        neighbors.RemoveAll(item => item == null);
        return neighbors;
    }

    public bool IsCoordinateInNaturalNeighbors(Vector2Int coordinate)
    {
        return GetValidDiagonalNeighborCoordinates().Contains(coordinate);
    }
    #endregion
}


