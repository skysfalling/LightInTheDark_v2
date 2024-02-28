using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoordinateMap
{
    public bool coordMapInitialized = false;
    public bool coordNeighborsInitialized = false;
    public List<Coordinate> CoordinateList { get; private set; }
    public Dictionary<Vector2Int, Coordinate> CoordinateValueMap { get; private set; }

    public List<Coordinate> coordinates { get { return CoordinateList; } private set { } }
    public List<WorldExitPath> worldExitPaths = new List<WorldExitPath>();
    public List<WorldZone> worldZones = new List<WorldZone>();

    public CoordinateMap(WorldRegion region)
    {
        List<Coordinate> newCoordList = new List<Coordinate>();
        Dictionary<Vector2Int, Coordinate> newCoordMap = new();

        int fullRegionWidth = WorldGeneration.GetFullRegionWidth_inChunks();
        int xCoordCount = fullRegionWidth;
        int yCoordCount = fullRegionWidth;

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                Coordinate newCoord = new Coordinate(this, new Vector2Int(x, y), region);
                newCoordList.Add(newCoord);
                newCoordMap[newCoord.LocalCoordinate] = newCoord;
            }
        }

        // Set Coordinate Map
        CoordinateList = newCoordList;
        CoordinateValueMap = newCoordMap;

        coordMapInitialized = true;
        coordNeighborsInitialized = true;
    }

    #region == HANDLE COORDINATE MAP ================================ ////

    /*
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

        //UpdateAllWorldZones(); // Update all world zones to new values
        yield return new WaitUntil(() => zonesInitialized);
        yield return new WaitForSeconds(0.5f);

        //UpdateAllWorldExitPaths(); // Update all world paths to new values
        yield return new WaitUntil(() => exitPathsInitialized);
    }
    */



    void SetAllCoordinateTypesToDefault()
    {
        CoordinateValueMap.ToList().ForEach(entry => entry.Value.type = Coordinate.TYPE.NULL);

        ResetAllCoordinatesToDefault();
    }


    void ResetAllCoordinatesToDefault()
    {
        foreach (Coordinate coord in CoordinateList)
        {
            Vector2Int coordinateVector = coord.LocalCoordinate;
            Coordinate worldCoordinate = CoordinateValueMap[coordinateVector];

            int coordMax = WorldGeneration.GetFullRegionWidth_inChunks();

            // Check if the position is in a corner & close it
            if ((coordinateVector == Vector2Int.zero) 
                || (coordinateVector == new Vector2Int(coordMax - 1, 0))
                || coordinateVector == new Vector2Int(0, coordMax - 1) 
                || coordinateVector == new Vector2Int(coordMax - 1, coordMax - 1))
                {
                worldCoordinate.type = Coordinate.TYPE.CLOSED;
                worldCoordinate.debugColor = Color.black;
            }
            // Check if the position is on the border
            else if (coordinateVector.x == 0 || coordinateVector.y == 0 
                || coordinateVector.x == coordMax - 1 || coordinateVector.y == coordMax - 1)
            {
                worldCoordinate.type = Coordinate.TYPE.BORDER;
                worldCoordinate.debugColor = Color.black;

                if (coordinateVector.x == 0) { CoordinateValueMap[coordinateVector].borderEdgeDirection = WorldDirection.WEST; }
                if (coordinateVector.y == 0) { CoordinateValueMap[coordinateVector].borderEdgeDirection = WorldDirection.SOUTH; }
                if (coordinateVector.x == coordMax - 1) { CoordinateValueMap[coordinateVector].borderEdgeDirection = WorldDirection.EAST; }
                if (coordinateVector.y == coordMax - 1) { CoordinateValueMap[coordinateVector].borderEdgeDirection = WorldDirection.NORTH; }
            }
            else
            {
                worldCoordinate.type = Coordinate.TYPE.NULL;
                worldCoordinate.debugColor = Color.grey;
            }
        }
    }


    public void DestroyCoordinateMap()
    {
        CoordinateList = new();
        CoordinateValueMap = new();
    }

    #endregion

    #region == GET MAP COORDINATES ================================ ////

    public Coordinate GetCoordinateAt(Vector2Int coordinate)
    {
        if (!coordMapInitialized) { return null; }

        // Use the dictionary for fast lookups
        if (CoordinateValueMap.TryGetValue(coordinate, out Coordinate foundCoord))
        {
            return foundCoord;
        }
        return null;
    }

    public List<Coordinate> GetAllCoordinatesOfType(Coordinate.TYPE type)
    {
        List<Coordinate> typeList = new();
        foreach(Coordinate coord in CoordinateList)
        {
            if (coord.type == type)
            {
                typeList.Add(coord);
            }
        }
        return typeList;
    }

    public List<Coordinate.TYPE> GetCoordinateTypesFromList(List<Coordinate> worldCoords)
    {
        List<Coordinate.TYPE> typeList = new();
        foreach (Coordinate coord in worldCoords)
        {
            typeList.Add(CoordinateValueMap[coord.LocalCoordinate].type); // Get type from map
        }
        return typeList;
    }

    public List<Coordinate> GetCoordinatesOnBorder(WorldDirection direction)
    {
        List<Coordinate> coordinatesOnEdge = new List<Coordinate>();
        List<Coordinate> borderMap = GetAllCoordinatesOfType(Coordinate.TYPE.BORDER);
        foreach (Coordinate coord in borderMap)
        {
            if (coord.borderEdgeDirection == direction)
            {
                coordinatesOnEdge.Add(coord);
            }
        }
        return coordinatesOnEdge;
    }

    #endregion

    #region == SET MAP COORDINATES ================================================================ ///
    public bool SetMapCoordinateToType(Coordinate worldCoord, Coordinate.TYPE type, Color? debugColor = null)
    {
        if (!coordMapInitialized || worldCoord == null) { return false; }
        CoordinateValueMap[worldCoord.LocalCoordinate].type = type;

        if (debugColor.HasValue) { worldCoord.debugColor = debugColor.Value; }

        return true;
    }
    public bool SetMapCoordinatesToType(List<Coordinate> coords, Coordinate.TYPE conversionType, Color? debugColor = null)
    {
        if (!coordMapInitialized) { return false; }
        foreach (Coordinate coordinate in coords)
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

    public Coordinate GetCoordinateAtWorldExit(WorldDirection direction, int index)
    {
        List<Coordinate> borderCoords = GetCoordinatesOnBorder(direction);
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
            //exitPathsInitialized = false;
            UpdateAllWorldExitPaths(); // Update again 
        }
        else
        {
            //exitPathsInitialized = true;
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
        int centerX = Mathf.CeilToInt(WorldGeneration.GetFullRegionWidth_inChunks() / 2);
        int centerY = Mathf.CeilToInt(WorldGeneration.GetFullRegionWidth_inChunks() / 2);
        Coordinate centerCoordinate = GetCoordinateAt(new Vector2Int(centerX, centerY));

        Debug.Log($"Create Zone at {centerCoordinate.LocalCoordinate}");
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
            //zonesInitialized = false;
            UpdateAllWorldZones();
        }
        else 
        { 
            //zonesInitialized = true;
        }
    }

    public bool IsCoordinateInZone(Coordinate coord)
    {
        foreach (WorldZone zone in worldZones)
        {
            if (zone.GetZoneCoordinates().Contains(coord)) { return true; }
        }
        return false;
    }
    #endregion

    #region == COORDINATE PATHFINDING =================================///

    public List<Coordinate> FindWorldCoordinatePath(Vector2Int startCoord, Vector2Int endCoord, float pathRandomness = 0)
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
        Dictionary<Vector2Int, Coordinate> parents = new Dictionary<Vector2Int, Coordinate>();
        foreach (var coord in CoordinateValueMap.Keys)
        {
            gCost[coord] = float.MaxValue;
        }
        gCost[startCoord] = 0;

        // Initialize the heuristic costs
        Dictionary<Vector2Int, float> fCost = new Dictionary<Vector2Int, float>();
        foreach (var coord in CoordinateValueMap.Keys)
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

            foreach (Coordinate neighbor in CoordinateValueMap[current].GetValidNaturalNeighbors())
            {
                if (closedSet.Contains(neighbor.LocalCoordinate) || !IsCoordinateValidForPathfinding(neighbor.LocalCoordinate))
                        continue; // Skip non-traversable neighbors and those already evaluated

                float tentativeGCost = gCost[current] + Vector2Int.Distance(current, neighbor.LocalCoordinate);

                if (tentativeGCost < gCost[neighbor.LocalCoordinate])
                {
                    // This path to neighbor is better than any previous one. Record it!
                    parents[neighbor.LocalCoordinate] = CoordinateValueMap[current];
                    gCost[neighbor.LocalCoordinate] = tentativeGCost;
                    fCost[neighbor.LocalCoordinate] = tentativeGCost + Vector2Int.Distance(neighbor.LocalCoordinate, endCoord);

                    if (!openSet.Contains(neighbor.LocalCoordinate))
                        openSet.Add(neighbor.LocalCoordinate);
                }
            }
        }

        // If we reach here, then there is no path
        return new List<Coordinate>();
    }

    // Helper method to retrace path from end to start using parent references
    List<Coordinate> RetracePath(Vector2Int startCoord, Vector2Int endCoord, Dictionary<Vector2Int, Coordinate> parents)
    {
        List<Coordinate> path = new List<Coordinate>();
        Vector2Int currentCoord = endCoord;

        while (currentCoord != startCoord)
        {
            path.Add(CoordinateValueMap[currentCoord]);
            currentCoord = parents[currentCoord].LocalCoordinate; // Move to the parent coordinate
        }
        path.Add(CoordinateValueMap[startCoord]); // Add the start coordinate at the end
        path.Reverse(); // Reverse the list to start from the beginning

        return path;
    }

    public bool IsCoordinateValidForPathfinding(Vector2Int candidate)
    {
        // Check Types
        if (CoordinateValueMap[candidate] != null && 
           (CoordinateValueMap[candidate].type == Coordinate.TYPE.NULL 
           || CoordinateValueMap[candidate].type == Coordinate.TYPE.PATH ))
        {
            return true;
        }
        return false;
    }

    #endregion

}
