using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class WorldCoordinateMap : MonoBehaviour
{
    public static WorldCoordinateMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public static bool coordMapInitialized { get; private set; }
    bool _forceAllPathsReset = false;

    #region == COORDINATE MAP ================================ ////
    public static List<WorldCoordinate> CoordinateList { get; private set; }
    public static Dictionary<Vector2Int, WorldCoordinate> CoordinateMap { get; private set; }
    public static Dictionary<WorldCoordinate, List<WorldCoordinate>> CoordinateNeighborMap { get; private set; }

    static void InitializeCoordinateMap(bool forceReset = false)
    {
        if (coordMapInitialized) return;

        List<WorldCoordinate> newCoordList = new List<WorldCoordinate>();
        Dictionary<Vector2Int, WorldCoordinate> newCoordMap = new();

        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();

        int xCoordCount = Mathf.CeilToInt(realFullWorldSize.x / realChunkAreaSize.x);
        int yCoordCount = Mathf.CeilToInt(realFullWorldSize.y / realChunkAreaSize.y);

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                WorldCoordinate newCoord = new WorldCoordinate(new Vector2Int(x, y));
                newCoordList.Add(newCoord);
                newCoordMap[newCoord.Coordinate] = newCoord;
            }
        }

        // Set Coordinate Map
        CoordinateList = newCoordList;
        CoordinateMap = newCoordMap;

        coordMapInitialized = true;

        SetAllCoordinateTypesToDefault();
    }

    static void ResetBorderCoordinatesToDefault()
    {
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();

        int xCoordCount = Mathf.CeilToInt(realFullWorldSize.x / realChunkAreaSize.x);
        int yCoordCount = Mathf.CeilToInt(realFullWorldSize.y / realChunkAreaSize.y);

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                Vector2Int coordinateVector = new Vector2Int(x, y);

                // Check if the position is in a corner & close it
                if ((x == 0 && y == 0) || (x == xCoordCount - 1 && y == 0) ||
                    (x == 0 && y == yCoordCount - 1) || (x == xCoordCount - 1 && y == yCoordCount - 1))
                {
                    CoordinateMap[coordinateVector].type = WorldCoordinate.TYPE.CLOSED;
                }
                // Check if the position is on the border
                else if (x == 0 || y == 0 || x == xCoordCount - 1 || y == yCoordCount - 1)
                {
                    CoordinateMap[coordinateVector].type = WorldCoordinate.TYPE.BORDER;
                    if (x == 0) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.West; }
                    if (y == 0) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.South; }
                    if (x == xCoordCount - 1) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.East; }
                    if (y == yCoordCount - 1) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.North; }
                }
            }
        }
    }

    static void SetAllCoordinateTypesToDefault()
    {
        CoordinateMap.ToList().ForEach(entry => entry.Value.type = WorldCoordinate.TYPE.NULL);

        ResetBorderCoordinatesToDefault();
    }

    public void DestroyCoordinateMap()
    {
        CoordinateList = new();
        CoordinateMap = new();
        coordMapInitialized = false;

        // Destroy World Zones
        worldZones = new List<WorldZone>();

        // Destroy World Paths
        worldExitPaths = new List<WorldExitPath>();
    }

    public void UpdateCoordinateMap()
    {
        InitializeCoordinateMap(); // Make sure CoordinateMap is initialized

        // Initialize Random Seed :: IMPORTANT To keep the same results per seed
        WorldGeneration.InitializeRandomSeed();

        ResetBorderCoordinatesToDefault();

        UpdateAllWorldZones(); // Update all world zones to new values

        Invoke("DelayedPathsUpdate", 1);
        
    }

    void DelayedPathsUpdate() { UpdateAllWorldExitPaths(_forceAllPathsReset); }

    #endregion

    #region == GET COORDINATES ================================ ////

    public static List<Vector2> GetCoordinateMapPositions()
    {
        List<WorldCoordinate> coordMap = CoordinateList;
        List<Vector2> coordMapPositions = new List<Vector2>();

        foreach (WorldCoordinate coord in coordMap) { coordMapPositions.Add(coord.Position); }
        return coordMapPositions;
    }

    public static WorldCoordinate GetCoordinate(Vector2Int coordinate)
    {
        if (!coordMapInitialized) { return null; }
        // Use the dictionary for fast lookups
        if (CoordinateMap.TryGetValue(coordinate, out WorldCoordinate foundCoord))
        {
            return foundCoord;
        }
        return null;
    }

    public static WorldCoordinate GetCoordinateAtPosition(Vector2 position)
    {
        foreach (WorldCoordinate coord in CoordinateList) { 
            if (coord.Position == position)
            {
                return coord;
            }
        }
        return null;
    }

    public static WorldCoordinate.TYPE GetTypeAtCoord(WorldCoordinate coordinate)
    {
        return GetCoordinate(coordinate.Coordinate).type;
    }

    public static List<WorldCoordinate> GetAllCoordinatesOfType(WorldCoordinate.TYPE type)
    {
        List<WorldCoordinate> typeList = new();
        foreach(WorldCoordinate coord in CoordinateList)
        {
            if (coord.type == type)
            {
                typeList.Add(coord);
            }
        }
        return typeList;
    }

    public static List<WorldCoordinate> GetCoordinatesOnBorderDirection(WorldDirection edgeDirection)
    {
        List<WorldCoordinate> coordinatesOnEdge = new List<WorldCoordinate>();
        List<WorldCoordinate> borderMap = GetAllCoordinatesOfType(WorldCoordinate.TYPE.BORDER);
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

    #region // SET MAP COORDINATES ================================================================ ///
    public static void SetMapCoordinateToType(WorldCoordinate coord, WorldCoordinate.TYPE type)
    {
        CoordinateMap[coord.Coordinate].type = type;
    }

    public static List<WorldCoordinate> SetMapCoordinatesToType(List<WorldCoordinate> coords, WorldCoordinate.TYPE conversionType)
    {
        foreach (WorldCoordinate coordinate in coords)
        {
            SetMapCoordinateToType(coordinate, conversionType);
        }
        return coords;
    }
    #endregion


    #region == {{ WORLD EXIT PATHS }} ======================================================== ////
    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();

    public void CreateWorldExitPath()
    {
        if (coordMapInitialized == false)
        {
            Debug.LogError("Cannot Create World Exit Path with Uninitialized WorldCoordinate Map");
            return;
        }

        WorldExit defaultStart = new WorldExit(WorldDirection.West, 5);
        WorldExit defaultEnd = new WorldExit(WorldDirection.East, 5);
        worldExitPaths.Add(new WorldExitPath(defaultStart, defaultEnd));
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
    
    public void UpdateAllWorldExitPaths(bool forceReset = false)
    {
        foreach (WorldExitPath path in worldExitPaths) 
        {
            path.Reset(forceReset);
            path.Update(); 
        }

        Debug.Log($"Force Paths Reset {forceReset}");
        _forceAllPathsReset = false; // Paths have been reset
    }

    #endregion

    // =====================================================================================

    #region == WORLD ZONES ==================================== ////
    public List<WorldZone> worldZones = new List<WorldZone>();
    public void CreateWorldZone()
    {
        if (coordMapInitialized == false)
        {
            Debug.LogError("Cannot Create World Zone with Uninitialized WorldCoordinate Map");
            return;
        }

        // Get Center Coordinate
        int centerX = Mathf.CeilToInt(WorldGeneration.GetFullWorldArea().x / 2);
        int centerY = Mathf.CeilToInt(WorldGeneration.GetFullWorldArea().y / 2);
        WorldCoordinate centerCoordinate = GetCoordinate(new Vector2Int(centerX, centerY));

        Debug.Log($"Create Zone at {centerCoordinate.Coordinate}");
        worldZones.Add(new WorldZone(centerCoordinate, WorldZone.TYPE.NATURAL));
    }

    void UpdateAllWorldZones()
    {
        foreach (WorldZone zone in worldZones) { 
            zone.Reset();

            // Force Reset Paths
            if (zone.IsInitialized() == false) { _forceAllPathsReset = true; }

            zone.Update(); 
        }
    }

    public bool IsCoordinateInZone(WorldCoordinate coord)
    {
        foreach (WorldZone zone in worldZones)
        {
            if (zone.GetZoneCoordinates().Contains(coord)) { return true; }
        }
        return false;
    }
    #endregion

    #region == COORDINATE PATHFINDING =================================///
    /* A* Pathfinding implementation
    * - gCost is the known cost from the starting node
    * - hCost is the estimated distance to the end node
    * - fCost is gCost + hCost
    */

    public static List<WorldCoordinate> FindCoordinatePath(Vector2Int startCoord, Vector2Int endCoord, float pathRandomness = 0)
    {
        // Initialize the open set with the start coordinate
        List<Vector2Int> openSet = new List<Vector2Int> { startCoord };
        // Initialize the closed set as an empty collection of Vector2Int
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        // Initialize costs for all coordinates to infinity, except the start coordinate
        Dictionary<Vector2Int, float> gCost = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, WorldCoordinate> parents = new Dictionary<Vector2Int, WorldCoordinate>();
        foreach (var coord in CoordinateMap.Keys)
        {
            gCost[coord] = float.MaxValue;
        }
        gCost[startCoord] = 0;

        // Initialize the heuristic costs
        Dictionary<Vector2Int, float> fCost = new Dictionary<Vector2Int, float>();
        foreach (var coord in CoordinateMap.Keys)
        {
            fCost[coord] = float.MaxValue;
        }
        fCost[startCoord] = Vector2Int.Distance(startCoord, endCoord);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                Vector2Int candidate = openSet[i];
                // Convert FCost and HCost checks to work with Vector2Int by accessing WorldCoordinate properties
                if (fCost[candidate] <= fCost[current] &&
                    CoordinateMap[current].type == WorldCoordinate.TYPE.NULL &&
                    UnityEngine.Random.Range(0f, 1f) <= pathRandomness)
                {
                    current = openSet[i];
                }
            }

            if (current == endCoord)
            {
                // Path has been found
                return RetracePath(startCoord, endCoord, parents);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (WorldCoordinate neighbor in GetCoordinateNaturalNeighbors(CoordinateMap[current]))
            {
                if (closedSet.Contains(neighbor.Coordinate) || CoordinateMap[neighbor.Coordinate].type != WorldCoordinate.TYPE.NULL) // Remove invalid neighbors
                    continue; // Skip non-traversable neighbors and those already evaluated

                float tentativeGCost = gCost[current] + Vector2Int.Distance(current, neighbor.Coordinate);

                if (tentativeGCost < gCost[neighbor.Coordinate])
                {
                    // This path to neighbor is better than any previous one. Record it!
                    parents[neighbor.Coordinate] = CoordinateMap[current];
                    gCost[neighbor.Coordinate] = tentativeGCost;
                    fCost[neighbor.Coordinate] = tentativeGCost + Vector2Int.Distance(neighbor.Coordinate, endCoord);

                    if (!openSet.Contains(neighbor.Coordinate))
                        openSet.Add(neighbor.Coordinate);
                }
            }
        }

        // If we reach here, then there is no path
        return new List<WorldCoordinate>();
    }

    // Helper method to retrace path from end to start using parent references
    static List<WorldCoordinate> RetracePath(Vector2Int startCoord, Vector2Int endCoord, Dictionary<Vector2Int, WorldCoordinate> parents)
    {
        List<WorldCoordinate> path = new List<WorldCoordinate>();
        Vector2Int currentCoord = endCoord;

        while (currentCoord != startCoord)
        {
            path.Add(CoordinateMap[currentCoord]);
            currentCoord = parents[currentCoord].Coordinate; // Move to the parent coordinate
        }
        path.Add(CoordinateMap[startCoord]); // Add the start coordinate at the end
        path.Reverse(); // Reverse the list to start from the beginning

        return path;
    }

    public static float GetCoordinateDistance(WorldCoordinate a, WorldCoordinate b)
    {
        // This link has more heuristics :: https://www.enjoyalgorithms.com/blog/a-star-search-algorithm
        return Vector2.Distance(a.Position, b.Position);
    }
    #endregion


}
