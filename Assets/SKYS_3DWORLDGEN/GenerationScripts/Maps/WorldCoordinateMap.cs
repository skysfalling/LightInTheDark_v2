using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class WorldCoordinate
{
    public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }
    public TYPE type;
    public WorldDirection borderEdgeDirection;

    public Vector2Int Coordinate { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector3 WorldPosition { 
        get { 
            return new Vector3(Position.x, 0, Position.y); 
        } 
        private set { } 
    }

    // ASTAR PATHFINDING VALUES
    public float GCost = float.MaxValue;
    public float HCost = 0;
    public float FCost => GCost + HCost;
    public WorldCoordinate Parent = null;
    public WorldCoordinate(Vector2Int coord)
    {
        Coordinate = coord;

        // Calculate position
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2 half_FullWorldSize = realFullWorldSize * 0.5f;
        Vector2 newPos = new Vector2(coord.x * realChunkAreaSize.x, coord.y * realChunkAreaSize.y);
        newPos -= Vector2.one * half_FullWorldSize;
        newPos += Vector2.one * realChunkAreaSize * 0.5f;

        Position = newPos;
        WorldPosition = new Vector3(Position.x, 0, Position.y);
    }
}

public class WorldCoordinateMap : MonoBehaviour
{
    public static WorldCoordinateMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public bool mapInitialized { get; private set; }
    public bool pathsInitialized { get; private set; }

    public void InitializeCoordinateMap()
    {
        GetCoordinateMap();

        if (CoordinateMap.Count > 0)
        {
            mapInitialized = true;
        }
    }

    public void ResetCoordinateMap()
    {
        InitializeCoordinateMap();

        // Reset Borders
        List<WorldCoordinate> borderCoords = GetCoordinatesOnAllBorders();
        ConvertCoordinatesToType(borderCoords, WorldCoordinate.TYPE.BORDER);

        // Reset Paths


        /*
        // Reset Zones
        List<WorldCoordinate> zoneCoords = GetAllCoordinatesOfType(WorldCoordinate.TYPE.ZONE);
        ConvertCoordinatesToType(zoneCoords, WorldCoordinate.TYPE.NULL);
        */
    }

    public void ResetCoordinatePaths()
    {
        pathsInitialized = false;
        ConvertCoordinatesToType(GetAllCoordinatesOfType(WorldCoordinate.TYPE.PATH), WorldCoordinate.TYPE.NULL);
    }

    #region == COORDINATE MAP ================================ ////
    private static List<WorldCoordinate> CoordinateMap = new List<WorldCoordinate>();

