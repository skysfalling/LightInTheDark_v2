using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation
{
    public class Region : MonoBehaviour
    {
        // [[ PRIVATE VARIABLES ]]
        string _prefix = "(( REGION ))";
        WorldGeneration _generationParent;
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkMap _chunkMap;
        GameObject _combinedMeshObject;

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public bool Initialized { get; private set; }
        public WorldGeneration GenerationParent => _generationParent;
        public Coordinate Coordinate => _coordinate;
        public CoordinateMap CoordinateMap => _coordinateMap;
        public ChunkMap ChunkMap => _chunkMap;
        public Vector3 CenterPosition => Coordinate.ScenePosition;
        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = CenterPosition;
                origin -= WorldGeneration.Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += WorldGeneration.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }
        public void SetReferences(WorldGeneration parent, Coordinate coordinate)
        {
            this._generationParent = parent;
            this._coordinate = coordinate;

            // Set the transform to the center
            transform.position = CenterPosition;
        }

        public void Initialize()
        {
            StartCoroutine(InitializationSequence());
        }

        IEnumerator InitializationSequence()
        {
            // Create the coordinate map for the region
            this._coordinateMap = new CoordinateMap(this);
            yield return new WaitUntil(() => this._coordinateMap.Initialized);

            // Create the chunk map for the region
            this._chunkMap = new ChunkMap(this, this._coordinateMap);
            yield return new WaitUntil(() => this._chunkMap.Initialized);

            // Update the chunk map to reflect the coordinate map
            this._chunkMap.UpdateMap();

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
            WorldGeneration.DestroyGameObject(this.gameObject);
        }

        public void NewSeedGeneration()
        {
            WorldGeneration.InitializeSeedRandom();
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
            this._combinedMeshObject = WorldGeneration.CreateMeshObject($"CombinedChunkMesh", combinedMesh, WorldGeneration.Settings.materialLibrary.DefaultGroundMaterial);
            this._combinedMeshObject.transform.parent = this.transform;
            MeshCollider collider = _combinedMeshObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
        }
    }
}
