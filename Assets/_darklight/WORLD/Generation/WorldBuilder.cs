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
            OverrideSettings(customWorldGenSettings);
            InitializeSeedRandom();


            if (WorldBuilder.Settings != null)
            {
                Debug.LogError($"World Settings {WorldBuilder.Settings.ChunkVec3Dimensions_inCellUnits}");

            }


            _ = InitializationSequenceAsync();
        }


        /// <summary>
        /// Orchestrates the entire initialization sequence asynchronously, divided into stages.
        /// </summary>
        async Task InitializationSequenceAsync()
        {

            this._coordinateMap = new CoordinateMap(this);
            while (this._coordinateMap.Initialized == false)
            {
                await Task.Delay(1000);
            }

            // Stage 0: Create Regions
            base.NewTaskBot("CreateRegions", async () =>
            {
                foreach (Coordinate regionCoordinate in CoordinateMap.AllCoordinates)
                {
                    while (regionCoordinate.Initialized == false)
                    {
                        await Task.Delay(1000);
                    }

                    GameObject regionObject = new GameObject($"New Region ({regionCoordinate.ValueKey})");
                    RegionBuilder region = regionObject.AddComponent<RegionBuilder>();
                    region.transform.parent = this.transform;
                    region.AssignToWorld(this, regionCoordinate);
                    _regionMap[regionCoordinate.ValueKey] = region;
                }
                await Task.Yield();
            });

            // Stage 1: Initialize Regions
            base.NewTaskBot("InitializeRegions", async () =>
            {
                foreach (RegionBuilder region in AllRegions)
                {
                    region.Initialize();
                    while (region.Initialized == false)
                    {
                        await Task.Delay(1000);
                    }
                }
            });

            // Stage 2: Generate Exits
            base.NewTaskBot("GenerateExits", async () =>
            {
                Debug.Log("GenerateExits task started");
                foreach (RegionBuilder region in AllRegions)
                {
                    region.GenerateNecessaryExits(true);
                    await Task.Delay(1000);
                }
            });

            // Stage 3: Generate Paths Between Exits
            base.NewTaskBot("GeneratePathsBetweenExits", async () =>
            {
                foreach (RegionBuilder region in AllRegions)
                {
                    region.CoordinateMap.GeneratePathsBetweenExits();
                }
                await Task.Yield();

            });

            // Stage 4: Zone Generation and Height Assignments
            base.NewTaskBot("ZoneGeneration", async () =>
            {
                foreach (RegionBuilder region in AllRegions)
                {
                    region.CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.TYPE> { Zone.TYPE.FULL });
                }
                await Task.Yield();

            });

            await ExecuteAllBotsInQueue(); ///

            // Mark initialization as complete
            Initialized = true;

        }

        #endregion

        public async Task CreateRegionAsync(Coordinate regionCoordinate)
        {

            await Task.Yield();
        }

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

        private void OnDrawGizmos() {
            foreach (Coordinate coord in CoordinateMap.AllCoordinates)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(coord.ScenePosition, Vector3.one * 0.5f);
            }
        }
    }
}