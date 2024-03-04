using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MapBorder { NORTH, SOUTH, EAST, WEST }

public class CoordinateMap
{
    #region << STATIC FUNCTIONS <<
    public static MapBorder? GetMapBorderInNaturalDirection(WorldDirection direction)
    {
        switch (direction)
        {
            case WorldDirection.NORTH:
                return MapBorder.NORTH;
            case WorldDirection.SOUTH:
                return MapBorder.SOUTH;
            case WorldDirection.WEST:
                return MapBorder.WEST;
            case WorldDirection.EAST:
                return MapBorder.EAST;
            default:
                return null;
        }
    }

    public static MapBorder? GetOppositeBorder(MapBorder border)
    {
        switch (border)
        {
            case MapBorder.NORTH:
                return MapBorder.SOUTH;
            case MapBorder.SOUTH:
                return MapBorder.NORTH;
            case MapBorder.WEST:
                return MapBorder.EAST;
            case MapBorder.EAST:
                return MapBorder.WEST;
            default: return null;
        }
    }

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

    // public access variables
    public bool Initialized { get; private set; }

    // >> main coordinate reference
    WorldGeneration _worldGeneration = null;
    WorldRegion _worldRegion = null;
    WorldChunk _worldChunk = null;
    Coordinate[][] _coordinateMap;

    // >> _coordinateMap reference lists
    HashSet<Vector2Int> _positions = new();
    HashSet<Coordinate> _coordinates = new();
    Dictionary<Vector2Int, Coordinate> _positionMap = new();
    Dictionary<Coordinate.TYPE, HashSet<Vector2Int>> _typeMap = new();
    Dictionary<MapBorder, HashSet<Vector2Int>> _borderPositionsMap = new(); // Enum , Sorted List of Border Coordinates
    Dictionary<MapBorder, HashSet<Vector2Int>> _borderExitMap = new();
    Dictionary<MapBorder, Vector2Int[]> _borderIndexMap = new();
    Dictionary<MapBorder, (Vector2Int, Vector2Int)> _borderCornersMap = new Dictionary<MapBorder, (Vector2Int, Vector2Int)>();
    // >> public access lists
    public int maxCoordinateValue { get; private set; } = 0;
    public List<Vector2Int> allPositions { get { return _positions.ToList(); } }
    public List<Coordinate> allCoordinates { get { return _coordinates.ToList(); } }
    public List<Vector2Int> exitPositions = new List<Vector2Int>();
    public List<WorldPath> worldPaths = new List<WorldPath>();
    public List<WorldZone> worldZones = new List<WorldZone>();

    // == [[ CONSTRUCTOR ]] ======================================================================== >>>>

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

        Initialized = true;
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

        Initialized = true;
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

