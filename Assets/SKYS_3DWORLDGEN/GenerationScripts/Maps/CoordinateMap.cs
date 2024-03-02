using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
    Dictionary<Coordinate.TYPE, HashSet<Vector2Int>> _typeMap = new();
    Dictionary<MapBorder, List<Vector2Int>> _borderPositionsMap = new(); // Enum , Sorted List of Border Coordinates
    WorldGeneration _worldGeneration = null;
    WorldRegion _worldRegion = null;
    WorldChunk _worldChunk = null;

    // >> public access lists
    public List<Vector2Int> allPositions { get { return _positions.ToList(); } }
    public List<Coordinate> allCoordinates { get { return _coordinates.ToList(); } }
    public List<Vector2Int> exitPositions = new List<Vector2Int>();
    public List<WorldPath> worldPaths = new List<WorldPath>();
    public List<WorldZone> worldZones = new List<WorldZone>();

    public CoordinateMap(WorldGeneration worldGeneration)
    {
        _worldGeneration = worldGeneration;

        // << CREATE WORLD COORDINATES >> =================================================================

        int fullRegionWidth = WorldGeneration.WorldWidth_inRegions;
        int coordMax = fullRegionWidth;

        _coordinateMap = new Coordinate[coordMax][]; // initialize row
        for (int x = 0; x < coordMax; x++)
        {
            _coordinateMap[x] = new Coordinate[coordMax]; // initialize column

            for (int y = 0; y < coordMax; y++)
            {
                Vector2Int newPosition = new Vector2Int(x, y);
                _positions.Add(newPosition);
                _coordinateMap[x][y] = new Coordinate(this, newPosition, worldGeneration); // Create and store Region Coordinate
                _coordinates.Add(_coordinateMap[x][y]);
                _positionMap[newPosition] = _coordinateMap[x][y];
            }
        }

        SetAllCoordinatesToDefault(coordMax, WorldGeneration.BoundaryWallCount);

        _initialized = true;
    }

    public CoordinateMap(WorldRegion region)
    {
        _worldRegion = region;

        // << CREATE REGION COORDINATES >> =================================================================

        int fullRegionWidth = WorldGeneration.GetFullRegionWidth_inChunks();
        int coordMax = fullRegionWidth;

        _coordinateMap = new Coordinate[coordMax][]; // initialize row
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

        SetAllCoordinatesToDefault(coordMax, WorldGeneration.BoundaryWallCount);

        _initialized = true;
    }

    public CoordinateMap(WorldChunk chunk)
    {
        _worldChunk = chunk;

        // << CREATE REGION COORDINATES >> =================================================================

        int fullChunkWidth = WorldGeneration.ChunkWidth_inCells;
        int coordMax = fullChunkWidth;

        _coordinateMap = new Coordinate[coordMax][]; // initialize row
        for (int x = 0; x < coordMax; x++)
        {
            _coordinateMap[x] = new Coordinate[coordMax]; // initialize column

            for (int y = 0; y < coordMax; y++)
            {
                Vector2Int newPosition = new Vector2Int(x, y);
                _positions.Add(newPosition);
                _coordinateMap[x][y] = new Coordinate(this, newPosition, chunk); // Create and store Chunk Coordinate
                _coordinates.Add(_coordinateMap[x][y]);
                _positionMap[newPosition] = _coordinateMap[x][y];
            }
        }

        SetAllCoordinatesToDefault(coordMax, WorldGeneration.BoundaryWallCount);

        _initialized = true;
    }

    void SetAllCoordinatesToDefault(int coordMax, int borderOffset)
    {
        // << ASSIGN COORDINATE TYPES >> =================================================================
        // ** Set Coordinate To Type updates the TypeMap accordingly

        // >> initialize _border positions
        _borderPositionsMap[MapBorder.WEST] = new();
        _borderPositionsMap[MapBorder.EAST] = new();
        _borderPositionsMap[MapBorder.NORTH] = new();
        _borderPositionsMap[MapBorder.SOUTH] = new();

        // >> store coordinate range
        Vector2Int playableMapRange = new Vector2Int(borderOffset, coordMax - (borderOffset + 1));
        HashSet<Vector2Int> cornerCoordinates = new HashSet<Vector2Int>() {
            new Vector2Int(playableMapRange.x, playableMapRange.x), // 0 0
            new Vector2Int(playableMapRange.y, playableMapRange.y), // max max
            new Vector2Int(playableMapRange.x, playableMapRange.y), // 0 max
            new Vector2Int(playableMapRange.y, playableMapRange.x)  // max 0
            };

        // >> iterate through positions
        foreach (Vector2Int pos in _positions)
        {
            if (cornerCoordinates.Contains(pos) || pos.x < playableMapRange.x || pos.x > playableMapRange.y || pos.y < playableMapRange.x || pos.y > playableMapRange.y)
            {
                // Set Type to Closed
                SetCoordinateToType(pos, Coordinate.TYPE.CLOSED);
            }
            else if (pos.x == playableMapRange.x || pos.x == playableMapRange.y || pos.y == playableMapRange.x || pos.y == playableMapRange.y)
            {
                // Set Type to Border
                SetCoordinateToType(pos, Coordinate.TYPE.BORDER);

                // Set Border Map
                if (pos.x == playableMapRange.x) { _borderPositionsMap[MapBorder.WEST].Add(pos); } // WEST
                if (pos.x == playableMapRange.y) { _borderPositionsMap[MapBorder.EAST].Add(pos); } // EAST
                if (pos.y == playableMapRange.x) { _borderPositionsMap[MapBorder.NORTH].Add(pos); } // NORTH
                if (pos.y == playableMapRange.y) { _borderPositionsMap[MapBorder.SOUTH].Add(pos); } // SOUTH
            }
            else
            {
                // Set Type to Null
                SetCoordinateToType(pos, Coordinate.TYPE.NULL);
            }
        }
    }

    void SetCoordinateToType(Vector2Int position, Coordinate.TYPE newType)
    {
        if (!_positions.Contains(position)) return;

        // Remove Old Type
        Coordinate coordinate = _positionMap[position];
        Coordinate.TYPE currentType = coordinate.type;
        if (_typeMap.ContainsKey(currentType))
        {
            _typeMap[currentType].Remove(position);
        }

        // Assign New Type
        coordinate.type = newType;
        switch (newType)
        {
            case Coordinate.TYPE.CLOSED: coordinate.debugColor = Color.black; break;
            case Coordinate.TYPE.BORDER: coordinate.debugColor = Color.black; break;
            case Coordinate.TYPE.NULL: coordinate.debugColor = Color.grey; break;
            case Coordinate.TYPE.EXIT: coordinate.debugColor = Color.red; break;
            case Coordinate.TYPE.PATH: coordinate.debugColor = Color.white; break;
            case Coordinate.TYPE.ZONE: coordinate.debugColor = Color.green; break;
        }

        // If new TYPE key not found, create
        if (!_typeMap.ContainsKey(newType))
        {
            _typeMap[newType] = new HashSet<Vector2Int> { position };
        }
        // else if the map doesnt already include the coordinate
        else if (!_typeMap[newType].Contains(position))
        {
            _typeMap[newType].Add(position);
        }
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

    public Coordinate GetCoordinateAt(Vector2Int position)
    {
        if (_initialized && _positions.Contains(position))
        {
            return _positionMap[position];
        }
        return null;
    }

    public Coordinate.TYPE? GetCoordinateTypeAt(Vector2Int position)
    {
        if (_initialized && _positions.Contains(position))
        {
            return _positionMap[position].type;
        }
        return null;
    }

    public HashSet<Vector2Int> GetAllPositionsOfType(Coordinate.TYPE type)
    {
        if (!_typeMap.ContainsKey(type)) { _typeMap[type] = new(); } 
        return _typeMap[type];
    }

    // == [[ WORLD EXITS ]] ======================================================================== >>>>
    public void ConvertCoordinateToExit(Coordinate coordinate)
    {
        if (coordinate == null) return;
        if (coordinate.type != Coordinate.TYPE.BORDER)
        {
            Debug.Log("Cannot convert non border coordinate to exit");
            return;
        }

        exitPositions.Add(coordinate.localPosition);

        SetCoordinateToType(coordinate.localPosition, Coordinate.TYPE.EXIT);
    }

    public void GenerateRandomExits()
    {
        // Determine the number of exits to create, with a minimum of 2 and a maximum of 4
        int numberOfExits = UnityEngine.Random.Range(2, 5); // Unity's Random.Range is inclusive for min and exclusive for max

        // Combine all border positions into a single list for easier access
        List<Vector2Int> allBorderPositions = _typeMap[Coordinate.TYPE.BORDER].ToList();

        // Shuffle the list of all border positions using the Fisher-Yates shuffle algorithm
        for (int i = allBorderPositions.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            Vector2Int temp = allBorderPositions[i];
            allBorderPositions[i] = allBorderPositions[swapIndex];
            allBorderPositions[swapIndex] = temp;
        }

        // Ensure not to exceed the number of available border positions
        numberOfExits = Mathf.Min(numberOfExits, allBorderPositions.Count);

        // Convert the first N positions into exits, where N is the number of exits determined
        for (int i = 0; i < numberOfExits; i++)
        {
            Vector2Int exitPosition = allBorderPositions[i];
            Coordinate coordinate = _positionMap[exitPosition]; // Assuming _positionMap contains all coordinates
            ConvertCoordinateToExit(coordinate);
        }

        Debug.Log($"{numberOfExits} exits have been created on the map borders.");
    }

    // == [[ WORLD PATH ]] ================================================================================ >>>>
    public void CreatePathFrom(Vector2Int start, Vector2Int end)
    {
        WorldPath newPath = new WorldPath(this, start, end, 0.25f);
        worldPaths.Add(newPath);

        // Remove Exits from path positions
        List<Vector2Int> positions = newPath.positions;
        if (GetCoordinateTypeAt(start) == Coordinate.TYPE.EXIT) { positions.Remove(start); }
        if (GetCoordinateTypeAt(end) == Coordinate.TYPE.EXIT) { positions.Remove(end); }

        // Assign Path Type
        SetCoordinatesToType(newPath.positions, Coordinate.TYPE.PATH);
        Debug.Log($"Created Path from {start} to {end} with {newPath.positions.Count}");
    }

    public void GeneratePathsBetweenExits()
    {
        // Clear existing paths
        worldPaths.Clear();

        // Ensure there's more than one exit to connect
        if (exitPositions.Count < 2)
        {
            Debug.LogWarning("Not enough exits to generate paths.");
            return;
        }

        List<Vector2Int> sortedExits = exitPositions.OrderBy(pos => pos.x).ThenBy(pos => pos.y).ToList();
        for (int i = 0; i < sortedExits.Count - 1; i++)
        {
            Vector2Int start = sortedExits[i];
            Vector2Int end = sortedExits[i + 1]; // Connect to the next exit in the list

            CreatePathFrom(start, end);
        }

        //Connect the last exit back to the first to ensure all exits are interconnected
        CreatePathFrom(sortedExits[sortedExits.Count - 1], sortedExits[0]);

        Debug.Log($"Generated new paths connecting all exits.");
    }

    public bool IsCoordinateValidForPathfinding(Vector2Int candidate)
    {
        // Check Types
        if (_positionMap.ContainsKey(candidate) && _positionMap[candidate] != null 
            && (_positionMap[candidate].type == Coordinate.TYPE.NULL 
            || _positionMap[candidate].type == Coordinate.TYPE.PATH
            || _positionMap[candidate].type == Coordinate.TYPE.EXIT ))
        {
            return true;
        }
        return false;
    }

    // == [[ WORLD ZONES ]] ================================================================================ >>>>
    public bool CreateWorldZone(Vector2Int position, WorldZone.TYPE zoneType, int zoneHeight)
    {
        Debug.Log($"Attempting to create zone at {position}");

        // Temporarily create the zone to check its positions
        WorldZone tempZone = new WorldZone(this, GetCoordinateAt(position), zoneType, zoneHeight);

        // Check if any of the zone's positions are in the BORDER or CLOSED categories
        HashSet<Vector2Int> validPositions = GetAllPositionsOfType(Coordinate.TYPE.NULL);

        // Check for intersection between the zone's positions and invalid positions
        bool hasInvalidPosition = tempZone.positions.Any(pos => !validPositions.Contains(pos));
        if (hasInvalidPosition)
        {
            Debug.Log($"Zone at {position} includes invalid coordinate types. Zone creation aborted.");
            return false; // Abort the creation of the zone
        }

        // If no invalid positions are found, add the zone
        worldZones.Add(tempZone);
        SetCoordinatesToType(tempZone.positions, Coordinate.TYPE.ZONE);
        Debug.Log($"Zone successfully created at {position} with type {zoneType}.");
        return true;
    }

    public void GenerateRandomZones(int minZones, int maxZones)
    {
        worldZones.Clear();

        // Determine the random number of zones to create within the specified range
        int numZonesToCreate = Random.Range(minZones, maxZones + 1);

        // A list to hold potential positions for zone creation
        List<Vector2Int> potentialPositions = GetAllPositionsOfType(Coordinate.TYPE.NULL).ToList();

        // Shuffle the list of potential positions to randomize the selection
        ShuffleList(potentialPositions);

        // Attempt to create zones up to the determined number or until potential positions run out
        int zonesCreated = 0;
        for(int i = 0; i < potentialPositions.Count; i++)
        {
            if (zonesCreated >= numZonesToCreate) break; // Stop if we've created the desired number of zones

            // Attempt to create a zone at the current position

            WorldZone.TYPE zoneType = GetRandomWorldZoneType();
            int randomHeight = Random.Range(0, 5);

            CreateWorldZone(potentialPositions[i], zoneType, randomHeight);
            zonesCreated++;
        }

        Debug.Log($"Attempted to create {numZonesToCreate} zones. Successfully created {zonesCreated}.");
    }

    WorldZone.TYPE GetRandomWorldZoneType()
    {
        // Get all values defined in the WorldZone.TYPE enum
        var zoneTypes = System.Enum.GetValues(typeof(WorldZone.TYPE));

        // Choose a random index
        int randomIndex = Random.Range(0, zoneTypes.Length);

        // Return the randomly selected WorldZone.TYPE
        return (WorldZone.TYPE)zoneTypes.GetValue(randomIndex);
    }

    // Utility method to shuffle a list in place
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
