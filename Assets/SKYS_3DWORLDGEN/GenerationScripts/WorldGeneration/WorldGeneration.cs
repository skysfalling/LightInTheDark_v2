using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Unity.VisualScripting;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Handles the procedural generation of the game world in Unity.
/// Responsible for initializing and populating the world with terrain, landscapes,
/// resources, and points of interest. Interacts with various world components to
/// ensure a cohesive and dynamically generated game environment.
/// </summary>
/// 
public enum DebugColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }
public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
public enum WorldSpace { World, Region, Chunk, Cell }

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

        Thread mainThread = Thread.CurrentThread;
        mainThread.Name = "Main Thread";
        Debug.Log($"{mainThread.Name} say hi! ");
    }

    // STATIC GENERATION VALUES ========================================================= ///
    public static string GameSeed = "Default Game Seed";
    public static int CurrentSeed { get { return GameSeed.GetHashCode(); }}

    // STATIC GENERATION DIMENSIONS ==================================== ///

    // >>>> WorldCell { in Unity Worldspace Units }
    public static int CellWidth_inWorldSpace = 4; // Size of each WorldCell 

    // >>>> WorldChunk { in WorldCell Units }
    public static int ChunkWidth_inCells = 10; 
    public static int ChunkDepth_inCells = 5;
    public static Vector3Int GetChunkVec3Dimensions_inCells() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells); }
    public static Vector3Int GetChunkVec3Dimensions_inWorldSpace() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells) * CellWidth_inWorldSpace; }
    public static int GetChunkWidth_inWorldSpace() { return ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldRegion { in WorldChunk Units }
    public static int PlayRegionWidth_inChunks = 5; // in World Chunks
    public static int PlayRegionBoundaryOffset = 1; // Boundary offset value 
    public static int RegionMaxGroundHeight = 10; // Maximum chunk height
    public static int GetPlayRegionWidth_inCells() { return PlayRegionWidth_inChunks * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inChunks() { return PlayRegionWidth_inChunks + (PlayRegionBoundaryOffset * 2); } // Include BoundaryOffset on both sides
    public static int GetFullRegionWidth_inCells() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inWorldSpace() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldGeneration { in WorldRegion Units }
    public static int WorldWidth_inRegions = 3;
    public static int GetWorldWidth_inCells() { return WorldWidth_inRegions * GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetWorldWidth_inWorldSpace() { return GetWorldWidth_inCells() * CellWidth_inWorldSpace; }


    string _prefix = "[ WORLD GENERATION ] ";
    [HideInInspector] public bool generation_finished = false;
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _worldGenerationRoutine;

    public string gameSeed = GameSeed;
    public static void InitializeRandomSeed(string newGameSeed = "") { 

        if (newGameSeed != "" && newGameSeed != GameSeed)
        {
            GameSeed = newGameSeed;
            Debug.Log($"Initialize Random Seed to => {GameSeed} :: {CurrentSeed}");
        }

        UnityEngine.Random.InitState(CurrentSeed);
    }

    public List<WorldRegion> worldRegions = new List<WorldRegion>();
    public void CreateRegions()
    {
        worldRegions = new();

        for (int x = 0; x < WorldWidth_inRegions; x++)
        {
            for (int y = 0; y < WorldWidth_inRegions; y++)
            {
                GameObject regionObject = new GameObject($"New Region ({x} , {y})");
                WorldRegion region = regionObject.AddComponent<WorldRegion>();
                Vector2Int regionCoordinate = new Vector2Int(x, y);
                region.Initialize(regionCoordinate);

                regionObject.transform.parent = this.transform;


                worldRegions.Add(region);
            }
        }
    }

    #region == INITIALIZE ======
    public void StartGeneration()
    {
        // Reset Generation
        if (generation_finished)
        {
            Reset();
            if (_worldGenerationRoutine != null) { StopCoroutine(_worldGenerationRoutine); }
        }

        // Start Routine
        _worldGenerationRoutine = StartCoroutine(GenerateRegionChunks());
    }

    public void Reset()
    {
        Destroy(_worldGenerationObject);
        Destroy(_worldBorderObject);
    }
    #endregion

    #region == GENERATE CHUNKS ================================================ ///
    IEnumerator GenerateRegionChunks(float delay = 0.25f)
    {
        generation_finished = false;
        if (_worldGenerationObject != null) { Reset(); }

        //CoordinateMap worldCoordMap = FindObjectOfType<CoordinateMap>();

        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"WORLD GENERATION :: START GENERATION");

        // [[ STAGE 0 ]] ==> INITIALIZE MAPS
        /*
        if (!CoordinateMap.coordMapInitialized || !WorldChunkMap.chunkMapInitialized)
        {
            //FindObjectOfType<WorldRegionMap>().UpdateRegionMap();
        }
        yield return new WaitUntil(() => CoordinateMap.coordMapInitialized);
        yield return new WaitUntil(() => WorldChunkMap.chunkMapInitialized);

        float stage0Time = Time.realtimeSinceStartup - startTime;
        Debug.Log($"WORLD GENERATION :: MAPS INITIALIZED :: Stage Duration {stage0Time}");

        // [[ STAGE 1 ]] ==> INITIALIZE ZONES
        yield return new WaitUntil(() => CoordinateMap.zonesInitialized);
        float stage1Time = Time.realtimeSinceStartup - stage0Time;
        Debug.Log($"WORLD GENERATION :: ZONES INITIALIZED :: Stage Duration {stage1Time}");

        // [[ STAGE 2 ]] ==> INITIALIZE PATHS
        yield return new WaitUntil(() => CoordinateMap.exitPathsInitialized);
        float stage2Time = Time.realtimeSinceStartup - stage1Time;
        Debug.Log($"WORLD GENERATION :: PATHS INITIALIZED :: Stage Duration {stage2Time}");
        */

        // [[ STAGE 3 ]] ==> INITIALIZE CHUNKS
        foreach (WorldChunk chunk in WorldChunkMap.ChunkList)
        {
            yield return new WaitUntil(() => chunk.generation_finished);

            CreateMeshObject($"Chunk {chunk.coordinate} :: height {chunk.groundHeight}",
                chunk.chunkMesh.mesh, WorldMaterialLibrary.Instance.chunkMaterial);
        }
        //float stage3Time = Time.realtimeSinceStartup - stage2Time;
        //Debug.Log($"WORLD GENERATION :: CHUNK MESH CREATED :: Stage Duration {stage2Time}" +
            //$"\n -> {WorldChunkMap.ChunkList.Count} CHUNKS");

        // [[ GENERATE COMBINED MESHES ]] ========================================== >>
        /*
        // Create Combined Mesh of world chunks
        Mesh combinedMesh = CombineChunks(WorldChunkMap.ChunkList);
        _worldGenerationObject = CreateCombinedMeshObject(combinedMesh, WorldMaterialLibrary.Instance.chunkMaterial);
        _worldGenerationObject.transform.parent = transform;
        _worldGenerationObject.name = "(WORLD GENERATION) Combined Ground Mesh";
        MeshCollider collider = _worldGenerationObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;
        */
        
        /*
        // Create Combined Mesh
        Mesh combinedBorderMesh = CombineChunks(_borderChunks);
        _worldBorderObject = CreateCombinedMeshObject(combinedBorderMesh, WorldMaterialLibrary.chunkMaterial);
        _worldBorderObject.transform.parent = transform;
        _worldBorderObject.name = "(WORLD GENERATION) Combined Ground Border";  
        */

        #region [[ INITIALIZE MAPS ]] ============================================= >>
        /*
        // Initialize Spawn Map
        /*
        WorldSpawnMap worldSpawnMap = FindObjectOfType<WorldSpawnMap>();
        FindObjectOfType<WorldSpawnMap>().InitializeSpawnMap();
        yield return new WaitUntil(() => worldCellMap.initialized);
        Debug.Log(_prefix + "COMPLETE : Initialized World Spawn Map");
        */

        /*
        // [[ ENVIRONMENT GENERATION ]] ===================================
        WorldEnvironment worldEnvironment = FindObjectOfType<WorldEnvironment>();
        worldEnvironment.StartEnvironmentGeneration();
        yield return new WaitUntil(() => worldEnvironment.generation_finished);
        Debug.Log(_prefix + "COMPLETE : Finished Environment Generation");
        */
        #endregion

        generation_finished = true;
        _worldGenerationRoutine = null;
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
            meshes.Add(chunk.chunkMesh.mesh);
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

    public static GameObject CreateMeshObject(string name, Mesh mesh, Material material)
    {
        GameObject worldObject = new GameObject(name);

        MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();

        worldObject.AddComponent<MeshRenderer>().material = material;
        worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;

        return worldObject;
    }
    #endregion

}