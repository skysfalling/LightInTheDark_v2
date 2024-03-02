using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldRegion : MonoBehaviour
{
    // >>>> Init
    private bool _initialized = false;
    public bool IsInitialized() { return _initialized; }

    // >>>> World Size
    private int _worldWidth_inRegions = WorldGeneration.WorldWidth_inRegions;
    private int _worldWidth_inWorldSpace = WorldGeneration.GetWorldWidth_inWorldSpace();

    // >>>> Play Region
    private int _playRegionWidth_inChunks = WorldGeneration.PlayRegionWidth_inChunks;
    private int _playRegionBoundaryOffset = WorldGeneration.BoundaryWallCount;
    private int _playRegionWidth_inCells = WorldGeneration.GetPlayRegionWidth_inCells();

    // >>>> Full Region
    private int _fullRegionWidth_inChunks = WorldGeneration.GetFullRegionWidth_inChunks();
    private int _fullRegionWidth_inCells = WorldGeneration.GetFullRegionWidth_inChunks();
    private int _fullRegionWidth_inWorldSpace = WorldGeneration.GetFullRegionWidth_inWorldSpace();

    // >>>> Region Height
    private int _regionMaxGroundHeight = WorldGeneration.MaxChunkHeight;

    // PUBLIC VARIABLES
    public WorldGeneration worldGeneration;
    public CoordinateMap coordinateMap;
    public Coordinate coordinate;
    public WorldChunkMap worldChunkMap;

    public Vector2Int localCoordinatePosition;
    public Vector3 centerPosition_inWorldSpace;
    public Vector3 originPosition_inWorldSpace;

    public Material defaultMaterial;

    public void Initialize(WorldGeneration worldGeneration, Coordinate regionCoordinate)
    {
        this.worldGeneration = worldGeneration;
        this.coordinate = regionCoordinate;
        this.localCoordinatePosition = regionCoordinate.Value;

        float worldWidthRadius = _worldWidth_inWorldSpace * 0.5f;
        float regionWidthRadius = _fullRegionWidth_inWorldSpace * 0.5f;
        float chunkWidthRadius = WorldGeneration.GetChunkWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        centerPosition_inWorldSpace = new Vector3(this.localCoordinatePosition.x, 0, this.localCoordinatePosition.y) * _fullRegionWidth_inWorldSpace;
        centerPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        centerPosition_inWorldSpace += regionWidthRadius * new Vector3(1, 0, 1);

        // >> Origin Coordinate Position { Bottom Left }
        originPosition_inWorldSpace = new Vector3(this.localCoordinatePosition.x, 0, this.localCoordinatePosition.y) * _fullRegionWidth_inWorldSpace;
        originPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        originPosition_inWorldSpace += chunkWidthRadius * new Vector3(1, 0, 1);

        // Set the transform to the center
        transform.position = centerPosition_inWorldSpace;

        // Create the coordinate map for the region
        this.coordinateMap = new CoordinateMap(this);
        this.worldChunkMap = new WorldChunkMap(this, this.coordinateMap);


        _initialized = true;
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
            if (this.worldGeneration.coordinateRegionMap.GetCoordinateAt(neighborPosition) == null)
            {
                // Neighbor not found
                currentRegion.coordinateMap.CloseMapBorder(currentBorderWithNeighbor); // close borders on chunks

                Debug.Log($"REGION {currentRegion.coordinate.Value} -> CLOSED {getCurrentBorder} Border");
            }
            // else if shares a neighbor...
            else
            {
                Coordinate neighborRegionCoordinate = this.worldGeneration.coordinateRegionMap.GetCoordinateAt(neighborPosition);
                WorldRegion neighborRegion = this.worldGeneration.regionMap[neighborRegionCoordinate.Value];

                // if neighbor has exits on shared border
                MapBorder matchingBorderOnNeighbor = (MapBorder)CoordinateMap.GetOppositeBorder(currentBorderWithNeighbor);
                HashSet<Vector2Int> neighborBorderExits = neighborRegion.coordinateMap.GetExitsOnBorder(matchingBorderOnNeighbor);

                // if neighbor has exits, match exits
                if (neighborBorderExits != null && neighborBorderExits.Count > 0)
                {
                    Debug.Log($"REGION {currentRegion.coordinate.Value} & REGION {neighborRegion.coordinate.Value} share exit");

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

    public void ResetChunkMap()
    {
        this.worldChunkMap = new WorldChunkMap(this, this.coordinateMap);
    }

    public void CreateChunkMeshObjects()
    {
        ResetChunkMap();
        foreach (WorldChunk chunk in worldChunkMap.allChunks)
        {
            chunk.CreateChunkMeshObject(this);
        }
    }
}
