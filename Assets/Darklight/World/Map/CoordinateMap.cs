namespace Darklight.World.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builder;
    using Generation;
    using UnityEngine;
    using Random = UnityEngine.Random;

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
                default:
                    return null;
            }
        }

        public static Dictionary<WorldDirection, Vector2Int> _directionVectorMap =
            new()
            {
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

        public static Vector2Int CalculateNeighborCoordinateValue(
            Vector2Int center,
            WorldDirection direction
        )
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

        #region [[ PRIVATE VARIABLES ]]


        string _prefix = ">> Coordinate Map << ";

        // >> map creation values
        UnitSpace _mapUnitSpace;
        Vector3 _mapOriginPosition;
        int _mapWidthCount;
        int _coordinateSize;

        // >> quick reference lists
        Dictionary<Vector2Int, Coordinate> _coordinateMap = new();
        Dictionary<Coordinate.TYPE, HashSet<Vector2Int>> _typeMap = new();

        // >>>> border References
        Dictionary<BorderDirection, HashSet<Vector2Int>> _borderMap = new();
        Dictionary<BorderDirection, HashSet<Vector2Int>> _borderExitMap = new();
        Dictionary<BorderDirection, Vector2Int[]> _borderIndexMap = new();
        Dictionary<BorderDirection, (Vector2Int, Vector2Int)> _borderCornersMap = new();
        Dictionary<BorderDirection, bool> _activeBorderMap = new(4);

        // >>>> zone references
        List<Zone> _zones = new();
        Dictionary<Zone, HashSet<Vector2Int>> _zoneMap = new();

        #endregion

        #region [[ PUBLIC ACCESSOR VARIABLES ]]
        public bool Initialized { get; private set; }
        public int MaxCoordinateValue => _mapWidthCount;
        public UnitSpace UnitSpace => _mapUnitSpace;
        public int CoordinateSize => _coordinateSize;
        public List<Vector2Int> AllCoordinateValues => _coordinateMap.Keys.ToList();
        public List<Coordinate> AllCoordinates => _coordinateMap.Values.ToList();
        public Dictionary<BorderDirection, bool> ActiveBorderMap => _activeBorderMap;

        public List<Vector2Int> Exits => _borderExitMap.Values.SelectMany(exit => exit).ToList(); // Collapse all values into list
        public List<Path> Paths = new();
        public List<Zone> Zones => _zones;
        public Dictionary<Zone, HashSet<Vector2Int>> ZoneMap => _zoneMap;
        #endregion

        #region == [[ CONSTRUCTOR ]] ======================================================================== >>>>
        public CoordinateMap(Transform transform, Vector3 originPosition, int mapWidthCount, int coordinateSize)
        {
            _mapUnitSpace = UnitSpace.WORLD;
            _mapOriginPosition = originPosition;
            _mapWidthCount = mapWidthCount;
            _coordinateSize = coordinateSize;
        }
        public CoordinateMap(WorldBuilder parent)
        {
            _mapUnitSpace = UnitSpace.WORLD;
            _mapOriginPosition = parent.OriginPosition;
            _mapWidthCount = WorldBuilder.Settings.WorldWidth_inRegionUnits;
            _coordinateSize = WorldBuilder.Settings.RegionFullWidth_inGameUnits;
        }
        public CoordinateMap(RegionBuilder parent)
        {
            _mapUnitSpace = UnitSpace.REGION;
            _mapOriginPosition = parent.OriginPosition;
            _mapWidthCount = WorldBuilder.Settings.RegionFullWidth_inChunkUnits;
            _coordinateSize = WorldBuilder.Settings.ChunkWidth_inGameUnits;
        }
        public CoordinateMap(ChunkData parent)
        {
            _mapUnitSpace = UnitSpace.CHUNK;
            _mapOriginPosition = parent.OriginPosition;
            _mapWidthCount = WorldBuilder.Settings.ChunkWidth_inCellUnits;
            _coordinateSize = WorldBuilder.Settings.CellSize_inGameUnits;
        }

        // Other methods and properties..
        public async Task InitializeDefaultMap()
        {
            int coordMax = _mapWidthCount;
            int borderOffset = WorldBuilder.Settings.RegionBoundaryOffset_inChunkUnits;

            // Create Coordinate grid
            for (int x = 0; x < _mapWidthCount; x++)
            {
                for (int y = 0; y < _mapWidthCount; y++)
                {
                    // Calculate Coordinate Value
                    Vector2Int newCoordinateValue = new Vector2Int(x, y);

                    // Create Coordinate
                    Coordinate newCoordinate = new Coordinate(
                        this,
                        _mapOriginPosition,
                        newCoordinateValue,
                        _coordinateSize
                    );

                    // Add new Coordinate to map
                    _coordinateMap[newCoordinateValue] = newCoordinate;
                }
            }

            // >> initialize _border positions and indexes in a loop
            foreach (BorderDirection direction in Enum.GetValues(typeof(BorderDirection)))
            {
                _borderMap[direction] = new HashSet<Vector2Int>();
                _borderIndexMap[direction] = new Vector2Int[coordMax];
            }

            // >> store coordinate map range
            Vector2Int mapRange = new Vector2Int(borderOffset, coordMax - (borderOffset + 1));
            // >> store border corners
            List<Vector2Int> corners = new List<Vector2Int>()
            {
                new Vector2Int(mapRange.x, mapRange.x), // min min { SOUTH WEST }
                new Vector2Int(mapRange.y, mapRange.y), // max max { NORTH EAST }
                new Vector2Int(mapRange.x, mapRange.y), // min max { NORTH WEST }
                new Vector2Int(mapRange.y, mapRange.x) // max min { SOUTH EAST }
            };
            // >> store references to the corners of each border
            _borderCornersMap[BorderDirection.NORTH] = (corners[2], corners[1]); // NW, NE
            _borderCornersMap[BorderDirection.SOUTH] = (corners[0], corners[3]); // SW, SE
            _borderCornersMap[BorderDirection.EAST] = (corners[1], corners[3]); // NE, SE
            _borderCornersMap[BorderDirection.WEST] = (corners[0], corners[2]); // SW, NW

            // >> initialize the active border map
            foreach (BorderDirection direction in Enum.GetValues(typeof(BorderDirection)))
            {
                _activeBorderMap[direction] = false;
            }

            // << METHODS >> =================================================================
            void HandleBorderCoordinate(Vector2Int pos, Vector2Int mapRange)
            {
                SetCoordinateToType(pos, Coordinate.TYPE.BORDER);
                BorderDirection borderType = DetermineBorderType(pos, mapRange);

                _borderMap[borderType].Add(pos);
                if (borderType == BorderDirection.EAST || borderType == BorderDirection.WEST)
                {
                    _borderIndexMap[borderType][pos.y] = pos;
                }
                else
                {
                    _borderIndexMap[borderType][pos.x] = pos;
                }
            }

            BorderDirection DetermineBorderType(Vector2Int pos, Vector2Int range)
            {
                if (pos.x == range.y)
                    return BorderDirection.EAST;
                if (pos.x == range.x)
                    return BorderDirection.WEST;
                if (pos.y == range.y)
                    return BorderDirection.NORTH;
                if (pos.y == range.x)
                    return BorderDirection.SOUTH;
                return default; // Return a default or undefined value
            }

            bool IsCornerOrOutsideBounds(
                Vector2Int pos,
                Vector2Int mapRange,
                List<Vector2Int> cornerCoordinates
            )
            {
                return cornerCoordinates.Contains(pos)
                    || pos.x < mapRange.x
                    || pos.x > mapRange.y
                    || pos.y < mapRange.x
                    || pos.y > mapRange.y;
            }

            bool IsOnBorder(Vector2Int pos, Vector2Int mapRange)
            {
                return pos.x == mapRange.x
                    || pos.x == mapRange.y
                    || pos.y == mapRange.x
                    || pos.y == mapRange.y;
            }

            // << ASSIGN COORDINATE TYPES >> =================================================================
            // ** Set Coordinate To Type updates the TypeMap accordingly
            foreach (Vector2Int pos in AllCoordinateValues)
            {
                // CLOSED
                if (IsCornerOrOutsideBounds(pos, mapRange, corners))
                {
                    SetCoordinateToType(pos, Coordinate.TYPE.CLOSED);
                }
                // BORDER
                else if (IsOnBorder(pos, mapRange))
                {
                    HandleBorderCoordinate(pos, mapRange);
                }
                // NULL
                else
                {
                    SetCoordinateToType(pos, Coordinate.TYPE.NULL);
                }
            }

            Initialized = true;
            await Task.CompletedTask;
        }
        #endregion

        #region [[ GET COORDINATE ]] ======================================================================== >>>>
        public Coordinate GetCoordinateAt(Vector2Int valueKey)
        {
            return Initialized && _coordinateMap.TryGetValue(valueKey, out var coordinate)
                ? coordinate
                : null;
        }

        public Coordinate GetClosestCoordinateAt(Vector3 scenePosition)
        {
            foreach (Coordinate coordinate in _coordinateMap.Values)
            {
                if (coordinate.ScenePosition.x == scenePosition.x && coordinate.ScenePosition.z == scenePosition.z)
                {
                    return coordinate;
                }
            }
            return null;
        }

        public Coordinate.TYPE? GetCoordinateTypeAt(Vector2Int valueKey)
        {
            if (Initialized && _coordinateMap.TryGetValue(valueKey, out var coordinate))
            {
                return coordinate.Type;
            }
            return null;
        }

        public List<Coordinate.TYPE?> GetCoordinateTypesAt(List<Vector2Int> valueKeys)
        {
            List<Coordinate.TYPE?> result = new();
            foreach (Vector2Int value in valueKeys)
            {
                result.Add(GetCoordinateTypeAt(value));
            }
            return result;
        }

        public HashSet<Vector2Int> GetAllCoordinatesValuesOfType(Coordinate.TYPE type)
        {
            if (!_typeMap.ContainsKey(type))
            {
                _typeMap[type] = new();
            }
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

        public Dictionary<Vector2Int, Coordinate> GetCoordinateValueMapFrom(
            List<Coordinate> coordinates
        )
        {
            Dictionary<Vector2Int, Coordinate> result = new();
            foreach (Coordinate coordinate in coordinates)
            {
                result[coordinate.ValueKey] = GetCoordinateAt(coordinate.ValueKey); // Make sure reference is to the coordinate map
            }
            return result;
        }

        public Vector2Int GetRandomCoordinateValueOfType(Coordinate.TYPE type)
        {
            List<Vector2Int> coordinatesOfType = _coordinateMap
                .Keys.Where(coord => _coordinateMap[coord].Type == type)
                .ToList();
            if (coordinatesOfType.Count > 0)
            {
                int randomIndex = Random.Range(0, coordinatesOfType.Count);
                return coordinatesOfType[randomIndex];
            }
            else
            {
                return Vector2Int.zero; // or any default value you prefer
            }
        }

        #endregion

        #region [[ SET COORDINATE ]] ======================================================================== >>>>
        void SetCoordinateToType(Vector2Int valueKey, Coordinate.TYPE newType)
        {
            // Get reference to the target coordinate
            Coordinate targetCoordinate = _coordinateMap[valueKey];
            if (targetCoordinate == null)
                return;

            // Remove Old Type
            Coordinate.TYPE oldType = targetCoordinate.Type;
            if (_typeMap.ContainsKey(oldType))
            {
                _typeMap[oldType].Remove(valueKey);
            }

            // Assign New Type
            targetCoordinate.SetType(newType);

            // If new TYPE key not found, create or add to it
            _typeMap.TryAdd(newType, new HashSet<Vector2Int>());
            _typeMap[newType].Add(valueKey);
        }

        void SetCoordinateToType(Coordinate coordinate, Coordinate.TYPE newType)
        {
            SetCoordinateToType(coordinate.ValueKey, newType);
        }

        void SetCoordinatesToType(List<Vector2Int> valueKeys, Coordinate.TYPE type)
        {
            foreach (Vector2Int key in valueKeys)
            {
                if (_coordinateMap.ContainsKey(key))
                {
                    SetCoordinateToType(key, type);
                }
            }
        }

        public void SetCoordinatesOfTypeTo(
            List<Vector2Int> positions,
            Coordinate.TYPE targetType,
            Coordinate.TYPE convertType
        )
        {
            foreach (Vector2Int pos in positions)
            {
                // Check if the position is within the map boundaries
                if (_coordinateMap.ContainsKey(pos))
                {
                    // Retrieve the coordinate at the given position
                    Coordinate coordinate = _coordinateMap[pos];

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

            // >> set the border as active on the map
            _activeBorderMap[mapBorder] = true;

            // >> get all related values on border
            List<Vector2Int> borderValues = _borderMap[mapBorder].ToList();

            // >> set all related values on border to type
            SetCoordinatesToType(borderValues, Coordinate.TYPE.CLOSED);
        }

        public void SetInactiveCornersToType(Coordinate.TYPE type)
        {
            List<Vector2Int> inactiveCorners = new List<Vector2Int>();

            // Mapping corners to their adjacent borders
            var cornerBordersMap = new Dictionary<Vector2Int, List<BorderDirection>>
            {
                [_borderCornersMap[BorderDirection.NORTH].Item1] = new List<BorderDirection>
                {
                    BorderDirection.NORTH,
                    BorderDirection.WEST
                }, // NW
                [_borderCornersMap[BorderDirection.NORTH].Item2] = new List<BorderDirection>
                {
                    BorderDirection.NORTH,
                    BorderDirection.EAST
                }, // NE
                [_borderCornersMap[BorderDirection.SOUTH].Item1] = new List<BorderDirection>
                {
                    BorderDirection.SOUTH,
                    BorderDirection.WEST
                }, // SW
                [_borderCornersMap[BorderDirection.SOUTH].Item2] = new List<BorderDirection>
                {
                    BorderDirection.SOUTH,
                    BorderDirection.EAST
                }, // SE
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

        #endregion

        // == [[ FIND COORDINATE ]] ================================================================= >>>>
        public Coordinate FindClosestCoordinateOfType(Coordinate targetCoordinate, List<Coordinate.TYPE> typeList)
        {
            // using BFS algorithm
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            queue.Enqueue(targetCoordinate.ValueKey);
            visited.Add(targetCoordinate.ValueKey);

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
                    foreach (Vector2Int neighbor in currentCoordinate.GetNaturalNeighborValues())
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        // == [[ WORLD EXITS ]] ======================================================================== >>>>
        public void ConvertCoordinateToExit(Coordinate coordinate)
        {
            if (coordinate == null)
                return;
            if (coordinate.Type != Coordinate.TYPE.BORDER)
            {
                //Debug.Log($"Cannot convert non border coordinate {coordinate.Value} {coordinate.type} to exit");
                return;
            }

            // Search borders for exit
            foreach (BorderDirection direction in _borderMap.Keys)
            {
                if (_borderMap[direction].Contains(coordinate.ValueKey))
                {
                    // Once border found, update coordinate maps
                    _borderExitMap.TryAdd(direction, new HashSet<Vector2Int>());
                    _borderExitMap[direction].Add(coordinate.ValueKey);
                }
            }

            SetCoordinateToType(coordinate, Coordinate.TYPE.EXIT);
        }

        public void GenerateRandomExits()
        {
            // Determine the number of exits to create, with a minimum of 2 and a maximum of 4
            int numberOfExits = UnityEngine.Random.Range(2, 5); // Unity's Random.Range is inclusive for min and exclusive for max

            // Combine all border positions into a single list for easier access
            List<Vector2Int> allBorderValues = new(_borderMap.Values.SelectMany(values => values));

            // Ensure not to exceed the number of available border positions
            numberOfExits = Mathf.Min(numberOfExits, allBorderValues.Count);

            // Convert random border coordinates to exits
            for (int i = 0; i < numberOfExits; i++)
            {
                Vector2Int randomValue = allBorderValues[Random.Range(0, allBorderValues.Count)];
                Coordinate coordinate = GetCoordinateAt(randomValue);
                ConvertCoordinateToExit(coordinate);
            }

            //Debug.Log($"{numberOfExits} exits have been created on the map borders.");
        }

        public void GenerateRandomExitOnBorder(BorderDirection borderType)
        {
            List<Vector2Int> allBorderPositions = _borderMap[borderType].ToList();
            Vector2Int randomCoordinate = allBorderPositions[
                Random.Range(0, allBorderPositions.Count)
            ];
            ConvertCoordinateToExit(GetCoordinateAt(randomCoordinate));
        }

        public void CreateMatchingExit(
            BorderDirection neighborBorder,
            Vector2Int neighborExitCoordinate
        )
        {
            // Determine the relative position of the exit based on the neighbor border
            Vector2Int matchingCoordinate;
            switch (neighborBorder)
            {
                case BorderDirection.NORTH:
                    // if this border is NORTH, then the matching neighbor's border is SOUTH
                    matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, 0);
                    break;
                case BorderDirection.SOUTH:
                    // if this border is SOUTH, then the matching neighbor's border is NORTH
                    matchingCoordinate = new Vector2Int(
                        neighborExitCoordinate.x,
                        this.MaxCoordinateValue - 1
                    );
                    break;
                case BorderDirection.EAST:
                    // if this border is EAST, then the matching neighbor's border is WEST
                    matchingCoordinate = new Vector2Int(0, neighborExitCoordinate.y);
                    break;
                case BorderDirection.WEST:
                    // if this border is EAST, then the matching neighbor's border is WEST
                    matchingCoordinate = new Vector2Int(
                        this.MaxCoordinateValue - 1,
                        neighborExitCoordinate.y
                    );
                    break;
                default:
                    throw new ArgumentException("Invalid MapBorder value.", nameof(neighborBorder));
            }

            ConvertCoordinateToExit(GetCoordinateAt(matchingCoordinate));
            //Debug.Log($"Created Exit {matchingCoordinate} to match {neighborExitCoordinate}");
        }

        // == [[ WORLD PATH ]] ================================================================================ >>>>
        public Path CreatePathFrom(
            Vector2Int start,
            Vector2Int end,
            List<Coordinate.TYPE> validTypes,
            bool applyPathTypeToEnd = false
        )
        {
            Path newPath = new Path(
                this,
                start,
                end,
                validTypes,
                WorldBuilder.Settings.PathRandomness
            );
            Paths.Add(newPath);

            // Remove Ends
            if (applyPathTypeToEnd)
            {
                SetCoordinatesToType(newPath.AllPositions, Coordinate.TYPE.PATH);
            }
            else
            {
                // Remove Exits from path positions
                List<Vector2Int> positionsWithoutEnds = newPath.AllPositions;
                positionsWithoutEnds.Remove(start);
                positionsWithoutEnds.Remove(end);
                SetCoordinatesToType(positionsWithoutEnds, Coordinate.TYPE.PATH);
            }

            // Assign Path Type
            return newPath;
        }

        public async Task GeneratePathsBetweenExits()
        {
            // Clear existing paths
            Paths.Clear();

            // Ensure there's more than one exit to connect
            if (Exits.Count < 2)
            {
                Debug.LogWarning("Not enough exits to generate paths.");
                return;
            }

            List<Vector2Int> sortedExits = Exits
                .OrderBy(pos => pos.x)
                .ThenBy(pos => pos.y)
                .ToList();
            for (int i = 0; i < sortedExits.Count - 1; i++)
            {
                Vector2Int start = sortedExits[i];
                Vector2Int end = sortedExits[i + 1]; // Connect to the next exit in the list

                CreatePathFrom(
                    start,
                    end,
                    new List<Coordinate.TYPE>() { Coordinate.TYPE.NULL, Coordinate.TYPE.EXIT }
                );
            }

            //Connect the last exit back to the first to ensure all exits are interconnected
            //CreatePathFrom(sortedExits[sortedExits.Count - 1], sortedExits[0]);

            //Debug.Log($"Generated new paths connecting all exits.");
            await Task.CompletedTask;
        }

        // == [[ WORLD ZONES ]] ================================================================================ >>>>
        public bool CreateWorldZone(Vector2Int position, Zone.Shape zoneType)
        {
            //Debug.Log($"Attempting to create zone at {position}");

            // Temporarily create the zone to check its positions
            Zone newZone = new Zone(GetCoordinateAt(position), zoneType, Zones.Count);

            // Check if the zone is valid
            if (!newZone.Valid)
            {
                //Debug.Log($"Zone at {position} includes invalid coordinate types. Zone creation aborted.");
                return false; // Abort the creation of the zone
            }

            // If no invalid positions are found, add the zone
            _zones.Add(newZone);
            _zoneMap[newZone] = new HashSet<Vector2Int>(newZone.Positions);
            SetCoordinatesToType(newZone.Positions, Coordinate.TYPE.ZONE);


            // Find the closest PATH Coordinate
            Coordinate closestPathCoordinate = FindClosestCoordinateOfType(
                newZone.CenterCoordinate,
                new List<Coordinate.TYPE>() { Coordinate.TYPE.PATH }
            );
            if (closestPathCoordinate != null)
            {
                // Find the closest ZONE Coordinate
                Coordinate zonePathConnection = newZone.GetClosestExternalNeighborTo(
                    closestPathCoordinate.ValueKey
                );

                Path zonePath = CreatePathFrom(
                    closestPathCoordinate.ValueKey,
                    zonePathConnection.ValueKey,
                    new List<Coordinate.TYPE>() { Coordinate.TYPE.NULL, Coordinate.TYPE.PATH },
                    true
                );
                Debug.Log($"Created zone path from {zonePath.StartPosition} to {zonePath.EndPosition} -> {zonePath.AllPositions.Count}");
                //Debug.Log($"Zone successfully created at {position} with type {zoneType}.");
            }


            return true;
        }

        public Zone GetZoneFromCoordinate(Vector2Int coordinateValue)
        {
            foreach (Zone zone in _zoneMap.Keys)
            {
                if (_zoneMap[zone].Contains(coordinateValue))
                {
                    return zone;
                }
            }
            return null;
        }

        public async Task GenerateRandomZones(int minZones, int maxZones, List<Zone.Shape> types)
        {
            Zones.Clear();

            // Determine the random number of zones to create within the specified range
            int numZonesToCreate = Random.Range(minZones, maxZones + 1);

            // A list to hold potential positions for zone creation
            List<Vector2Int> potentialPositions = GetAllCoordinatesValuesOfType(
                    Coordinate.TYPE.NULL
                )
                .ToList();

            // Shuffle the list of potential positions to randomize the selection
            ShuffleList(potentialPositions);

            // Attempt to create zones up to the determined number or until potential positions run out
            int zonesCreated = 0;
            for (int i = 0; i < potentialPositions.Count; i++)
            {
                if (zonesCreated >= numZonesToCreate)
                    break; // Stop if we've created the desired number of zones

                // Attempt to create a zone at the current position

                Zone.Shape zoneType = Zone.GetRandomTypeFromList(types);

                bool valid = CreateWorldZone(potentialPositions[i], zoneType);

                if (valid)
                {
                    zonesCreated++;
                }
            }

            //Debug.Log($"Attempted to create {numZonesToCreate} zones. Successfully created {zonesCreated}.", this._worldRegion.gameObject);
            await Task.CompletedTask;
        }

        public Zone GetZoneFromCoordinate(Coordinate coordinate)
        {
            foreach (Zone zone in _zones)
            {
                if (zone.Positions.Contains(coordinate.ValueKey))
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
