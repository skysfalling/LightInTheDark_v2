using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

[System.Serializable]
public class WorldChunk
{
    WorldGeneration _worldGeneration;
    string prefix = " [[ WORLD CHUNK ]]";
    bool _initialized = false;
    Vector2 _realChunkAreaSize { get { return WorldGeneration.GetRealChunkAreaSize(); } }
    
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
    public WorldCoordinate coordinate;


    public static Vector3 GroundPosition { get; private set; }
    public static Vector3 GroundMeshDimensions { get; private set; }
    public static Vector3 GroundMeshSpawnPosition { get; private set; }

    // Active Edges
    bool _northEdgeActive;
    bool _southEdgeActive;  
    bool _eastEdgeActive;
    bool _westEdgeActive;

    // Mesh
    [HideInInspector] public Mesh mesh;

    public WorldChunk(WorldCoordinate coordinate)
    {
        this.coordinate = coordinate;
        this.groundHeight = 0;
        GroundPosition = new Vector3( coordinate.WorldPosition.x, _realChunkHeight, coordinate.WorldPosition.z);
        GroundMeshDimensions = new Vector3(_realChunkAreaSize.x, _realChunkHeight, _realChunkAreaSize.y);
        GroundMeshSpawnPosition = new Vector3(GroundPosition.x, _realChunkHeight * 0.5f, GroundPosition.z);
    }

    public void SetGroundHeight(int height)
    {
        this.groundHeight = height;
        RecalcuatePosition();
    }

    public Vector3 GetGroundWorldPosition() 
    {
        RecalcuatePosition();
        return GroundPosition; 
    }

    void RecalcuatePosition()
    {
        if (coordinate == null) return;
        GroundPosition = new Vector3(coordinate.WorldPosition.x, _realChunkHeight, coordinate.WorldPosition.z);
        GroundMeshDimensions = new Vector3(_realChunkAreaSize.x, _realChunkHeight, _realChunkAreaSize.y);
        GroundMeshSpawnPosition = new Vector3(GroundPosition.x, _realChunkHeight * 0.5f, GroundPosition.z);
    }
    

    // ================ INITIALIZE WORLD CHUNK ============================= >>
    #region
    public void Initialize()
    {
        _initialized = false;

        CreateMesh();
        OffsetMesh(this.coordinate.WorldPosition);
        CreateCells();
        DetermineChunkEdges();
        SetChunkType();
        CreateCellTypeMap();

        _initialized = true;
    }
    void CreateMesh()
    {
        int cellSize = WorldGeneration.CellSize;
        Vector3Int chunkDimensions = WorldGeneration.GetRealChunkDimensions();

        Mesh newMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Initialize UV list

        // Helper method to add vertices for a face
        // 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid
        List<Vector3> AddFace(Vector3 start, Vector3 u, Vector3 v, int uDivisions, int vDivisions, Vector3 faceNormal)
        {
            List<Vector3> faceVertices = new List<Vector3>();

            for (int i = 0; i <= vDivisions; i++)
            {
                for (int j = 0; j <= uDivisions; j++)
                {

                    Vector3 newVertex = start + (i * cellSize * v) + (j * cellSize * u);
                    vertices.Add(newVertex);
                    faceVertices.Add(newVertex);

                    // Standard UV rectangle for each face
                    float uCoord = 1 - (j / (float)uDivisions); // Flipped horizontally
                    float vCoord = i / (float)vDivisions;
                    uvs.Add(new Vector2(uCoord, vCoord));
                }
            }

            return faceVertices;
        }

        Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }


        // << FACES >>
        // note** :: starts the faces at -fullsized_chunkDimensions.y so that top of chunk is at y=0
        // -- the chunks will be treated as a 'Generated Ground' to build upon

        Vector3Int fullsize_chunkDimensions = chunkDimensions * cellSize;
        Vector3 newFaceStartOffset = new Vector3((fullsize_chunkDimensions.x) * 0.5f, -(fullsize_chunkDimensions.y), (fullsize_chunkDimensions.z) * 0.5f);

