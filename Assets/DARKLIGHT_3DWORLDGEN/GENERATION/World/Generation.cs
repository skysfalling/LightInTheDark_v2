using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.ThirdDimensional.World
{
    public enum UnitSpace{ WORLD, REGION, CHUNK, CELL, GAME }
    public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
    public enum BorderDirection { NORTH, SOUTH, EAST, WEST }


    [System.Serializable]
    public class GenerationSettings
    {
        // [[ NECESSARY STORED DATA ]] 
        private string _seed = "Default Game Seed";
        private int _cellSize = 2; // in Units
        private int _chunkWidth = 10; // in Cells
        private int _chunkDepth = 10; // in Cells
        private int _chunkMaxHeight = 25; // in Cells
        private int _regionWidth = 7; // in Chunks
        private int _regionBoundaryOffset = 0; // in Chunks
        private int _worldWidth = 5; // in Regions

        // >> Public Accessors
        public string Seed => _seed;

        public string name = "hello";

        // >>>> WORLD CELL
        public int CellSize_inGameUnits => _cellSize;

        // >>>> WORLD CHUNK
        public int ChunkWidth_inCellUnits => _chunkWidth;
        public int ChunkDepth_inCellUnits => _chunkDepth;
        public int ChunkMaxHeight_inCellUnits => _chunkMaxHeight;
        public Vector3Int ChunkVec3Dimensions_inCellUnits => new Vector3Int(_chunkWidth, _chunkDepth, _chunkWidth);
        public int ChunkWidth_inGameUnits => _chunkWidth * _cellSize;
        public int ChunkDepth_inGameUnits => _chunkDepth * _cellSize;
        public int ChunkMaxHeight_inGameUnits => _chunkMaxHeight * _cellSize;
        public Vector3Int ChunkVec3Dimensions_inGameUnits => ChunkVec3Dimensions_inCellUnits * _cellSize;

        // >>>> WORLD REGION
        public int RegionWidth_inChunkUnits => _regionWidth;
        public int RegionBoundaryOffset_inChunkUnits => _regionBoundaryOffset;
        public int RegionFullWidth_inChunkUnits => RegionWidth_inChunkUnits + (_regionBoundaryOffset * 2);
        public int RegionWidth_inCellUnits => _regionWidth * _chunkWidth;
        public int RegionFullWidth_inCellUnits => RegionFullWidth_inChunkUnits * _chunkWidth;
        public int RegionFullWidth_inGameUnits => RegionFullWidth_inCellUnits * _cellSize;

        // >>>> WORLD GENERATION
        public int WorldWidth_inRegionUnits => _worldWidth;
        public int WorldWidth_inChunkUnits => _worldWidth * RegionFullWidth_inChunkUnits;
        public int WorldWidth_inCellUnits => _worldWidth * RegionFullWidth_inCellUnits;
        public int WorldWidth_inGameUnits => WorldWidth_inCellUnits * _cellSize;

        public GenerationSettings() { }
    }

    public class Generation : MonoBehaviour
    {
        // [[ PRIVATE VARIABLES ]] 
        string _prefix = "[ WORLD GENERATION ] ";
        Coroutine _generationSequence;
        static GenerationSettings _settings = new();
        CoordinateMap _coordinateMap;
        List<Region> _regions = new();
        Dictionary<Vector2Int, Region> _regionMap = new();

        // [[ PUBLIC STATIC VARIABLES ]]
        public static GenerationSettings Settings { get { return _settings; } }
        public static string Seed { get { return Settings.Seed; } }
        public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
        public static void InitializeSeedRandom()
        {
            UnityEngine.Random.InitState(EncodedSeed);
        }

        // [[ PUBLIC ACCESS VARIABLES ]]
        public bool Initialized { get; private set; }
        public CoordinateMap CoordinateMap { get { return _coordinateMap; } }
        public Vector3 CenterPosition { get { return transform.position; } }
        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = CenterPosition; // Start at center
                origin -= Settings.WorldWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }
        public List<Region> AllRegions { get { return _regions; } }
        public Dictionary<Vector2Int, Region> RegionMap { get { return _regionMap; } }


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
            this._coordinateMap = new CoordinateMap(this);
            this._regions = new();

            // >> create a region at each coordinate
            Debug.Log($"{_prefix} Begin Initialization => Creating {CoordinateMap.AllCoordinates.Count} Regions");
            for (int i = 0; i < CoordinateMap.AllCoordinates.Count; i++)
            {
                Coordinate regionCoordinate = CoordinateMap.AllCoordinates[i];

                // Create a new object for each region
                GameObject regionObject = new GameObject($"New Region ({regionCoordinate.Value})");
                Region region = regionObject.AddComponent<Region>();
                region.SetReferences(this, regionCoordinate); // << Set References to parent & Coordinate
                regionObject.transform.parent = this.transform;

                _regions.Add(region);
                _regionMap[regionCoordinate.Value] = region;
            }

            // >> initialize each region
            foreach (Region region in _regions)
            {
                region.Initialize();
                yield return new WaitUntil(() => region.Initialized);
            }

            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"Stage 0: Region Initialization {Time.time - startTime} seconds.");

            // Grouped operations: Initial exits generation
            foreach (var region in _regions)
            {
                region.GenerateNecessaryExits(true);
            }
            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"Stage 1: Exits Generation (First Pass) completed in {Time.time - startTime} seconds.");

            startTime = Time.time; // Reset start time for the next stage
                                   // Grouped operations: Second pass for exits and path generation
            foreach (var region in _regions)
            {
                region.GenerateNecessaryExits(false); // Second pass without creating new
                region.CoordinateMap.GeneratePathsBetweenExits(); // Assuming independent of exits generation
            }
            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"Stage 2: Exits Generation (Second Pass) and Path Generation completed in {Time.time - startTime} seconds.");

            startTime = Time.time; // Reset start time for the next stage
                                   // Combined zones and height assignments in a single step to minimize delays
            foreach (var region in _regions)
            {
                region.CoordinateMap.GenerateRandomZones(1, 3); // Zone generation

                region.ChunkMap.UpdateMap(); // update chunk map to match coordinate type values


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

            foreach (Region region in AllRegions)
            {
                region.ChunkMap.GenerateChunkMeshes();
            }

            foreach (Region region in AllRegions)
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

        public void Reset()
        {
            for (int i = 0; i < _regions.Count; i++)
            {
                if (_regions[i] != null)
                    _regions[i].Destroy();
            }
            _regions.Clear();
            _regionMap.Clear();
            this._coordinateMap = null;

            Initialized = false;
        }
    }
}