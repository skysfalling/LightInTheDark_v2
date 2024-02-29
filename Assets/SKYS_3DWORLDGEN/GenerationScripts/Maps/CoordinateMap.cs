using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapBorder { NORTH, SOUTH, EAST, WEST }



public class CoordinateMap
{
    #region << STATIC FUNCTIONS <<
    public static Dictionary<WorldDirection, Vector2Int> _directionVectorMap = new() {
        { WorldDirection.NORTH, new Vector2Int(0, 1) },
        { WorldDirection.SOUTH, new Vector2Int(0, -1) },
        { WorldDirection.WEST, new Vector2Int(-1, 0) },
        { WorldDirection.EAST, new Vector2Int(1, 0) },
        { WorldDirection.NORTHWEST, new Vector2Int(-1, 1) },
        { WorldDirection.NORTHEAST, new Vector2Int(1, 1) },
        { WorldDirection.SOUTHWEST, new Vector2Int(-1, -1) },
        { WorldDirection.SOUTHEAST, new Vector2Int(1, -1) }
    };
    public static Vector2Int GetDirectionVector(WorldDirection direction)
    {
        return _directionVectorMap[direction];
    }
    public static WorldDirection? GetDirectionEnum(Vector2Int direction)
    {
        foreach (var pair in _directionVectorMap)
        {
            if (pair.Value == direction)
            {
                return pair.Key; // Return the WorldDirection if a match is found
            }
        }
        return null;
    }
    public static List<Vector2Int> CalculateNaturalNeighborPositions(Vector2Int center)
    {
        return new List<Vector2Int>()
        {
            center + _directionVectorMap[WorldDirection.NORTH],
            center + _directionVectorMap[WorldDirection.SOUTH],
            center + _directionVectorMap[WorldDirection.EAST],
            center + _directionVectorMap[WorldDirection.WEST]
        };
    }

    public static List<Vector2Int> CalculateDiagonalNeighborPositions(Vector2Int center)
    {
        return new List<Vector2Int>()
        {
            center + _directionVectorMap[WorldDirection.NORTHEAST],
            center + _directionVectorMap[WorldDirection.NORTHWEST],
            center + _directionVectorMap[WorldDirection.SOUTHEAST],
            center + _directionVectorMap[WorldDirection.SOUTHWEST]
        };
    }
    #endregion

    bool _initialized;
    public bool IsInitialized() {  return _initialized; }

    // >> main coordinate reference
    Coordinate[][] _coordinateMap;

    // >> _coordinateMap reference lists
    HashSet<Vector2Int> _positions = new();
    HashSet<Coordinate> _coordinates = new();
    Dictionary<Vector2Int, Coordinate> _positionMap = new();
    Dictionary<Coordinate.TYPE, HashSet<Coordinate>> _typeMap = new();
    Dictionary<MapBorder, List<Vector2Int>> _borderPositionsMap = new(); // Enum , Sorted List of Border Coordinates

    public WorldRegion WorldRegion { get; private set; }
    public List<Vector2Int> allPositions { get { return _positions.ToList(); } }
    public List<Coordinate> allCoordinates { get { return _coordinates.ToList(); } }

    public List<Vector2Int> exitPositions = new();
    public List<WorldPath> worldPaths = new List<WorldPath>();
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

    void SetCoordinateToType(Coordinate coordinate, Coordinate.TYPE type)
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
    }

    void SetCoordinateToType(Vector2Int position, Coordinate.TYPE type)
    {
        Coordinate coordinate = GetCoordinateAt(position);
        SetCoordinateToType(coordinate, type);
    }

    void SetCoordinatesToType(List<Vector2Int> positions,  Coordinate.TYPE type)
    {
        foreach (Vector2Int pos in positions)
        {
            if (_positions.Contains(pos))
            {
                SetCoordinateToType(pos, type);
            }
        }
    }

    public void ConvertCoordinateToExit(Coordinate coordinate)
    {
        if (coordinate == null) return;
        if (coordinate.type != Coordinate.TYPE.BORDER)
        {
            Debug.Log("Cannot convert non border coordinate to exit");
            return;
        }

        SetCoordinateToType(coordinate, Coordinate.TYPE.EXIT);
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

        Debug.Log($"Create Zone at {centerCoordinate.localPosition}");
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

    public bool IsCoordinateValidForPathfinding(Vector2Int candidate)
    {
        // Check Types
        if (_positionMap[candidate] != null &&
           (_positionMap[candidate].type == Coordinate.TYPE.NULL
           || _positionMap[candidate].type == Coordinate.TYPE.PATH))
        {
            return true;
        }
        return false;
    }

}