        // Forward face
        Vector3 forwardFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, 1));
        Vector3 forwardFaceNormal = Vector3.forward; // Normal for forward face
        AddFace(forwardFaceStartVertex, Vector3.right, Vector3.up, chunkDimensions.x, chunkDimensions.y, forwardFaceNormal);

        // Back face
        Vector3 backFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, -1));
        Vector3 backFaceNormal = Vector3.back; // Normal for back face
        AddFace(backFaceStartVertex, Vector3.left, Vector3.up, chunkDimensions.x, chunkDimensions.y, backFaceNormal);

        // Right face
        Vector3 rightFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        Vector3 rightFaceNormal = Vector3.right; // Normal for right face
        AddFace(rightFaceStartVertex, Vector3.forward, Vector3.up, chunkDimensions.z, chunkDimensions.y, rightFaceNormal);

        // Left face
        Vector3 leftFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, 1));
        Vector3 leftFaceNormal = Vector3.left; // Normal for left face
        AddFace(leftFaceStartVertex, Vector3.back, Vector3.up, chunkDimensions.z, chunkDimensions.y, leftFaceNormal);

        // Bottom face
        Vector3 bottomFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        Vector3 bottomFaceNormal = Vector3.down; // Normal for bottom face
        AddFace(bottomFaceStartVertex, Vector3.right, Vector3.forward, chunkDimensions.x, chunkDimensions.z, bottomFaceNormal);

        // Top face
        Vector3 topFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 0, -1));
        Vector3 topFaceNormal = Vector3.up; // Normal for top face
        List<Vector3> topfacevertices = AddFace(topFaceStartVertex, Vector3.left, Vector3.forward, chunkDimensions.x, chunkDimensions.z, topFaceNormal);

        // Helper method to dynamically generate triangles for a face
        void AddFaceTriangles(int faceStartIndex, int uDivisions, int vDivisions)
        {
            for (int i = 0; i < vDivisions; i++)
            {
                for (int j = 0; j < uDivisions; j++)
                {
                    int rowStart = faceStartIndex + i * (uDivisions + 1);
                    int nextRowStart = faceStartIndex + (i + 1) * (uDivisions + 1);

                    int bottomLeft = rowStart + j;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = nextRowStart + j;
                    int topRight = topLeft + 1;

                    // Add two triangles for each square
                    List<int> newSquareMesh = new List<int>() { bottomLeft, topRight, topLeft, topRight, bottomLeft, bottomRight };
                    triangles.AddRange(newSquareMesh);
                }
            }
        }


        // ITERATE through 6 faces
        // Triangles generation
        int vertexCount = 0;
        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            if (faceIndex < 4) // Side faces (XY plane)
            {
                AddFaceTriangles(vertexCount, chunkDimensions.x, chunkDimensions.y);
                vertexCount += (chunkDimensions.x + 1) * (chunkDimensions.y + 1);
            }
            else // Vertical faces (XZ plane)
            {
                AddFaceTriangles(vertexCount, chunkDimensions.x, chunkDimensions.z);
                vertexCount += (chunkDimensions.x + 1) * (chunkDimensions.z + 1);
            }
        }

        // Apply the vertices and triangles to the mesh
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uvs.ToArray();

        // Recalculate normals for proper lighting
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        this.mesh = newMesh;
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

    #endregion

    // ================ CREATE & INITIALIZE WORLD CELLS ============================== >>
    public List<WorldCell> localCells = new List<WorldCell>();
    Dictionary<WorldCell.TYPE, List<WorldCell>> _cellTypeMap = new Dictionary<WorldCell.TYPE, List<WorldCell>>();
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
                if (colIndex + verticesPerRow + 1 < sortedVertices.Count)
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
    void OffsetMesh(Vector2 chunkPosition)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += new Vector3(chunkPosition.x, 0, chunkPosition.y);
        }
        mesh.vertices = vertices;
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
        if (!_initialized) { return new List<WorldCell>(); }
        if (!_cellTypeMap.ContainsKey(cellType)) { _cellTypeMap[cellType] = new List<WorldCell>(); }
        return _cellTypeMap[cellType];
    }

    public WorldCell GetRandomCellOfType(WorldCell.TYPE cellType)
    {
        if (!_initialized) { return null; }
        List<WorldCell> cells = GetCellsOfType(cellType);
        return cells[UnityEngine.Random.Range(0, cells.Count)];
    }
}


