using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(WorldGenerationStats))]
public class WorldGeneration : MonoBehaviour
{
    string _prefix = "[ WORLD GENERATION ] ";
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _worldGenerationRoutine;
    List<WorldChunk> _chunks = new List<WorldChunk>();
    List<WorldChunk> _borderChunks = new List<WorldChunk>();

    public bool generation_finished = false;
    public Material chunkMaterial; // Assign a material in the inspector
    public Material borderChunkMaterial; // Assign a material in the inspector

    public Vector2 maxChunkCount = new Vector2(10, 10);
    Vector2 _fullWorldSize;

    public int steps = 10; // Number of steps in the random walk

    [Header("Cells")]
    [Range(0, 16)]
    public int cellSize = 4; // Size of each subdivision cell

    [Header("Chunks")]
    [Range(0, 21)]
    public int chunk_width_in_cells = 5; // Length of width in cells
    [Range(0, 21)]
    public int chunk_height_in_cells = 3; // Length of height in cells
    [HideInInspector] public Vector3Int chunkDimensions; // default Chunk Dimensions [ in cellSize units ]
    [HideInInspector] public Vector3Int fullsize_chunkDimensions; // default Chunk Dimensions [ in cellSize units ]

    private void Start()
    {
        StartGeneration();
    }

    public void StartGeneration()
    {
        Reset();

        if (_worldGenerationRoutine != null) { StopCoroutine(_worldGenerationRoutine); }
        _worldGenerationRoutine = StartCoroutine(Generate());

    }

    public void Reset()
    {
        Destroy(_worldGenerationObject);
        Destroy(_worldBorderObject);

        FindObjectOfType<WorldChunkMap>().Reset();
        FindObjectOfType<WorldCellMap>().Reset();
        FindObjectOfType<WorldSpawnMap>().Reset();
        FindObjectOfType<WorldEnvironment>().Reset();
        FindObjectOfType<WorldGenerationStats>().UpdateStats();

        _chunks.Clear();
        _borderChunks.Clear();
    }

    IEnumerator Generate(float delay = 0.25f)
    {
        generation_finished = false;
        Debug.Log(_prefix + "Start Generation");

        if (_worldGenerationObject != null)
        {
            Destroy(_worldGenerationObject);
            _chunks.Clear();
        }

        // << Set Chunk Dimensions >>
        chunkDimensions = new Vector3Int(chunk_width_in_cells, chunk_height_in_cells, chunk_width_in_cells);
        fullsize_chunkDimensions = chunkDimensions * cellSize;
        _fullWorldSize = maxChunkCount * new Vector2(fullsize_chunkDimensions.x, fullsize_chunkDimensions.z);
        Debug.Log(_prefix + "_fullWorldSize " + _fullWorldSize);

        // Get All Possible Chunk Positions
        List<Vector3> allPossibleChunkPositions = new List<Vector3>();

        // Calculate all possible chunk positions
        Vector2 halfBorderPos = _fullWorldSize * 0.5f;
        for (float x = -halfBorderPos.x; x <= halfBorderPos.x; x += fullsize_chunkDimensions.x)
        {
            for (float z = -halfBorderPos.y; z <= halfBorderPos.y; z += fullsize_chunkDimensions.z)
            {
                allPossibleChunkPositions.Add(new Vector3(x, 0, z));
            }
        }

        // Copy Positions to Border Positions
        List<Vector3> borderPositions = new List<Vector3>(allPossibleChunkPositions);

        // [[ GENERATE RANDOM PATH ]] ========================================== >>
        List<Vector3> positions = GenerateRandomWalkPositions(steps);

        // Create Chunks for each position
        foreach (Vector3 position in positions)
        {
            WorldChunk newChunk = CreateChunkMesh(position);
            _chunks.Add(newChunk);

            // Remove chunk from border positions
            if (borderPositions.Contains(position)) { borderPositions.Remove(position); }


            for (int i =  0; i < 4; i++)
            {
                Debug.Log($"Border Chunk position {borderPositions[i]}\n" +
                    $"Chunk position {positions[i]}");
            }

        }

        // Create Combined Mesh
        Mesh combinedMesh = CombineChunks(_chunks);
        _worldGenerationObject = CreateCombinedMeshObject(combinedMesh, chunkMaterial);
        _worldGenerationObject.transform.parent = transform;
        _worldGenerationObject.name = "(GEN) Ground Layout";

        Debug.Log($"{_prefix} chunk Count : {positions.Count()} / {allPossibleChunkPositions.Count()}");
        Debug.Log($"{_prefix} borderChunk Count : {borderPositions.Count()} / {allPossibleChunkPositions.Count()}");


        // Add collider
        MeshCollider collider = _worldGenerationObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;

        Debug.Log(_prefix + "COMPLETE : CombinedMesh Layout");

        // [[ GENERATE BORDER CHUNKS ]] ========================================== >>
        foreach (Vector3 pos in borderPositions)
        {
            WorldChunk newChunk = CreateChunkMesh(pos);
            _borderChunks.Add(newChunk);
        }

        // Create Combined Mesh
        Mesh combinedBorderMesh = CombineChunks(_borderChunks);
        _worldBorderObject = CreateCombinedMeshObject(combinedBorderMesh, chunkMaterial);
        _worldBorderObject.transform.parent = transform;
        _worldBorderObject.name = "(GEN) Ground Border";

        // [[ INITIALIZE MAPS ]] ============================================= >>
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


        generation_finished = true;

    }

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

    // =========================================== CREATE CHUNK MESH ==============================================
    
    /// <summary>
    /// Creates a chunk mesh with specified dimensions, vertices, triangles, and UVs. 
    /// Constructs faces of the chunk and computes triangles for each face.
    /// </summary>
    /// <returns>A Mesh object representing a chunk of terrain or object.</returns>
    WorldChunk CreateChunkMesh(Vector3 position)
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

        Vector2 halfBorder = _fullWorldSize * 0.5f; // Half of the world border size

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
                Vector3 direction = potentialDirections[randomIndex] * fullsize_chunkDimensions.x;
                Vector3 proposedPos = currentPos + direction;

                // Clamping the position within the world border
                if (!positions.Contains(proposedPos) &&
                    proposedPos.x >= -halfBorder.x && proposedPos.x <= halfBorder.x &&
                    proposedPos.z >= -halfBorder.y && proposedPos.z <= halfBorder.y)
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

    void OnDrawGizmos()
    {
        // Draw World Border
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_fullWorldSize.x, 1, _fullWorldSize.y));
    }
}

