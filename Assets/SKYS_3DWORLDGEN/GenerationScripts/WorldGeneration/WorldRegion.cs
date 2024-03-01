using System.Collections;
using System.Collections.Generic;
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
    private int _playRegionBoundaryOffset = WorldGeneration.PlayRegionBoundaryOffset;
    private int _playRegionWidth_inCells = WorldGeneration.GetPlayRegionWidth_inCells();

    // >>>> Full Region
    private int _fullRegionWidth_inChunks = WorldGeneration.GetFullRegionWidth_inChunks();
    private int _fullRegionWidth_inCells = WorldGeneration.GetFullRegionWidth_inChunks();
    private int _fullRegionWidth_inWorldSpace = WorldGeneration.GetFullRegionWidth_inWorldSpace();

    // >>>> Region Height
    private int _regionMaxGroundHeight = WorldGeneration.RegionMaxGroundHeight;

    // PUBLIC VARIABLES
    public WorldGeneration worldGeneration;
    public CoordinateMap coordinateMap;
    public WorldChunkMap worldChunkMap;

    public Vector2Int regionCoordinate;
    public Vector3 centerPosition_inWorldSpace;
    public Vector3 originPosition_inWorldSpace;

    public Material defaultMaterial;

    public void Initialize(WorldGeneration worldGeneration, Vector2Int regionCoordinate)
    {
        this.worldGeneration = worldGeneration;
        this.regionCoordinate = regionCoordinate;

        float worldWidthRadius = _worldWidth_inWorldSpace * 0.5f;
        float regionWidthRadius = _fullRegionWidth_inWorldSpace * 0.5f;
        float chunkWidthRadius = WorldGeneration.GetChunkWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        centerPosition_inWorldSpace = new Vector3(this.regionCoordinate.x, 0, this.regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        centerPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        centerPosition_inWorldSpace += regionWidthRadius * new Vector3(1, 0, 1);

        // >> Origin Coordinate Position { Bottom Left }
        originPosition_inWorldSpace = new Vector3(this.regionCoordinate.x, 0, this.regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        originPosition_inWorldSpace -= worldWidthRadius * new Vector3(1, 0, 1);
        originPosition_inWorldSpace += chunkWidthRadius * new Vector3(1, 0, 1);

        // Set the transform to the center
        transform.position = centerPosition_inWorldSpace;

        // Create the coordinate map for the region
        this.coordinateMap = new CoordinateMap(this);
        this.coordinateMap.GenerateRandomExits();
        this.coordinateMap.GeneratePathsBetweenExits();
        this.coordinateMap.GenerateRandomZones(1, 3);

        this.worldChunkMap = new WorldChunkMap(this, this.coordinateMap);
        _initialized = true;
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
