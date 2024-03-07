using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Darklight.ThirdDimensional.Generation
{
    using WorldGen = WorldGeneration;
    public class CoordinateMap
    {
        #region << STATIC FUNCTIONS <<
        public static BorderDirection? GetBorderDirection(WorldDirection direction)
        {
            switch (direction)
            {
                case WorldDirection.NORTH:
                    return BorderDirection.NORTH;
                case WorldDirection.SOUTH:
                    return BorderDirection.SOUTH;
                case WorldDirection.WEST:
                    return BorderDirection.WEST;
                case WorldDirection.EAST:
                    return BorderDirection.EAST;
                default:
                    return null;
            }
        }
        public static BorderDirection? GetOppositeBorder(BorderDirection border)
        {
            switch (border)
            {
                case BorderDirection.NORTH:
                    return BorderDirection.SOUTH;
                case BorderDirection.SOUTH:
                    return BorderDirection.NORTH;
                case BorderDirection.WEST:
                    return BorderDirection.EAST;
                case BorderDirection.EAST:
                    return BorderDirection.WEST;
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
        public static WorldDirection? GetEnumFromDirectionVector(Vector2Int direction)
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
        public static Vector2Int CalculateNeighborCoordinateValue(Vector2Int center,  WorldDirection direction)
        {
            return center + _directionVectorMap[direction];
        }
        public static List<Vector2Int> CalculateNaturalNeighborCoordinateValues(Vector2Int center)
        {
            return new List<Vector2Int>()
            {
                CalculateNeighborCoordinateValue(center, WorldDirection.NORTH),
                CalculateNeighborCoordinateValue(center, WorldDirection.SOUTH),
                CalculateNeighborCoordinateValue(center, WorldDirection.EAST),
                CalculateNeighborCoordinateValue(center, WorldDirection.WEST)
            };
        }
        public static List<Vector2Int> CalculateDiagonalNeighborCoordinateValues(Vector2Int center)
        {
            return new List<Vector2Int>()
            {
                CalculateNeighborCoordinateValue(center, WorldDirection.NORTHWEST),
                CalculateNeighborCoordinateValue(center, WorldDirection.NORTHEAST),
                CalculateNeighborCoordinateValue(center, WorldDirection.SOUTHWEST),
                CalculateNeighborCoordinateValue(center, WorldDirection.SOUTHEAST)
            };
        }
        #endregion

        // >> map creation values
        UnitSpace _mapUnitSpace;
        Vector3 _mapOriginPosition;
        int _mapWidthCount;
        int _coordinateSize;

        // >> quick reference lists
        Dictionary<Vector2Int, Coordinate> _valueMap = new();
        Dictionary<Coordinate.TYPE, HashSet<Vector2Int>> _typeMap = new();

        // >>>> Border References
        Dictionary<BorderDirection, HashSet<Vector2Int>> _borderPositionsMap = new();
        Dictionary<BorderDirection, HashSet<Vector2Int>> _borderExitMap = new();
        Dictionary<BorderDirection, Vector2Int[]> _borderIndexMap = new();
        Dictionary<BorderDirection, (Vector2Int, Vector2Int)> _borderCornersMap = new();
        Dictionary<BorderDirection, bool> _activeBorderMap = new(4);

        // >>>> Custom Generation References
        List<Zone> _zones = new();
        Dictionary<Zone, HashSet<Vector2Int>> _zoneMap = new();


        // >> public access variables
        public bool Initialized { get; private set; }
        public int MaxCoordinateValue { get; private set; }
        public UnitSpace UnitSpace => _mapUnitSpace;
        public int CoordinateSize => _coordinateSize;
        public IEnumerable<Vector2Int> AllCoordinateValues => _valueMap.Keys;
        public IEnumerable<Coordinate> AllCoordinates => _valueMap.Values;
        public Dictionary<BorderDirection, bool> ActiveBorderMap => _activeBorderMap;

        public List<Vector2Int> Exits = new();
        public List<Path> Paths = new();

        public List<Zone> Zones => _zones;

        // == [[ CONSTRUCTOR ]] ======================================================================== >>>>
        public CoordinateMap(object parent)
        {
            // [[ CREATE REGION COORDINATE MAP ]]
            if (parent is WorldGen worldGeneration)
            {
                _mapUnitSpace = UnitSpace.WORLD;
                _mapOriginPosition = worldGeneration.OriginPosition;
                _mapWidthCount = WorldGen.Settings.WorldWidth_inRegionUnits;
                _coordinateSize = WorldGen.Settings.RegionFullWidth_inGameUnits;
            }
            // [[ CREATE CHUNK COORDINATE MAP ]]
            else if (parent is Region region)
            {
                _mapUnitSpace = UnitSpace.REGION;
                _mapOriginPosition = region.OriginPosition;
                _mapWidthCount = WorldGen.Settings.RegionFullWidth_inChunkUnits;
                _coordinateSize = WorldGen.Settings.ChunkWidth_inGameUnits;
            }
            // [[ CREATE CELL COORDINATE MAP ]]
            else if (parent is Chunk chunk)
            {
                _mapUnitSpace = UnitSpace.CHUNK;
                _mapOriginPosition = chunk.OriginPosition;
                _mapWidthCount = WorldGen.Settings.ChunkWidth_inCellUnits;
                _coordinateSize = WorldGen.Settings.CellSize_inGameUnits;
            }
            else
            {
                throw new ArgumentException("Unsupported parent type for CoordinateMap.");
            }

            // Initialize Coordinate grid
            for (int x = 0; x < _mapWidthCount; x++)
            {
                for (int y = 0; y < _mapWidthCount; y++)
                {
                    // Calculate Coordinate Value
                    Vector2Int newCoordinateValue = new Vector2Int(x, y);

                    // Create Coordinate
                    Coordinate newCoordinate = new Coordinate(this, _mapOriginPosition, newCoordinateValue, _coordinateSize);

                    // Add new Coordinate to map
                    _valueMap[newCoordinateValue] = newCoordinate;
                }
            }

            SetAllCoordinatesToDefault();
            Initialized = true;
        }


        // == [[ GET COORDINATE ]] ======================================================================== >>>>
        public Coordinate GetCoordinateAt(Vector2Int position)
        {
            return Initialized && _valueMap.TryGetValue(position, out var coordinate) ? coordinate : null;
        }

        public Coordinate.TYPE? GetCoordinateTypeAt(Vector2Int position)
        {
            if (Initialized && _valueMap.TryGetValue(position, out var coordinate))
            {
                return coordinate.Type;
            }
            return null;
        }

        public HashSet<Vector2Int> GetAllPositionsOfType(Coordinate.TYPE type)
        {
            if (!_typeMap.ContainsKey(type)) { _typeMap[type] = new(); }
            return _typeMap[type];
        }

        public HashSet<Vector2Int> GetExitsOnBorder(BorderDirection border)
        {
            if (_borderExitMap.ContainsKey(border))
            {
                return _borderExitMap[border];
            }
            return null;
        }
        
        // == [[ SET COORDINATE ]] ======================================================================== >>>>
        void SetAllCoordinatesToDefault()
        {
            int coordMax = _mapWidthCount;
            int borderOffset = WorldGeneration.Settings.RegionBoundaryOffset_inChunkUnits;

            // >> store coordinate map range
            Vector2Int mapRange = new Vector2Int(borderOffset, coordMax - (borderOffset + 1));

            // << ASSIGN COORDINATE TYPES >> =================================================================
            // ** Set Coordinate To Type updates the TypeMap accordingly

            // >> initialize _border positions
            _borderPositionsMap[BorderDirection.NORTH] = new();
            _borderPositionsMap[BorderDirection.SOUTH] = new();
            _borderPositionsMap[BorderDirection.EAST] = new();
            _borderPositionsMap[BorderDirection.WEST] = new();

            // >> initialize _border indexes
            _borderIndexMap[BorderDirection.NORTH] = new Vector2Int[coordMax];
            _borderIndexMap[BorderDirection.SOUTH] = new Vector2Int[coordMax];
            _borderIndexMap[BorderDirection.EAST] = new Vector2Int[coordMax];
            _borderIndexMap[BorderDirection.WEST] = new Vector2Int[coordMax];

            // Helper method to determine the border type based on position
            BorderDirection DetermineBorderType(Vector2Int pos, Vector2Int range)
            {
                if (pos.x == range.y) return BorderDirection.EAST;
                if (pos.x == range.x) return BorderDirection.WEST;
                if (pos.y == range.y) return BorderDirection.NORTH;
                if (pos.y == range.x) return BorderDirection.SOUTH;
                return default; // Return a default or undefined value
            }



            // >> store border corners
            List<Vector2Int> cornerCoordinates = new List<Vector2Int>() {
                new Vector2Int(mapRange.x, mapRange.x), // min min { SOUTH WEST }
                new Vector2Int(mapRange.y, mapRange.y), // max max { NORTH EAST }
                new Vector2Int(mapRange.x, mapRange.y), // min max { NORTH WEST }
                new Vector2Int(mapRange.y, mapRange.x)  // max min { SOUTH EAST }
            };

            _borderCornersMap[BorderDirection.NORTH] = (cornerCoordinates[2], cornerCoordinates[1]); // NW, NE
            _borderCornersMap[BorderDirection.SOUTH] = (cornerCoordinates[0], cornerCoordinates[3]); // SW, SE
            _borderCornersMap[BorderDirection.EAST] = (cornerCoordinates[1], cornerCoordinates[3]); // NE, SE
            _borderCornersMap[BorderDirection.WEST] = (cornerCoordinates[0], cornerCoordinates[2]); // SW, NW

            _activeBorderMap[BorderDirection.NORTH] = false;
            _activeBorderMap[BorderDirection.SOUTH] = false;
            _activeBorderMap[BorderDirection.EAST] = false;
            _activeBorderMap[BorderDirection.WEST] = false;

            // >> iterate through positions
            foreach (Vector2Int pos in AllCoordinateValues)
            {
                if (cornerCoordinates.Contains(pos) || // is corner
                    pos.x < mapRange.x || pos.x > mapRange.y || pos.y < mapRange.x || pos.y > mapRange.y) // or is outside bounds
                {
                    // Set Corners to Closed
                    SetCoordinateToType(pos, Coordinate.TYPE.CLOSED);
                }
                else if (pos.x == mapRange.x || pos.x == mapRange.y || pos.y == mapRange.x || pos.y == mapRange.y)
                {
                    // Set Type to Border
                    SetCoordinateToType(pos, Coordinate.TYPE.BORDER);

                    BorderDirection borderType = DetermineBorderType(pos, mapRange);

                    switch (borderType)
                    {
                        case BorderDirection.EAST:
                            _borderPositionsMap[BorderDirection.EAST].Add(pos);
                            _borderIndexMap[BorderDirection.EAST][pos.y] = pos;
                            break;
                        case BorderDirection.WEST:
                            _borderPositionsMap[BorderDirection.WEST].Add(pos);
                            _borderIndexMap[BorderDirection.WEST][pos.y] = pos;
                            break;
                        case BorderDirection.NORTH:
                            _borderPositionsMap[BorderDirection.NORTH].Add(pos);
                            _borderIndexMap[BorderDirection.NORTH][pos.x] = pos;
                            break;
                        case BorderDirection.SOUTH:
                            _borderPositionsMap[BorderDirection.SOUTH].Add(pos);
                            _borderIndexMap[BorderDirection.SOUTH][pos.x] = pos;
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

        void SetCoordinateToType(Vector2Int valueKey, Coordinate.TYPE newType)
        {
            if (!_valueMap.ContainsKey(valueKey)) return;

            // Remove Old Type
            Coordinate coordinate = _valueMap[valueKey];
            Coordinate.TYPE currentType = coordinate.Type;
            if (_typeMap.ContainsKey(currentType))
            {
                _typeMap[currentType].Remove(valueKey);
            }

            // Assign New Type
            coordinate.SetType(newType);


            // If new TYPE key not found, create
            if (!_typeMap.ContainsKey(newType))
            {
                _typeMap[newType] = new HashSet<Vector2Int> { valueKey };
            }
            // else if the map doesnt already include the coordinate
            else if (!_typeMap[newType].Contains(valueKey))
            {
                _typeMap[newType].Add(valueKey);
            }
        }

        void SetCoordinatesToType(List<Vector2Int> positions, Coordinate.TYPE type)
        {
            foreach (Vector2Int pos in positions)
            {
                if (_valueMap.ContainsKey(pos))
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
                if (_valueMap.ContainsKey(pos))
                {
                    // Retrieve the coordinate at the given position
                    Coordinate coordinate = _valueMap[pos];

                    // Check if the coordinate's current type matches the targetType
                    if (coordinate.Type == targetType)
                    {
                        // If so, set the coordinate to the new convertType
                        SetCoordinateToType(pos, convertType);
                    }
                }
            }
        }

        public void CloseMapBorder(BorderDirection mapBorder)
        {
            // Destroy that border >:#!! 

            _activeBorderMap[mapBorder] = true;

            List<Vector2Int> positions = _borderPositionsMap[mapBorder].ToList();

            SetCoordinatesToType(positions, Coordinate.TYPE.CLOSED);
        }

        public void SetInactiveCornersToType(Coordinate.TYPE type)
        {
            List<Vector2Int> inactiveCorners = new List<Vector2Int>();

            // Mapping corners to their adjacent borders
            var cornerBordersMap = new Dictionary<Vector2Int, List<BorderDirection>>
            {
                [_borderCornersMap[BorderDirection.NORTH].Item1] = new List<BorderDirection> { BorderDirection.NORTH, BorderDirection.WEST }, // NW
                [_borderCornersMap[BorderDirection.NORTH].Item2] = new List<BorderDirection> { BorderDirection.NORTH, BorderDirection.EAST }, // NE
                [_borderCornersMap[BorderDirection.SOUTH].Item1] = new List<BorderDirection> { BorderDirection.SOUTH, BorderDirection.WEST }, // SW
                [_borderCornersMap[BorderDirection.SOUTH].Item2] = new List<BorderDirection> { BorderDirection.SOUTH, BorderDirection.EAST }, // SE
            };

            // Iterate through each corner
            foreach (var corner in cornerBordersMap)
            {
                bool allBordersInactive = true;

                // Check if all adjacent borders of the corner are inactive
                foreach (var border in corner.Value)
                {
                    if (_activeBorderMap[border])
                    {
                        allBordersInactive = false;
                        break;
                    }
                }

                // If all adjacent borders are inactive, add the corner to the list
                if (allBordersInactive)
                {
                    inactiveCorners.Add(corner.Key);
                }
            }

            // Set inactive corners
            SetCoordinatesToType(inactiveCorners, type);
        }

        // == [[ FIND COORDINATE ]] ================================================================= >>>>
        public Coordinate FindClosestCoordinateOfType(Coordinate targetCoordinate, List<Coordinate.TYPE> typeList)
        {

                // using BFS algorithm
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
                queue.Enqueue(targetCoordinate.Value);
                visited.Add(targetCoordinate.Value);

                while (queue.Count > 0)
                {
                    Vector2Int currentValue = queue.Dequeue();
                    Coordinate currentCoordinate = GetCoordinateAt(currentValue);

                    if (currentCoordinate != null)
                    {
                        // Check if the current coordinate is the target type
                        if (typeList.Contains(currentCoordinate.Type))
                        {
                            return GetCoordinateAt(currentValue);
                        }

                        // Get the neighbors of the current coordinate
                        foreach (Vector2Int neighbor in currentCoordinate.GetNaturalNeighborCoordinateValues())
                        {
                            if (!visited.Contains(neighbor))
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }

                }

                return targetCoordinate; 
        }



        // == [[ WORLD EXITS ]] ======================================================================== >>>>
        public void ConvertCoordinateToExit(Coordinate coordinate)
        {
            if (coordinate == null) return;
            if (coordinate.Type != Coordinate.TYPE.BORDER)
            {
                //Debug.Log($"Cannot convert non border coordinate {coordinate.Value} {coordinate.type} to exit");
                return;
            }

            // Get border
            for (int i = 0; i < _borderPositionsMap.Keys.Count; i++)
            {
                BorderDirection borderType = _borderPositionsMap.Keys.ToList()[i];
                if (_borderPositionsMap.ContainsKey(borderType)
                    && _borderPositionsMap[borderType].Contains(coordinate.Value))
                {
                    // Once border found, update coordinate maps
                    if (!_borderExitMap.ContainsKey(borderType)) { _borderExitMap[borderType] = new(); }

                    _borderExitMap[borderType].Add(coordinate.Value);
                    Exits.Add(coordinate.Value);
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

        public void GenerateRandomExitOnBorder(BorderDirection borderType)
        {
            List<Vector2Int> allBorderPositions = _borderPositionsMap[borderType].ToList();
            Vector2Int randomCoordinate = allBorderPositions[Random.Range(0, allBorderPositions.Count)];
            ConvertCoordinateToExit(GetCoordinateAt(randomCoordinate));
        }

        public void SetMatchingExit(BorderDirection neighborBorder, Vector2Int neighborExitCoordinate)
        {
            // Determine the relative position of the exit based on the neighbor border
            Vector2Int matchingCoordinate = Vector2Int.zero;
            BorderDirection matchingBorder;
            switch (neighborBorder)
            {
                case BorderDirection.NORTH:
                    // if this border is NORTH, then the matching neighbor's border is SOUTH
                    matchingBorder = BorderDirection.SOUTH;
                    matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, 0);
                    break;
                case BorderDirection.SOUTH:
                    // if this border is SOUTH, then the matching neighbor's border is NORTH
                    matchingBorder = BorderDirection.NORTH;
                    matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, this.MaxCoordinateValue - 1);
                    break;
                case BorderDirection.EAST:
                    // if this border is EAST, then the matching neighbor's border is WEST
                    matchingBorder = BorderDirection.WEST;
                    matchingCoordinate = new Vector2Int(0, neighborExitCoordinate.y);
                    break;
                case BorderDirection.WEST:
                    // if this border is EAST, then the matching neighbor's border is WEST
                    matchingBorder = BorderDirection.EAST;
                    matchingCoordinate = new Vector2Int(this.MaxCoordinateValue - 1, neighborExitCoordinate.y);
                    break;
                default:
                    throw new ArgumentException("Invalid MapBorder value.", nameof(neighborBorder));
            }

            ConvertCoordinateToExit(GetCoordinateAt(matchingCoordinate));
            //Debug.Log($"Created Exit {matchingCoordinate} to match {neighborExitCoordinate}");
        }

        // == [[ WORLD PATH ]] ================================================================================ >>>>
        public Path CreatePathFrom(Vector2Int start, Vector2Int end, List<Coordinate.TYPE> validTypes)
        {
            Path newPath = new Path(this, start, end, validTypes, WorldGeneration.Settings.PathRandomness);
            Paths.Add(newPath);

            // Remove Exits from path positions
            List<Vector2Int> positions = newPath.AllPositions;
            if (GetCoordinateTypeAt(start) == Coordinate.TYPE.EXIT) { positions.Remove(start); }
            if (GetCoordinateTypeAt(end) == Coordinate.TYPE.EXIT) { positions.Remove(end); }

            // Assign Path Type
            SetCoordinatesToType(newPath.AllPositions, Coordinate.TYPE.PATH);
            //Debug.Log($"Created Path from {start} to {end} with {newPath.positions.Count}");

            return newPath;
        }

        public void GeneratePathsBetweenExits()
        {
            // Clear existing paths
            Paths.Clear();

            // Ensure there's more than one exit to connect
            if (Exits.Count < 2)
            {
                Debug.LogWarning("Not enough exits to generate paths.");
                return;
            }

            List<Vector2Int> sortedExits = Exits.OrderBy(pos => pos.x).ThenBy(pos => pos.y).ToList();
            for (int i = 0; i < sortedExits.Count - 1; i++)
            {
                Vector2Int start = sortedExits[i];
                Vector2Int end = sortedExits[i + 1]; // Connect to the next exit in the list

                CreatePathFrom(start, end, new List<Coordinate.TYPE>() { Coordinate.TYPE.NULL, Coordinate.TYPE.EXIT});
            }

            //Connect the last exit back to the first to ensure all exits are interconnected
            //CreatePathFrom(sortedExits[sortedExits.Count - 1], sortedExits[0]);

            //Debug.Log($"Generated new paths connecting all exits.");
        }



        // == [[ WORLD ZONES ]] ================================================================================ >>>>
        public bool CreateWorldZone(Vector2Int position, Zone.TYPE zoneType, int zoneHeight)
        {
            //Debug.Log($"Attempting to create zone at {position}");

            // Temporarily create the zone to check its positions
            Zone newZone = new Zone(GetCoordinateAt(position), zoneType, zoneHeight, Zones.Count);

            // Check if the zone is valid
            if (!newZone.Valid)
            {
                //Debug.Log($"Zone at {position} includes invalid coordinate types. Zone creation aborted.");
                return false; // Abort the creation of the zone
            }

            // If no invalid positions are found, add the zone
            Zones.Add(newZone);
            SetCoordinatesToType(newZone.AllPositions, Coordinate.TYPE.ZONE);

            // Find the closest PATH Coordinate
            Coordinate closestPathCoordinate = FindClosestCoordinateOfType(newZone.CenterCoordinate, new List<Coordinate.TYPE>() { Coordinate.TYPE.PATH });


            // Find the closese ZONE Coordinate
            Vector2Int closestZoneValue = newZone.GetClosestCoordinateValueTo(closestPathCoordinate.Value);

            Path zonePath = CreatePathFrom(closestPathCoordinate.Value, closestZoneValue, new List<Coordinate.TYPE>() { Coordinate.TYPE.NULL, Coordinate.TYPE.ZONE, Coordinate.TYPE.PATH });
            Debug.Log($"Created zone path from {zonePath.StartPosition} to {zonePath.EndPosition} -> {zonePath.AllPositions.Count}");





            //Debug.Log($"Zone successfully created at {position} with type {zoneType}.");
            return true;
        }

        public void GenerateRandomZones(int minZones, int maxZones, List<Zone.TYPE> types)
        {
            Zones.Clear();

            // Determine the random number of zones to create within the specified range
            int numZonesToCreate = Random.Range(minZones, maxZones + 1);

            // A list to hold potential positions for zone creation
            List<Vector2Int> potentialPositions = GetAllPositionsOfType(Coordinate.TYPE.NULL).ToList();

            // Shuffle the list of potential positions to randomize the selection
            ShuffleList(potentialPositions);

            // Attempt to create zones up to the determined number or until potential positions run out
            int zonesCreated = 0;
            for (int i = 0; i < potentialPositions.Count; i++)
            {
                if (zonesCreated >= numZonesToCreate) break; // Stop if we've created the desired number of zones

                // Attempt to create a zone at the current position

                Zone.TYPE zoneType = Zone.GetRandomTypeFromList(types);
                int randomHeight = Random.Range(0, 5);

                CreateWorldZone(potentialPositions[i], zoneType, randomHeight);
                zonesCreated++;


            }

            //Debug.Log($"Attempted to create {numZonesToCreate} zones. Successfully created {zonesCreated}.", this._worldRegion.gameObject);
        }

        public Zone GetZoneFromCoordinate(Coordinate coordinate)
        {
            foreach (Zone zone in _zones)
            {
                if (zone.AllPositions.Contains(coordinate.Value))
                {
                    return zone;
                }
            }
            return null;
        }

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
}



