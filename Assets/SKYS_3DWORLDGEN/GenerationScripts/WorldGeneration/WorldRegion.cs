using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRegion : MonoBehaviour
{
    // >>>> World Size
    private int _worldWidth_inRegions = WorldGeneration.WorldWidth_inRegions;
    private int _worldWidth_inWorldSpace = WorldGeneration.GetWorldWidth_inWorldSpace();

    // >>>> Play Region
    private int _playRegionWidth_inChunks = WorldGeneration.PlayRegionWidth_inChunks;
    private int _playRegionBoundaryOffset = WorldGeneration.PlayRegionBoundaryOffset;
    private int _playRegionWidth_inCells = WorldGeneration.GetPlayRegionWidth_inCells();

    // >>>> Full Region
    private int _fullRegionWidth_inChunks = WorldGeneration.GetFullRegionWidth_inChunks();
    private int _fullRegionWidth_inCells = WorldGeneration.GetFulRegionWidth_inCells();
    private int _fullRegionWidth_inWorldSpace = WorldGeneration.GetFulRegionWidth_inWorldSpace();

    // >>>> Region Height
    private int _regionMaxGroundHeight = WorldGeneration.RegionMaxGroundHeight;

    [SerializeField] Vector2Int _regionCoordinate;
    [SerializeField] Vector3 _regionCenter;


    public void Initialize(Vector2Int regionCoordinate)
    {
        _regionCoordinate = regionCoordinate;

        float worldWidthRadius = _worldWidth_inWorldSpace * 0.5f;
        float regionWidthRadius = _fullRegionWidth_inWorldSpace * 0.5f;


        // Calculate based on region width
        _regionCenter = new Vector3(_regionCoordinate.x, 0, _regionCoordinate.y) * _fullRegionWidth_inWorldSpace;
        _regionCenter -= worldWidthRadius * new Vector3(1, 0, 1);
        _regionCenter += regionWidthRadius * new Vector3(1, 0, 1);
    }

    public Vector3 GetRegionCenter() { return _regionCenter; }

    public void Update() { }
}
