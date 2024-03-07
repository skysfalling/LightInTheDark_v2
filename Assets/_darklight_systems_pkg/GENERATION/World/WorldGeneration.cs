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

namespace Darklight.ThirdDimensional.Generation
{
    public enum UnitSpace{ WORLD, REGION, CHUNK, CELL, GAME }
    public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
    public enum BorderDirection { NORTH, SOUTH, EAST, WEST }

    public class WorldGeneration : MonoBehaviour
    {

        // Static Reference for 
        public static WorldGeneration Instance;
        private void Awake() {
            if (Instance == null)
            {
                Instance = this;
            } else {
                Destroy(this);
            }
        }


        public static GenerationSettings Settings { get { return _settings; } }
        public void OverrideSettings(CustomWorldGenerationSettings customSettings)
        {
            if (customSettings == null) { _settings = new GenerationSettings(); return; }
            _settings = new GenerationSettings(customSettings);
        }
        public static string Seed { get { return Settings.Seed; } }
        public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
        public static void InitializeSeedRandom()
        {
            UnityEngine.Random.InitState(EncodedSeed);
        }

        // [[ PRIVATE VARIABLES ]] 
        string _prefix = "[ WORLD GENERATION ] ";
        Coroutine _generationSequence;
        static GenerationSettings _settings = new();
        CoordinateMap _coordinateMap;
        List<Region> _regions = new();
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
        public List<Region> AllRegions { get { return _regions; } }
        public Dictionary<Vector2Int, Region> RegionMap { get { return _regionMap; } }

        // [[ PUBLIC INSPECTOR VARIABLES ]] 
        public CustomWorldGenerationSettings customWorldGenSettings; // Settings Scriptable Object


        // ================================== INITIALIZATION ============================================== >>>>

        public void Initialize()
        {
            OverrideSettings(customWorldGenSettings);

            StartCoroutine(InitializationSequence());
        }

        public IEnumerator InitializationSequence()
        {
            float stage_delay = 0.1f;
            float startTime = Time.time; // Capture the start time of the initialization
            InitializeSeedRandom();

            // << CREATE REGIONS >>
            this._coordinateMap = new CoordinateMap(this);
            this._regions = new();

            // >> get all coordinates from the map
            HashSet<Coordinate> allCoordinates = CoordinateMap.AllCoordinates.ToHashSet();

            // >> create a region at each coordinate
            Debug.Log($"{_prefix} Begin Initialization => Creating {CoordinateMap.AllCoordinates.Count()} Regions");
            for (int i = 0; i < CoordinateMap.AllCoordinates.ToList().Count; i++)
            {


                Coordinate regionCoordinate = CoordinateMap.AllCoordinates.ToList()[i];

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
            Debug.Log($"{_prefix} Init Stage 0: Region Initialization {Time.time - startTime} seconds.");

            // Grouped operations: Initial exits generation
            foreach (var region in _regions)
            {
                region.GenerateNecessaryExits(true);
            }
            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"{_prefix} Init Stage 1: Exits Generation completed in {Time.time - startTime} seconds.");


            // [[ STAGE 2]]
            startTime = Time.time; // Reset start time for the next stage
            foreach (var region in _regions)
            {
                region.CoordinateMap.GeneratePathsBetweenExits();
            }
            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"{_prefix} Init Stage 2: Path Generation completed in {Time.time - startTime} seconds.");

            startTime = Time.time; // Reset start time for the next stage
                                   // Combined zones and height assignments in a single step to minimize delays
            foreach (var region in _regions)
            {
                region.CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.TYPE>() { Zone.TYPE.FULL }); // Zone generation

                region.ChunkMap.UpdateMap(); // update chunk map to match coordinate type values
            }
            yield return new WaitForSeconds(stage_delay);
            Debug.Log($"{_prefix} Init Stage 3: Zone Generation and Height Assignments completed in {Time.time - startTime} seconds.");

            Initialized = true;
            Debug.Log($"Total Initialization Time: {Time.time - startTime} seconds.");
        }

        public void StartGeneration()
        {
            if (_generationSequence == null)
            {
                _generationSequence = StartCoroutine(GenerationSequence());
            }
            else
            {
                StopCoroutine(_generationSequence);
                _generationSequence = StartCoroutine(GenerationSequence());
            }
        }

        public IEnumerator GenerationSequence()
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

            _generationSequence = null;
        }

        #region == WORLD GENERATION ============================================== >>>>
        public static GameObject CreateChunkMeshObject(ChunkMesh chunkMesh)
        {
            GameObject worldObject = new GameObject($"Chunk at {chunkMesh.ParentChunk.Coordinate.Value}");

            MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = chunkMesh.Mesh;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();

            worldObject.AddComponent<MeshRenderer>().material = Settings.materialLibrary.DefaultGroundMaterial;
            worldObject.AddComponent<MeshCollider>().sharedMesh = chunkMesh.Mesh;

            return worldObject;
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