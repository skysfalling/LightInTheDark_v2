using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPath
{
    public Vector2Int start { get; private set; }
    public Vector2Int end { get; private set; }
    public List<Vector2Int> positions { get; private set; }
    float _pathRandomness = 0;
    bool _initialized = false;

    public WorldPath(CoordinateMap coordinateMap, Vector2Int start, Vector2Int end, float pathRandomness = 0)
    {
        this.start = start;
        this.end = end;
        this._pathRandomness = pathRandomness;

        positions = WorldPathfinder.FindPath(coordinateMap, this.start, this.end, _pathRandomness);
    }

    public void Initialize()
    {
        /*
        if (typesAreValid)
        {
            //Debug.Log($"Found Valid Path from {_startCoordinate} -> {_endCoordinate}");

            _pathChunks = WorldChunkMap.GetChunksAtCoordinates(_positions);

            WorldChunk startChunk = WorldChunkMap.GetChunkAt(_start);
            WorldChunk endChunk = WorldChunkMap.GetChunkAt(_end);
            DeterminePathChunkHeights(_startHeight, _endHeight);

            // Set Coordinate Path Type
            CoordinateMap.SetMapCoordinatesToType(_positions, Coordinate.TYPE.PATH, GetRGBAFromDebugColor(_pathColor));

            _initialized = true;
        }
        */
    }

    public void Reset()
    {
        if (!_initialized) return;

        // Reset Coordinate Path Type
        if (positions != null && positions.Count > 0)
        {
            positions.Clear();

            _initialized = false;
        }
    }


    public bool IsInitialized() 
    { 
        return _initialized; 
    }


}


