using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Region : MonoBehaviour
    {
        // [[ PRIVATE VARIABLES ]]
        WorldGeneration _generationParent;
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkMap _chunkMap;
        GameObject _combinedMeshObject;

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public bool Initialized { get; private set; }
        public WorldGeneration GenerationParent { get; private set; }
        public Coordinate Coordinate { get; private set; }
        public CoordinateMap CoordinateMap => _coordinateMap;
        public ChunkMap ChunkMap => _chunkMap;
        public Vector3 CenterPosition
        {
            get
            {
                Vector3 center = Coordinate.Position;
                return center;
            }
        }
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
            this.GenerationParent = parent;
            this.Coordinate = coordinate;

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

            this._chunkMap.UpdateMap();

            Initialized = true;
        }

        public void GenerateNecessaryExits(bool createExits)
        {
            Region currentRegion = this;
            Coordinate currentRegionCoordinate = this.Coordinate;

            // iterate through all region neighbors
            Dictionary<WorldDirection, Vector2Int> neighborDirectionMap = currentRegionCoordinate.NeighborDirectionMap;
            List<WorldDirection> allNeighborDirections = neighborDirectionMap.Keys.ToList();
            for (int j = 0; j < allNeighborDirections.Count; j++)
            {
                WorldDirection neighborDirection = allNeighborDirections[j];
                Vector2Int neighborCoordinateValue = neighborDirectionMap[allNeighborDirections[j]];
                BorderDirection? getCurrentBorder = CoordinateMap.GetMapBorderInNaturalDirection(neighborDirection); // get region border
                if (getCurrentBorder == null) continue;

                // defined map border variable
                BorderDirection currentBorderWithNeighbor = (BorderDirection)getCurrentBorder;

                // close borders that dont share a neighbor
                if (this.GenerationParent.CoordinateMap.GetCoordinateAt(neighborCoordinateValue) == null)
                {
                    // Neighbor not found
                    currentRegion.CoordinateMap.CloseMapBorder(currentBorderWithNeighbor); // close borders on chunks

                    //Debug.Log($"REGION {currentRegion.coordinate.Value} -> CLOSED {getCurrentBorder} Border");
                }
                // else if shares a neighbor...
                else
                {
                    Region neighborRegion = this.GenerationParent.RegionMap[neighborCoordinateValue];

                    // if neighbor has exits on shared border
                    BorderDirection matchingBorderOnNeighbor = (BorderDirection)CoordinateMap.GetOppositeBorder(currentBorderWithNeighbor);
                    HashSet<Vector2Int> neighborBorderExits = neighborRegion.CoordinateMap.GetExitsOnBorder(matchingBorderOnNeighbor);

                    // if neighbor has exits, match exits
                    if (neighborBorderExits != null && neighborBorderExits.Count > 0)
                    {
                        //Debug.Log($"REGION {currentRegion.coordinate.Value} & REGION {neighborRegion.coordinate.Value} share exit");

                        foreach (Vector2Int exit in neighborBorderExits)
                        {
                            //Debug.Log($"Region {currentRegionCoordinate.Value} Border {getCurrentBorder} ->");
                            currentRegion.CoordinateMap.SetMatchingExit(matchingBorderOnNeighbor, exit);
                        }
                    }
                    // if neighbor has no exits, randomly make some
                    else if (createExits)
                    {
                        // randomly decide how many 
                        currentRegion.CoordinateMap.GenerateRandomExitOnBorder(currentBorderWithNeighbor);
                    }
                }

            }

            // Clean up inactive corners
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
            _coordinateMap.GenerateRandomExits();
            _coordinateMap.GeneratePathsBetweenExits();
            _coordinateMap.GenerateRandomZones(1, 3);
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
                meshes.Add(chunk.ChunkMesh.mesh);
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

            Mesh combinedMesh = new Mesh();
            combinedMesh.vertices = newVertices.ToArray();
            combinedMesh.triangles = newTriangles.ToArray();
            combinedMesh.uv = newUVs.ToArray(); // Set the combined UVs

            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }


        public void CreateCombinedChunkMesh()
        {
            this.ChunkMap.UpdateMap();

            // Create Combined Mesh of world chunks
            Mesh combinedMesh = CombineChunks(this.ChunkMap.AllChunks.ToList());
            this._combinedMeshObject = WorldGeneration.CreateMeshObject($"CombinedChunkMesh", combinedMesh, GenerationParent.materialLibrary.DefaultGroundMaterial);
            this._combinedMeshObject.transform.parent = this.transform;
            MeshCollider collider = _combinedMeshObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
        }
    }
}