        Initialized = true;
    }

    // == [[ GET COORDINATE ]] ======================================================================== >>>>
    public Coordinate GetCoordinateAt(Vector2Int position)
    {
        if (Initialized && _positions.Contains(position))
        {
            return _positionMap[position];
        }
        return null;
    }

    public Coordinate.TYPE? GetCoordinateTypeAt(Vector2Int position)
    {
        if (Initialized && _positions.Contains(position))
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

    public HashSet<Vector2Int> GetExitsOnBorder(MapBorder border)
    {
        if (_borderExitMap.ContainsKey(border))
        {
            return _borderExitMap[border];
        }
        return null;
    }

    public 

    // == [[ SET COORDINATE ]] ======================================================================== >>>>
    void SetAllCoordinatesToDefault(int coordMax, int borderOffset)
    {
        this.maxCoordinateValue = coordMax;

        // << ASSIGN COORDINATE TYPES >> =================================================================
        // ** Set Coordinate To Type updates the TypeMap accordingly

        // >> initialize _border positions
        _borderPositionsMap[MapBorder.NORTH] = new();
        _borderPositionsMap[MapBorder.SOUTH] = new();
        _borderPositionsMap[MapBorder.EAST] = new();
        _borderPositionsMap[MapBorder.WEST] = new();


        _borderIndexMap[MapBorder.NORTH] = new Vector2Int[coordMax];
        _borderIndexMap[MapBorder.SOUTH] = new Vector2Int[coordMax];
        _borderIndexMap[MapBorder.EAST] = new Vector2Int[coordMax];
        _borderIndexMap[MapBorder.WEST] = new Vector2Int[coordMax];

        // Helper method to determine the border type based on position
        MapBorder DetermineBorderType(Vector2Int pos, Vector2Int range)
        {
            if (pos.x == range.y) return MapBorder.EAST;
            if (pos.x == range.x) return MapBorder.WEST;
            if (pos.y == range.y) return MapBorder.NORTH;
            if (pos.y == range.x) return MapBorder.SOUTH;
            return default; // Return a default or undefined value
        }

        // >> store coordinate range
        Vector2Int playableMapRange = new Vector2Int(borderOffset, coordMax - (borderOffset + 1));

        // >> store border corners
        List<Vector2Int> cornerCoordinates = new List<Vector2Int>() {
            new Vector2Int(playableMapRange.x, playableMapRange.x), // 0 0 { SOUTH WEST }
            new Vector2Int(playableMapRange.y, playableMapRange.y), // max max { NORTH EAST }
            new Vector2Int(playableMapRange.x, playableMapRange.y), // 0 max { NORTH WEST }
            new Vector2Int(playableMapRange.y, playableMapRange.x)  // max 0 { SOUTH EAST }
            };
        _borderCornersMap[MapBorder.NORTH] = (cornerCoordinates[2], cornerCoordinates[1]); // NW, NE
        _borderCornersMap[MapBorder.SOUTH] = (cornerCoordinates[0], cornerCoordinates[3]); // SW, SE
        _borderCornersMap[MapBorder.EAST] = (cornerCoordinates[1], cornerCoordinates[3]); // NE, SE
        _borderCornersMap[MapBorder.WEST] = (cornerCoordinates[0], cornerCoordinates[2]); // SW, NW

        // >> iterate through positions
        foreach (Vector2Int pos in _positions)
        {
            if (cornerCoordinates.Contains(pos) || // is corner
                (pos.x < playableMapRange.x || pos.x > playableMapRange.y || pos.y < playableMapRange.x || pos.y > playableMapRange.y)) // or is outside bounds
            {
                // Set Corners to Closed
                SetCoordinateToType(pos, Coordinate.TYPE.CLOSED);
            }
            else if (pos.x == playableMapRange.x || pos.x == playableMapRange.y || pos.y == playableMapRange.x || pos.y == playableMapRange.y)
            {
                // Set Type to Border
                SetCoordinateToType(pos, Coordinate.TYPE.BORDER);

                MapBorder borderType = DetermineBorderType(pos, playableMapRange);

                switch (borderType)
                {
                    case MapBorder.EAST:
                        _borderPositionsMap[MapBorder.EAST].Add(pos);
                        _borderIndexMap[MapBorder.EAST][pos.y] = pos;
                        break;
                    case MapBorder.WEST:
                        _borderPositionsMap[MapBorder.WEST].Add(pos);
                        _borderIndexMap[MapBorder.WEST][pos.y] = pos;
                        break;
                    case MapBorder.NORTH:
                        _borderPositionsMap[MapBorder.NORTH].Add(pos);
                        _borderIndexMap[MapBorder.NORTH][pos.x] = pos;
                        break;
                    case MapBorder.SOUTH:
                        _borderPositionsMap[MapBorder.SOUTH].Add(pos);
                        _borderIndexMap[MapBorder.SOUTH][pos.x] = pos;
                        break;
                    default:
                        // Handle any non-border or undefined cases, if necessary
                        break;
                }
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
            case Coordinate.TYPE.BORDER: coordinate.debugColor = Color.magenta; break;
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

    void SetCoordinatesToType(List<Vector2Int> positions, Coordinate.TYPE type)
    {
        foreach (Vector2Int pos in positions)
        {
            if (_positions.Contains(pos))
            {
                SetCoordinateToType(pos, type);
            }
        }
    }

    public void SetCoordinatesOfTypeTo(List<Vector2Int> positions, Coordinate.TYPE targetType, Coordinate.TYPE convertType)
    {
        foreach (Vector2Int pos in positions)
        {
            // Check if the position is within the map boundaries
            if (_positions.Contains(pos))
            {
                // Retrieve the coordinate at the given position
                Coordinate coordinate = _positionMap[pos];

                // Check if the coordinate's current type matches the targetType
                if (coordinate.type == targetType)
                {
                    // If so, set the coordinate to the new convertType
                    SetCoordinateToType(pos, convertType);
                }
            }
        }
    }

    public void CloseMapBorder(MapBorder mapBorder)
    {
        // Destroy that border >:#!! 
        List<Vector2Int> positions = _borderPositionsMap[mapBorder].ToList();

        SetCoordinatesToType(positions, Coordinate.TYPE.CLOSED);

        // Retrieve and nullify corners for the specific border
        if (_borderCornersMap.TryGetValue(mapBorder, out var corners))
        {
            SetCoordinatesToType(new List<Vector2Int> { corners.Item1, corners.Item2 }, Coordinate.TYPE.CLOSED);
        }
    }

    // == [[ WORLD EXITS ]] ======================================================================== >>>>
    public void ConvertCoordinateToExit(Coordinate coordinate)
    {
        if (coordinate == null) return;
        if (coordinate.type != Coordinate.TYPE.BORDER)
        {
            //Debug.Log($"Cannot convert non border coordinate {coordinate.Value} {coordinate.type} to exit");
            return;
        }

        // Get border
        for (int i = 0; i < _borderPositionsMap.Keys.Count; i++)
        {
            MapBorder borderType = _borderPositionsMap.Keys.ToList()[i];
            if (_borderPositionsMap.ContainsKey(borderType) 
                && _borderPositionsMap[borderType].Contains(coordinate.Value))
            {
                // Once border found, update coordinate maps
                if (!_borderExitMap.ContainsKey(borderType)) { _borderExitMap[borderType] = new(); }

                _borderExitMap[borderType].Add(coordinate.Value);
                exitPositions.Add(coordinate.Value);
                break;
            }
        }

        SetCoordinateToType(coordinate.Value, Coordinate.TYPE.EXIT);
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
            Coordinate coordinate = GetCoordinateAt(exitPosition);
            ConvertCoordinateToExit(coordinate);
        }

        //Debug.Log($"{numberOfExits} exits have been created on the map borders.");
    }

    public void GenerateRandomExitOnBorder(MapBorder borderType)
    {
        List<Vector2Int> allBorderPositions = _borderPositionsMap[borderType].ToList();
        Vector2Int randomCoordinate = allBorderPositions[Random.Range(0, allBorderPositions.Count)];
        ConvertCoordinateToExit(GetCoordinateAt(randomCoordinate));
    }

    public void SetMatchingExit(MapBorder neighborBorder, Vector2Int neighborExitCoordinate)
    {
        // Determine the relative position of the exit based on the neighbor border
        Vector2Int matchingCoordinate = Vector2Int.zero;
        MapBorder matchingBorder;
        switch (neighborBorder)
        {
            case MapBorder.NORTH:
                // if this border is NORTH, then the matching neighbor's border is SOUTH
                matchingBorder = MapBorder.SOUTH;
                matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, 0);
                break;
            case MapBorder.SOUTH:
                // if this border is SOUTH, then the matching neighbor's border is NORTH
                matchingBorder = MapBorder.NORTH;
                matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, this.maxCoordinateValue - 1);
                break;
            case MapBorder.EAST:
                // if this border is EAST, then the matching neighbor's border is WEST
                matchingBorder = MapBorder.WEST;
                matchingCoordinate = new Vector2Int(0, neighborExitCoordinate.y);
                break;
            case MapBorder.WEST:
                // if this border is EAST, then the matching neighbor's border is WEST
                matchingBorder = MapBorder.EAST;
                matchingCoordinate = new Vector2Int(this.maxCoordinateValue - 1, neighborExitCoordinate.y);
                break;
            default:
                throw new ArgumentException("Invalid MapBorder value.", nameof(neighborBorder));
        }

        ConvertCoordinateToExit(GetCoordinateAt(matchingCoordinate));
        //Debug.Log($"Created Exit {matchingCoordinate} to match {neighborExitCoordinate}");
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
        //Debug.Log($"Created Path from {start} to {end} with {newPath.positions.Count}");
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

        //Debug.Log($"Generated new paths connecting all exits.");
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
        //Debug.Log($"Attempting to create zone at {position}");

        // Temporarily create the zone to check its positions
        WorldZone tempZone = new WorldZone(this, GetCoordinateAt(position), zoneType, zoneHeight);

        // Check if any of the zone's positions are in the BORDER or CLOSED categories
        HashSet<Vector2Int> validPositions = GetAllPositionsOfType(Coordinate.TYPE.NULL);

        // Check for intersection between the zone's positions and invalid positions
        bool hasInvalidPosition = tempZone.positions.Any(pos => !validPositions.Contains(pos));
        if (hasInvalidPosition)
        {
            //Debug.Log($"Zone at {position} includes invalid coordinate types. Zone creation aborted.");
            return false; // Abort the creation of the zone
        }

        // If no invalid positions are found, add the zone
        worldZones.Add(tempZone);
        SetCoordinatesToType(tempZone.positions, Coordinate.TYPE.ZONE);
        //Debug.Log($"Zone successfully created at {position} with type {zoneType}.");
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

        //Debug.Log($"Attempted to create {numZonesToCreate} zones. Successfully created {zonesCreated}.", this._worldRegion.gameObject);
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
