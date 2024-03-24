using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Generation.System
{
    using Debug = UnityEngine.Debug;
    using Bot;
    using Settings;
    using Map;
    using Unit;
    using Darklight.UnityExt;

    public class WorldGenerationSystem : TaskBotQueen, ITaskEntity, IUnityEditorListener
    {
        #region [[ STATIC INSTANCE ]] ---- >> 
        /// <summary> A singleton instance of the WorldGenerationSystem class. </summary>
        public static WorldGenerationSystem Instance;
        public static string Prefix => "< WORLD GENERATION SYSTEM >";
        #endregion

        #region [[ GENERATION SETTINGS ]] ---- >> 
        /// <summary> Contains settings used during the world generation process. </summary>
        [SerializeField] private GenerationSettings _settings = new GenerationSettings();
        public GenerationSettings Settings => _settings;
        #endregion

        #region ---- (( INSTANTIATE OBJECTS ))
        public static HashSet<GameObject> InstantiatedObjects { get; private set; } = new HashSet<GameObject>();
        public static GameObject CreateGameObjectOnGrid(string name, GridMap2D.Coordinate coordinate, Transform parent = null)
        {
            GameObject newObject = new GameObject($"{name} :: {coordinate.PositionKey}");
            newObject.transform.position = coordinate.GetPositionInScene();
            newObject.transform.parent = parent;

            InstantiatedObjects.Add(newObject);
            return newObject;
        }

        /// <summary>
        /// Creates a GameObject with the given name, mesh, and material.
        /// </summary>
        /// <param name="name">The name of the GameObject.</param>
        /// <param name="mesh">The mesh for the GameObject.</param>
        /// <param name="material">The material for the GameObject.</param>
        /// <returns>The created GameObject.</returns>
        public GameObject CreateMeshObject(string name, Mesh mesh, Material material = null)
        {
            if (material == null && defaultMaterial != null)
            {
                Debug.LogWarning($"{name} Material is null -> setMaterial to GenerationParent default");
                defaultMaterial = material;
            }
            else if (material == null) { Debug.LogWarning($"{name} Material is null"); }
            else if (mesh == null) { Debug.LogWarning($"{name} Mesh is null"); }

            GameObject worldObject = new GameObject(name);
            worldObject.transform.parent = transform;
            worldObject.transform.localPosition = Vector3.zero;

            MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();

            if (material == null)
            {
                worldObject.AddComponent<MeshRenderer>().material = defaultMaterial;
                worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
            else
            {
                worldObject.AddComponent<MeshRenderer>().material = material;
                worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            InstantiatedObjects.Add(worldObject);

            return worldObject;
        }

        public static void DestroyAllGeneration()
        {
            Debug.Log($"{Prefix} DestroyAllGeneration() -> Count {InstantiatedObjects.Count}");

            // Destroy all instantiated objects
            foreach (GameObject gameObject in InstantiatedObjects)
            {
                DestroyWithEditorContext(gameObject);
            }
        }

        /// <summary> Destroy GameObject in Play and Edit mode </summary>
        public static void DestroyWithEditorContext(GameObject gameObject)
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

        #region ---- (( DATA HANDLING ))
        public GridMap2D<Region> RegionGridMap { get; private set; } = null;
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
                TaskBotConsole.Log($"Set Singleton");
            }

            // << INIT >> -------------------------- //
            this.Name = "WorldGenerationSystem";
            _settings.Initialize();
            await base.Initialize();

            // Assign GridMap to this Transform
            RegionGridMap = new GridMap2D<Region>(transform, transform.position, _settings, UnitSpace.WORLD, UnitSpace.REGION);
            await RegionGridMap.Initialize();

            TaskBotConsole.Log($"Initialized Region Grid Map");
            TaskBotConsole.Log($"Region Count: {RegionGridMap.PositionKeys.Count}", 1);
            if (RegionGridMap.PositionKeys.Count > 200)
            {
                TaskBotConsole.Log($"Region Count is too high. Consider reducing the region width.", 0, Darklight.Console.LogEntry.Severity.Error);
                return;
            }

            // [[ ADD BOT CLONES TO EXECUTION QUEUE ]]
            // Enqueue a TaskBot clone for each position
            await EnqueueClones("CreateRegionOperators", RegionGridMap.PositionKeys, position =>
            {
                Region region = RegionGridMap.DataMap[position];
                return new TaskBot(this, $"CreateRegionOperator {position}", async () =>
                {
                    GameObject newObject = CreateGameObjectOnGrid("RegionOperator", region.CoordinateValue, this.transform);
                    RegionMonoOperator regionMonoOperator = newObject.AddComponent<RegionMonoOperator>();
                    await regionMonoOperator.Initialize(region, _settings);
                });
                // i love my little bots <3 
            });
        }
        #endregion

        public override void Reset()
        {
            base.Reset();
            TaskBotConsole.Reset(); // reset console
            RegionGridMap.Reset(); // reset data

            DestroyAllGeneration();
        }

        public void OnEditorReloaded()
        {
            Reset();
        }
    }

    #region==== CUSTOM UNITY EDITOR ================== )) 
#if UNITY_EDITOR

    [CustomEditor(typeof(WorldGenerationSystem))]
    public class WorldGenerationSystemEditor : TaskBotQueenEditor
    {
        SerializedObject _serializedObject;
        WorldGenerationSystem _worldGenSystem;
        private void OnSceneGUI()
        {
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            /*
            GridMap2DEditor.DrawGridMap2D_SceneGUI(worldGenSystem.RegionGridMap, GridMap2DEditor.View.COORD_FLAG, (coordinate) =>
            {
                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
            */
        }

        [DrawGizmo(GizmoType.Selected)]
        public static void GridMap2DGizmos(WorldGenerationSystem worldGen, GizmoType gizmoType)
        {
            if (worldGen == null || gizmoType == GizmoType.NonSelected) return;

            GridMap2DEditor.View view = GridMap2DEditor.View.COORD_FLAG;
            GridMap2DEditor.DrawGridMap2D_SceneGUI(worldGen.RegionGridMap, view, (GridMap2D.Coordinate coordinate) => { });
        }
    }
}

#endif
#endregion