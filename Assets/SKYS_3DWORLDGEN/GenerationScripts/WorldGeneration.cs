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

    // CREATE SINGLETON ==================================== ///
    public static WorldGeneration Instance;
    public void Awake()
    {
        if (Instance != null) { Destroy(Instance); }
        Instance = this;
    }
    // ========================================================= ///

    // STATIC GENERATION DIMENSIONS ==================================== ///
    public static int CellSize = 4; // Size of each WorldCell // Size of each WorldCell { in Unity Units }
    public static Vector2Int ChunkArea = new Vector2Int(5, 5); // Area of each WorldChunk { in WorldCell Units }
    public static int ChunkDepth = 3;
    public static Vector2Int PlayZoneArea = new Vector2Int(10, 10); // Default size of PlayArea { in WorldChunk Units }
    public static int BoundaryOffset = 1; // Boundary offset value 

    Vector2Int _fullWorldArea; // PlayZoneArea + (BoundaryOffset * 2 * Vector2Int.one)
    public static Vector2Int GetFullWorldArea() { return PlayZoneArea + (BoundaryOffset * 2 * Vector2Int.one); } // Include BoundaryOffset on both sides
    public static Vector3Int GetChunkDimensions() { return new Vector3Int(ChunkArea.x, ChunkDepth, ChunkArea.y); }


    // CONVERT TO REAL UNITS
    Vector2Int _realChunkAreaSize;// realWorldChunkSize * cellSize
    Vector2Int _realPlayAreaSize; // PlayZoneArea * realChunkSize
    Vector2Int _realFullWorldSize; // _fullWorldArea * cellSize
    public static Vector2Int GetRealChunkAreaSize() { return ChunkArea * CellSize; }
    public static Vector3Int GetRealChunkDimensions() { return new Vector3Int(ChunkArea.x, ChunkDepth, ChunkArea.y) * CellSize; }
    public static Vector2Int GetRealPlayAreaSize() { return PlayZoneArea * GetRealChunkAreaSize(); }
    public static Vector2Int GetRealFullWorldSize() { return GetFullWorldArea() * GetRealChunkAreaSize(); }

    public void InitializeWorldDimensions()
    {
        _fullWorldArea = GetFullWorldArea();
        _realChunkAreaSize = GetRealChunkAreaSize();
        _realPlayAreaSize = GetRealPlayAreaSize();
        _realFullWorldSize = GetRealFullWorldSize();
    }

    string _prefix = "[ WORLD GENERATION ] ";
    [HideInInspector] public bool generation_finished = false;
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _worldGenerationRoutine;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    List<WorldChunk> _borderChunks = new List<WorldChunk>();

    // == {{ WORLD EXITS }} ============================////
    [Header("World Exits")]
    public List<WorldExit> worldExits = new List<WorldExit>();
    public void SetWorldExitCoordinates()
    {
        Debug.Log("SetWorldExitCoordinates " + worldExits.Count);
        foreach (WorldExit exit in worldExits) { 
            exit.Coordinate = WorldChunkMap.GetWorldExitCoordinate(exit);

            if (exit.Coordinate != null)
                exit.Coordinate.ChunkType = WorldChunk.TYPE.EXIT;
        }
    }


    #region == INITIALIZE ======
    private void Start()
    {
        StartGeneration();
    }

    public void StartGeneration()
    {
        Reset();
        InitializeWorldDimensions();

        if (_worldGenerationRoutine != null) { StopCoroutine(_worldGenerationRoutine); }
        _worldGenerationRoutine = StartCoroutine(Generate());

    }

    public void Reset()
    {
        Destroy(_worldGenerationObject);
        Destroy(_worldBorderObject);

        FindObjectOfType<WorldChunkMap>().Reset();
        FindObjectOfType<WorldCellMap>().Reset();
        FindObjectOfType<WorldEnvironment>().Reset();
        FindObjectOfType<WorldStatTracker>().UpdateStats();

        _worldChunks.Clear();
        _borderChunks.Clear();
    }
    #endregion

    #region == GENERATE CHUNKS ================================================ ///
    IEnumerator Generate(float delay = 0.25f)
    {
        generation_finished = false;
        if (_worldGenerationObject != null) { Reset(); }

        // Get All Chunk Positions
        List<Vector2> allWorldChunkPositions = WorldChunkMap.GetCoordinateMapPositions();


        /*
        // [[ GENERATE PLAY AREA CHUNKS ]] ========================================== >>
        foreach (Vector2 position in allPlayAreaChunkPositions)
        {
            WorldChunk newChunk = new WorldChunk(position);
            _worldChunks.Add(newChunk);
        }

        // [[ GENERATE BORDER CHUNKS ]] ========================================== >>
        foreach (Vector2 position in allBorderChunkPositions)
        {
            WorldChunk newChunk = new WorldChunk(position);
            _borderChunks.Add(newChunk);
        }

        // [[ GENERATE COMBINED MESHES ]] ========================================== >>
        // Create Combined Mesh of world chunks
        Mesh combinedMesh = CombineChunks(_worldChunks);
        _worldGenerationObject = CreateCombinedMeshObject(combinedMesh, WorldMaterialLibrary.chunkMaterial);
        _worldGenerationObject.transform.parent = transform;
        _worldGenerationObject.name = "(WORLD GENERATION) Combined Ground Mesh";
        MeshCollider collider = _worldGenerationObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;

        // Create Combined Mesh
        Mesh combinedBorderMesh = CombineChunks(_borderChunks);
        _worldBorderObject = CreateCombinedMeshObject(combinedBorderMesh, WorldMaterialLibrary.chunkMaterial);
        _worldBorderObject.transform.parent = transform;
        _worldBorderObject.name = "(WORLD GENERATION) Combined Ground Border";
        */

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
        /*
        WorldSpawnMap worldSpawnMap = FindObjectOfType<WorldSpawnMap>();
        FindObjectOfType<WorldSpawnMap>().InitializeSpawnMap();
        yield return new WaitUntil(() => worldCellMap.initialized);
        Debug.Log(_prefix + "COMPLETE : Initialized World Spawn Map");
        */

        // [[ ENVIRONMENT GENERATION ]] ===================================
        WorldEnvironment worldEnvironment = FindObjectOfType<WorldEnvironment>();
        worldEnvironment.StartEnvironmentGeneration();
        yield return new WaitUntil(() => worldEnvironment.generation_finished);
        Debug.Log(_prefix + "COMPLETE : Finished Environment Generation");
        #endregion

        generation_finished = true;

    }

    public List<WorldChunk> GetChunks()
    {
        if (_worldChunks.Count == 0 || _worldChunks == null)
        {
            return new List<WorldChunk>();
        }

        return _worldChunks;
    }

    public WorldChunk GetChunkAt(Vector2 position)
    {
        foreach (WorldChunk chunk in _worldChunks)
        {
            if (chunk.coordinate == position) { return chunk; }
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

        foreach (WorldChunk chunk in _worldChunks)
        {
            foreach (WorldCell cell in chunk.localCells)
            {
                cells.Add(cell);
            }
        }

        return cells;
    }
    #endregion

    #region == CREATE COMBINED CHUNK MESH ==============================================
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

}