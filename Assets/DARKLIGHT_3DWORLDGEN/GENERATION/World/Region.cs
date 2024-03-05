using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    using WorldGen = Generation;

    public class Region : MonoBehaviour
    {
        // >>>> Init
        public bool Initialized { get; private set; }

        // >>>> Combined Chunk Mesh
        private GameObject _combinedChunkMeshObject;

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public Generation WorldGenerationParent { get; private set; }
        public Coordinate Coordinate { get; private set; }
        public CoordinateMap CoordinateMap { get; private set; }
        public WorldChunkMap WorldChunkMap { get; private set; }
        public Vector2Int localCoordinatePosition { get; private set; }
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
                origin -= WorldGen.Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += WorldGen.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }

        // PUBLIC INSPECTOR VARIABLES
        public Material defaultMaterial;

        public void SetReferences(Generation worldGeneration, Coordinate coordinate)
        {
            this.WorldGenerationParent = worldGeneration;
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
            this.CoordinateMap = new CoordinateMap(this);
            yield return new WaitUntil(() => this.CoordinateMap.Initialized);

            // Create the chunk map for the region
            this.WorldChunkMap = new WorldChunkMap(this, this.CoordinateMap);
            yield return new WaitUntil(() => this.WorldChunkMap.Initialized);

            this.WorldChunkMap.UpdateMap();

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
                Vector2Int neighborPosition = neighborDirectionMap[allNeighborDirections[j]];
                BorderDirection? getCurrentBorder = CoordinateMap.GetMapBorderInNaturalDirection(neighborDirection); // get region border
                if (getCurrentBorder == null) continue;

                // defined map border variable
                BorderDirection currentBorderWithNeighbor = (BorderDirection)getCurrentBorder;

                // close borders that dont share a neighbor
                if (this.WorldGenerationParent.CoordinateMap.GetCoordinateAt(neighborPosition) == null)
                {
                    // Neighbor not found
                    currentRegion.CoordinateMap.CloseMapBorder(currentBorderWithNeighbor); // close borders on chunks

                    //Debug.Log($"REGION {currentRegion.coordinate.Value} -> CLOSED {getCurrentBorder} Border");
                }
                // else if shares a neighbor...
                else
                {
                    Coordinate neighborRegionCoordinate = this.WorldGenerationParent.CoordinateMap.GetCoordinateAt(neighborPosition);
                    Region neighborRegion = this.WorldGenerationParent.RegionMap[neighborRegionCoordinate.Value];

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
        }

        public void Destroy()
        {
            WorldGen.DestroyGameObject(this.gameObject);
        }

        public void NewSeedGeneration()
        {
            WorldGen.InitializeSeedRandom();
            CoordinateMap = new CoordinateMap(this);
            CoordinateMap.GenerateRandomExits();
            CoordinateMap.GeneratePathsBetweenExits();
            CoordinateMap.GenerateRandomZones(1, 3);
        }

        public void ResetCoordinateMap()
        {
            CoordinateMap = new CoordinateMap(this);
        }

        public void ResetChunkMap()
        {
            this.WorldChunkMap = new WorldChunkMap(this, this.CoordinateMap);
        }


        // [[ GENERATE COMBINED MESHES ]] ========================================== >>
        /// <summary>
        /// Combines multiple Mesh objects into a single mesh. This is useful for optimizing rendering by reducing draw calls.
        /// </summary>
        /// <param name="meshes">A List of Mesh objects to be combined.</param>
        /// <returns>A single combined Mesh object.</returns>
        Mesh CombineChunks(List<WorldChunk> chunks)
        {
            // Get Meshes from chunks
            List<Mesh> meshes = new List<Mesh>();
            foreach (WorldChunk chunk in chunks)
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
            this.WorldChunkMap.UpdateMap();

            // Create Combined Mesh of world chunks
            Mesh combinedMesh = CombineChunks(this.WorldChunkMap.AllChunks.ToList());
            this._combinedChunkMeshObject = WorldGen.CreateMeshObject($"CombinedChunkMesh", combinedMesh, WorldGenerationParent.GetChunkMaterial());
            this._combinedChunkMeshObject.transform.parent = this.transform;
            MeshCollider collider = _combinedChunkMeshObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
        }
    }
}
