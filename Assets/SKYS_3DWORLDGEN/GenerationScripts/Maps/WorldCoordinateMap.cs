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
    public static bool coordNeighborsInitialized { get; private set; }
    public static bool exitPathsInitialized { get; private set; }
    public static bool zonesInitialized { get; private set; }

    public static List<WorldCoordinate> CoordinateList { get; private set; }
    public static Dictionary<Vector2Int, WorldCoordinate> CoordinateMap { get; private set; }

    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();
    public List<WorldZone> worldZones = new List<WorldZone>();

    #region == HANDLE COORDINATE MAP ================================ ////

    public void UpdateCoordinateMap()
    {
        StartCoroutine(UpdateRoutine());
    }

    IEnumerator UpdateRoutine()
    {
        zonesInitialized = false;
        exitPathsInitialized = false;

        if (!coordMapInitialized)
        {
            StartCoroutine(InitializationRoutine()); // Make sure CoordinateMap is initialized
        }
        yield return new WaitUntil(() => coordMapInitialized);
        yield return new WaitUntil(() => coordNeighborsInitialized);
        yield return new WaitUntil(() => WorldChunkMap.chunkMapInitialized);

        // Initialize Random Seed :: IMPORTANT To keep the same results per seed
        WorldGeneration.InitializeRandomSeed();

        ResetAllCoordinatesToDefault(); // Set all world coordinates to default values

        UpdateAllWorldZones(); // Update all world zones to new values
        yield return new WaitUntil(() => zonesInitialized);
        yield return new WaitForSeconds(0.5f);

        UpdateAllWorldExitPaths(); // Update all world paths to new values
        yield return new WaitUntil(() => exitPathsInitialized);



    }

    static IEnumerator InitializationRoutine(bool forceReset = false)
    {
        if (coordMapInitialized) yield return null;

        List<WorldCoordinate> newCoordList = new List<WorldCoordinate>();
        Dictionary<Vector2Int, WorldCoordinate> newCoordMap = new();

        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkArea();

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

        // Set Coordinate Map Type Defaults
        SetAllCoordinateTypesToDefault();

        coordMapInitialized = true;


        // Initialize individual Coordinates
        foreach (WorldCoordinate coord in CoordinateList) 
        { 
            coord.InitializeNeighborMap();
            yield return new WaitUntil(() => coord.foundNeighbors);
        }

        coordNeighborsInitialized = true;
    }

    static void SetAllCoordinateTypesToDefault()
    {
        CoordinateMap.ToList().ForEach(entry => entry.Value.type = WorldCoordinate.TYPE.NULL);

        ResetAllCoordinatesToDefault();
    }


    static void ResetAllCoordinatesToDefault()
    {
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkArea();

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


    public void DestroyCoordinateMap()
    {
        CoordinateList = new();
        CoordinateMap = new();

        coordMapInitialized = false;
        coordNeighborsInitialized = false;
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


    // << GET NEIGHBORS >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


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

    public void CreateWorldExitPath()
    {
        if (coordMapInitialized == false)
        {
            Debug.LogError("Cannot Create World Exit Path with Uninitialized WorldCoordinate Map");
            return;
        }

        WorldExit defaultStart = new WorldExit(WorldDirection.WEST, 0);
        WorldExit defaultEnd = new WorldExit(WorldDirection.EAST, 0);
        worldExitPaths.Add(new WorldExitPath(defaultStart, defaultEnd));
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

    public void UpdateAllWorldExitPaths()
    {
        //Debug.Log($"Update All Paths :: forceReset {forceReset}");
        bool exitPathsAreInitialized = true;

        foreach (WorldExitPath exitPath in worldExitPaths) 
        {
            exitPath.Reset();
            exitPath.EditorUpdate();

            // Check if the path is initialized
            if (exitPath.IsInitialized() == false)
            {
                exitPathsAreInitialized = false;
            }
        }

        if (exitPathsAreInitialized == false)
        {
            exitPathsInitialized = false;
            UpdateAllWorldExitPaths(); // Update again 
        }
        else
        {
            exitPathsInitialized = true;
        }
    }

    #endregion

    #region == HANDLE WORLD ZONES ==================================== ////
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
        worldZones.Add(new WorldZone(centerCoordinate, WorldZone.TYPE.NATURAL_CROSS));
    }

    void UpdateAllWorldZones()
    {
        bool zonesAreInitialized = true;
        foreach (WorldZone zone in worldZones) {

            zone.Initialize();

            // Check if the zone is initialized
            if (zone.IsInitialized() == false) 
            { 
                zonesAreInitialized = false;
            }
        }

        if (zonesAreInitialized == false) { 
            zonesInitialized = false;
            UpdateAllWorldZones();
        }
        else 
        { 
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

        // Initialize Random Seed :: IMPORTANT To keep the same results per seed
        WorldGeneration.InitializeRandomSeed();

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

            foreach (WorldCoordinate neighbor in CoordinateMap[current].GetValidNaturalNeighbors())
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
