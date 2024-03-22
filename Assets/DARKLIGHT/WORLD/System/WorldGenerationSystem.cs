using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World
{
    using Debug = UnityEngine.Debug;
    using Bot;
    using Settings;
    using Map;
    using System.Threading.Tasks;
    using Darklight.World.Generation;
    using Darklight.World.Builder;

    #region (( GLOBAL SPATIAL ENUMS ))
    /// <summary>
    /// Represents the spatial scope for operations or elements within the world generation context.
    /// </summary>
    public enum UnitSpace { WORLD, REGION, CHUNK, CELL, GAME }
    /// <summary>
    /// Defines cardinal and intercardinal directions for world layout and neighbor identification.
    /// </summary>
    public enum Direction { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
    /// <summary> Specifies the directions for borders relative to a given region or chunk. </summary>
    public enum EdgeDirection { WEST, NORTH, EAST, SOUTH }
    #endregion

    public class WorldGenerationSystem : TaskBotQueen, ITaskEntity
    {
        #region [[ STATIC INSTANCE ]] ---- >> 
        /// <summary> A singleton instance of the WorldGenerationSystem class. </summary>
        public static WorldGenerationSystem Instance;
        public static string Prefix => "< WORLD GENERATION SYSTEM >";

        /// <summary> Destroy GameObject in Play and Edit mode </summary>
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

        #region [[ GENERATION SETTINGS ]] ---- >> 
        /// <summary> Contains settings used during the world generation process. </summary>
        [SerializeField] private GenerationSettings _settings = new GenerationSettings();
        public GenerationSettings Settings => _settings;
        #endregion

        #region ---- (( INSTANTIATE OBJECTS ))
        public HashSet<GameObject> InstantiatedObjects { get; private set; } = new HashSet<GameObject>();

        public Task<GameObject> CreateGameObjectAt(string name, GridMap2D.Coordinate coordinate)
        {
            GameObject newObject = new GameObject($"{name} :: {coordinate.PositionKey}");
            newObject.transform.parent = this.transform;
            newObject.transform.position = coordinate.GetPositionInScene();

            InstantiatedObjects.Add(newObject);
            return Task.FromResult(newObject);
        }
        #endregion

        #region ---- (( DATA HANDLING ))
        public GridMap2D<Generation.Region> RegionGridMap { get; private set; } = null;
        #endregion

        #region --------------- UNITY MAIN ----))
        public Material defaultMaterial;

        public override void Awake()
        {
            base.Awake();
            _ = Initialize();
        }
        #endregion

        #region --------------- TASK BOT QUEEN --))
        public override async Task Initialize()
        {
            // << Singelton Instance >> 
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            // << INIT >> -------------------------- //
            this.Name = "WorldGenerationSystem";
            _settings.Initialize();
            await base.Initialize();
            RegionGridMap = new GridMap2D<Generation.Region>(transform, UnitSpace.REGION); // Assign GridMap to this Transform
            RegionGridMap.Initialize(_settings);
            Debug.Log($"{Prefix} Initialized");
            await RegionGridMap.InitializeDataMap(); // Initialize Data



            // [[ ADD BOT CLONES TO EXECUTION QUEUE ]]
            // Enqueue a TaskBot clone for each position
            await EnqueueClones("CreateRegionOperators", RegionGridMap.PositionKeys, position =>
            {
                Generation.Region region = RegionGridMap.DataMap[position];
                return new TaskBot(this, $"CreateRegionOperator {position}", async () =>
                {
                    GameObject newObject = await CreateGameObjectAt("RegionOperator", region.CoordinateValue);
                    RegionMonoOperator regionMonoOperator = newObject.AddComponent<RegionMonoOperator>();
                    await regionMonoOperator.Initialize(region);
                });
                // i love my little bots <3 

            });
        }
        #endregion

        public override void Reset()
        {
            base.Reset();
            TaskBotConsole.Reset();
            RegionGridMap.Reset();

            foreach (GameObject obj in InstantiatedObjects)
            {
                DestroyGameObject(obj);
            }
        }
    }

    #region==== CUSTOM UNITY EDITOR ================== )) 
#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGenerationSystem))]
    public class WorldGenerationSystemEditor : TaskBotQueenEditor
    {
        SerializedObject _serializedObject;
        WorldGenerationSystem _worldGenSystem;


        public override void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _worldGenSystem = (WorldGenerationSystem)target;
        }

        private void OnSceneGUI()
        {
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            GridMap2DEditor.DrawGridMap2D_SceneGUI(worldGenSystem.RegionGridMap, GridMap2DEditor.View.COORD_FLAG, (coordinate) =>
            {
                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
        }
    }
}

#endif
#endregion
