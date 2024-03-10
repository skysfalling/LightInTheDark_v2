using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using UnityEditor;
using UnityEngine;

namespace Darklight.World.Generation
{
    using DarklightEditor = Darklight.Unity.CustomInspectorGUI;
    [RequireComponent(typeof(ChunkGeneration))]
    public class RegionBuilder : AsyncTaskQueen, ITaskQueen
    {
        // [[ PRIVATE VARIABLES ]]
        string _prefix = "[[ REGION BUILDER ]]";
        WorldBuilder _generationParent;
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkGeneration _chunkGeneration;
        GameObject _combinedMeshObject;
        HashSet<GameObject> _regionObjects = new();

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public bool Initialized { get; private set; }
        public WorldBuilder GenerationParent => _generationParent;
        public Coordinate Coordinate => _coordinate;
        public CoordinateMap CoordinateMap => _coordinateMap;
        public ChunkGeneration ChunkGeneration => GetComponent<ChunkGeneration>();
        public Vector3 CenterPosition
        {
            get
            {
                if (Coordinate == null) { return Vector3.zero; }
                else { return Coordinate.ScenePosition; }
            }
        }
        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = CenterPosition;
                origin -= WorldBuilder.Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += WorldBuilder.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }


        // [[ PUBLIC REFERENCE VARIABLES ]]	
        public Material defaultMaterial;
        static GenerationSettings _regionSettings = new();
        public CustomGenerationSettings customRegionSettings;
        public static GenerationSettings Settings => _regionSettings;
        /// <summary> Override the default generation settings. </summary>
        public void OverrideSettings(CustomGenerationSettings customSettings)
        {
            if (customSettings == null) { _regionSettings = new GenerationSettings(); return; }
            _regionSettings = new GenerationSettings(customSettings);
        }

        #region [[ RANDOM SEED ]] -------------------------------------- >> 
        public static string Seed { get { return Settings.Seed; } }

        public static void InitializeSeedRandom()
        {
            UnityEngine.Random.InitState(Settings.Seed.GetHashCode());
        }
        #endregion
        public bool initializeOnStart;

        public void AssignToWorld(WorldBuilder parent, Coordinate coordinate, string taskQueenName = "Region Task Queen")
        {
            this._generationParent = parent;
            this._coordinate = coordinate;

            // Set the transform to the center
            transform.position = CenterPosition;

            _regionSettings = WorldBuilder.Settings;
            asyncTaskConsole.Log(this, "Assigned to World Builder with settings " + $"{_regionSettings}");
        }

        public void Start()
        {
            Debug.Log("Start");
            if (initializeOnStart == true)
            {
                Debug.Log($"{_prefix} Initialize On Start");
                this.Initialize();
            }
        }

        public override void Initialize(string name = "RegionAsyncTaskQueen")
        {
            if (WorldBuilder.Settings != null)
            {
                _regionSettings = WorldBuilder.Settings;
            }

            if (Initialized) return;
            Initialized = true;
            base.Initialize(name);

            _ = InitializationSequence();
        }

        public void Reset()
        {
            Initialized = false;
            _coordinateMap = null;

            foreach (GameObject gameObject in _regionObjects)
            {
                DestroyGameObject(gameObject);
            }
        }

        async Task InitializationSequence()
        {
            // Create the coordinate map
            base.NewTaskBot("Initialize Coordinate Map", async () =>
            {
                this._coordinateMap = new CoordinateMap(this);
                await Task.Delay(500);
                await Task.Yield();
            });

            // Create the chunk map for the region
            base.NewTaskBot("Initialize Chunk Generation", async () =>
            {
                this._chunkGeneration = GetComponent<ChunkGeneration>();
                await Task.Run(() => this._coordinateMap.Initialized);

                // Create the chunk objects if the _generationParent is null
                if (WorldBuilder.Instance)
                {
                    _chunkGeneration.Initialize(this, _coordinateMap, false);
                }
                else
                {
                    _chunkGeneration.Initialize(this, _coordinateMap, true);
                }
                await Task.Run(() => new WaitUntil(() => _chunkGeneration.Initialized));

                asyncTaskConsole.Log(this, "Waiting for chunkMeshes to initialize");
                // wait for each chunkMesh to be created
                foreach (Chunk chunk in _chunkGeneration.AllChunks)
                {
                    await Task.Run(() => new WaitUntil(() => chunk.ChunkMesh != null));
                    await Task.Yield();
                }

                await Task.Yield();
            });

            // combine the chunk mesh if WorldBuilder exists 
            base.NewTaskBot("Mesh Generation", async () =>
            {
                asyncTaskConsole.Log(this, $"Mesh Generation Started :: {_chunkGeneration.AllChunks.Count} Chunks");

                GameObject combinedObject = CreateCombinedChunkMesh(_chunkGeneration.AllChunks.ToList());

                asyncTaskConsole.Log(this, $"Mesh Generation Complete");
                await Task.Yield();
            });


            // Execute through all tasks
            await base.ExecuteAllBotsInQueue();

            Initialized = true;
            //Debug.Log($"{_prefix} Initialized at {Coordinate.ValueKey}");
        }

