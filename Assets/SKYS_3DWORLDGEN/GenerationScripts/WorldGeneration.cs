using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Handles the procedural generation of the game world in Unity.
/// Responsible for initializing and populating the world with terrain, landscapes,
/// resources, and points of interest. Interacts with various world components to
/// ensure a cohesive and dynamically generated game environment.
/// </summary>


[RequireComponent(typeof(WorldPathfinder))]
[RequireComponent(typeof(WorldInteractor))]
[RequireComponent(typeof(WorldStatTracker))]
[RequireComponent(typeof(WorldMaterialLibrary))]
public class WorldGeneration : MonoBehaviour
{
    public static WorldGeneration Instance;
    public void Awake()
    {
        if (Instance != null) { Destroy(Instance); }
        Instance = this;
    }

    string _prefix = "[ WORLD GENERATION ] ";
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _worldGenerationRoutine;
    List<WorldChunk> _chunks = new List<WorldChunk>();
    List<WorldChunk> _borderChunks = new List<WorldChunk>();

    [HideInInspector] public bool generation_finished = false;

    [Space(10), Header("Materials")]
    public Material chunkMaterial; // Assign a material in the inspector
    public Material borderChunkMaterial; // Assign a material in the inspector

    [Header("World Cells")]
    [Range(1, 16)] public int cellSize = 4; // Size of each subdivision cell

    // {{ CHUNK DIMENSIONS }} =====================================
    [Header("World Chunks")]
    [Range(0, 21)] public int worldChunk_widthInCells = 5; // Length of width in cells
    [Range(0, 21)] public int worldChunk_heightInCells = 3; // Length of height in cells
    [HideInInspector] public Vector3Int worldChunkDimensions { get; private set; } // chunkDimensions { WorldCell size units }
    [HideInInspector] public Vector3Int realWorldChunkSize { get; private set; } // fullSize Chunk Dimensions [ in Unity units ]

    // {{ AREA DIMENSIONS }} =====================================
    [Header("World Area")]
    [Range(1, 21)] public int worldPlayArea_widthInChunks = 10; // Size of PlayArea { in WorldChunk Size Units }
    [Range(1, 5)] public int worldChunkBoundaryOffset = 1; // size of boundary offset
    [HideInInspector] public int worldBoundary_widthInChunks { get; private set; }
    [HideInInspector] public Vector2Int realWorldPlayAreaSize { get; private set; } // worldChunkArea * cellSize
    [HideInInspector] public Vector2Int realWorldBoundarySize { get; private set; } // worldChunkArea + 1 for exit chunks * cellSize


    [Header("Generation Values")]
    public int steps = 10; // Number of steps in the random walk

    private void Start()
    {
        StartGeneration();
    }

    #region == Initialzer Functions ======
    public void StartGeneration()
    {
        Reset();
        SetWorldDimensions();

        if (_worldGenerationRoutine != null) { StopCoroutine(_worldGenerationRoutine); }
        _worldGenerationRoutine = StartCoroutine(Generate());

    }

    public void SetWorldDimensions()
    {
        worldChunkDimensions = new Vector3Int(worldChunk_widthInCells, worldChunk_heightInCells, worldChunk_widthInCells);
        realWorldChunkSize = worldChunkDimensions * cellSize;

        realWorldPlayAreaSize = worldPlayArea_widthInChunks  * new Vector2Int(realWorldChunkSize.x, realWorldChunkSize.z);

        worldBoundary_widthInChunks = worldPlayArea_widthInChunks + worldChunkBoundaryOffset;
        realWorldBoundarySize = worldBoundary_widthInChunks * new Vector2Int(realWorldChunkSize.x, realWorldChunkSize.z);
    }

    public void Reset()
    {
        Destroy(_worldGenerationObject);
        Destroy(_worldBorderObject);

        FindObjectOfType<WorldChunkMap>().Reset();
        FindObjectOfType<WorldCellMap>().Reset();
        FindObjectOfType<WorldSpawnMap>().Reset();
        FindObjectOfType<WorldEnvironment>().Reset();
        FindObjectOfType<WorldStatTracker>().UpdateStats();

        _chunks.Clear();
        _borderChunks.Clear();
    }
    #endregion

