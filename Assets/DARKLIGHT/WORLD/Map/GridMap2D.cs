using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.World.Settings;
using UnityEngine;
using Darklight.Bot;


#if UNITY_EDITOR
using UnityEditor;
#endif  

namespace Darklight.World.Map
{

    [System.Serializable]
    public class GridMap2D
    {
        #region ============== STATIC FUNCTIONS ===================== ////
        static Dictionary<Direction, Vector2Int> DirectionVectorMap =
                new Dictionary<Direction, Vector2Int>()
                {
            { Direction.NORTH, new Vector2Int(0, 1) },
            { Direction.SOUTH, new Vector2Int(0, -1) },
            { Direction.WEST, new Vector2Int(-1, 0) },
            { Direction.EAST, new Vector2Int(1, 0) },
            { Direction.NORTHWEST, new Vector2Int(-1, 1) },
            { Direction.NORTHEAST, new Vector2Int(1, 1) },
            { Direction.SOUTHWEST, new Vector2Int(-1, -1) },
            { Direction.SOUTHEAST, new Vector2Int(1, -1) }
                };
        public static Vector2Int GetVectorFromDirection(Direction direction)
        {
            return DirectionVectorMap[direction];
        }
        public static Direction? GetDirectionFromVector(Vector2Int vector)
        {
            if (DirectionVectorMap.ContainsValue(vector))
            {
                return DirectionVectorMap.FirstOrDefault(x => x.Value == vector).Key;
            }
            return null;
        }
        public static Vector2Int GetVectorFromEdgeDirection(EdgeDirection edgeDirection)
        {
            Direction direction = (Direction)ConvertToDirection(edgeDirection);
            return DirectionVectorMap[direction];
        }
        public static EdgeDirection? ConvertToEdgeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.NORTH:
                    return EdgeDirection.NORTH;
                case Direction.SOUTH:
                    return EdgeDirection.SOUTH;
                case Direction.WEST:
                    return EdgeDirection.WEST;
                case Direction.EAST:
                    return EdgeDirection.EAST;
                default:
                    return null;
            }
        }
        public static Direction? ConvertToDirection(EdgeDirection direction)
        {
            switch (direction)
            {
                case EdgeDirection.NORTH:
                    return Direction.NORTH;
                case EdgeDirection.SOUTH:
                    return Direction.SOUTH;
                case EdgeDirection.WEST:
                    return Direction.WEST;
                case EdgeDirection.EAST:
                    return Direction.EAST;
                default:
                    return null;
            }
        }
        public static EdgeDirection? DetermineBorderEdge(Vector2Int positionKey, int mapWidth)
        {
            if (positionKey == new Vector2Int(0, 0)
                || positionKey == new Vector2Int(0, mapWidth - 1)
                || positionKey == new Vector2Int(mapWidth - 1, 0)
                || positionKey == new Vector2Int(mapWidth - 1, mapWidth - 1))
                return null;

            bool isLeftOrRightColumn = positionKey.x == 0 || positionKey.x == mapWidth - 1;
            bool isTopOrBottomRow = positionKey.y == 0 || positionKey.y == mapWidth - 1;

            if (isTopOrBottomRow)
            {
                if (positionKey.y == 0)
                    return EdgeDirection.SOUTH;
                if (positionKey.y == mapWidth - 1)
                    return EdgeDirection.NORTH;
            }
            else if (isLeftOrRightColumn)
            {
                if (positionKey.x == 0)
                    return EdgeDirection.WEST;
                if (positionKey.x == mapWidth - 1)
                    return EdgeDirection.EAST;
            }
            return null;
        }

        /// <summary>
        /// Determines the corner edges based on the given position key and map width.
        /// </summary>
        /// <param name="positionKey">The position key.</param>
        /// <param name="mapWidth">The width of the map.</param>
        /// <returns>A tuple containing two nullable EdgeDirection values representing the corner edges.</returns>
        (EdgeDirection?, EdgeDirection?) DetermineCornerEdgeDirections(Vector2Int positionKey, int mapWidth)
        {
            EdgeDirection? edge1 = null;
            EdgeDirection? edge2 = null;

            // Get X value edge
            if (positionKey == new Vector2Int(0, 0))
            {
                edge1 = EdgeDirection.WEST;
                edge2 = EdgeDirection.SOUTH;
            }
            else if (positionKey == new Vector2Int(mapWidth - 1, mapWidth - 1))
            {
                edge1 = EdgeDirection.EAST;
                edge2 = EdgeDirection.NORTH;
            }
            else if (positionKey == new Vector2Int(0, mapWidth - 1))
            {
                edge1 = EdgeDirection.WEST;
                edge2 = EdgeDirection.NORTH;
            }
            else if (positionKey == new Vector2Int(mapWidth - 1, 0))
            {
                edge1 = EdgeDirection.EAST;
                edge2 = EdgeDirection.SOUTH;
            }

            return (edge1, edge2);
        }

        public static EdgeDirection? DetermineOppositeEdgeDirection(EdgeDirection direction)
        {
            switch (direction)
            {
                case EdgeDirection.NORTH:
                    return EdgeDirection.SOUTH;
                case EdgeDirection.SOUTH:
                    return EdgeDirection.NORTH;
                case EdgeDirection.WEST:
                    return EdgeDirection.EAST;
                case EdgeDirection.EAST:
                    return EdgeDirection.WEST;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a Dictionary of all positions in Direction
        ///     { NORTH, SOUTH, EAST, WEST, NORTHWEST, NORTHEAST, SOUTHWEST, SOUTHEAST }
        /// </summary>
        public static Dictionary<Direction, Vector2Int> GetDirectionMap(Vector2Int positionKey)
        {
            Dictionary<Direction, Vector2Int> result = new Dictionary<Direction, Vector2Int>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                Vector2Int neighborValue = positionKey + GetVectorFromDirection(direction);
                result[direction] = neighborValue;
            }
            return result;
        }

        /// <summary>
        /// Returns a Dictionary of all positions in EdgeDirection
        ///     {NORTH, SOUTH, EAST, WEST }
        /// </summary>
        public static Dictionary<EdgeDirection, Vector2Int> GetEdgeDirectionMap(Vector2Int positionKey)
        {
            Dictionary<EdgeDirection, Vector2Int> result = new Dictionary<EdgeDirection, Vector2Int>();
            foreach (EdgeDirection edgeDirection in Enum.GetValues(typeof(EdgeDirection)))
            {
                Direction directionValue = (Direction)ConvertToDirection(edgeDirection);
                Vector2Int neighborValue = positionKey + GetVectorFromDirection(directionValue);
                result[edgeDirection] = neighborValue;
            }
            return result;
        }

        // system random to get random values inside a Serializable class
        private static readonly System.Random sysRandom = new System.Random();
        #endregion

        #region class Coordinate
        public class Coordinate
        {
            /// <summary>
            /// Represents the possible flags for a coordinate.
            /// </summary>
            public enum Flag { NULL, BORDER, CORNER, EXIT, PATH, ZONE, CLOSED }

            GridMap2D _parentGrid;
            UnitSpace _unitSpace;
            [SerializeField] private Flag _flag = Flag.NULL;
            [SerializeField] private Vector2Int _key;
            [SerializeField] private int _size;

            /// <summary>
            /// Gets the parent grid of the coordinate.
            /// </summary>
            public GridMap2D ParentGrid { get { return _parentGrid; } }

            /// <summary>
            /// Gets or sets the current flag of the coordinate.
            /// </summary>
            public Flag CurrentFlag { get { return _flag; } set { _flag = value; } }

            /// <summary>
            /// Gets the position key of the coordinate.
            /// </summary>
            public Vector2Int PositionKey { get { return _key; } }

            /// <summary>
            /// Gets the size of the coordinate.
            /// </summary>
            public int Size { get { return _size; } }
            /// <summary>
            /// Gets the position of the coordinate in the scene.
            /// </summary>
            public Vector3 GetPositionInScene()
            {
                return _parentGrid.OriginPosition + new Vector3(_key.x, 0, _key.y) * _size;
            }

            public Dictionary<Direction, Vector2Int> DirectionMap => GetDirectionMap(_key);
            public Dictionary<EdgeDirection, Vector2Int> EdgeDirectionMap => GetEdgeDirectionMap(_key);
            /// <summary>
            /// Initializes a new instance of the <see cref="Coordinate"/> class.
            /// </summary>
            /// <param name="parentGridMap">The parent grid map.</param>
            /// <param name="positionKey">The position key of the coordinate.</param>
            public Coordinate(GridMap2D parentGridMap, Vector2Int positionKey)
            {
                this._parentGrid = parentGridMap;
                this._unitSpace = ParentGrid._coordinateUnitSpace;
                this._size = ParentGrid.CoordinateSize;
                this._key = positionKey;
            }

            /// <summary>
            /// Sets the flag of the coordinate.
            /// </summary>
            /// <param name="newFlag">The new flag to set.</param>
            public void SetFlag(Flag newFlag)
            {
                _parentGrid.SetCoordinateFlag(_key, newFlag);
            }

            /// <summary>
            /// Gets the color associated with a specific flag.
            /// </summary>
            /// <param name="flag">The flag to get the color for.</param>
            public Color GetFlagColor(Flag flag)
            {
                switch (flag)
                {
                    case Flag.CLOSED: return Color.black;
                    case Flag.EXIT: return Color.red;
                    case Flag.PATH: return Color.white;
                    case Flag.ZONE: return Color.green;
                }
                return Color.grey;
            }

            /// <summary>
            /// Gets the color associated with the current flag of the coordinate.
            /// </summary>
            public Color GetCurrentFlagColor()
            {
                return GetFlagColor(_flag);
            }

            #endregion
        }

        #region class Border

        public class Border
        {
            public EdgeDirection Direction { get; private set; }
            public List<Vector2Int> Positions { get; private set; }
            public bool isClosed;

            public Border(EdgeDirection direction, List<Vector2Int> positions, bool isClosed)
            {
                this.Direction = direction;
                this.Positions = positions;
                this.isClosed = isClosed;
            }

            public void AddPosition(Vector2Int position)
            {
                if (Positions == null) { Positions = new List<Vector2Int>(); }
                if (Positions.Contains(position) == false)
                {
                    Positions.Add(position);
                }
            }

            public void RemovePosition(Vector2Int position)
            {
                if (Positions == null) { return; }
                if (Positions.Contains(position))
                {
                    Positions.Remove(position);
                }
            }
        }

        #endregion

        #region class Path
        public class Path
        {
            List<Vector2Int> _positions = new();
            public Vector2Int StartPosition { get; private set; }
            public Vector2Int EndPosition { get; private set; }
            public List<Vector2Int> AllPositions => _positions;

            public Path(GridMap2D gridMap2D, Vector2Int start, Vector2Int end, List<Coordinate.Flag> validTypes, float pathRandomness = 0.5f)
            {
                this.StartPosition = start;
                this.EndPosition = end;

                _positions = FindPath(gridMap2D, this.StartPosition, this.EndPosition, validTypes, pathRandomness);
            }
        }


        #endregion

        #region << MAP DATA <<
        Dictionary<Vector2Int, Coordinate> _map = new Dictionary<Vector2Int, Coordinate>();
        Dictionary<Coordinate.Flag, HashSet<Vector2Int>> _mapFlags = new();
        Dictionary<EdgeDirection, Border> _mapBorders = new();
        Dictionary<(EdgeDirection, EdgeDirection), Vector2Int> _mapCorners = new();
        Dictionary<(Vector2Int, Vector2Int), Path> _paths = new();
        #endregion

        #region << PRIVATE VARIABLES <<
        string _prefix = "[[ GridMap2D ]] ";
        Transform _transform; // to use as position parent
        [SerializeField] GenerationSettings _settings;
        [SerializeField] UnitSpace _mapUnitSpace; // defines GridMap sizing
        [SerializeField] UnitSpace _coordinateUnitSpace; // defines Coordinate sizing
        [SerializeField] int _mapWidth = 11; // width count of the grid [[ grid will always be a square ]]
        [SerializeField] int _coordinateSize = 1; // size of each GridCoordinate to determine position offsets
        #endregion

        #region << PUBLIC ACCESSORS <<
        public int MapWidth { get { return _mapWidth; } private set { _mapWidth = value; } }
        public int CoordinateSize { get { return _coordinateSize; } private set { _coordinateSize = value; } }
        public Dictionary<Vector2Int, Coordinate> FullMap { get { return _map; } }
        public List<Vector2Int> PositionKeys { get { return _map.Keys.ToList(); } }
        public List<Coordinate> CoordinateValues { get { return _map.Values.ToList(); } }
        public Dictionary<EdgeDirection, Border> MapBorders { get { return _mapBorders; } }
        public Dictionary<(EdgeDirection, EdgeDirection), Vector2Int> MapCorners { get { return _mapCorners; } }
        public Vector3 OriginPosition
        {
            get
            {
                if (_transform != null) { return _transform.position; }
                else { return Vector3.zero; }
            }
        }
        public Vector3 CenterPosition
        {
            get
            {
                return OriginPosition + new Vector3(_mapWidth / 2, 0, _mapWidth / 2) * _coordinateSize;
            }
        }
        #endregion

        #region << INSPECTOR VALUES <<
        public CustomGenerationSettings customGenerationSettings = null;
        public TaskBotQueen taskBotQueen = new TaskBotQueen();
        #endregion

        #region [[ CONSTRUCTORS ]]
        public GridMap2D()
        {
            this._transform = null;
            this._mapUnitSpace = UnitSpace.GAME;
            this._coordinateUnitSpace = UnitSpace.GAME;
        }

        /// <summary>
        /// Main GridMap2D constructor
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="unitSpace"></param>
        /// <param name="mapWidth"></param>
        /// <param name="coordinateSize"></param>
        public GridMap2D(Transform transform, UnitSpace unitSpace)
        {
            this._transform = transform;
            this._mapUnitSpace = unitSpace;
        }
        #endregion

        #region [[ INITIALIZATION ]]

        /// <summary>
        /// Initializes the grid map by creating coordinate objects for each position in the map.
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Log($"{_prefix} Initializing GridMap2D. [ _mapWidth : {_mapWidth} , _coordinateSize : {_coordinateSize} ]");

            // Create Coordinate grid
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapWidth; y++)
                {
                    // Calculate Grid Key
                    Vector2Int gridKey = new Vector2Int(x, y);

                    // Create Coordinate Tuple
                    Coordinate coordinate = new Coordinate(this, gridKey);
                    _map[gridKey] = coordinate;

                    // >> Check if BORDER
                    EdgeDirection? borderEdge = DetermineBorderEdge(gridKey, _mapWidth);
                    if (borderEdge != null)
                    {
                        EdgeDirection borderDirection = (EdgeDirection)borderEdge;
                        if (_mapBorders.ContainsKey(borderDirection) == false)
                        {
                            _mapBorders[borderDirection] = new Border(borderDirection, new List<Vector2Int>(), false);
                        }
                        _mapBorders[borderDirection].AddPosition(gridKey); // << add position to set
                        coordinate.SetFlag(Coordinate.Flag.BORDER);
                        //Debug.Log($"{gridKey} -> BORDER ({borderDirection})");
                        continue;
                    }

                    // >> Check if CORNER
                    (EdgeDirection?, EdgeDirection?) CornerEdgeDirections = DetermineCornerEdgeDirections(gridKey, _mapWidth);
                    if (CornerEdgeDirections.Item1 != null && CornerEdgeDirections.Item2 != null)
                    {

                        // store corner in map
                        (EdgeDirection, EdgeDirection) cornerTuple = ((EdgeDirection)CornerEdgeDirections.Item1, (EdgeDirection)CornerEdgeDirections.Item2);
                        _mapCorners[cornerTuple] = gridKey; // << overwrite corner
                        coordinate.SetFlag(Coordinate.Flag.CORNER);

                        // remove from the border map
                        if (_mapBorders.ContainsKey(cornerTuple.Item1))
                            _mapBorders[cornerTuple.Item1].RemovePosition(gridKey);
                        if (_mapBorders.ContainsKey(cornerTuple.Item2))
                            _mapBorders[cornerTuple.Item2].RemovePosition(gridKey);

                        //Debug.Log($"{gridKey} -> CORNER ({cornerTuple.Item1}, {cornerTuple.Item2}) ");
                        continue;
                    }

                    // >> Set to NULL
                    coordinate.SetFlag(Coordinate.Flag.NULL);
                }
            }
        }

        public void Initialize(int width, int size)
        {
            _mapWidth = width;
            _coordinateSize = size;
            Initialize();
        }
        public void Initialize(GenerationSettings settings)
        {
            _settings = settings;
            _mapWidth = _settings.WorldWidth_inRegionUnits;
            _coordinateSize = _settings.RegionFullWidth_inGameUnits;
            Initialize();
        }
        public void Initialize(CustomGenerationSettings customSettings)
        {
            _settings.Initialize(customSettings);
            _mapWidth = _settings.WorldWidth_inRegionUnits;
            _coordinateSize = _settings.RegionFullWidth_inGameUnits;
            Initialize();
        }
        #endregion

        #region ( GET COORDINATE ) ============================== ////
        public Coordinate GetCoordinateAt(Vector2Int valueKey)
        {
            return _map.TryGetValue(valueKey, out var coordinate)
                ? coordinate
                : null;
        }

        public Coordinate.Flag? GetCoordinateTypeAt(Vector2Int positionKey)
        {
            if (_map.TryGetValue(positionKey, out var coordinate))
            {
                return coordinate.CurrentFlag;
            }
            return null;
        }

        public List<Coordinate.Flag?> GetCoordinateTypesAt(List<Vector2Int> positionKey)
        {
            List<Coordinate.Flag?> result = new();
            foreach (Vector2Int value in positionKey)
            {
                result.Add(GetCoordinateTypeAt(value));
            }
            return result;
        }

        public HashSet<Vector2Int> GetAllCoordinatesWithFlag(Coordinate.Flag flag)
        {
            if (_mapFlags.ContainsKey(flag))
            {
                return _mapFlags[flag];
            }
            return new HashSet<Vector2Int>();
        }

        public Vector2Int? GetRandomCoordinateWithFlag(Coordinate.Flag flag)
        {
            HashSet<Vector2Int> positionsWithFlag = GetAllCoordinatesWithFlag(flag);
            if (positionsWithFlag.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, positionsWithFlag.Count);
                return positionsWithFlag.ElementAt(randomIndex);
            }
            return null;
        }

        public Coordinate GetClosestCoordinateTo(Vector3 scenePosition)
        {
            float closestDistance = float.MaxValue;
            Coordinate closestCoordinate = null;
            foreach (Coordinate coordinate in _map.Values)
            {
                float distance = Vector3.Distance(coordinate.GetPositionInScene(), scenePosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCoordinate = coordinate;
                }
            }
            return closestCoordinate;
        }

        #endregion

        #region ( SET COORDINATE ) ============================== ////

        public void SetCoordinateFlag(Vector2Int positionKey, Coordinate.Flag newFlag)
        {
            Coordinate targetCoordinate = GetCoordinateAt(positionKey);
            if (targetCoordinate == null) { return; }

            Coordinate.Flag oldFlag = targetCoordinate.CurrentFlag;
            if (_mapFlags.ContainsKey(oldFlag))
            {
                _mapFlags[oldFlag].Remove(targetCoordinate.PositionKey);
            }

            targetCoordinate.CurrentFlag = newFlag;

            if (_mapFlags.ContainsKey(newFlag) == false)
                _mapFlags.TryAdd(newFlag, new HashSet<Vector2Int>());
            _mapFlags[newFlag].Add(targetCoordinate.PositionKey);
        }
        public void SetCoordinateFlag(Coordinate coordinate, Coordinate.Flag newFlag)
        {
            SetCoordinateFlag(coordinate.PositionKey, newFlag);
        }
        public void SetCoordinatesToFlag(List<Vector2Int> positionKeys, Coordinate.Flag newFlag)
        {
            foreach (Vector2Int positionKey in positionKeys)
            {
                SetCoordinateFlag(positionKey, newFlag);
            }
        }
        public void ConvertCoordinateFlags(List<Vector2Int> positionKeys, Coordinate.Flag targetType, Coordinate.Flag convertType)
        {
            foreach (Vector2Int positionKey in positionKeys)
            {
                if (_map.ContainsKey(positionKey))
                {
                    Coordinate coordinate = _map[positionKey];
                    if (coordinate.CurrentFlag == targetType)
                    {
                        SetCoordinateFlag(positionKey, convertType);
                    }
                }
            }
        }

        #endregion

        #region == [[ FIND COORDINATE ]] ================================================================= >>>>
        public Coordinate FindClosestCoordinateTo(Coordinate targetCoordinate, List<Coordinate.Flag> validFlags)
        {
            // using BFS algorithm
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            queue.Enqueue(targetCoordinate.PositionKey);
            visited.Add(targetCoordinate.PositionKey);

            while (queue.Count > 0)
            {
                Vector2Int currentValue = queue.Dequeue();
                Coordinate currentCoordinate = GetCoordinateAt(currentValue);

                if (currentCoordinate != null)
                {
                    // Check if the current coordinate is the target type
                    if (validFlags.Contains(currentCoordinate.CurrentFlag))
                    {
                        return GetCoordinateAt(currentValue);
                    }

                    // Get the neighbors of the current coordinate
                    foreach (Vector2Int neighbor in currentCoordinate.EdgeDirectionMap.Values)
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
        #endregion

        #region == [[ HANDLE BORDERS ]] =================================================================== >>>>

        public void CreateRandomExitOnBorder(EdgeDirection edgeDirection, int count = 1)
        {
            List<Vector2Int> borderPositions = _mapBorders[edgeDirection].Positions;
            if (borderPositions == null || borderPositions.Count == 0)
            {
                Debug.Assert(false, "Cannot create exit on empty border.");
                return;
            }

            bool closed = _mapBorders[edgeDirection].isClosed;
            if (closed)
            {
                Debug.Assert(false, "Cannot create exit on closed border.");
                return;
            }

            // Ensure count does not exceed the number of available border positions
            count = Mathf.Min(count, borderPositions.Count);

            // Convert random border coordinates to exit
            int validExitCount = 0;
            while (validExitCount < count)
            {
                int randomInt = sysRandom.Next(0, borderPositions.Count);
                Coordinate coordinate = GetCoordinateAt(borderPositions[randomInt]);
                if (coordinate != null && coordinate.CurrentFlag == Coordinate.Flag.BORDER)
                {
                    coordinate.SetFlag(Coordinate.Flag.EXIT); // Set coordinate flag
                    validExitCount++;
                }
            }

            //Debug.Log($"{numberOfExits} exits have been created on the map borders.");
        }

        public void CreateMatchingExitOnBorder(EdgeDirection neighborBorder, Vector2Int neighborExitCoordinate)
        {
            // Determine the relative position of the exit based on the neighbor border
            Vector2Int matchingCoordinate;
            switch (neighborBorder)
            {
                // if neighbor border is NORTH, then the matching exit border is SOUTH
                case EdgeDirection.NORTH:
                    matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, 0);
                    break;
                // if neighbor border is SOUTH then the matching exit border is NORTH
                case EdgeDirection.SOUTH:
                    matchingCoordinate = new Vector2Int(neighborExitCoordinate.x, this._mapWidth - 1);
                    break;
                // if neighbor border is EAST then the matching exit border is WEST
                case EdgeDirection.EAST:
                    matchingCoordinate = new Vector2Int(0, neighborExitCoordinate.y);
                    break;
                // if neighbor border is WEST then the matching exit border is EAST
                case EdgeDirection.WEST:
                    matchingCoordinate = new Vector2Int(this._mapWidth - 1, neighborExitCoordinate.y);
                    break;
                default:
                    throw new ArgumentException("Invalid MapBorder value.", nameof(neighborBorder));
            }

            // Set coordinate flag to exit
            Coordinate coordinate = GetCoordinateAt(matchingCoordinate);
            if (coordinate != null)
            {
                coordinate.SetFlag(Coordinate.Flag.EXIT);
            }
            //Debug.Log($"Created Exit {matchingCoordinate} to match {neighborExitCoordinate}");
        }

        public void CloseMapBorder(EdgeDirection mapBorder)
        {
            // >> set the border as closed on the map
            if (_mapBorders.ContainsKey(mapBorder) == false)
            {
                Debug.Assert(false, "Cannot close border that does not exist.");
                return;
            }
            _mapBorders[mapBorder].isClosed = true;

            // >> set all related values on border to flag
            SetCoordinatesToFlag(_mapBorders[mapBorder].Positions, Coordinate.Flag.CLOSED);
        }

        /*
                public void SetOpenCornerFlags(Coordinate.Flag flag)
                {
                    HashSet<Vector2Int> openCorners = new HashSet<Vector2Int>();
                    foreach (Vector2Int position in _mapCorners.Values)
                    {
                        (EdgeDirection?, EdgeDirection?) cornerEdges = DetermineCornerEdgeDirections(position, _mapWidth);

                        bool edge1Closed = cornerEdges.Item1 != null && IsBorderClosed((EdgeDirection)cornerEdges.Item1);
                        bool edge2Closed = cornerEdges.Item2 != null && IsBorderClosed((EdgeDirection)cornerEdges.Item2);
                        if (!edge1Closed && !edge2Closed)
                        {
                            openCorners.Add(position);
                        }
                    }
                    SetCoordinatesToFlag(openCorners.ToList(), flag);
                }
                */
        #endregion

        #region == [[ HANDLE PATHFINDING ]] ================================================================ >>>>
        public static List<Vector2Int> FindPath(GridMap2D gridMap2D, Vector2Int startPosition, Vector2Int endCoord, List<Coordinate.Flag> validFlags, float pathRandomness = 0)
        {
            // A* Pathfinding implementation
            // gCost is the known cost from the starting node
            // hCost is the estimated distance to the end node
            // fCost is gCost + hCost

            // Helper function to calculate validity of values
            bool IsCoordinateValidForPathfinding(Vector2Int candidate)
            {
                Coordinate coordinate = gridMap2D.GetCoordinateAt(candidate);
                if (coordinate == null) { return false; }
                else if (validFlags.Contains(coordinate.CurrentFlag)) { return true; }
                return false;
            }

            // Store all possible positions from the coordinate map
            List<Vector2Int> positions = gridMap2D.PositionKeys;
            // Initialize the open set with the start coordinate
            List<Vector2Int> openSet = new List<Vector2Int> { startPosition };
            // Initialize the closed set as an empty collection of Vector2Int
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

            // Initialize costs for all coordinates to infinity, except the start coordinate
            Dictionary<Vector2Int, float> gCost = new Dictionary<Vector2Int, float>();
            Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
            foreach (Vector2Int pos in positions)
            {
                gCost[pos] = float.MaxValue;
            }
            gCost[startPosition] = 0;

            // Initialize the heuristic costs
            Dictionary<Vector2Int, float> fCost = new Dictionary<Vector2Int, float>();
            foreach (Vector2Int pos in positions)
            {
                fCost[pos] = float.MaxValue;
            }
            fCost[startPosition] = Vector2Int.Distance(startPosition, endCoord);

            while (openSet.Count > 0)
            {
                Vector2Int current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    Vector2Int candidate = openSet[i];
                    // Convert FCost and HCost checks to work with Vector2Int by accessing WorldCoordinate properties
                    if (fCost[candidate] <= fCost[current] && UnityEngine.Random.Range(0f, 1f) <= pathRandomness) // Apply randomness
                    {
                        current = openSet[i];
                    }
                }

                if (current == endCoord)
                {
                    // Path has been found
                    return RetracePath(startPosition, endCoord, parents);
                }

                openSet.Remove(current);
                closedSet.Add(current);


                // [[ ITERATE THROUGH NATURAL NEIGHBORS ]]
                foreach (Vector2Int pos in GetEdgeDirectionMap(current).Values)
                {
                    if (closedSet.Contains(pos) || IsCoordinateValidForPathfinding(pos) == false)
                        continue; // Skip non-traversable neighbors and those already evaluated

                    float tentativeGCost = gCost[current] + Vector2Int.Distance(current, pos);

                    if (tentativeGCost < gCost[pos])
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        parents[pos] = current;
                        gCost[pos] = tentativeGCost;
                        fCost[pos] = tentativeGCost + Vector2Int.Distance(pos, endCoord);

                        if (!openSet.Contains(pos))
                            openSet.Add(pos);
                    }
                }
            }

            // If we reach here, then there is no path
            return new List<Vector2Int>();
        }

        // Helper method to retrace path from end to start using parent references
        static List<Vector2Int> RetracePath(Vector2Int startCoord, Vector2Int endCoord, Dictionary<Vector2Int, Vector2Int> parents)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int currentCoord = endCoord;

            while (currentCoord != startCoord)
            {
                path.Add(currentCoord);
                currentCoord = parents[currentCoord]; // Move to the parent coordinate
            }
            path.Add(startCoord); // Add the start coordinate at the end
            path.Reverse(); // Reverse the list to start from the beginning

            return path;
        }

        public Path CreatePathFrom(Vector2Int start, Vector2Int end, List<Coordinate.Flag> validFlags, bool applyPathFlagToEnd = false)
        {
            Path newPath = new Path(this, start, end, validFlags, _settings.PathRandomness);
            _paths[(start, end)] = newPath;

            // Remove Ends
            if (applyPathFlagToEnd)
            {
                SetCoordinatesToFlag(newPath.AllPositions, Coordinate.Flag.PATH);
            }
            else
            {
                // Remove Exits from path positions
                List<Vector2Int> positionsWithoutEnds = newPath.AllPositions;
                positionsWithoutEnds.Remove(start);
                positionsWithoutEnds.Remove(end);
                SetCoordinatesToFlag(newPath.AllPositions, Coordinate.Flag.PATH);
            }

            // Assign Path Type
            return newPath;
        }


        #endregion

        public virtual void Reset()
        {
            Debug.Log($"{_prefix} Resetting GridMap2D");
            _map.Clear();
            if (customGenerationSettings)
            {
                Initialize(customGenerationSettings);
                return;
            }
            Initialize(_mapWidth, _coordinateSize);
        }
    }

    public interface IGridMapData
    {
        GridMap2D gridMapParent { get; set; }
        GridMap2D.Coordinate coordinateValue { get; set; }
        Vector2Int positionKey { get; set; }
        Task Initialize(GridMap2D parent, Vector2Int positionKey);
    }

    /// <summary>
    /// An enhanced version of GridMap2D that stores a DataMap of <<typeparamref name="T"/>>
    /// </summary>
    /// <typeparam name="IGridMapData"></typeparam>
    [System.Serializable]
    public class GridMap2D<T> : GridMap2D where T : IGridMapData, new()
    {
        public bool DataInitialized { get { return DataMap != null; } }
        public Dictionary<Vector2Int, T> DataMap { get; private set; } = null;
        public List<T> DataValues { get { return DataMap.Values.ToList(); } }
        public GridMap2D() : base() { }
        public GridMap2D(Transform transform, UnitSpace unitSpace) : base(transform, unitSpace) { }
        public virtual async Task InitializeDataMap()
        {
            DataMap = new Dictionary<Vector2Int, T>();
            foreach (Vector2Int position in PositionKeys)
            {
                // Create a new instance of T
                T newData = new T();
                // Initialize the new instance with the parent grid (this) and the position key
                await newData.Initialize(this, position);
                // Add the instance to the DataMap
                DataMap[position] = newData;
            }
        }

        public virtual T GetDataAt(Vector2Int positionKey)
        {
            return DataMap.TryGetValue(positionKey, out var data)
                ? data
                : default;
        }

        public virtual List<T> GetDataAt(List<Vector2Int> positionKeys)
        {
            List<T> result = new();
            foreach (Vector2Int value in positionKeys)
            {
                result.Add(GetDataAt(value));
            }
            return result;
        }

        public override void Reset()
        {
            base.Reset();
            if (DataMap != null)
            {
                DataMap.Clear();
            }
        }
    }

    #region ============== [[ CUSTOM PROPERTY DRAWER ]] ====== >>>>
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GridMap2D))]
    public class GridMap2DDrawer : PropertyDrawer
    {
        bool initialized = false;
        bool setCustomSettings = false;
        bool mapSettingsFoldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the target object the property belongs to
            UnityEngine.Object targetObject = property.serializedObject.targetObject;

            // Reflection to call ResetMap
            GridMap2D gridMap2DField = fieldInfo.GetValue(targetObject) as GridMap2D;

            // Fetch the objects needed
            SerializedProperty generationSettingsProp = property.FindPropertyRelative("_settings");
            SerializedProperty customGenerationSettingsProp = property.FindPropertyRelative("customGenerationSettings");
            SerializedProperty mapWidthProp = property.FindPropertyRelative("_mapWidth");
            SerializedProperty coordinateSizeProp = property.FindPropertyRelative("_coordinateSize");

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);

            // << DRAW LABEL >>
            EditorGUILayout.LabelField($"{label} >> Grid Map 2D", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(customGenerationSettingsProp);
            if (gridMap2DField.customGenerationSettings == null)
            {
                setCustomSettings = false;
                DrawDefaultSettingsFoldout(mapWidthProp, coordinateSizeProp);
            }
            else
            {
                if (setCustomSettings == false)
                {
                    initialized = false;
                }

                EditorGUILayout.LabelField($"{mapWidthProp.name} override -> WorldWidth : {gridMap2DField.customGenerationSettings.WorldWidth}");
                EditorGUILayout.LabelField($"{coordinateSizeProp.name} override -> RegionWidth : {gridMap2DField.customGenerationSettings.RegionWidth}");
            }
            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck() || initialized == false)
            {
                initialized = true;
                setCustomSettings = true;
                property.serializedObject.ApplyModifiedProperties();
                if (gridMap2DField != null)
                {
                    gridMap2DField.Reset();
                }
            }
        }

        void DrawDefaultSettingsFoldout(SerializedProperty mapWidthProp, SerializedProperty coordinateSizeProp)
        {
            mapSettingsFoldout = EditorGUILayout.Foldout(mapSettingsFoldout, "Default Map Settings");
            if (mapSettingsFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                mapWidthProp.intValue = EditorGUILayout.IntSlider(new GUIContent("Map Width"), mapWidthProp.intValue, 1, 100);
                coordinateSizeProp.intValue = EditorGUILayout.IntSlider(new GUIContent("Coordinate Size"), coordinateSizeProp.intValue, 1, 100);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
        }
    }
#endif
    #endregion

}

/*


        // == [[ WORLD PATH ]] ================================================================================ >>>>
        

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

*/