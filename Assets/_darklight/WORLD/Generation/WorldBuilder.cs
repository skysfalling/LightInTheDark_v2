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
    public class WorldBuilder : AsyncTaskQueen, ITaskQueen
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
        public void OverrideSettings(CustomGenerationSettings customSettings)
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

        // [[ PRIVATE VARIABLES ]] 
        string _prefix = "[ WORLD BUILDER ] ";
        Coroutine _generationCoroutine;
        CoordinateMap _coordinateMap;
        Dictionary<Vector2Int, RegionBuilder> _regionMap = new();

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
        public List<RegionBuilder> AllRegions { get { return _regionMap.Values.ToList(); } }
        public Dictionary<Vector2Int, RegionBuilder> RegionMap { get { return _regionMap; } }

        // [[ PUBLIC INSPECTOR VARIABLES ]] 
        public CustomGenerationSettings customWorldGenSettings; // Settings Scriptable Object
        public bool initializeOnStart;

        #region == INITIALIZATION ============================================== >>>> 
        private void Start()
        {
            if (initializeOnStart == true)
            {
                Debug.Log($"{_prefix} Initialize On Start");
                Initialize();
            }
        }

        public override void Initialize(string name = "WorldBuilderAsyncTaskQueen")
        {
            base.Initialize(name);
            _ = InitializeAndGenerateAsync();
        }

        /// <summary>
        /// Represents an asynchronous operation that can return a value.
        /// </summary>
        async Task InitializeAndGenerateAsync()
        {
            OverrideSettings(customWorldGenSettings);
            InitializeSeedRandom();

            this._coordinateMap = new CoordinateMap(this);
            await InitializationSequenceAsync();

            // This will yield control back to the caller, allowing it to continue
            // executing while the heavy computation is running
            await Task.Yield();

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
                    RegionBuilder region = regionObject.AddComponent<RegionBuilder>();
                    regionObject.transform.parent = this.transform;
                    region.AssignToWorld(this, regionCoordinate);
                    _regionMap[regionCoordinate.ValueKey] = region;
                    await Task.Yield(); // Efficiently yields back to the main thread
                }
            });

            // Stage 1: Initialize Regions
            base.NewTaskBot("InitializeRegions", async () =>
            {
                foreach (RegionBuilder region in AllRegions)
                {
                    region.Initialize();
                    await Task.Run(() => region.Initialized);
                    await Task.Yield();
                }
            });

            // Stage 2: Generate Exits
            base.NewTaskBot("GenerateExits", async () =>
            {
                Debug.Log("GenerateExits task started");
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
                    region.CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.TYPE> { Zone.TYPE.FULL });
                    await Task.Yield();
                }
            });

            // Run all bots
            await base.ExecuteAllBotsInQueue();

            // Mark initialization as complete
            Initialized = true;
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

            Initialized = false;
        }
    }
}