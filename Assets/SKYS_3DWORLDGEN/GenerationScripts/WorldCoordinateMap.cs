using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WorldCoordinate
{
    public WorldChunk.TYPE ChunkType = WorldChunk.TYPE.EMPTY;
    public WorldDirection borderEdgeDirection;

    // ASTAR PATHFINDING
    public Vector2 Position { get; set; }
    public Vector3 WorldPosition { get; set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public WorldCoordinate Parent { get; set; }

    public bool goldenPath;

    public WorldCoordinate(Vector2 position)
    {
        Position = position;
        WorldPosition = new Vector3(position.x, 0, position.y);

        GCost = float.MaxValue;
        HCost = 0;
        Parent = null;
    }
    public WorldCoordinate(Vector2 position, WorldChunk.TYPE type)
    {
        Position = position;
        WorldPosition = new Vector3(position.x, 0, position.y);

        ChunkType = type;

        GCost = float.MaxValue;
        HCost = 0;
        Parent = null;
    }
}

public class WorldCoordinateMap : MonoBehaviour
{
    public bool showGizmos;

    // >> COORDINATE MAP ================================ >>
    static List<WorldCoordinate> CoordinateMap = new List<WorldCoordinate>();

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
                    newCoord.ChunkType = WorldChunk.TYPE.CLOSED;
                }
                // Check if the position is on the border
                else if (x == 0 || y == 0 || x == xCoordCount - 1 || y == yCoordCount - 1)
                {
                    newCoord.ChunkType = WorldChunk.TYPE.BORDER;
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
        foreach (WorldCoordinate coord in CoordinateMap) { 
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
            if (coord.ChunkType == WorldChunk.TYPE.BORDER || coord.ChunkType == WorldChunk.TYPE.EXIT)
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

    public static WorldCoordinate GetCoordinateAtWorldExit(WorldExit worldExit)
    {
        WorldDirection direction = worldExit.edgeDirection;
        int index = worldExit.edgeIndex;

        List<WorldCoordinate> borderCoords = GetCoordinatesOnBorderDirection(direction);
        if (borderCoords.Count > index)
        {
            return borderCoords[index];
        }
        return null;
    }

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

        Debug.Log("Find Coord Path ");
        int iterations = 0;
        while (openSet.Count > 0 && iterations < 1000)
        {
            iterations++;
            if (iterations % 100 == 0)
            {
                Debug.Log($"Coordinate Pathfinding Iteration {iterations}");
            }

            WorldCoordinate currentCoordinate = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {

                // Check if best option
                if (openSet[i].FCost <= currentCoordinate.FCost && 
                    openSet[i].HCost < currentCoordinate.HCost &&
                    openSet[i].ChunkType == WorldChunk.TYPE.EMPTY)
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
            else if (iterations == 999)
            {
                Debug.Log($"BREAK Coordinate Pathfinding at Iteration {iterations}");
                return RetracePath(startCoordinate, currentCoordinate);

            }

            List<WorldCoordinate> currentNeighbors = GetAllCoordinateNeighbors(currentCoordinate);
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
                        openSet.Add(neighbor);
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
        path.Reverse();
        return path;
    }

    public static List<WorldCoordinate> GetAllCoordinateNeighbors(WorldCoordinate coordinate)
    {
        List<WorldCoordinate> worldCoordinates = GetCoordinateMap();
        List<WorldCoordinate> neighbors = new List<WorldCoordinate>(new WorldCoordinate[4]);
        int chunkWidth = WorldGeneration.GetRealChunkAreaSize().x;
        int chunkLength = WorldGeneration.GetRealChunkAreaSize().y;

        // Calculate neighbor positions
        Vector2 leftPosition = coordinate.Position + new Vector2(-chunkWidth, 0);
        Vector2 rightPosition = coordinate.Position + new Vector2(chunkWidth, 0);
        Vector2 forwardPosition = coordinate.Position + new Vector2(0, chunkLength);
        Vector2 backwardPosition = coordinate.Position + new Vector2(0, -chunkLength);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = worldCoordinates.Find(c => c.Position == leftPosition);     // Left
        neighbors[1] = worldCoordinates.Find(c => c.Position == rightPosition);    // Right
        neighbors[2] = worldCoordinates.Find(c => c.Position == forwardPosition);  // Forward
        neighbors[3] = worldCoordinates.Find(c => c.Position == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public static float GetCoordinateDistance(WorldCoordinate a, WorldCoordinate b)
    {
        return Vector2.Distance(a.Position, b.Position);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!showGizmos) { return; }

        // Draw Coordinates
        foreach (WorldCoordinate coord in GetCoordinateMap())
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(coord.WorldPosition, Vector3.one);
        }
    }
}
