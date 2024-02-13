using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum DebugColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }
public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }


public class WorldCoordinateMap : MonoBehaviour
{
    public static WorldCoordinateMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public static bool coordMapInitialized { get; private set; }
    public bool exitPathsInitialized = false;
    public bool zonesInitialized = false;
    bool _forceAllPathsReset = false;

    public static List<WorldCoordinate> CoordinateList { get; private set; }
    public static Dictionary<Vector2Int, WorldCoordinate> CoordinateMap { get; private set; }
    public static Dictionary<WorldCoordinate, List<WorldCoordinate>> CoordinateNeighborMap { get; private set; }
    public static Dictionary<WorldCoordinate.TYPE, List<WorldCoordinate>> CoordinateTypeMap { get; private set; }


    #region == HANDLE COORDINATE MAP ================================ ////

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

        SetAllCoordinateTypesToDefault();

        coordMapInitialized = true;
        Debug.Log("Coord Map initialized");
    }

    static void ResetAllCoordinatesToDefault()
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
                WorldCoordinate worldCoordinate = CoordinateMap[coordinateVector];

                // Check if the position is in a corner & close it
                if ((x == 0 && y == 0) || (x == xCoordCount - 1 && y == 0) ||
                    (x == 0 && y == yCoordCount - 1) || (x == xCoordCount - 1 && y == yCoordCount - 1))
                {
                    worldCoordinate.type = WorldCoordinate.TYPE.CLOSED;
                    worldCoordinate.debugColor = Color.black;
                }
                // Check if the position is on the border
                else if (x == 0 || y == 0 || x == xCoordCount - 1 || y == yCoordCount - 1)
                {
                    worldCoordinate.type = WorldCoordinate.TYPE.BORDER;
                    worldCoordinate.debugColor = Color.black;

                    if (x == 0) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.WEST; }
                    if (y == 0) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.SOUTH; }
                    if (x == xCoordCount - 1) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.EAST; }
                    if (y == yCoordCount - 1) { CoordinateMap[coordinateVector].borderEdgeDirection = WorldDirection.NORTH; }
                }
                else
                {
                    worldCoordinate.type = WorldCoordinate.TYPE.NULL;
                    worldCoordinate.debugColor = Color.grey;
                }
            }
        }
    }

    static void SetAllCoordinateTypesToDefault()
    {
        CoordinateMap.ToList().ForEach(entry => entry.Value.type = WorldCoordinate.TYPE.NULL);

        ResetAllCoordinatesToDefault();
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
        StartCoroutine(UpdateRoutine());
    }

    IEnumerator UpdateRoutine()
    {
        InitializeCoordinateMap(); // Make sure CoordinateMap is initialized

        yield return new WaitUntil(() => coordMapInitialized);

        // Initialize Random Seed :: IMPORTANT To keep the same results per seed
        WorldGeneration.InitializeRandomSeed();

        // Set all coordinate to default values
        ResetAllCoordinatesToDefault();

        UpdateAllWorldZones(); // Update all world zones to new values

        yield return new WaitForSeconds(0.5f);
        UpdateAllWorldExitPaths(_forceAllPathsReset);
    }

    #endregion

    #region == GET MAP COORDINATES ================================ ////

    public static WorldCoordinate GetCoordinateAt(Vector2Int coordinate)
    {
        if (!coordMapInitialized) { return null; }

        // Use the dictionary for fast lookups
        if (CoordinateMap.TryGetValue(coordinate, out WorldCoordinate foundCoord))
        {
            return foundCoord;
        }
        return null;
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

    public static List<WorldCoordinate.TYPE> GetCoordinateTypesFromList(List<WorldCoordinate> worldCoords)
    {
        List<WorldCoordinate.TYPE> typeList = new();
        foreach (WorldCoordinate coord in worldCoords)
        {
            typeList.Add(CoordinateMap[coord.Coordinate].type); // Get type from map
        }
        return typeList;
    }

    public static List<WorldCoordinate> GetCoordinatesOnBorder(WorldDirection direction)
    {
        List<WorldCoordinate> coordinatesOnEdge = new List<WorldCoordinate>();
        List<WorldCoordinate> borderMap = GetAllCoordinatesOfType(WorldCoordinate.TYPE.BORDER);
        foreach (WorldCoordinate coord in borderMap)
        {
            if (coord.borderEdgeDirection == direction)
            {
                coordinatesOnEdge.Add(coord);
            }
        }
        return coordinatesOnEdge;
    }

    public static WorldCoordinate GetCoordinateNeighborInDirection(WorldCoordinate worldCoordinate, WorldDirection direction)
    {
        if (worldCoordinate == null) { return null; }

        WorldCoordinate neighborCoordinate = null;
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

        neighborCoordinate = GetCoordinateAt(worldCoordinate.Coordinate + directionVector);

        return neighborCoordinate;
    }

    public static List<WorldCoordinate> GetCoordinateNaturalNeighbors(WorldCoordinate worldCoord)
    {
        List<WorldCoordinate> neighbors = new List<WorldCoordinate>(new WorldCoordinate[4]);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.WEST);
        neighbors[1] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.EAST);
        neighbors[2] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.NORTH);
        neighbors[3] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.SOUTH);

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public static List<WorldCoordinate> GetCoordinateDiagonalNeighbors(WorldCoordinate worldCoord)
    {
        List<WorldCoordinate> neighbors = new List<WorldCoordinate>(new WorldCoordinate[4]);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.NORTHEAST);
        neighbors[1] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.NORTHWEST);
        neighbors[2] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.SOUTHEAST);
        neighbors[3] = GetCoordinateNeighborInDirection(worldCoord, WorldDirection.SOUTHWEST);

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

    public static List<WorldCoordinate> GetAllCoordinateNeighbors(Vector2Int coordinate)
    {
        WorldCoordinate coord = CoordinateMap[coordinate];
        List<WorldCoordinate> neighbors = GetCoordinateNaturalNeighbors(coord);
        neighbors.AddRange(GetCoordinateDiagonalNeighbors(coord));
        return neighbors;
    }




    #endregion

    #region == SET MAP COORDINATES ================================================================ ///
    public static bool SetMapCoordinateToType(WorldCoordinate worldCoord, WorldCoordinate.TYPE type, Color? debugColor = null)
    {
        if (!coordMapInitialized || worldCoord == null) { return false; }
        CoordinateMap[worldCoord.Coordinate].type = type;

        if (debugColor.HasValue) { worldCoord.debugColor = debugColor.Value; }

        return true;
    }
    public static bool SetMapCoordinatesToType(List<WorldCoordinate> coords, WorldCoordinate.TYPE conversionType, Color? debugColor = null)
    {
        if (!coordMapInitialized) { return false; }
        foreach (WorldCoordinate coordinate in coords)
        {
            SetMapCoordinateToType(coordinate, conversionType, debugColor);
        }
        return true;
    }
    #endregion

    // =====================================================================================

    #region == HANDLE WORLD PATHS ======================================================== ////
    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();

    public void CreateWorldExitPath()
    {
        if (coordMapInitialized == false)
        {
            Debug.LogError("Cannot Create World Exit Path with Uninitialized WorldCoordinate Map");
            return;
        }

        WorldExit defaultStart = new WorldExit(WorldDirection.WEST, 0);
        WorldExit defaultEnd = new WorldExit(WorldDirection.EAST, 9);
        worldExitPaths.Add(new WorldExitPath(defaultStart, defaultEnd));
    }

    public static WorldCoordinate GetWorldExitPathConnection(WorldExit exit)
    {
        switch (exit.borderDirection)
        {
            case WorldDirection.WEST:
                return GetCoordinateNeighborInDirection(exit.worldCoordinate, WorldDirection.EAST);
            case WorldDirection.EAST:
                return GetCoordinateNeighborInDirection(exit.worldCoordinate, WorldDirection.WEST);
            case WorldDirection.NORTH:
                return GetCoordinateNeighborInDirection(exit.worldCoordinate, WorldDirection.SOUTH);
            case WorldDirection.SOUTH:
                return GetCoordinateNeighborInDirection(exit.worldCoordinate, WorldDirection.NORTH);
        }

        return null;
    }

    public static WorldCoordinate GetCoordinateAtWorldExit(WorldDirection direction, int index)
    {
        List<WorldCoordinate> borderCoords = GetCoordinatesOnBorder(direction);
        if (borderCoords.Count > index)
        {
            return borderCoords[index];
        }
        return null;
    }

    public void UpdateAllWorldExitPaths(bool forceReset = false)
    {
        Debug.Log($"Update All Paths :: forceReset {forceReset}");


        bool pathsAreInitialized = true;
        foreach (WorldExitPath path in worldExitPaths) 
        {
            path.Reset(forceReset);
            path.EditorUpdate();

            // Check if the path is initialized
            if (path.IsInitialized() == false)
            {
                pathsAreInitialized = false;
            }
        }

        if (pathsAreInitialized == false)
        {
            exitPathsInitialized = false;
            UpdateAllWorldExitPaths(true);
        }
        else
        {
            // Debug.Log($"Force Paths Reset {forceReset}");
            _forceAllPathsReset = false; // Paths have been reset
            exitPathsInitialized = true;
        }
    }

    #endregion

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
        WorldCoordinate centerCoordinate = GetCoordinateAt(new Vector2Int(centerX, centerY));

        Debug.Log($"Create Zone at {centerCoordinate.Coordinate}");
        worldZones.Add(new WorldZone(centerCoordinate, WorldZone.TYPE.NATURAL));
    }

    void UpdateAllWorldZones()
    {
        bool zonesAreInitialized = true;
        foreach (WorldZone zone in worldZones) {
            zone.Reset();
            zone.Update();


            // Check if the zone is initialized
            if (zone.IsInitialized() == false) 
            { 
                zonesAreInitialized = false; 
            }

        }

        if (zonesAreInitialized == false) { 
            _forceAllPathsReset = true;
            zonesInitialized = false;
        }
        else { 
            zonesInitialized = true; 
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

    public static List<WorldCoordinate> FindWorldCoordinatePath(Vector2Int startCoord, Vector2Int endCoord, float pathRandomness = 0)
    {
        // A* Pathfinding implementation
        // gCost is the known cost from the starting node
        // hCost is the estimated distance to the end node
        // fCost is gCost + hCost

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
                    IsCoordinateValidForPathfinding(candidate) &&
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
                if (closedSet.Contains(neighbor.Coordinate) || !IsCoordinateValidForPathfinding(neighbor.Coordinate))
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

    public static bool IsCoordinateValidForPathfinding(Vector2Int candidate)
    {
        // Check Types
        if (CoordinateMap[candidate] != null && 
           (CoordinateMap[candidate].type == WorldCoordinate.TYPE.NULL 
           || CoordinateMap[candidate].type == WorldCoordinate.TYPE.PATH ))
        {
            return true;
        }
        return false;
    }

    #endregion


}
