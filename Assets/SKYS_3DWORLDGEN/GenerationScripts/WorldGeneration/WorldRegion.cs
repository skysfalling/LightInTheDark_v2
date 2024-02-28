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
    public Vector2Int regionCoordinate;
    public Vector3 centerPosition;

    public void Initialize(Vector2Int regionCoordinate)
    {
        this.regionCoordinate = regionCoordinate;
        //this.coordinateMap = new CoordinateMap(WorldSpace.Region, this.transform);

        float worldWidthRadius = _worldWidth_inWorldSpace * 0.5f;
        float regionWidthRadius = _fullRegionWidth_inWorldSpace * 0.5f;

        // Calculate based on region width
        centerPosition = new Vector3(this.regionCoordinate.x, 0, this.regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        centerPosition -= worldWidthRadius * new Vector3(1, 0, 1);
        centerPosition += regionWidthRadius * new Vector3(1, 0, 1);

        _initialized = true;
    }
}
