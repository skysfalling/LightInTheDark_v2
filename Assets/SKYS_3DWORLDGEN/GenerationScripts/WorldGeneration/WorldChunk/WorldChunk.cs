using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WorldChunk
{
    public enum FaceType{ Front, Back, Right, Left, Top, Bottom }


    WorldGeneration _worldGeneration;
    string prefix = " [[ WORLD CHUNK ]]";
    Vector2 _realChunkAreaSize { get { return WorldGeneration.GetRealChunkAreaSize(); } }

    public WorldChunkMesh chunkMesh;
    public int groundHeight { get; private set; }
    int _realChunkHeight { get { return groundHeight * WorldGeneration.CellSize; } }


    /// <summary>
    /// Defines World Chunks based on wall count / location
    /// </summary>
    public enum TYPE
    {
        /// <summary>No walls present.</summary>
        EMPTY,
        /// <summary>One sidewall present.</summary>
        WALL,
        /// <summary>Two parallel walls present, forming a hallway.</summary>
        HALLWAY,
        /// <summary>Two perpendicular walls present, forming a corner.</summary>
        CORNER,
        /// <summary>Three walls present, forming a dead end.</summary>
        DEADEND,
        /// <summary>Enclosed by walls on all four sides.</summary>
        CLOSED,
        /// <summary>Indicates a boundary limit, set by WorldCoordinateMap.</summary>
        BORDER,
        /// <summary>Indicates an exit point, set by WorldExit.</summary>
        EXIT
    }
    public TYPE type;
    public Vector2Int coordinate;
    public bool initialized = false;

    public WorldCoordinate worldCoordinate { get { return WorldCoordinateMap.CoordinateMap[coordinate]; } }
    public Vector3 groundPosition { get; private set; }
    public Vector3 groundMeshDimensions { get; private set; }

    // Active Edges
    bool _northEdgeActive;
    bool _southEdgeActive;  
    bool _eastEdgeActive;
    bool _westEdgeActive;

    public WorldChunk(WorldCoordinate worldCoord)
    {
        this.coordinate = worldCoord.Coordinate;
        this.groundHeight = 0;
        groundPosition = new Vector3(worldCoord.WorldPosition.x, _realChunkHeight, worldCoord.WorldPosition.z);
        groundMeshDimensions = new Vector3(_realChunkAreaSize.x, _realChunkHeight, _realChunkAreaSize.y);
    }

    public void SetGroundHeight(int height)
    {
        this.groundHeight = height;
        RecalcuatePosition();
    }

    public Vector3 GetGroundWorldPosition() 
    {
        RecalcuatePosition();
        return groundPosition; 
    }

    void RecalcuatePosition()
    {
        if (coordinate == null) return;
        groundPosition = new Vector3(worldCoordinate.WorldPosition.x, _realChunkHeight, worldCoordinate.WorldPosition.z);
        groundMeshDimensions = new Vector3(_realChunkAreaSize.x, _realChunkHeight, _realChunkAreaSize.y);
    }


    #region ================ INITIALIZE WORLD CHUNK ============================= >>
    public void Initialize()
    {
        initialized = false;

        DetermineChunkHeightFromNeighbors();

        chunkMesh = new WorldChunkMesh(groundHeight, groundPosition);


        CreateCells();

        DetermineChunkEdges();
        SetChunkType();
        CreateCellTypeMap();

        initialized = true;
    }


    void DetermineChunkEdges()
    {
        // Initialize all edges as active
        _northEdgeActive = true;
        _southEdgeActive = true;
        _eastEdgeActive = true;
        _westEdgeActive = true;

        // Define the edge positions
        float northEdgeZ = float.MinValue;
        float southEdgeZ = float.MaxValue;
        float eastEdgeX = float.MinValue;
        float westEdgeX = float.MaxValue;

        // Find the edge positions
        foreach (WorldCell cell in localCells)
        {
            if (cell.position.z > northEdgeZ) northEdgeZ = cell.position.z;
            if (cell.position.z < southEdgeZ) southEdgeZ = cell.position.z;
            if (cell.position.x > eastEdgeX) eastEdgeX = cell.position.x;
            if (cell.position.x < westEdgeX) westEdgeX = cell.position.x;
        }

        // Check each cell
        foreach (WorldCell cell in localCells)
        {
            if (cell.type == WorldCell.TYPE.EMPTY)
            {
                if (cell.position.z == northEdgeZ) _northEdgeActive = false;
                if (cell.position.z == southEdgeZ) _southEdgeActive = false;
                if (cell.position.x == eastEdgeX) _eastEdgeActive = false;
                if (cell.position.x == westEdgeX) _westEdgeActive = false;
            }
        }

        /*
        // Log the active edges
        Debug.Log($"North Edge Active: {NorthEdgeActive}\n " +
                  $"South Edge Active: {SouthEdgeActive}\n " +
                  $"East Edge Active: {EastEdgeActive}\n " +
                  $"West Edge Active: {WestEdgeActive}\n ");
        */
    }
    void SetChunkType()
    {
        // Get Edge Count
        int activeEdgeCount = 0;
        if (_northEdgeActive) { activeEdgeCount++; }
        if (_southEdgeActive) { activeEdgeCount++; }
        if (_eastEdgeActive) { activeEdgeCount++; }
        if (_westEdgeActive) { activeEdgeCount++; }

        // Set Type
        if (activeEdgeCount == 4) { type = TYPE.CLOSED; return; }
        if (activeEdgeCount == 3) { type = TYPE.DEADEND; return; }
        if (activeEdgeCount == 2)
        {
            // Check for parallel edges
            if (_northEdgeActive && _southEdgeActive) { type = TYPE.HALLWAY; return; }
            if (_eastEdgeActive && _westEdgeActive) { type = TYPE.HALLWAY; return; }
            type = TYPE.CORNER;
        }
        if (activeEdgeCount == 1) { type = TYPE.WALL; return; }
        if (activeEdgeCount == 0) { type = TYPE.EMPTY; return; }
    }
    public void DetermineChunkHeightFromNeighbors()
    {
        if (WorldCoordinateMap.CoordinateMap[coordinate].type != WorldCoordinate.TYPE.NULL) { return; }

        List<WorldCoordinate> neighbors = WorldCoordinateMap.GetAllCoordinateNeighbors(coordinate);
        if (neighbors.Count == 0) return; // Exit if there are no neighbors

        int totalHeight = 0;

        foreach (WorldCoordinate neighborCoord in neighbors)
        {
            WorldChunk neighbor = WorldChunkMap.GetChunkAt(neighborCoord);
            if (neighbor != null)
            {
                totalHeight += neighbor.groundHeight;
            }
        }

        int averageHeight = Mathf.RoundToInt(totalHeight / neighbors.Count);

        // Optionally, you might want to round the average height or apply other logic
        // For simplicity, we're directly setting the average height
        SetGroundHeight(averageHeight);
    }
    #endregion

    // ================ CREATE & INITIALIZE WORLD CELLS ============================== >>
    public List<WorldCell> localCells = new List<WorldCell>();
    Dictionary<WorldCell.TYPE, List<WorldCell>> _cellTypeMap = new Dictionary<WorldCell.TYPE, List<WorldCell>>();
    void CreateCells()
    {
        localCells.Clear();

        foreach(MeshQuad quad in chunkMesh.meshQuads)
        {
            // Spawn top face cells
            if (quad.faceType == FaceType.Top)
            {
                WorldCell newCell = new WorldCell(this, quad);
                localCells.Add(newCell);
            }
        }
    }

    void CreateCellTypeMap()
    {
        _cellTypeMap.Clear();
        foreach (WorldCell cell in localCells)
        {
            // Create new List for new key
            if (!_cellTypeMap.ContainsKey(cell.type))
            {
                _cellTypeMap[cell.type] = new List<WorldCell>();
            }

            _cellTypeMap[cell.type].Add(cell);
        }
    }

    // ================== SPAWN OBJECTS ============= >>
    public List<WorldCell> FindSpace(EnvironmentObject envObj)
    {
        Dictionary<int, List<WorldCell>> availableSpace = new Dictionary<int, List<WorldCell>>();
        int spaceIndex = 0;

        foreach (WorldCell startCell in localCells)
        {
            if (IsSpaceAvailable(startCell, envObj))
            {
                availableSpace[spaceIndex] = GetCellsInArea(startCell, envObj.space);
                spaceIndex++;
            }
        }

        // Get Random Available Space
        if (availableSpace.Count > 0)
        {
            return availableSpace[UnityEngine.Random.Range(0, availableSpace.Keys.Count)];
        }

        // Return Empty
        return new List<WorldCell>();
    }
    public bool IsSpaceAvailable(WorldCell startCell, EnvironmentObject envObj)
    {
        List<WorldCell> cellsInArea = GetCellsInArea(startCell, envObj.space);
        List<WorldCell.TYPE> requiredTypes = envObj.spawnCellTypeRequirements;

        // Check Cell Count ( Also Area Size )
        int spawnAreaSize = envObj.space.x * envObj.space.y;
        if (cellsInArea.Count != spawnAreaSize) { return false; }

        // Check Validity of Cell Types
        foreach (WorldCell cell in cellsInArea)
        {
            if (!requiredTypes.Contains(cell.type)) { return false; }
        }

        string cellAreaList = "";
        foreach (WorldCell cell in cellsInArea)
        {
            cellAreaList += $"{cell.position} {cell.type}\n";
        }

        /*
        Debug.Log($"{prefix} SpaceAvailableAt {startCell.position}\n" +
            $"\tEnvObj Prefab : {envObj.prefab.name}\n" +
            $"\tSpace Needed : {envObj.space} {spawnAreaSize}\n" +
            $"\tFound Space Count : {cellsInArea.Count}\n" +
            $"\tRequired Type : {requiredTypes[0]}\n" +
            $"\tCell Area : {cellAreaList}");
        */

        return true;
    }

    // NOTE :: This is specifically starting with the top left cell.
    private List<WorldCell> GetCellsInArea(WorldCell startCell, Vector2Int space)
    {
        List<WorldCell> areaCells = new List<WorldCell> ();
        int cellSize = WorldGeneration.CellSize;

        for (int x = 0; x < space.x; x++)
        {
            for (int z = 0; z < space.y; z++)
            {
                WorldCell cell = GetCellAt(startCell.position.x + (x * cellSize), startCell.position.z + (z * cellSize));
                if (cell != null && !areaCells.Contains(cell))
                {
                    areaCells.Add(cell);
                }
            }
        }

        return areaCells;
    }

    private WorldCell GetCellAt(float x, float z)
    {
        //Debug.Log($"{prefix} GetCellAt {x} {z}\n");

        foreach (WorldCell cell in localCells)
        {
            if (cell.position.x == x && cell.position.z == z) { return cell; }
        }

        return null;
    }

    public void MarkArea(List<WorldCell> area, WorldCell.TYPE markType)
    {
        foreach (WorldCell cell in area) { cell.SetCellType(markType); }
    }

    // ================= HELPER FUNCTIONS ============================== >>
    public List<WorldCell> GetCellsOfType(WorldCell.TYPE cellType)
    {
        if (!initialized) { return new List<WorldCell>(); }
        if (!_cellTypeMap.ContainsKey(cellType)) { _cellTypeMap[cellType] = new List<WorldCell>(); }
        return _cellTypeMap[cellType];
    }

    public WorldCell GetRandomCellOfType(WorldCell.TYPE cellType)
    {
        if (!initialized) { return null; }
        List<WorldCell> cells = GetCellsOfType(cellType);
        return cells[UnityEngine.Random.Range(0, cells.Count)];
    }
}


