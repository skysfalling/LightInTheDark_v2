using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

[System.Serializable]
public class WorldChunk
{
    string prefix = " [[ WORLD CHUNK ]]";
    public bool initialized = false;
    WorldGeneration _worldGeneration;

    /*
     * EMPTY : 0 walls
     * WALL : 1 sidewall
     * HALLWAY : 2 parallel walls
     * CORNER : 2 perpendicular walls
     * DEADEND : 3 walls
     * CLOSED : 4 walls
     */
    public enum TYPE { EMPTY , WALL, HALLWAY, CORNER, DEADEND, CLOSED }
    public TYPE type;

    // EDGES
    public bool NorthEdgeActive { get; private set; }
    public bool SouthEdgeActive { get; private set; }
    public bool EastEdgeActive { get; private set; }
    public bool WestEdgeActive { get; private set; }

    [HideInInspector] public Mesh mesh;
    public Vector3 position;

    public List<WorldCell> localCells = new List<WorldCell>();
    Dictionary<WorldCell.TYPE, List<WorldCell>> _cellTypeMap = new Dictionary<WorldCell.TYPE, List<WorldCell>>();

    public WorldChunk(Mesh mesh, Vector3 position, int width = 3, int height = 3, int cellSize = 4)
    {
        this.mesh = mesh;
        this.position = position;

        OffsetMesh(position);
        CreateCells();
    }

    // ================ INITIALIZE =============================>>
    #region
    public void Initialize()
    {
        initialized = false;

        _worldGeneration = GameObject.FindObjectOfType<WorldGeneration>();

        DetermineChunkEdges();
        SetChunkType();
        CreateCellTypeMap();

        initialized = true;
    }
    void DetermineChunkEdges()
    {
        // Initialize all edges as active
        NorthEdgeActive = true;
        SouthEdgeActive = true;
        EastEdgeActive = true;
        WestEdgeActive = true;

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
                if (cell.position.z == northEdgeZ) NorthEdgeActive = false;
                if (cell.position.z == southEdgeZ) SouthEdgeActive = false;
                if (cell.position.x == eastEdgeX) EastEdgeActive = false;
                if (cell.position.x == westEdgeX) WestEdgeActive = false;
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
        if (NorthEdgeActive) { activeEdgeCount++; }
        if (SouthEdgeActive) { activeEdgeCount++; }
        if (EastEdgeActive) { activeEdgeCount++; }
        if (WestEdgeActive) { activeEdgeCount++; }

        // Set Type
        if (activeEdgeCount == 4) { type = TYPE.CLOSED; return; }
        if (activeEdgeCount == 3) { type = TYPE.DEADEND; return; }
        if (activeEdgeCount == 2)
        {
            // Check for parallel edges
            if (NorthEdgeActive && SouthEdgeActive) { type = TYPE.HALLWAY; return; }
            if (EastEdgeActive && WestEdgeActive) { type = TYPE.HALLWAY; return; }
            type = TYPE.CORNER;
        }
        if (activeEdgeCount == 1) { type = TYPE.WALL; return; }
        if (activeEdgeCount == 0) { type = TYPE.EMPTY; return; }
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
    #endregion

    // ================ CREATE WORLD CELLS ==============================>>
    void OffsetMesh(Vector3 position)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += position;
        }
        mesh.vertices = vertices;
    }
    void CreateCells()
    {
        localCells.Clear();
        int cell_index = 0;

        // Get topface vertices
        List<Vector3> topfacevertices = new List<Vector3>();
        foreach (Vector3 vertice in mesh.vertices)
        {
            if (vertice.y >= 0)
            {
                topfacevertices.Add(vertice);
            }
        }

        // Sort topface vertices
        List<Vector3> uniquevertices = topfacevertices.Distinct().ToList();
        List<Vector3> sortedVertices = uniquevertices.OrderBy(v => v.z).ThenBy(v => v.x).ToList();

        // Determine the number of vertices per row
        int verticesPerRow = 1;
        for (int i = 1; i < sortedVertices.Count; i++)
        {
            if (sortedVertices[i].z != sortedVertices[0].z)
            {
                break;
            }
            verticesPerRow++;
        }

        // Group sorted vertices into squares
        for (int rowStartIndex = 0; rowStartIndex < sortedVertices.Count - verticesPerRow; rowStartIndex += verticesPerRow)
        {
            for (int colIndex = rowStartIndex; colIndex < rowStartIndex + verticesPerRow - 1; colIndex++)
            {
                // Check for invalid indexes
                if (colIndex + verticesPerRow < sortedVertices.Count)
                {
                    Vector3 bottomLeft = sortedVertices[colIndex];
                    Vector3 bottomRight = sortedVertices[colIndex + 1];
                    Vector3 topLeft = sortedVertices[colIndex + verticesPerRow];
                    Vector3 topRight = sortedVertices[colIndex + verticesPerRow + 1];

                    // Create a square (as a cube) at each set of vertices
                    localCells.Add(new WorldCell(this, cell_index, new Vector3[] { bottomLeft, bottomRight, topLeft, topRight }));
                    cell_index++;
                }
            }
        }
    }

    // ================== SPAWN OBJECTS ================================================================ >>
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
        int cellSize = _worldGeneration.cellSize;

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


    // ================= HELPER FUNCTIONS ==============================>>
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