    public static List<WorldCoordinate> GetCoordinateMap(bool forceReset = false)
    {
        if (!forceReset && CoordinateMap != null && CoordinateMap.Count > 0) return CoordinateMap;


        List<WorldCoordinate> coordMap = new List<WorldCoordinate>();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();

        int xCoordCount = Mathf.CeilToInt(realFullWorldSize.x / realChunkAreaSize.x);
        int yCoordCount = Mathf.CeilToInt(realFullWorldSize.y / realChunkAreaSize.y);

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                WorldCoordinate newCoord = new WorldCoordinate(new Vector2Int(x, y));

                // Check if the position is in a corner & close it
                if ((x == 0 && y == 0) ||
                    (x == xCoordCount - 1 && y == 0) ||
                    (x == 0 && y == yCoordCount - 1) ||
                    (x == xCoordCount - 1 && y == yCoordCount - 1))
                {
                    newCoord.type = WorldCoordinate.TYPE.CLOSED;
                }
                // Check if the position is on the border
                else if (x == 0 || y == 0 || x == xCoordCount - 1 || y == yCoordCount - 1)
                {
                    newCoord.type = WorldCoordinate.TYPE.BORDER;
                    if (x == 0) { newCoord.borderEdgeDirection = WorldDirection.West; }
                    if (y == 0) { newCoord.borderEdgeDirection = WorldDirection.South; }
                    if (x == xCoordCount - 1) { newCoord.borderEdgeDirection = WorldDirection.East; }
                    if (y == yCoordCount - 1) { newCoord.borderEdgeDirection = WorldDirection.North; }
                }

                coordMap.Add(newCoord);
            }
        }

        CoordinateMap = coordMap;
        Debug.Log("New Coordinate Map " + CoordinateMap.Count);

        return CoordinateMap;
    }

    public static List<Vector2> GetCoordinateMapPositions()
    {
        List<WorldCoordinate> coordMap = GetCoordinateMap();
        List<Vector2> coordMapPositions = new List<Vector2>();

        foreach (WorldCoordinate coord in coordMap) { coordMapPositions.Add(coord.Position); }
        return coordMapPositions;
    }

    public static WorldCoordinate GetCoordinate(Vector2Int coordinate)
    {
        foreach (WorldCoordinate worldCoord in GetCoordinateMap())
        {
            if (worldCoord.Coordinate == coordinate)
            {
                return worldCoord;
            }
        }
        return null;
    }

    public static WorldCoordinate GetCoordinateAtPosition(Vector2 position)
    {
        foreach (WorldCoordinate coord in GetCoordinateMap()) { 
            if (coord.Position == position)
            {
                return coord;
            }
        }
        return null;
    }

    public static List<WorldCoordinate> GetAllCoordinatesOfType(WorldCoordinate.TYPE type)
    {
        List<WorldCoordinate> typeList = new();
        foreach(WorldCoordinate coord in GetCoordinateMap())
        {
            if (coord.type == type)
            {
                typeList.Add(coord);
            }
        }
        return typeList;
    }

    public static List<WorldCoordinate> ConvertCoordinatesToType(List<WorldCoordinate> coords, WorldCoordinate.TYPE conversionType)
    {
        foreach (WorldCoordinate coordinate in coords)
        {
            coordinate.type = conversionType;
        }
        return coords;
    }

    public static List<WorldCoordinate> GetCoordinatesOnAllBorders()
    {
        List<WorldCoordinate> borderCoords = new();
        List<WorldCoordinate> coordMap = GetCoordinateMap();
        foreach (WorldCoordinate coord in coordMap)
        {
            if (coord.type == WorldCoordinate.TYPE.BORDER || coord.type == WorldCoordinate.TYPE.EXIT)
            {
                borderCoords.Add(coord);
            }
        }
        return borderCoords;
    }

    public static List<WorldCoordinate> GetCoordinatesOnBorderDirection(WorldDirection edgeDirection)
    {
        List<WorldCoordinate> coordinatesOnEdge = new List<WorldCoordinate>();
        List<WorldCoordinate> borderMap = GetCoordinatesOnAllBorders();
        foreach (WorldCoordinate coord in borderMap)
        {
            if (coord.borderEdgeDirection == edgeDirection)
            {
                coordinatesOnEdge.Add(coord);
            }
        }
        return coordinatesOnEdge;
    }

    public static List<WorldCoordinate> GetCoordinateNaturalNeighbors(WorldCoordinate coordinate)
    {
        List<WorldCoordinate> neighbors = new List<WorldCoordinate>(new WorldCoordinate[4]);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = GetCoordinateNeighborInDirection(coordinate, WorldDirection.West);
        neighbors[1] = GetCoordinateNeighborInDirection(coordinate, WorldDirection.East);
        neighbors[2] = GetCoordinateNeighborInDirection(coordinate, WorldDirection.North);
        neighbors[3] = GetCoordinateNeighborInDirection(coordinate, WorldDirection.South);

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public static List<WorldCoordinate> GetCoordinateDiagonalNeighbors(WorldCoordinate coordinate)
    {
        if (coordinate == null) { return null; }
        int chunkWidth = WorldGeneration.GetRealChunkAreaSize().x;
        int chunkLength = WorldGeneration.GetRealChunkAreaSize().y;

        List<WorldCoordinate> neighbors = new List<WorldCoordinate>(new WorldCoordinate[4]);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = GetCoordinateAtPosition(coordinate.Position + new Vector2(-chunkWidth, -chunkLength)); // SOUTH WEST
        neighbors[1] = GetCoordinateAtPosition(coordinate.Position + new Vector2(-chunkWidth, chunkLength)); // NORTH WEST
        neighbors[2] = GetCoordinateAtPosition(coordinate.Position + new Vector2(chunkWidth, -chunkLength)); // SOUTH EAST
        neighbors[3] = GetCoordinateAtPosition(coordinate.Position + new Vector2(chunkWidth, chunkLength)); // NORTH EAST

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public static List<WorldCoordinate> GetAllCoordinateNeighbors(WorldCoordinate coordinate)
    {
        List<WorldCoordinate> neighbors = GetCoordinateNaturalNeighbors(coordinate);
        neighbors.AddRange(GetCoordinateDiagonalNeighbors(coordinate));
        return neighbors;
    }

    public static WorldCoordinate GetCoordinateNeighborInDirection(WorldCoordinate coordinate, WorldDirection direction)
    {
        if (coordinate == null) { return null; }
        int chunkWidth = WorldGeneration.GetRealChunkAreaSize().x;
        int chunkLength = WorldGeneration.GetRealChunkAreaSize().y;

        WorldCoordinate coord = null;
        switch (direction)
        {
            case WorldDirection.West:
                coord = GetCoordinateAtPosition(coordinate.Position + new Vector2(-chunkWidth, 0));
                break;
            case WorldDirection.East:
                coord = GetCoordinateAtPosition(coordinate.Position + new Vector2(chunkWidth, 0));
                break;
            case WorldDirection.North:
                coord = GetCoordinateAtPosition(coordinate.Position + new Vector2(0, chunkLength));
                break;
            case WorldDirection.South:
                coord = GetCoordinateAtPosition(coordinate.Position + new Vector2(0, -chunkLength));
                break;
        }

        return coord;
    }

    #endregion

    #region == {{ WORLD EXITS }} ======================================================== ////
    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();

    public void CreateWorldExitPath()
    {
        WorldExit defaultStart = new WorldExit(WorldDirection.West, 5);
        WorldExit defaultEnd = new WorldExit(WorldDirection.East, 5);
        worldExitPaths.Add(new WorldExitPath(defaultStart, defaultEnd));
    }

    public void InitializeWorldExitPaths()
    {
        ResetCoordinatePaths();

        // Initialize New Exit Paths
        foreach (WorldExitPath exitPath in worldExitPaths)
        {
            exitPath.Initialize();
        }
        pathsInitialized = true;
    }

    public static WorldCoordinate GetWorldExitPathConnection(WorldExit exit)
    {
        switch (exit.borderDirection)
        {
            case WorldDirection.West:
                return GetCoordinateNeighborInDirection(exit.Coordinate, WorldDirection.East);
            case WorldDirection.East:
                return GetCoordinateNeighborInDirection(exit.Coordinate, WorldDirection.West);
            case WorldDirection.North:
                return GetCoordinateNeighborInDirection(exit.Coordinate, WorldDirection.South);
            case WorldDirection.South:
                return GetCoordinateNeighborInDirection(exit.Coordinate, WorldDirection.North);
        }

        return null;
    }

    public static WorldCoordinate GetCoordinateAtWorldExit(WorldExit worldExit)
    {
        WorldDirection direction = worldExit.borderDirection;
        int index = worldExit.borderIndex;

        List<WorldCoordinate> borderCoords = GetCoordinatesOnBorderDirection(direction);
        if (borderCoords.Count > index)
        {
            return borderCoords[index];
        }
        return null;
    }
    #endregion

    #region == COORDINATE PATHFINDING =================================///
    /* A* Pathfinding implementation
    * - gCost is the known cost from the starting node
    * - hCost is the estimated distance to the end node
    * - fCost is gCost + hCost
    */

    public static List<WorldCoordinate> FindCoordinatePath(WorldCoordinate startCoordinate, WorldCoordinate endCoordinate, float pathRandomness = 0)
    {
        List<WorldCoordinate> openSet = new List<WorldCoordinate>();
        HashSet<WorldCoordinate> closedSet = new HashSet<WorldCoordinate>();
        openSet.Add(startCoordinate);

        while (openSet.Count > 0)
        {
            WorldCoordinate currentCoordinate = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost <= currentCoordinate.FCost &&
                    openSet[i].HCost < currentCoordinate.HCost &&
                    openSet[i].type == WorldCoordinate.TYPE.NULL &&
                    Random.Range(0f, 1f) <= pathRandomness)
                {
                    currentCoordinate = openSet[i];
                }
            }

            openSet.Remove(currentCoordinate);
            closedSet.Add(currentCoordinate);

            if (currentCoordinate == endCoordinate)
            {
                return RetracePath(startCoordinate, endCoordinate);
            }

            List<WorldCoordinate> currentNeighbors = GetCoordinateNaturalNeighbors(currentCoordinate);
            foreach (WorldCoordinate neighbor in currentNeighbors)
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCostToNeighbor = currentCoordinate.GCost + GetCoordinateDistance(currentCoordinate, neighbor);
                // Apply randomness here to affect the movement cost
                newMovementCostToNeighbor += Random.Range(-pathRandomness, pathRandomness);

                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetCoordinateDistance(neighbor, endCoordinate);
                    neighbor.Parent = currentCoordinate;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<WorldCoordinate>(); // Return an empty path if there is no path
    }

    static List<WorldCoordinate> RetracePath(WorldCoordinate startCoordinate, WorldCoordinate endCoordinate)
    {
        List<WorldCoordinate> path = new List<WorldCoordinate>();
        WorldCoordinate currentCoordinate = endCoordinate;

        while (currentCoordinate != startCoordinate)
        {
            path.Add(currentCoordinate);
            currentCoordinate = currentCoordinate.Parent;
        }
        path.Add(startCoordinate);

        path.Reverse();
        return path;
    }

    public static float GetCoordinateDistance(WorldCoordinate a, WorldCoordinate b)
    {
        // This link has more heuristics :: https://www.enjoyalgorithms.com/blog/a-star-search-algorithm
        return Vector2.Distance(a.Position, b.Position);
    }
    #endregion


}
