using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Darklight.World.Generation
{    public class Region : AsyncTaskQueen, ITaskQueen
    {
        // [[ PRIVATE VARIABLES ]]
        string _prefix = "(( REGION ))";
        WorldBuilder _generationParent;
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkMap _chunkMap;
        GameObject _combinedMeshObject;

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public bool Initialized { get; private set; }
        public WorldBuilder GenerationParent => _generationParent;
        public Coordinate Coordinate => _coordinate;
        public CoordinateMap CoordinateMap => _coordinateMap;
        public ChunkMap ChunkMap => _chunkMap;
        public Vector3 CenterPosition => Coordinate.ScenePosition;
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
		public bool initializeOnStart = true;

        public void SetReferences(WorldBuilder parent, Coordinate coordinate, string taskQueenName = "Region Task Queen")
        {
            this._generationParent = parent;
            this._coordinate = coordinate;

            // Set the transform to the center
            transform.position = CenterPosition;

            this.name = taskQueenName;
        }

		public void Start()
        {
            Debug.Log("Start");
			if (initializeOnStart == true)
            {
                this.Initialize();
            }        
        }

        public override void Initialize(string name = "RegionAsyncTaskQueen")
        {
            base.Initialize(name);
            _ = InitializationSequence();
        }

        public async Task InitializationSequence()
        {
            // Create the coordinate map for the region
            NewTaskBot("Initialize Coordinate Map", async () =>
            {
                this._coordinateMap = new CoordinateMap(this);
                await Task.Delay(100);
            });

            // Create the chunk map for the region
            NewTaskBot("Initialize Chunk Map", async () =>
            {
                this._chunkMap = new ChunkMap(this, this._coordinateMap);
                await Task.Delay(100);
            });

            // Update the chunk map to reflect the coordinate map
            NewTaskBot("Update Chunk Map", async () =>
            {
                this._chunkMap.UpdateMap();
                await Task.Delay(100);
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
                    Region neighborRegion = this.GenerationParent.RegionMap[neighborCoordinateValue];
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

        public void NewSeedGeneration()
        {
            WorldBuilder.InitializeSeedRandom();
            _coordinateMap = new CoordinateMap(this);
            CoordinateMap.GenerateRandomExits();
            CoordinateMap.GeneratePathsBetweenExits();
            CoordinateMap.GenerateRandomZones(1, 3, new List<Zone.TYPE>() { Zone.TYPE.FULL }); // Zone generation
        }

        public void ResetCoordinateMap()
        {
            _coordinateMap = new CoordinateMap(this);
        }

        public void ResetChunkMap()
        {
            _chunkMap = new ChunkMap(this, _coordinateMap);
        }

        // == MESH GENERATION ============================================== >>>>

        /// <summary> Create GameObject from ChunkMesh </summary>
        public GameObject CreateChunkMeshObject(ChunkMesh chunkMesh)
        {
            GameObject worldObject = new GameObject($"Chunk at {chunkMesh.ParentChunk.Coordinate.ValueKey}");

            MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = chunkMesh.Mesh;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();

            worldObject.AddComponent<MeshRenderer>().material = WorldBuilder.Settings.materialLibrary.DefaultGroundMaterial;
            worldObject.AddComponent<MeshCollider>().sharedMesh = chunkMesh.Mesh;

            return worldObject;
        }

        /// <summary> Create GameObject with a given name, mesh and material </summary>
        public GameObject CreateMeshObject(string name, Mesh mesh, Material material)
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

        // [[ GENERATE COMBINED MESHES ]] ========================================== >>
        /// <summary>
        /// Combines multiple Mesh objects into a single mesh. This is useful for optimizing rendering by reducing draw calls.
        /// </summary>
        /// <param name="meshes">A List of Mesh objects to be combined.</param>
        /// <returns>A single combined Mesh object.</returns>
        Mesh CombineChunks(List<Chunk> chunks)
        {
            // Get Meshes from chunks
            List<Mesh> meshes = new List<Mesh>();
            foreach (Chunk chunk in chunks)
            {
                meshes.Add(chunk.ChunkMesh.Mesh);
            }

            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>(); // Add a list for the new UVs

            int vertexOffset = 0; // Keep track of the vertex offset

            foreach (Mesh mesh in meshes)
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
        public void CreateCombinedChunkMesh()
        {
            this.ChunkMap.UpdateMap();

            // Create Combined Mesh of world chunks
            Mesh combinedMesh = CombineChunks(this.ChunkMap.AllChunks.ToList());
            this._combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, WorldBuilder.Settings.materialLibrary.DefaultGroundMaterial);
            this._combinedMeshObject.transform.parent = this.transform;
            MeshCollider collider = _combinedMeshObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
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
