using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldChunk
{
    public enum FaceType{ Front, Back, Left, Right, Top, Bottom }


    WorldGeneration _worldGeneration;
    string prefix = " [[ WORLD CHUNK ]]";
    int _chunkWidth { get { return WorldGeneration.ChunkWidth_inCells; } }

    public CoordinateMap cellCoordinateMap { get; }
    public WorldChunkMesh chunkMesh;
    public Color debugColor = Color.white;
    public int groundHeight { get; private set; }
    int _realChunkHeight { get { return groundHeight * WorldGeneration.CellWidth_inWorldSpace; } }




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
    public WorldChunkMap chunkMap { get; private set; }
    public Vector2Int localPosition { get; private set; }
    public bool generation_finished { get; private set; }

    public Coordinate chunkCoordinate { get; private set; }
    public Vector3 groundPosition { get; private set; }
    public Vector3 groundMeshDimensions { get; private set; }
    public Vector3 originCoordinatePosition { get; }
    // Active Edges
    bool _northEdgeActive;
    bool _southEdgeActive;  
    bool _eastEdgeActive;
    bool _westEdgeActive;



    public List<WorldCell> localCells = new List<WorldCell>();
    Dictionary<WorldCell.TYPE, List<WorldCell>> _cellTypeMap = new();
    Dictionary<FaceType, HashSet<WorldCell>> _cellFaceMap = new();
    public WorldChunk(WorldChunkMap chunkMap, Coordinate coordinate)
    {
        this.chunkMap = chunkMap;
        this.chunkCoordinate = coordinate;
        this.localPosition = coordinate.localPosition;
        this.groundHeight = PerlinNoise.CalculateHeightFromNoise(this.localPosition);


        // Determine position & dimenstions
        groundPosition = new Vector3(coordinate.worldPosition.x, _realChunkHeight, coordinate.worldPosition.z);
        groundMeshDimensions = new Vector3(_chunkWidth, _realChunkHeight, _chunkWidth);

        // >> Origin Coordinate Position { Bottom Left }
        originCoordinatePosition = groundPosition;
        originCoordinatePosition -= WorldGeneration.GetChunkWidth_inWorldSpace() * new Vector3(0.5f, 0, 0.5f);

        // Create coordinate map
        this.cellCoordinateMap = new CoordinateMap(this);
    }

    public void CreateChunkMesh()
    {
        // Create chunkMesh
        chunkMesh = new WorldChunkMesh(this, groundHeight, groundPosition);

        // Create Cells
        localCells.Clear();
        foreach (MeshQuad quad in chunkMesh.meshQuads)
        {
            // Spawn top face cells
            if (quad.faceType != FaceType.Bottom)
            {
                WorldCell newCell = new WorldCell(this, quad);
                localCells.Add(newCell);

                if (!_cellFaceMap.ContainsKey(quad.faceType)) { _cellFaceMap[quad.faceType] = new(); }
                _cellFaceMap[quad.faceType].Add(newCell);
            }
        }

        DetermineChunkEdges();
        SetChunkType();
        CreateCellTypeMap();
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
        if (localPosition == null) return;
        groundPosition = new Vector3(chunkCoordinate.worldPosition.x, _realChunkHeight, chunkCoordinate.worldPosition.z);
        groundMeshDimensions = new Vector3(_chunkWidth, _realChunkHeight, _chunkWidth);
    }

    #region ================ INITIALIZE WORLD CHUNK ============================= >>

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
            if (cell.worldPosition.z > northEdgeZ) northEdgeZ = cell.worldPosition.z;
            if (cell.worldPosition.z < southEdgeZ) southEdgeZ = cell.worldPosition.z;
            if (cell.worldPosition.x > eastEdgeX) eastEdgeX = cell.worldPosition.x;
            if (cell.worldPosition.x < westEdgeX) westEdgeX = cell.worldPosition.x;
        }

        // Check each cell
        foreach (WorldCell cell in localCells)
        {
            if (cell.type == WorldCell.TYPE.EMPTY)
            {
                if (cell.worldPosition.z == northEdgeZ) _northEdgeActive = false;
                if (cell.worldPosition.z == southEdgeZ) _southEdgeActive = false;
                if (cell.worldPosition.x == eastEdgeX) _eastEdgeActive = false;
                if (cell.worldPosition.x == westEdgeX) _westEdgeActive = false;
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

    #endregion

    // ================ CREATE & INITIALIZE WORLD CELLS ============================== >>



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
            cellAreaList += $"{cell.worldPosition} {cell.type}\n";
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
        int cellSize = WorldGeneration.CellWidth_inWorldSpace;

        for (int x = 0; x < space.x; x++)
        {
            for (int z = 0; z < space.y; z++)
            {
                WorldCell cell = GetCellAt(startCell.worldPosition.x + (x * cellSize), startCell.worldPosition.z + (z * cellSize));
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
            if (cell.worldPosition.x == x && cell.worldPosition.z == z) { return cell; }
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
        if (!generation_finished) { return new List<WorldCell>(); }
        if (!_cellTypeMap.ContainsKey(cellType)) { _cellTypeMap[cellType] = new List<WorldCell>(); }
        return _cellTypeMap[cellType];
    }

    public WorldCell GetRandomCellOfType(WorldCell.TYPE cellType)
    {
        if (!generation_finished) { return null; }
        List<WorldCell> cells = GetCellsOfType(cellType);
        return cells[UnityEngine.Random.Range(0, cells.Count)];
    }
}


