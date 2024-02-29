using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum MapBorder { NORTH, SOUTH, EAST, WEST }


public class CoordinateMap
{
    bool _initialized;
    public bool IsInitialized() {  return _initialized; }

    // 
    Coordinate[][] _coordinateMap;

    // >> _coordinateMap reference lists
    HashSet<Vector2Int> _positions = new();
    HashSet<Coordinate> _coordinates = new();
    Dictionary<Vector2Int, Coordinate> _positionMap = new();
    Dictionary<Coordinate.TYPE, HashSet<Coordinate>> _typeMap = new();
    Dictionary<MapBorder, List<Vector2Int>> _borderPositionsMap = new(); // Enum , Sorted List of Border Coordinates

    public WorldRegion WorldRegion { get; private set; }
    public List<Coordinate> allCoordinates { get { return _coordinates.ToList(); } }


    public List<WorldExitPath> worldPaths = new List<WorldExitPath>();
    public List<WorldZone> worldZones = new List<WorldZone>();

    public CoordinateMap(WorldRegion region)
    {
        WorldRegion = region;

        int fullRegionWidth = WorldGeneration.GetFullRegionWidth_inChunks();
        int coordMax = fullRegionWidth;

        _coordinateMap = new Coordinate[coordMax][]; // initialize row

        // << CREATE REGION COORDINATES >>
        for (int x = 0; x < coordMax; x++)
        {
            _coordinateMap[x] = new Coordinate[coordMax]; // initialize column

            for (int y = 0; y < coordMax; y++)
            {
                Vector2Int newPosition = new Vector2Int(x, y);
                _positions.Add(newPosition);
                _coordinateMap[x][y] = new Coordinate(this, newPosition, region); // Create and store Region Coordinate
                _coordinates.Add(_coordinateMap[x][y]);
                _positionMap[newPosition] = _coordinateMap[x][y];
            }
        }

        // << ASSIGN COORDINATE TYPES >>
        // ** Set Coordinate To Type updates the TypeMap accordingly

        // >> initialize _border positions
        _borderPositionsMap[MapBorder.WEST] = new();
        _borderPositionsMap[MapBorder.EAST] = new();
        _borderPositionsMap[MapBorder.NORTH] = new();
        _borderPositionsMap[MapBorder.SOUTH] = new();

        // >> store coordinate range
        Vector2Int range = new Vector2Int(0, coordMax - 1);
        HashSet<Vector2Int> cornerCoordinates = new HashSet<Vector2Int>() {
            new Vector2Int(range.x, range.x), // 0 0
            new Vector2Int(range.y, range.y), // max max
            new Vector2Int(range.x, range.y), // 0 max
            new Vector2Int(range.y, range.x)  // max 0
            };

        // >> iterate through positions
        foreach (Vector2Int pos in _positions)
        {
            if (cornerCoordinates.Contains(pos))
            {
                // Set Type to Closed
                SetCoordinateToType(_positionMap[pos], Coordinate.TYPE.CLOSED);
            }
            else if ( pos.x == range.x || pos.x == range.y || pos.y == range.x || pos.y == range.y)
            {
                // Set Type to Border
                SetCoordinateToType(_positionMap[pos], Coordinate.TYPE.BORDER);

                // Set Border Map
                if (pos.x == range.x) { _borderPositionsMap[MapBorder.WEST].Add(pos); } // WEST
                if (pos.x == range.y) { _borderPositionsMap[MapBorder.EAST].Add(pos); } // EAST
                if (pos.y == range.x) { _borderPositionsMap[MapBorder.NORTH].Add(pos); } // NORTH
                if (pos.y == range.y) { _borderPositionsMap[MapBorder.SOUTH].Add(pos); } // SOUTH
            }
            else
            {
                // Set Type to Null
                SetCoordinateToType(_positionMap[pos], Coordinate.TYPE.NULL); 
            }
        }

        _initialized = true;
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

    public bool SetCoordinateToType(Coordinate coordinate, Coordinate.TYPE type)
    {
        // Remove coordinate from previous type reference
        if (_typeMap.ContainsKey(coordinate.type))
        {
            _typeMap[coordinate.type].Remove(coordinate);
        }

        // Assign type
        coordinate.type = type;
        switch (type)
        {
            case Coordinate.TYPE.CLOSED: coordinate.debugColor = Color.black; break;
            case Coordinate.TYPE.BORDER: coordinate.debugColor = Color.black; break;
            case Coordinate.TYPE.NULL: coordinate.debugColor = Color.grey; break;
            case Coordinate.TYPE.EXIT: coordinate.debugColor = Color.red; break;
        }

        // If new TYPE key not found, create
        if (!_typeMap.ContainsKey(type))
        {
            _typeMap[type] = new HashSet<Coordinate> { coordinate };
        }
        // else if the map doesnt already include the coordinate
        else if (!_typeMap[type].Contains(coordinate))
        {
            _typeMap[type].Add(coordinate);
        }

        return true;
    }
    #endregion

    #region == GET MAP COORDINATES ================================ ////

    public Coordinate GetCoordinateAt(Vector2Int coordinate)
    {
        if (_initialized && _positions.Contains(coordinate))
        {
            return _positionMap[coordinate];
        }
        return null;
    }

    public List<Coordinate> GetAllCoordinatesOfType(Coordinate.TYPE type)
    {
        if (!_typeMap.ContainsKey(type)) { _typeMap[type] = new(); } 
        return _typeMap[type].ToList();
    }

    #endregion



    // =====================================================================================

    #region == HANDLE WORLD PATHS ======================================================== ////

    public void CreateWorldExitPath()
    {
        if (_initialized == false)
        {
            Debug.LogError("Cannot Create World Exit Path with Uninitialized WorldCoordinate Map");
            return;
        }

        WorldExit defaultStart = new WorldExit(WorldDirection.WEST, 0);
        WorldExit defaultEnd = new WorldExit(WorldDirection.EAST, 0);
        worldPaths.Add(new WorldExitPath(defaultStart, defaultEnd));
    }

    public void UpdateAllWorldExitPaths()
    {
        //Debug.Log($"Update All Paths :: forceReset {forceReset}");
        bool exitPathsAreInitialized = true;

        foreach (WorldExitPath exitPath in worldPaths) 
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
        if (_initialized == false)
        {
            Debug.LogError("Cannot Create World Zone with Uninitialized WorldCoordinate Map");
            return;
        }

        // Get Center Coordinate
        int centerX = Mathf.CeilToInt(WorldGeneration.GetFullRegionWidth_inChunks() / 2);
        int centerY = Mathf.CeilToInt(WorldGeneration.GetFullRegionWidth_inChunks() / 2);
        Coordinate centerCoordinate = GetCoordinateAt(new Vector2Int(centerX, centerY));

        Debug.Log($"Create Zone at {centerCoordinate.LocalPosition}");
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
        foreach (Vector2Int pos in _positions)
        {
            gCost[pos] = float.MaxValue;
        }
        gCost[startCoord] = 0;

        // Initialize the heuristic costs
        Dictionary<Vector2Int, float> fCost = new Dictionary<Vector2Int, float>();
        foreach (Vector2Int pos in _positions)
        {
            fCost[pos] = float.MaxValue;
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

            foreach (Coordinate neighbor in _positionMap[current].GetValidNaturalNeighbors())
            {
                if (closedSet.Contains(neighbor.LocalPosition) || !IsCoordinateValidForPathfinding(neighbor.LocalPosition))
                        continue; // Skip non-traversable neighbors and those already evaluated

                float tentativeGCost = gCost[current] + Vector2Int.Distance(current, neighbor.LocalPosition);

                if (tentativeGCost < gCost[neighbor.LocalPosition])
                {
                    // This path to neighbor is better than any previous one. Record it!
                    parents[neighbor.LocalPosition] = _positionMap[current];
                    gCost[neighbor.LocalPosition] = tentativeGCost;
                    fCost[neighbor.LocalPosition] = tentativeGCost + Vector2Int.Distance(neighbor.LocalPosition, endCoord);

                    if (!openSet.Contains(neighbor.LocalPosition))
                        openSet.Add(neighbor.LocalPosition);
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
            path.Add(_positionMap[currentCoord]);
            currentCoord = parents[currentCoord].LocalPosition; // Move to the parent coordinate
        }
        path.Add(_positionMap[startCoord]); // Add the start coordinate at the end
        path.Reverse(); // Reverse the list to start from the beginning

        return path;
    }

    public bool IsCoordinateValidForPathfinding(Vector2Int candidate)
    {
        // Check Types
        if (_positionMap[candidate] != null && 
           (_positionMap[candidate].type == Coordinate.TYPE.NULL 
           || _positionMap[candidate].type == Coordinate.TYPE.PATH ))
        {
            return true;
        }
        return false;
    }

    #endregion

}
