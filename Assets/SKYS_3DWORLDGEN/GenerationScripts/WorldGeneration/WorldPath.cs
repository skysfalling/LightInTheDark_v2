using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class WorldPath
{
    public enum PathColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }
    PathColor _pathColor = PathColor.CLEAR;
    public static Color GetRGBAfromPathColorType(PathColor pathColor)
    {
        switch (pathColor)
        {
            case PathColor.BLACK:
                return Color.black;
            case PathColor.WHITE:
                return Color.white;
            case PathColor.RED:
                return Color.red;
            case PathColor.YELLOW:
                return Color.yellow;
            case PathColor.GREEN:
                return Color.green;
            case PathColor.BLUE:
                return Color.blue;
            default:
                return Color.clear;
        }
    }
    public static PathColor GetRandomPathColor()
    {
        // Get all values of the PathColor enum
        Array values = Enum.GetValues(typeof(PathColor));

        // Select a random index
        int randomIndex = UnityEngine.Random.Range(0, values.Length);

        // Return the random PathColor
        return (PathColor)values.GetValue(randomIndex);
    }


    Vector2Int _startCoordinate;
    Vector2Int _endCoordinate;
    List<WorldCoordinate> _pathCoords = new List<WorldCoordinate>();
    float _pathRandomness = 0;
    bool _initialized = false;

    public WorldPath(WorldCoordinate startCoord, WorldCoordinate endCoord, PathColor pathColor, float pathRandomness = 0)
    {
        this._startCoordinate = startCoord.Coordinate;
        this._endCoordinate = endCoord.Coordinate;
        this._pathColor = pathColor;
        this._pathRandomness = pathRandomness;
        Initialize();
    }

    public void Initialize()
    {
        if (_initialized || !WorldCoordinateMap.coordMapInitialized) return;
        _initialized = false;

        // Get Valid Path
        _pathCoords = WorldCoordinateMap.FindWorldCoordinatePath(_startCoordinate, _endCoordinate, _pathRandomness);

        // Set Coordinate Path Type
        WorldCoordinateMap.SetMapCoordinatesToType(_pathCoords, WorldCoordinate.TYPE.PATH);

        _initialized = true;
    }

    public void Reset()
    {
        if (!_initialized || !WorldCoordinateMap.coordMapInitialized) return;

        // Reset Coordinate Path Type
        if (_pathCoords != null && _pathCoords.Count > 0)
        {
            WorldCoordinateMap.SetMapCoordinatesToType(_pathCoords, WorldCoordinate.TYPE.NULL);
            _pathCoords.Clear();
        }

        _initialized = false;
    }


    public bool IsInitialized() { return _initialized; }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!_initialized) { return new List<WorldCoordinate>(); }
        return _pathCoords;
    }

    public void DeterminePathChunkHeights(int startHeight, int endHeight, float heightAdjustChance = 1f)
    {
        if (WorldChunkMap.chunkMapInitialized == false) return;

        List<WorldChunk> _pathChunks = WorldChunkMap.GetChunksAtCoordinates(_pathCoords);
        if (_pathChunks.Count == 0 ) return;

        // Assign start/end chunk heights
        WorldChunk startChunk = _pathChunks[0];
        WorldChunk endChunk = _pathChunks[_pathChunks.Count - 1];
        startChunk.groundHeight = startHeight;
        endChunk.groundHeight = endHeight;

        // Remove start and end chunks from consideration in the loop
        List<WorldChunk> midpathChunks = new List<WorldChunk>(_pathChunks);
        midpathChunks.RemoveAt(midpathChunks.Count - 1);
        midpathChunks.RemoveAt(0);

        int midpathChunkCount = midpathChunks.Count; // full path count
        int endpointHeightDifference = endHeight - startHeight;
        int currHeightLevel = startHeight; // current height level starting from the startHeight

        // Get Offset List
        int heightLeft = endpointHeightDifference;
        for (int i = 0; i < midpathChunkCount; i++)
        {
            // Determine heightOffset 
            int heightOffset = 0;
            if (heightLeft > 0) { heightOffset = 1; }
            else if (heightLeft < 0) { heightOffset = -1; }
            else { heightOffset = 0; }

            // Recalculate heightLeft with the new current height level
            currHeightLevel += heightOffset;
            heightLeft = endHeight - currHeightLevel;

            midpathChunks[i].groundHeight = currHeightLevel;
        }
    }

}


