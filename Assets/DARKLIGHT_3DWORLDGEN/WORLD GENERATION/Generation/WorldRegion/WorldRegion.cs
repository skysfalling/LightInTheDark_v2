using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorldRegion : MonoBehaviour
{
    // >>>> Init
    public bool Initialized { get; private set; }

    // >>>> Combined Chunk Mesh
    private GameObject _combinedChunkMeshObject;

    // PUBLIC REFERENCE VARIABLES
    public WorldGeneration worldGeneration { get; private set; }
    public CoordinateMap coordinateMap { get; private set; }
    public Coordinate coordinate { get; private set; }
    public WorldChunkMap worldChunkMap { get; private set; }
    public Vector2Int localCoordinatePosition { get; private set; }
    public Vector3 centerPosition_inWorldSpace { get; private set; }
    public Vector3 originPosition_inWorldSpace { get; private set; }

    // PUBLIC INSPECTOR VARIABLES
    public Material defaultMaterial;

    public void Initialize(WorldGeneration worldGeneration, Coordinate regionCoordinate)
    {
        this.worldGeneration = worldGeneration;
        this.coordinate = regionCoordinate;
        this.localCoordinatePosition = regionCoordinate.Value;

        int worldWidth = WorldGeneration.GetWorldWidth_inWorldSpace();
        int fullRegionWidth = WorldGeneration.GetFullRegionWidth_inWorldSpace();
        float worldWidthRadius = worldWidth * 0.5f;
        float regionWidthRadius = fullRegionWidth * 0.5f;
        float chunkWidthRadius = WorldGeneration.GetChunkWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        centerPosition_inWorldSpace = new Vector3(this.localCoordinatePosition.x, 0, this.localCoordinatePosition.y) * fullRegionWidth;
        centerPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        centerPosition_inWorldSpace += regionWidthRadius * new Vector3(1, 0, 1);

        // >> Origin Coordinate Position { Bottom Left }
        originPosition_inWorldSpace = new Vector3(this.localCoordinatePosition.x, 0, this.localCoordinatePosition.y) * fullRegionWidth;
        originPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        originPosition_inWorldSpace += chunkWidthRadius * new Vector3(1, 0, 1);

        // Set the transform to the center
        transform.position = centerPosition_inWorldSpace;

        StartCoroutine(InitializationSequence());
    }

    IEnumerator InitializationSequence() // Default delay of 1 second
    {
        // Create the coordinate map for the region
        this.coordinateMap = new CoordinateMap(this);
        yield return new WaitUntil(() => this.coordinateMap.Initialized);

        // Create the chunk map for the region
        this.worldChunkMap = new WorldChunkMap(this, this.coordinateMap);
        yield return new WaitUntil(() => this.worldChunkMap.Initialized);

        this.worldChunkMap.UpdateMap();

        Initialized = true;
        //Debug.Log($">>>> REGION {localCoordinatePosition} Initialized");

    }

    public void GenerateNecessaryExits(bool createExits)
    {
        WorldRegion currentRegion = this;
        Coordinate currentRegionCoordinate = this.coordinate;

        // iterate through all region neighbors
        Dictionary<WorldDirection, Vector2Int> neighborDirectionMap = currentRegionCoordinate.NeighborDirectionMap;
        List<WorldDirection> allNeighborDirections = neighborDirectionMap.Keys.ToList();
        for (int j = 0; j < allNeighborDirections.Count; j++)
        {
            WorldDirection neighborDirection = allNeighborDirections[j];
            Vector2Int neighborPosition = neighborDirectionMap[allNeighborDirections[j]];
            MapBorder? getCurrentBorder = CoordinateMap.GetMapBorderInNaturalDirection(neighborDirection); // get region border
            if (getCurrentBorder == null) continue;

            // defined map border variable
            MapBorder currentBorderWithNeighbor = (MapBorder)getCurrentBorder;

            // close borders that dont share a neighbor
            if (this.worldGeneration.CoordinateMap.GetCoordinateAt(neighborPosition) == null)
            {
                // Neighbor not found
                currentRegion.coordinateMap.CloseMapBorder(currentBorderWithNeighbor); // close borders on chunks

                //Debug.Log($"REGION {currentRegion.coordinate.Value} -> CLOSED {getCurrentBorder} Border");
            }
            // else if shares a neighbor...
            else
            {
                Coordinate neighborRegionCoordinate = this.worldGeneration.CoordinateMap.GetCoordinateAt(neighborPosition);
                WorldRegion neighborRegion = this.worldGeneration.RegionMap[neighborRegionCoordinate.Value];

                // if neighbor has exits on shared border
                MapBorder matchingBorderOnNeighbor = (MapBorder)CoordinateMap.GetOppositeBorder(currentBorderWithNeighbor);
                HashSet<Vector2Int> neighborBorderExits = neighborRegion.coordinateMap.GetExitsOnBorder(matchingBorderOnNeighbor);

                // if neighbor has exits, match exits
                if (neighborBorderExits != null && neighborBorderExits.Count > 0)
                {
                    //Debug.Log($"REGION {currentRegion.coordinate.Value} & REGION {neighborRegion.coordinate.Value} share exit");

                    foreach (Vector2Int exit in neighborBorderExits)
                    {
                        //Debug.Log($"Region {currentRegionCoordinate.Value} Border {getCurrentBorder} ->");
                        currentRegion.coordinateMap.SetMatchingExit(matchingBorderOnNeighbor, exit);
                    }
                }
                // if neighbor has no exits, randomly make some
                else if (createExits)
                {
                    // randomly decide how many 
                    currentRegion.coordinateMap.GenerateRandomExitOnBorder(currentBorderWithNeighbor);
                }
            }

        }
    }

    public void Destroy()
    {
        WorldGeneration.DestroyGameObject(this.gameObject);
    }

    public void NewSeedGeneration()
    {
        WorldGeneration.InitializeSeedRandom();
        coordinateMap = new CoordinateMap(this);
        coordinateMap.GenerateRandomExits();
        coordinateMap.GeneratePathsBetweenExits();
        coordinateMap.GenerateRandomZones(1, 3);
    }

    public void ResetCoordinateMap()
    {
        coordinateMap = new CoordinateMap(this);
    }

    public void ResetChunkMap()
    {
        this.worldChunkMap = new WorldChunkMap(this, this.coordinateMap);
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
            meshes.Add(chunk.chunkMesh.mesh);
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
        this.worldChunkMap.UpdateMap();

        // Create Combined Mesh of world chunks
        Mesh combinedMesh = CombineChunks(this.worldChunkMap.AllChunks.ToList());
        this._combinedChunkMeshObject = WorldGeneration.CreateMeshObject($"CombinedChunkMesh", combinedMesh, worldGeneration.GetChunkMaterial());
        this._combinedChunkMeshObject.transform.parent = this.transform;
        MeshCollider collider = _combinedChunkMeshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;
    }
}
