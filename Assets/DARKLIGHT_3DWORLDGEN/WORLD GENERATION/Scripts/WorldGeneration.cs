using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    static string _seed = "Default Game Seed";
    static int _cellWidthInWorldSpace = 2;
    static int _chunkWidthInCells = 10;
    static int _chunkDepthInCells = 10;
    static int _playRegionWidthInChunks = 7;
    static int _boundaryWallCount = 0;
    static int _maxChunkHeight = 25;
    static int _worldWidthInRegions = 5;

    public WorldGenerationSettings worldSettings;

    public static void LoadWorldGenerationSettings(WorldGenerationSettings worldSettings)
    {
        if (worldSettings == null)
        {
            Debug.LogError("WorldGenerationSettings ScriptableObject is not assigned.");
            return;
        }

        // Load settings from the ScriptableObject
        _seed = worldSettings.Seed;
        _cellWidthInWorldSpace = worldSettings.CellWidthInWorldSpace;
        _chunkWidthInCells = worldSettings.ChunkWidthInCells;
        _chunkDepthInCells = worldSettings.ChunkDepthInCells;
        _playRegionWidthInChunks = worldSettings.PlayRegionWidthInChunks;
        _boundaryWallCount = worldSettings.BoundaryWallCount;
        _maxChunkHeight = worldSettings.MaxChunkHeight;
        _worldWidthInRegions = worldSettings.WorldWidthInRegions;

        Debug.Log($"LoadWorldGenerationSettings with seed {_seed}");
    }

    #region [[ STATIC VARIABLES ]] ======================================================================
    // STATIC GENERATION VALUES ========================================================= ///
    public static string Seed { get { return _seed; } }
    public static int EncodedSeed { get { return _seed.GetHashCode(); }}
    public static void InitializeRandomSeed(string newSeed = "")
    {

        if (newSeed != "" && newSeed != _seed)
        {
            _seed = newSeed;
            Debug.Log($"Initialize Random Seed to => {_seed} :: {EncodedSeed}");
        }

        UnityEngine.Random.InitState(EncodedSeed);
    }

    // STATIC GENERATION DIMENSIONS ==================================== ///
    // >>>> WorldCell { in Unity Worldspace Units }
    public static int CellWidth_inWorldSpace { get{ return _cellWidthInWorldSpace; } } // Size of each WorldCell 

    // >>>> WorldChunk { in WorldCell Units }
    public static int ChunkWidth_inCells { get { return _chunkWidthInCells; } }
    public static int ChunkDepth_inCells { get { return _chunkDepthInCells; } }
    public static Vector3Int ChunkVec3Dimensions_inCells() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells); }
    public static Vector3Int GetChunkVec3Dimensions_inWorldSpace() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells) * CellWidth_inWorldSpace; }
    public static int GetChunkWidth_inWorldSpace() { return ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldRegion { in WorldChunk Units }
    public static int PlayRegionWidth_inChunks { get { return _playRegionWidthInChunks; } } // in World Chunks
    public static int BoundaryWallCount { get { return _boundaryWallCount; } } // Boundary offset value 
    public static int MaxChunkHeight { get { return _maxChunkHeight; } } // Maximum chunk height
    public static int GetPlayRegionWidth_inCells() { return PlayRegionWidth_inChunks * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inChunks() { return PlayRegionWidth_inChunks + (BoundaryWallCount * 2); } // Include BoundaryOffset on both sides
    public static int GetFullRegionWidth_inCells() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inWorldSpace() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldGeneration { in WorldRegion Units }
    public static int WorldWidth_inRegions { get { return _worldWidthInRegions; } } // in World Regions
    public static int GetWorldWidth_inCells() { return WorldWidth_inRegions * GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetWorldWidth_inWorldSpace() { return GetWorldWidth_inCells() * CellWidth_inWorldSpace; }
    #endregion ========================================================

    string _prefix = "[ WORLD GENERATION ] ";
    GameObject _worldGenerationObject;
    GameObject _worldBorderObject;
    Coroutine _generationSequence;

    public bool Initialized { get; private set; }
    public CoordinateMap coordinateRegionMap { get; private set; }
    public Vector2Int worldPosition { get; private set; }
    public Vector3 centerPosition_inWorldSpace { get; private set; }
    public Vector3 originPosition_inWorldSpace { get; private set; }
    public List<WorldRegion> worldRegions = new List<WorldRegion>();
    public Dictionary<Vector2Int, WorldRegion> regionMap { get; private set; } = new();

    public void Awake()
    {
        LoadWorldGenerationSettings(worldSettings);
    }

    public void Reset()
    {
        for (int i = 0; i < worldRegions.Count; i++)
        {
            worldRegions[i].Destroy();
        }
        worldRegions.Clear();
        this.coordinateRegionMap = null;

        Initialized = false;
    }

    #region == INITIALIZE ====================================== >>>>
    public void Initialize()
    {
        // >> center of the world location in the unity scene
        this.worldPosition = Vector2Int.zero;

        float worldWidthRadius = GetWorldWidth_inWorldSpace() * 0.5f;
        float regionWidthRadius = GetFullRegionWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        this.centerPosition_inWorldSpace = transform.position;

        // >> Origin Coordinate Position { Bottom Left }
        this.originPosition_inWorldSpace = new Vector3(this.worldPosition.x, 0, this.worldPosition.y) * WorldGeneration.GetWorldWidth_inWorldSpace();
        originPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        originPosition_inWorldSpace += regionWidthRadius * new Vector3(1, 0, 1);

        // << CREATE REGIONS >>
        this.coordinateRegionMap = new CoordinateMap(this);
        worldRegions = new();
        InitializeRandomSeed();

        StartCoroutine(InitializationSequence());
    }

    public IEnumerator InitializationSequence()
    {

        float stage_delay = 0.1f;
        float startTime = Time.time; // Capture the start time of the initialization

        // >> create a region at each coordinate
        Debug.Log($"{_prefix} Begin Initialization => Creating {coordinateRegionMap.allCoordinates.Count} Regions");
        for (int i = 0; i < coordinateRegionMap.allCoordinates.Count; i++)
        {
            Coordinate regionCoordinate = coordinateRegionMap.allCoordinates[i];

            // Create a new object for each region
            GameObject regionObject = new GameObject($"New Region ({regionCoordinate.Value})");
            WorldRegion region = regionObject.AddComponent<WorldRegion>();
            region.Initialize(this, regionCoordinate);
            regionObject.transform.parent = this.transform;

            worldRegions.Add(region);
            regionMap[regionCoordinate.Value] = region;

            yield return new WaitUntil(() => region.Initialized);

        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 0: Region Initialization {Time.time - startTime} seconds.");

        // Grouped operations: Initial exits generation
        foreach (var region in worldRegions)
        {
            region.GenerateNecessaryExits(true);
        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 1: Exits Generation (First Pass) completed in {Time.time - startTime} seconds.");

        startTime = Time.time; // Reset start time for the next stage
                               // Grouped operations: Second pass for exits and path generation
        foreach (var region in worldRegions)
        {
            region.GenerateNecessaryExits(false); // Second pass without creating new
            region.coordinateMap.GeneratePathsBetweenExits(); // Assuming independent of exits generation
        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 2: Exits Generation (Second Pass) and Path Generation completed in {Time.time - startTime} seconds.");

        startTime = Time.time; // Reset start time for the next stage
                               // Combined zones and height assignments in a single step to minimize delays
        foreach (var region in worldRegions)
        {
            region.coordinateMap.GenerateRandomZones(1, 3); // Zone generation

            region.worldChunkMap.UpdateMap(); // update chunk map to match coordinate type values


            // Assign heights to paths and zones together
            /*
            foreach (WorldPath path in region.coordinateMap.worldPaths)
            {
                region.worldChunkMap.SetChunksToHeightFromPath(path);
            }

            foreach (WorldZone zone in region.coordinateMap.worldZones)
            {
                region.worldChunkMap.SetChunksToHeightFromPositions(zone.positions, zone.zoneHeight);
            }
            */
        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 3: Zone Generation and Height Assignments completed in {Time.time - startTime} seconds.");

        Initialized = true;
        Debug.Log($"Total Initialization Time: {Time.time - startTime} seconds.");
    }
    #endregion ============================================================ ////

    public void StartGeneration()
    {
        if (_generationSequence == null)
        {
            _generationSequence = StartCoroutine(GenerationSequence());
        }
    }

    public IEnumerator GenerationSequence()
    {
        yield return new WaitUntil(() => Initialized); // wait until self initialization

        foreach (WorldRegion region in worldRegions)
        {
            region.worldChunkMap.GenerateChunkMeshes();
        }


        foreach (WorldRegion region in worldRegions)
        {
            region.CreateCombinedChunkMesh();
        }

        _generationSequence = null;
    }

    #region == WORLD GENERATION ============================================== >>>>
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

    public Material GetChunkMaterial()
    {
        return GetComponent<WorldMaterialLibrary>().chunkMaterial;
    }

}