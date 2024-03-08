using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; // Include this for Stopwatch
using Debug = UnityEngine.Debug;
using Darklight.Unity.Backend;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Generation
{
    #region [[ PUBLIC ENUMS ]] ------------------- // 

    /// <summary>
    /// Represents the spatial scope for operations or elements within the world generation context.
    /// </summary>
    public enum UnitSpace { WORLD, REGION, CHUNK, CELL, GAME }

    /// <summary>
    /// Defines cardinal and intercardinal directions for world layout and neighbor identification.
    /// </summary>
    public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }

    /// <summary> Specifies the directions for borders relative to a given region or chunk. </summary>
    public enum BorderDirection { NORTH, SOUTH, EAST, WEST }
    #endregion

    /// <summary> Initializes and handles the procedural world generation. </summary>
    [RequireComponent(typeof(WorldEdit))]
    public class WorldBuilder : AsyncTaskQueen
    {
        #region [[ STATIC INSTANCE ]] ------------------- // 
        /// <summary> A singleton instance of the WorldGeneration class. </summary>
        public static WorldBuilder Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        #endregion

        #region [[ GENERATION SETTINGS ]] -------------------------------------- >> 
        static GenerationSettings _settings = new();

        /// <summary> Contains settings used during the world generation process. </summary>
        public static GenerationSettings Settings => _settings;

        /// <summary> Override the default generation settings. </summary>
        public void OverrideSettings(CustomWorldGenerationSettings customSettings)
        {
            if (customSettings == null) { _settings = new GenerationSettings(); return; }
            _settings = new GenerationSettings(customSettings);
        }
        #endregion

        #region [[ RANDOM SEED ]] -------------------------------------- >> 
        public static string Seed { get { return Settings.Seed; } }
        public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
        public static void InitializeSeedRandom()
        {
            UnityEngine.Random.InitState(EncodedSeed);
        }
        #endregion

        #region [[ STAGE PROFILERS ]] -------------------------------------- >>
        /// <summary> Represents a stage in the initialization process, including its identifier, name, and execution time. </summary>
        public struct InitializationStage
        {
            public int id;
            public string name;
            public long time;
            public InitializationStage(int id, string name, long time)
            {
                this.id = id;
                this.name = name;
                this.time = time;
            }
        }

        public List<InitializationStage> InitStages { get; private set; } = new();

        #endregion

        // [[ PRIVATE VARIABLES ]] 
        string _prefix = "[ WORLD GENERATION ] ";
        Coroutine _generationCoroutine;
        CoordinateMap _coordinateMap;
        Dictionary<Vector2Int, Region> _regionMap = new();

        // [[ PUBLIC REFERENCE VARIABLES ]]
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
        public List<Region> AllRegions { get { return _regionMap.Values.ToList(); } }
        public Dictionary<Vector2Int, Region> RegionMap { get { return _regionMap; } }

        // [[ PUBLIC INSPECTOR VARIABLES ]] 
        public CustomWorldGenerationSettings customWorldGenSettings; // Settings Scriptable Object

        #region == INITIALIZATION ============================================== >>>> 

        /// <summary>
        /// Asynchronously initiates the world generation process, including seed initialization and region creation.
        /// </summary>
        public async Task InitializeAsync()
        {
            OverrideSettings(customWorldGenSettings);

            InitializeSeedRandom();

            this._coordinateMap = new CoordinateMap(this);

            await InitializationSequenceAsync();
        }

        /// <summary>
        /// Orchestrates the entire initialization sequence asynchronously, divided into stages.
        /// </summary>
        async Task InitializationSequenceAsync()
        {
            // Stage 0: Create Regions
            base.NewTaskBot("CreateRegions", async () =>
            {
                foreach (Coordinate regionCoordinate in CoordinateMap.AllCoordinates)
                {
                    GameObject regionObject = new GameObject($"New Region ({regionCoordinate.ValueKey})");
                    Region region = regionObject.AddComponent<Region>();
                    regionObject.transform.parent = this.transform;
                    region.SetReferences(this, regionCoordinate);
                    _regionMap[regionCoordinate.ValueKey] = region;
                    await Task.Yield(); // Efficiently yields back to the main thread
                }
            });

            // Stage 1: Initialize Regions
            base.NewTaskBot("InitializeRegions", async () =>
            {
                foreach (Region region in AllRegions)
                {
                    region.Initialize();
                    await Task.Yield();
                }
            });

            // Stage 2: Generate Exits
            base.NewTaskBot("GenerateExits", async () =>
            {
                foreach (var region in AllRegions)
                {
                    region.GenerateNecessaryExits(true);
                    await Task.Yield();
                }
            });

            // Stage 3: Generate Paths Between Exits
            base.NewTaskBot("GeneratePathsBetweenExits", async () =>
            {
                foreach (var region in AllRegions)
                {
                    region.CoordinateMap.GeneratePathsBetweenExits();
                    await Task.Yield();
                }
            });

            // Stage 4: Zone Generation and Height Assignments
            base.NewTaskBot("ZoneGeneration", async () =>
            {
                foreach (var region in AllRegions)
                {
                    while (region.Initialized == false)
                    {
                        await Task.Yield();
                    }

                    region.CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.TYPE> { Zone.TYPE.FULL });
                    region.ChunkMap.UpdateMap(); // Update chunk map to match coordinate type values
                }
            });

            // Run all bots
            await base.ExecuteAllBotsInQueue();

            // Mark initialization as complete
            Initialized = true;
        }

        #endregion

        #region  == MESH GENERATION ============================================== >>>>

        /// <summary>
        /// Initiates the mesh generation sequence
        /// </summary>
        public void StartGeneration()
        {
            if (_generationCoroutine == null)
            {
                _generationCoroutine = StartCoroutine(GenerationSequence());
            }
            else
            {
                StopCoroutine(_generationCoroutine);
                _generationCoroutine = StartCoroutine(GenerationSequence());
            }
        }

        IEnumerator GenerationSequence()
        {
            yield return new WaitUntil(() => Initialized); // wait until self initialization

            Debug.Log($"{_prefix} Starting Generation Sequence");

            if (Settings.ChunkMeshUnitSpace == UnitSpace.CHUNK)
            {
                foreach (Region region in AllRegions)
                {
                    region.ChunkMap.GenerateChunkMeshes(true);
                }
            }

            else if (Settings.ChunkMeshUnitSpace == UnitSpace.REGION)
            {
                foreach (Region region in AllRegions)
                {
                    region.ChunkMap.GenerateChunkMeshes(false);
                }

                foreach (Region region in AllRegions)
                {
                    region.CreateCombinedChunkMesh();
                }
            }

            _generationCoroutine = null;
        }

        /// <summary> Create GameObject from ChunkMesh </summary>
        public static GameObject CreateChunkMeshObject(ChunkMesh chunkMesh)
        {
            GameObject worldObject = new GameObject($"Chunk at {chunkMesh.ParentChunk.Coordinate.ValueKey}");

            MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = chunkMesh.Mesh;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();

            worldObject.AddComponent<MeshRenderer>().material = Settings.materialLibrary.DefaultGroundMaterial;
            worldObject.AddComponent<MeshCollider>().sharedMesh = chunkMesh.Mesh;

            return worldObject;
        }

        /// <summary> Create GameObject with a given name, mesh and material </summary>
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

        /// <summary> Destroy GameObject in Play andEdit mode </summary>
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

        /// <summary> Fully Reset the World Generation </summary>
        public void ResetGeneration()
        {
            for (int i = 0; i < AllRegions.Count; i++)
            {
                if (AllRegions[i] != null)
                    AllRegions[i].Destroy();
            }
            _regionMap.Clear();
            this._coordinateMap = null; // Clear coordinate map

            InitStages = new(); // reset stage tracking

            Initialized = false;
        }
    }
}