    IEnumerator Generate(float delay = 0.25f)
    {
        generation_finished = false;
        if (_worldGenerationObject != null) { Reset(); }

        // Get All Chunk Positions
        List<Vector2> allWorldChunkPositions = GetAllWorldChunkPositions();
        List<Vector2> allBorderChunkPositions = GetAllBoundaryChunkPositions();
        List<Vector2> allPlayAreaChunkPositions = GetAllPlayAreaChunkPositions();

        // [[ GENERATE PLAY AREA CHUNKS ]] ========================================== >>
        foreach (Vector3 position in allPlayAreaChunkPositions)
        {
            WorldChunk newChunk = CreateWorldChunk(position, worldChunkDimensions);
            _chunks.Add(newChunk);
        }

        // [[ GENERATE BORDER CHUNKS ]] ========================================== >>
        foreach (Vector3 position in allBorderChunkPositions)
        {
            WorldChunk newChunk = CreateWorldChunk(position, worldChunkDimensions);
            _borderChunks.Add(newChunk);
        }

        // [[ GENERATE WORLD CHUNKS ]] ========================================== >>
        // Create Combined Mesh of world chunks
        Mesh combinedMesh = CombineChunks(_chunks);
        _worldGenerationObject = CreateCombinedMeshObject(combinedMesh, chunkMaterial);
        _worldGenerationObject.transform.parent = transform;
        _worldGenerationObject.name = "(WORLD GENERATION) Ground Layout";
        MeshCollider collider = _worldGenerationObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;

        // Create Combined Mesh
        Mesh combinedBorderMesh = CombineChunks(_borderChunks);
        _worldBorderObject = CreateCombinedMeshObject(combinedBorderMesh, borderChunkMaterial);
        _worldBorderObject.transform.parent = transform;
        _worldBorderObject.transform.position += (Vector3Int.up * realWorldChunkSize);
        _worldBorderObject.name = "(GEN) Ground Border";

        #region [[ INITIALIZE MAPS ]] ============================================= >>    
        // Initialize Cell Map
        WorldCellMap worldCellMap = FindObjectOfType<WorldCellMap>();
        worldCellMap.InitializeCellMap();
        yield return new WaitUntil(() => worldCellMap.initialized);
        Debug.Log(_prefix + "COMPLETE : Initialized World Cell Map");

        // Initialize Chunk Map
        WorldChunkMap worldChunkMap = FindObjectOfType<WorldChunkMap>();
        FindObjectOfType<WorldChunkMap>().InitializeChunkMap();
        yield return new WaitUntil(() => worldChunkMap.initialized);
        Debug.Log(_prefix + "COMPLETE : Initialized World Chunk Map");

        // Initialize Spawn Map
        WorldSpawnMap worldSpawnMap = FindObjectOfType<WorldSpawnMap>();
        FindObjectOfType<WorldSpawnMap>().InitializeSpawnMap();
        yield return new WaitUntil(() => worldCellMap.initialized);
        Debug.Log(_prefix + "COMPLETE : Initialized World Spawn Map");

        // [[ ENVIRONMENT GENERATION ]] ===================================
        WorldEnvironment worldEnvironment = FindObjectOfType<WorldEnvironment>();
        worldEnvironment.StartEnvironmentGeneration();
        yield return new WaitUntil(() => worldEnvironment.generation_finished);
        Debug.Log(_prefix + "COMPLETE : Finished Environment Generation");
#endregion

        generation_finished = true;

    }

    public List<Vector2> GetAllWorldChunkPositions()
    {
        List<Vector2> chunkPositions = new();

        int worldBoundary_widthInCells = worldPlayArea_widthInChunks + (worldChunkBoundaryOffset);
        float halfSize_worldBoundary_widthInCells = worldBoundary_widthInCells * 0.5f;

        for (float x = -halfSize_worldBoundary_widthInCells; x <= halfSize_worldBoundary_widthInCells; x++)
        {
            for (float z = -halfSize_worldBoundary_widthInCells; z <= halfSize_worldBoundary_widthInCells; z++)
            {
                // Offset position
                Vector2 newPos = new Vector2(x * realWorldChunkSize.x, z * realWorldChunkSize.z);
                chunkPositions.Add(newPos);
            }
        }
        return chunkPositions;
    }

    public List<Vector2> GetAllBoundaryChunkPositions()
    {
        List<Vector2> allPositions = GetAllWorldChunkPositions();
        List<Vector2> boundaryPositions = new List<Vector2>();
        Vector2 halfSize_playArea = (Vector2)realWorldPlayAreaSize * 0.5f;
        Vector3 halfSize_chunkSize = (Vector3)realWorldChunkSize * 0.5f;

        float minX = -halfSize_playArea.x;
        float maxX = halfSize_playArea.x;
        float minZ = -halfSize_playArea.y;
        float maxZ = halfSize_playArea.y;

        boundaryPositions = allPositions.Where(position => 
            position.x <= minX || position.x  >= maxX || 
            position.y <= minZ || position.y >= maxZ)
                .ToList();

        return boundaryPositions;
    }

    public List<Vector2> GetAllPlayAreaChunkPositions()
    {
        List<Vector2> allPositions = GetAllWorldChunkPositions();
        List<Vector2> boundaryPositions = GetAllBoundaryChunkPositions();
        List<Vector2> playAreaPositions = allPositions.Where(position =>
            !boundaryPositions.Contains(position)
        ).ToList();

        return playAreaPositions;
    }


    #region == GENERATE CHUNKS ================================================ ///

    public List<WorldChunk> GetChunks()
    {
        if (_chunks.Count == 0 || _chunks == null)
        {
            return new List<WorldChunk>();
        }

        return _chunks;
    }

    public WorldChunk GetChunkAt(Vector3 position)
    {
        foreach (WorldChunk chunk in _chunks)
        {
            if (chunk.position == position) { return chunk; }
        }
        return null;
    }

