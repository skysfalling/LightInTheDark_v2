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

    public CoordinateMap coordinateMap;
    public WorldChunkMap worldChunkMap;

    public Vector2Int regionCoordinate;
    public Vector3 centerPosition;
    public Vector3 originCoordinatePosition;

    public Material defaultMaterial;

    public void Initialize(Vector2Int regionCoordinate)
    {
        this.regionCoordinate = regionCoordinate;

        float worldWidthRadius = _worldWidth_inWorldSpace * 0.5f;
        float regionWidthRadius = _fullRegionWidth_inWorldSpace * 0.5f;
        float chunkWidthRadius = WorldGeneration.GetChunkWidth_inWorldSpace() * 0.5f;

        // >> Center Position
        centerPosition = new Vector3(this.regionCoordinate.x, 0, this.regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        centerPosition -= worldWidthRadius * new Vector3(1, 0, 1);
        centerPosition += regionWidthRadius * new Vector3(1, 0, 1);

        // >> Origin Coordinate Position { Bottom Left }
        originCoordinatePosition = new Vector3(this.regionCoordinate.x, 0, this.regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        originCoordinatePosition -= worldWidthRadius * new Vector3(1, 0, 1);
        originCoordinatePosition += chunkWidthRadius * new Vector3(1, 0, 1);

        // Set the transform to the center
        transform.position = centerPosition;

        // Create the coordinate map for the region
        this.coordinateMap = new CoordinateMap(this);

        _initialized = true;
    }

    public void CreateChunkMap()
    {
        if (coordinateMap == null) { Debug.Log("Cannot create chunk map without a coordinate map"); return; }
        this.worldChunkMap = new WorldChunkMap(this.coordinateMap);
    }

    public void CreateChunkMeshObjects()
    {
        foreach (WorldChunk chunk in worldChunkMap.allChunks)
        {
            WorldGeneration.CreateMeshObject($"Chunk {chunk.localPosition} :: height {chunk.groundHeight}", chunk.chunkMesh.mesh, defaultMaterial);
        }
    }
}
