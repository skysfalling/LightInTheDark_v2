using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
public enum WorldSpace { World, Region, Chunk, Cell }

public class WorldGeneration : MonoBehaviour
{
    #region [[ STATIC VARIABLES ]] ======================================================================
    // STATIC GENERATION VALUES ========================================================= ///
    public static WorldGenerationSettings Settings = new WorldGenerationSettings();
    public static string Seed { get { return Settings.Seed; } }
    public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); }}
    public static void InitializeSeedRandom()
    {
        UnityEngine.Random.InitState(EncodedSeed);
    }

    // STATIC GENERATION DIMENSIONS ==================================== ///
    // >>>> WorldCell { in Unity Worldspace Units }
    public static int CellWidth_inWorldSpace { get{ return Settings.CellWidthInWorldSpace; } } // Size of each WorldCell 

    // >>>> WorldChunk { in WorldCell Units }
    public static int ChunkWidth_inCells { get { return Settings.ChunkWidthInCells; } }
    public static int ChunkDepth_inCells { get { return Settings.ChunkDepthInCells; } }
    public static Vector3Int ChunkVec3Dimensions_inCells() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells); }
    public static Vector3Int GetChunkVec3Dimensions_inWorldSpace() { return new Vector3Int(ChunkWidth_inCells, ChunkDepth_inCells, ChunkWidth_inCells) * CellWidth_inWorldSpace; }
    public static int GetChunkWidth_inWorldSpace() { return ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldRegion { in WorldChunk Units }
    public static int PlayRegionWidth_inChunks { get { return Settings.PlayRegionWidthInChunks; } } // in World Chunks
    public static int BoundaryWallCount { get { return Settings.BoundaryWallCount; } } // Boundary offset value 
    public static int MaxChunkHeight { get { return Settings.MaxChunkHeight; } } // Maximum chunk height
    public static int GetPlayRegionWidth_inCells() { return PlayRegionWidth_inChunks * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inChunks() { return PlayRegionWidth_inChunks + (BoundaryWallCount * 2); } // Include BoundaryOffset on both sides
    public static int GetFullRegionWidth_inCells() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetFullRegionWidth_inWorldSpace() { return GetFullRegionWidth_inChunks() * ChunkWidth_inCells * CellWidth_inWorldSpace; }


    // >>>> WorldGeneration { in WorldRegion Units }
    public static int WorldWidth_inRegions { get { return Settings.WorldWidthInRegions; } } // in World Regions
    public static int GetWorldWidth_inCells() { return WorldWidth_inRegions * GetFullRegionWidth_inChunks() * ChunkWidth_inCells; }
    public static int GetWorldWidth_inWorldSpace() { return GetWorldWidth_inCells() * CellWidth_inWorldSpace; }
    #endregion ========================================================

    // [[ PRIVATE VARIABLES ]] 
    string _prefix = "[ WORLD GENERATION ] ";
    Coroutine _generationSequence;

    // [[ PUBLIC ACCESS VARIABLES ]]
    public bool Initialized { get; private set; }
    public CoordinateMap CoordinateMap { get; private set; }
    public Vector3 CenterPosition { get { return transform.position; } }
    public Vector3 OriginPosition { get { return GetOriginPosition(); } }
    Vector3 GetOriginPosition()
    {
        float worldWidthRadius = GetWorldWidth_inWorldSpace() * 0.5f;
        float regionWidthRadius = GetFullRegionWidth_inWorldSpace() * 0.5f;

        Vector3 origin = CenterPosition;
        origin -= worldWidthRadius * new Vector3(1, 0, 1);
        origin += regionWidthRadius * new Vector3(1, 0, 1);
        return origin;
    }

    public List<WorldRegion> AllRegions { get; private set; } = new();
    public Dictionary<Vector2Int, WorldRegion> RegionMap { get; private set; } = new();

    public void Reset()
    {
        for (int i = 0; i < AllRegions.Count; i++)
        {
            if (AllRegions[i] != null)
                AllRegions[i].Destroy();
        }
        AllRegions.Clear();
        this.CoordinateMap = null;

        Initialized = false;
    }

    #region == INITIALIZE ====================================== >>>>
    public void Initialize()
    {
        InitializeSeedRandom();

        StartCoroutine(InitializationSequence());
    }

    public IEnumerator InitializationSequence()
    {
        float stage_delay = 0.1f;
        float startTime = Time.time; // Capture the start time of the initialization

        // << CREATE REGIONS >>
        this.CoordinateMap = new CoordinateMap(this);
        AllRegions = new();

        // >> create a region at each coordinate
        Debug.Log($"{_prefix} Begin Initialization => Creating {CoordinateMap.AllCoordinates.Count} Regions");
        for (int i = 0; i < CoordinateMap.AllCoordinates.Count; i++)
        {
            Coordinate regionCoordinate = CoordinateMap.AllCoordinates[i];

            // Create a new object for each region
            GameObject regionObject = new GameObject($"New Region ({regionCoordinate.Value})");
            WorldRegion region = regionObject.AddComponent<WorldRegion>();
            region.Initialize(this, regionCoordinate);
            regionObject.transform.parent = this.transform;

            AllRegions.Add(region);
            RegionMap[regionCoordinate.Value] = region;

            yield return new WaitUntil(() => region.Initialized);

        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 0: Region Initialization {Time.time - startTime} seconds.");

        // Grouped operations: Initial exits generation
        foreach (var region in AllRegions)
        {
            region.GenerateNecessaryExits(true);
        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 1: Exits Generation (First Pass) completed in {Time.time - startTime} seconds.");

        startTime = Time.time; // Reset start time for the next stage
                               // Grouped operations: Second pass for exits and path generation
        foreach (var region in AllRegions)
        {
            region.GenerateNecessaryExits(false); // Second pass without creating new
            region.coordinateMap.GeneratePathsBetweenExits(); // Assuming independent of exits generation
        }
        yield return new WaitForSeconds(stage_delay);
        Debug.Log($"Stage 2: Exits Generation (Second Pass) and Path Generation completed in {Time.time - startTime} seconds.");

        startTime = Time.time; // Reset start time for the next stage
                               // Combined zones and height assignments in a single step to minimize delays
        foreach (var region in AllRegions)
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

        foreach (WorldRegion region in AllRegions)
        {
            region.worldChunkMap.GenerateChunkMeshes();
        }

        foreach (WorldRegion region in AllRegions)
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