    public List<WorldChunk> GetBorderChunks()
    {
        if (_borderChunks.Count == 0 || _borderChunks == null)
        {
            return new List<WorldChunk>();
        }

        return _borderChunks;
    }

    public List<WorldCell> GetCells()
    {
        List<WorldCell> cells = new List<WorldCell>();

        foreach (WorldChunk chunk in _chunks)
        {
            foreach (WorldCell cell in chunk.localCells)
            {
                cells.Add(cell);
            }
        }

        return cells;
    }
    #endregion

    #region =========================================== CREATE CHUNK MESH ==============================================

    /// <summary>
    /// Creates a chunk mesh with specified dimensions, vertices, triangles, and UVs. 
    /// Constructs faces of the chunk and computes triangles for each face.
    /// </summary>
    /// <returns>A Mesh object representing a chunk of terrain or object.</returns>
    WorldChunk CreateWorldChunk(Vector3 position, Vector3Int chunkDimensions)
    {
        Mesh mesh = new Mesh();

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
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // << CREATE CHUNK >>
        WorldChunk chunk = new WorldChunk(mesh, position);
        return chunk;
    }

    /// <summary>
    /// Combines multiple Mesh objects into a single mesh. This is useful for optimizing rendering by reducing draw calls.
    /// </summary>
    /// <param name="meshes">A List of Mesh objects to be combined.</param>
    /// <returns>A single combined Mesh object.</returns>
    Mesh CombineChunks(List<WorldChunk> chunks)
    {
        // Get Meshes from chunks
        List<Mesh> meshes = new List<Mesh>();
        foreach (WorldChunk chunk in chunks)
        {
            meshes.Add(chunk.mesh);
        }

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>(); // Add a list for the new UVs

        int vertexOffset = 0; // Keep track of the vertex offset

        foreach (Mesh mesh in meshes)
        {
            newVertices.AddRange(mesh.vertices); // Add all vertices

            // Add all UVs from the current mesh
            newUVs.AddRange(mesh.uv);

            // Add the triangles, adjusted by the current vertex offset
            foreach (var tri in mesh.triangles)
            {
                newTriangles.Add(tri + vertexOffset);
            }

            // Update the vertex offset for the next mesh
            vertexOffset += mesh.vertexCount;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = newVertices.ToArray();
        combinedMesh.triangles = newTriangles.ToArray();
        combinedMesh.uv = newUVs.ToArray(); // Set the combined UVs

        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();

        return combinedMesh;
    }

    /// <summary>
    /// Creates a GameObject to represent the combined mesh in the scene, attaching necessary components like MeshFilter and MeshRenderer.
    /// </summary>
    /// <param name="combinedMesh">The combined Mesh to be represented.</param>
    GameObject CreateCombinedMeshObject(Mesh combinedMesh, Material material)
    {
        GameObject worldObject = new GameObject("CombinedChunk");
        worldObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        worldObject.AddComponent<MeshRenderer>().material = material;

        return worldObject;
    }
    #endregion

    // =========================================== GENERATION HANDLER ==============================================

    /// <summary>
    /// Generates a list of Vector3 positions based on a random walk algorithm.
    /// </summary>
    /// <param name="steps">Number of steps in the random walk.</param>
    /// <returns>A List of Vector3 positions representing the path of the random walk.</returns>
    List<Vector3> GenerateRandomWalkPositions(int steps)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 currentPos = Vector3.zero; // Starting position
        positions.Add(currentPos); // always have starter position

        if (steps <= 1) { return positions; }

        Vector2 halfSize_worldPlayArea = (Vector2)realWorldPlayAreaSize * 0.5f; // Half of the world border size

        for (int i = 0; i < steps; i++)
        {
            List<Vector3> potentialDirections = new List<Vector3>
            {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right
            };

            bool validPositionFound = false;
            while (potentialDirections.Count > 0 && !validPositionFound)
            {
                int randomIndex = UnityEngine.Random.Range(0, potentialDirections.Count);
                Vector3 direction = potentialDirections[randomIndex] * realWorldChunkSize.x;
                Vector3 proposedPos = currentPos + direction;

                // Clamping the position within the world border
                if (!positions.Contains(proposedPos) &&
                    proposedPos.x >= -halfSize_worldPlayArea.x && proposedPos.x <= halfSize_worldPlayArea.x &&
                    proposedPos.z >= -halfSize_worldPlayArea.y && proposedPos.z <= halfSize_worldPlayArea.y)
                {
                    validPositionFound = true;
                    currentPos = proposedPos;
                    positions.Add(currentPos);
                }
                else
                {
                    // Remove the direction and try another one
                    potentialDirections.RemoveAt(randomIndex);
                }
            }

            // If no valid position is found, end the path
            if (!validPositionFound)
            {
                break;
            }
        }

        return positions;
    }


    /// <summary>
    /// Generates a random direction on the XZ plane.
    /// </summary>
    /// <returns>A Vector3 representing a random direction on the XZ plane.</returns>
    Vector3 RandomDirectionOnXZ()
    {
        int choice = UnityEngine.Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector3.forward;
            case 1: return Vector3.back;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            default: return Vector3.forward;
        }
    }
}

