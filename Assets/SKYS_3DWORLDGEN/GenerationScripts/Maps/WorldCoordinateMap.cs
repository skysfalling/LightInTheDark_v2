using System.Collections.Generic;
using UnityEngine;
public class WorldCoordinate
{
    public enum TYPE { NULL, BORDER, EXIT, PATH, CLOSED }

    public TYPE type;
    public WorldDirection borderEdgeDirection;

    // ASTAR PATHFINDING
    public Vector2 Position { get; set; }
    public Vector3 WorldPosition { get; set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public WorldCoordinate Parent { get; set; }

    public WorldCoordinate(Vector2 position)
    {
        Position = position;
        WorldPosition = new Vector3(position.x, 0, position.y);

        GCost = float.MaxValue;
        HCost = 0;
        Parent = null;
    }
    public WorldCoordinate(Vector2 position, TYPE type)
    {
        Position = position;
        WorldPosition = new Vector3(position.x, 0, position.y);

        this.type = type;

        GCost = float.MaxValue;
        HCost = 0;
        Parent = null;
    }
}

public class WorldCoordinateMap : MonoBehaviour
{
    public static WorldCoordinateMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // >> COORDINATE MAP ================================ >>
    public static List<WorldCoordinate> CoordinateMap = new List<WorldCoordinate>();

    public static List<WorldCoordinate> GetCoordinateMap(bool forceReset = false)
    {
        if (!forceReset && CoordinateMap != null && CoordinateMap.Count > 0) return CoordinateMap;


        List<WorldCoordinate> coordMap = new List<WorldCoordinate>();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2 half_FullWorldSize = realFullWorldSize * 0.5f;

        int xCoordCount = Mathf.CeilToInt(realFullWorldSize.x / realChunkAreaSize.x);
        int yCoordCount = Mathf.CeilToInt(realFullWorldSize.y / realChunkAreaSize.y);

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                Vector2 newPos = new Vector2(x * realChunkAreaSize.x, y * realChunkAreaSize.y);
                newPos -= Vector2.one * half_FullWorldSize;
                newPos += Vector2.one * realChunkAreaSize * 0.5f;

                WorldCoordinate newCoord = new WorldCoordinate(newPos);

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

    #region == {{ WORLD EXITS }} ======================================================== ////
    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();

    public void InitializeWorldExits()
    {
        // Reset Borders
        List<WorldCoordinate> borderCoords = GetCoordinatesOnAllBorders();
        foreach (WorldCoordinate coord in borderCoords)
        {
            coord.type = WorldCoordinate.TYPE.BORDER;
        }

        // Initialize New Exits
        foreach (WorldExitPath exitPath in worldExitPaths)
        {
            exitPath.startExit.Initialize();
            exitPath.endExit.Initialize();
        }
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
    // A* Pathfinding implementation
    // - gCost is the known cost from the starting node
    // - hCost is the estimated distance to the end node
    // - fCost is gCost + hCost

    public static List<WorldCoordinate> FindCoordinatePath(WorldCoordinate startCoordinate, WorldCoordinate endCoordinate)
    {
        List<WorldCoordinate> openSet = new List<WorldCoordinate>();
        HashSet<WorldCoordinate> closedSet = new HashSet<WorldCoordinate>();
        openSet.Add(startCoordinate);

        while (openSet.Count > 0)
        {
            WorldCoordinate currentCoordinate = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {

                // Check if best option
                if (openSet[i].FCost <= currentCoordinate.FCost && 
                    openSet[i].HCost < currentCoordinate.HCost &&
                    openSet[i].type == WorldCoordinate.TYPE.NULL)
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

    private void OnDrawGizmosSelected()
    {
        List<WorldCoordinate> coordMap = GetCoordinateMap();
        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();

        // << DRAW COORDINATE MAP >>
        foreach (WorldCoordinate coord in coordMap)
        {
            // Draw Chunks
            Gizmos.color = Color.white;
            Vector3 chunkHeightOffset = realChunkDimensions.y * Vector3.down * 0.5f;

            switch (coord.type)
            {
                case WorldCoordinate.TYPE.NULL:
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.BORDER:
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.EXIT:
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.PATH:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
                case WorldCoordinate.TYPE.CLOSED:
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
                    break;
            }
        }
    }
}
