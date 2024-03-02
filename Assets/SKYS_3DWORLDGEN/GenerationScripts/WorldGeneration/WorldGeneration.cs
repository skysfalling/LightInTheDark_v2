using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Handles the procedural generation of the game world in   Unity.
/// Responsible for initializing and populating the world with terrain, landscapes,
/// resources, and points of interest. Interacts with various world components to
/// ensure a cohesive and dynamically generated game environment.
/// </summary>
/// 
public enum DebugColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }
public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
public enum WorldSpace { World, Region, Chunk, Cell }

[RequireComponent(typeof(WorldInteractor))]
[RequireComponent(typeof(WorldStatTracker))]
[RequireComponent(typeof(WorldMaterialLibrary))]
public class WorldGeneration : MonoBehaviour
{
    #region [[ STATIC VARIABLES ]] ======================================================================
    // STATIC GENERATION VALUES ========================================================= ///
    public static string GameSeed = "Default Game Seed";
    public static int CurrentSeed { get { return GameSeed.GetHashCode(); }}
    public static void InitializeRandomSeed(string newGameSeed = "")
    {

        if (newGameSeed != "" && newGameSeed != GameSeed)
        {
            GameSeed = newGameSeed;
            Debug.Log($"Initialize Random Seed to => {GameSeed} :: {CurrentSeed}");
        }

        UnityEngine.Random.InitState(CurrentSeed);
    }

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
    public static int PlayRegionBoundaryOffset = 0; // Boundary offset value 
    public static int RegionMaxGroundHeight = 10; // Maximum chunk height
    public static int GetPlayRegionWidth_inCells() { return PlayRegionWidth_inChunks * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inChunks() { return PlayRegionWidth_inChunks + (PlayRegionBoundaryOffset * 2); } // Include BoundaryOffset on both sides
    public static int GetFullRegionWidth_inCells() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inWorldSpace() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldGeneration { in WorldRegion Units }
    public static int WorldWidth_inRegions = 3;
    public static int GetWorldWidth_inCells() { return WorldWidth_inRegions * GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetWorldWidth_inWorldSpace() { return GetWorldWidth_inCells() * CellWidth_inWorldSpace; }
    #endregion ========================================================

    string _prefix = "[ WORLD GENERATION ] ";
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _worldGenerationRoutine;
    CoordinateMap _regionCoordinateMap;

    public bool initialized { get; private set; }
    public string gameSeed = GameSeed; // inspector value ( updated by custom editor )
    public Vector2Int worldCoordinate { get; private set; }
    public Vector3 centerPosition_inWorldSpace { get; private set; }
    public Vector3 originPosition_inWorldSpace { get; private set; }
    public List<WorldRegion> worldRegions = new List<WorldRegion>();

    public void Initialize()
    {
        // >>
        this.worldCoordinate = Vector2Int.zero;

        float worldWidthRadius = GetWorldWidth_inWorldSpace() * 0.5f;
        float regionWidthRadius = GetFullRegionWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        this.centerPosition_inWorldSpace = transform.position;

        // >> Origin Coordinate Position { Bottom Left }
        this.originPosition_inWorldSpace = new Vector3(this.worldCoordinate.x, 0, this.worldCoordinate.y) * WorldGeneration.GetWorldWidth_inWorldSpace();
        originPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        originPosition_inWorldSpace += regionWidthRadius * new Vector3(1, 0, 1);

        // << CREATE REGIONS >>
        this._regionCoordinateMap = new CoordinateMap(this);
        worldRegions = new();
        InitializeRandomSeed();

        for (int i = 0; i < _regionCoordinateMap.allCoordinates.Count; i++)
        {
            Coordinate regionCoordinate = _regionCoordinateMap.allCoordinates[i];

            // Create a new object for each region
            GameObject regionObject = new GameObject($"New Region ({regionCoordinate.localPosition})");
            WorldRegion region = regionObject.AddComponent<WorldRegion>();
            region.Initialize(this, regionCoordinate);

            regionObject.transform.parent = this.transform;

            worldRegions.Add(region);
        }

        initialized = true;
    }

    public void Reset()
    {
        for (int i = 0; i < worldRegions.Count; i++)
        {
            worldRegions[i].Destroy();
        }
        worldRegions.Clear();
        this._regionCoordinateMap = null;

        initialized = false;
    }

    public Material GetChunkMaterial()
    {
        return GetComponent<WorldMaterialLibrary>().chunkMaterial;
    }

    #region == INITIALIZE ======
    public void StartGeneration()
    {
        foreach(WorldRegion region in worldRegions)
        {
            region.CreateChunkMeshObjects();
        }
    }

    #endregion

    /*
    #region == GENERATE CHUNKS ================================================ ///
    IEnumerator GenerateRegionChunks(float delay = 0.25f)
    {
        generation_finished = false;
        if (_worldGenerationObject != null) { Reset(); }

        //CoordinateMap worldCoordMap = FindObjectOfType<CoordinateMap>();

        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"WORLD GENERATION :: START GENERATION");

        // [[ STAGE 0 ]] ==> INITIALIZE MAPS
        
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
        

        // [[ STAGE 3 ]] ==> INITIALIZE CHUNKS
        foreach (WorldChunk chunk in WorldChunkMap._chunkList)
        {
            yield return new WaitUntil(() => chunk.generation_finished);

            CreateMeshObject($"Chunk {chunk.coordinate} :: height {chunk.groundHeight}",
                chunk.chunkMesh.mesh, WorldMaterialLibrary.Instance.chunkMaterial);
        }
        //float stage3Time = Time.realtimeSinceStartup - stage2Time;
        //Debug.Log($"WORLD GENERATION :: CHUNK MESH CREATED :: Stage Duration {stage2Time}" +
            //$"\n -> {WorldChunkMap.ChunkList.Count} CHUNKS");

        // [[ GENERATE COMBINED MESHES ]] ========================================== >>
        
        // Create Combined Mesh of world chunks
        Mesh combinedMesh = CombineChunks(WorldChunkMap.ChunkList);
        _worldGenerationObject = CreateCombinedMeshObject(combinedMesh, WorldMaterialLibrary.Instance.chunkMaterial);
        _worldGenerationObject.transform.parent = transform;
        _worldGenerationObject.name = "(WORLD GENERATION) Combined Ground Mesh";
        MeshCollider collider = _worldGenerationObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;

    // Create Combined Mesh
    Mesh combinedBorderMesh = CombineChunks(_borderChunks);
    _worldBorderObject = CreateCombinedMeshObject(combinedBorderMesh, WorldMaterialLibrary.chunkMaterial);
    _worldBorderObject.transform.parent = transform;
    _worldBorderObject.name = "(WORLD GENERATION) Combined Ground Border";  


    generation_finished = true;
        _worldGenerationRoutine = null;
    }
    #endregion
    */

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

    public static void DestroyGameObject(GameObject gameObject)
    {
        // Check if we are running in the Unity Editor
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            // Use DestroyImmediate if in edit mode and not playing
            DestroyImmediate(gameObject);
            return;
        }
        else
#endif
        {
            // Use Destroy in play mode or in a build
            Destroy(gameObject);
        }
    }
    #endregion

}