        public void GenerateNecessaryExits(bool createExits)
        {
            Dictionary<WorldDirection, Vector2Int> neighborDirectionMap = this.Coordinate.NeighborDirectionMap;

            // Iterate directly over the keys of the map
            foreach (WorldDirection neighborDirection in neighborDirectionMap.Keys)
            {
                Vector2Int neighborCoordinateValue = neighborDirectionMap[neighborDirection];
                BorderDirection? currentBorderWithNeighbor = CoordinateMap.GetBorderDirection(neighborDirection);

                // Skip iteration if no border direction is found.
                if (!currentBorderWithNeighbor.HasValue) continue;

                // Check if the neighbor exists; close the border if it doesn't.
                if (this.GenerationParent.CoordinateMap.GetCoordinateAt(neighborCoordinateValue) == null)
                {
                    // Close borders on chunks if neighbor not found.
                    this.CoordinateMap.CloseMapBorder(currentBorderWithNeighbor.Value);
                }
                else
                {
                    // Proceed with exit handling if the neighbor exists.
                    BorderDirection borderInThisRegion = (BorderDirection)currentBorderWithNeighbor; // >> convert border direction to non-nullable type
                    // >> get reference to neighbor region
                    RegionBuilder neighborRegion = this.GenerationParent.RegionMap[neighborCoordinateValue];
                    // >> get matching border direction
                    BorderDirection matchingBorderOnNeighbor = (BorderDirection)CoordinateMap.GetOppositeBorder(borderInThisRegion);
                    // >> get exits on neighbor region
                    HashSet<Vector2Int> neighborBorderExits = neighborRegion.CoordinateMap.GetExitsOnBorder(matchingBorderOnNeighbor);

                    // If neighbor has exits, create matching exits.
                    if (neighborBorderExits != null && neighborBorderExits.Count > 0)
                    {
                        foreach (Vector2Int exit in neighborBorderExits)
                        {
                            this.CoordinateMap.CreateMatchingExit(matchingBorderOnNeighbor, exit);
                        }
                    }
                    // If neighbor has no exits and exits are to be created, generate them randomly.
                    else if (createExits)
                    {
                        this.CoordinateMap.GenerateRandomExitOnBorder(borderInThisRegion);
                    }

                }
            }

            // Clean up inactive corners once after all border processing is done.
            CoordinateMap.SetInactiveCornersToType(Coordinate.TYPE.BORDER);
        }

        public void Destroy()
        {
            DestroyGameObject(this.gameObject);
        }

        // == MESH GENERATION ============================================== >>>>


        /// <summary> Create GameObject with a given name, mesh and material </summary>
        public GameObject CreateMeshObject(string name, Mesh mesh, Material material = null)
        {
            GameObject worldObject = new GameObject(name);
            worldObject.transform.parent = transform;

            _regionObjects.Add(worldObject);

            MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();


            if (material == null)
            {
                Debug.LogError("Mesh material is null");
                worldObject.AddComponent<MeshRenderer>().material = defaultMaterial;
                worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
            else
            {
                worldObject.AddComponent<MeshRenderer>().material = material;
                worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }


            return worldObject;
        }

        /// <summary>
        /// Combines multiple Mesh objects into a single mesh. This is useful for optimizing rendering by reducing draw calls.
        /// </summary>
        /// <param name="meshes">A List of Mesh objects to be combined.</param>
        /// <returns>A single combined Mesh object.</returns>
        Mesh CombineChunks()
        {
            Debug.Log("Combine Chunks");

            // Get Meshes from chunks
            List<Mesh> meshes = new List<Mesh>();
            foreach (Chunk chunk in _chunkGeneration.AllChunks)
            {
                if (chunk != null && chunk.ChunkMesh != null && chunk.ChunkMesh.Mesh != null)
                {
                    meshes.Add(chunk.ChunkMesh.Mesh);
                }
                else
                {
                    Debug.LogWarning("Invalid chunk or mesh found while combining chunks.");
                }
            }

            Debug.Log("Extract Meshes");

            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>(); // Add a list for the new UVs

            int vertexOffset = 0; // Keep track of the vertex offset

            foreach (Mesh mesh in meshes)
            {
                if (mesh != null)
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
                else
                {
                    Debug.LogWarning("Invalid mesh found while combining chunks.");
                }
            }

            Mesh combinedMesh = new Mesh
            {
                vertices = newVertices.ToArray(),
                triangles = newTriangles.ToArray(),
                uv = newUVs.ToArray() // Set the combined UVs
            };

            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }

        public GameObject CreateCombinedChunkMesh(List<Chunk> chunks)
        {
            this.ChunkGeneration.UpdateMap();
            Debug.Log("Update Chunk Generation");

            // Create Combined Mesh of world chunks
            Mesh combinedMesh = CombineChunks();
            Debug.Log("Create Combined Mesh");

            try
            {
                Debug.LogError("Found material");
                if (WorldBuilder.Settings != null)
                {
                    this._combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, WorldBuilder.Settings.materialLibrary.DefaultGroundMaterial);
                }
                else if (RegionBuilder.Settings != null)
                {
                    this._combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, RegionBuilder.Settings.materialLibrary.DefaultGroundMaterial);
                }
                Debug.LogError("Found material");

            }
            catch (Exception ex)
            {
                Debug.LogError("Material is null" + $"{ex}");
                this._combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, null);
            }

            Debug.LogError("Created Object");
            this._combinedMeshObject.transform.parent = this.transform;
            MeshCollider collider = _combinedMeshObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;

            return this._combinedMeshObject;
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
    }
}