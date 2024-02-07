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


    WorldCoordinate _startCoordinate;
    WorldCoordinate _endCoordinate;
    List<WorldCoordinate> _pathCoords;
    List<WorldChunk> _pathChunks;
    bool _isValid;
    float _pathRandomness = 0;
    bool _initialized = false;

    public WorldPath(WorldCoordinate startCoord, WorldCoordinate endCoord, PathColor pathColor, float pathRandomness = 0)
    {
        this._startCoordinate = startCoord;
        this._endCoordinate = endCoord;
        this._pathColor = pathColor;
        this._pathRandomness = pathRandomness;
        Initialize();
    }

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = false;

        // Get Valid Path
        _pathCoords = WorldCoordinateMap.FindCoordinatePath(this._startCoordinate, this._endCoordinate, _pathRandomness);


        foreach ( WorldCoordinate coord in _pathCoords )
        {
            if (WorldCoordinateMap.GetTypeAtCoord(coord) == WorldCoordinate.TYPE.ZONE)
            {

                Debug.Log("Found Zone in Path");
                _isValid = false;
                return;
            }
            
        }



        _isValid = true;

        // Set Coordinate Path Type
        WorldCoordinateMap.SetMapCoordinatesToType(_pathCoords, WorldCoordinate.TYPE.PATH);

        // Set Chunk Path Color
        _pathChunks = WorldChunkMap.GetChunksAtCoordinates(_pathCoords);
        foreach(WorldChunk chunk in _pathChunks)
        {
            chunk.pathColor = _pathColor;
        }

        _initialized = true;
    }

    public void Reset()
    {
        // Set Coordinate Path Type
        WorldCoordinateMap.SetMapCoordinatesToType(_pathCoords, WorldCoordinate.TYPE.NULL);
        _pathCoords.Clear();
        _initialized = false;
        _isValid = true;
    }

    public void DeterminePathChunkHeights(int startHeight, int endHeight, float heightAdjustChance = 1f)
    {
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
            int heightOffset = 0;

            // Determine height direction
            if (heightLeft > 0) { heightOffset = 1; }
            else if (heightLeft < 0) { heightOffset = -1; }
            else { heightOffset = 0; }

            // Recalculate heightLeft with the new current height level
            currHeightLevel += heightOffset;
            heightLeft = endHeight - currHeightLevel;

            midpathChunks[i].groundHeight = currHeightLevel;
        }

    }

    public bool IsInitialized() { return _initialized; }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!_initialized) { return new List<WorldCoordinate>(); }
        return _pathCoords;
    }
}

[System.Serializable]
public class WorldExitPath
{
    WorldPath _worldPath;
    bool _initialized = false;

    public WorldPath.PathColor pathColor = WorldPath.PathColor.YELLOW;
    [Range(0, 1)] public float pathRandomness = 0f;
    public WorldExit startExit;
    public WorldExit endExit;

    WorldCoordinate _pathStart;
    WorldCoordinate _pathEnd;
    float _pathRandomness;

    public WorldExitPath(WorldExit startExit,  WorldExit endExit)
    {
        this.startExit = startExit;
        this.endExit = endExit;
        this.pathColor = WorldPath.GetRandomPathColor();
    }

    public void Update()
    {
        if (_initialized) { return; }

        _pathStart = startExit.PathConnectionCoord;
        _pathEnd = endExit.PathConnectionCoord;
        _pathRandomness = pathRandomness;

        _worldPath = new WorldPath(_pathStart, _pathEnd, pathColor, pathRandomness);

        if (_worldPath.IsInitialized())
        {
            _worldPath.DeterminePathChunkHeights(startExit.exitHeight, endExit.exitHeight);
        }

        _initialized = true;
    }

    public void Reset()
    {
        if (!_initialized) return;

        // Check if values are incorrectly initialized
        if (_pathStart != startExit.PathConnectionCoord 
            || _pathEnd != endExit.PathConnectionCoord
            || _pathRandomness != pathRandomness)
        {
            _worldPath.Reset();
            _initialized = false;
        }
    }

    public bool IsInitialized() {
        return _initialized; 
    }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!_initialized) { return new List<WorldCoordinate>(); }

        return _worldPath.GetPathCoordinates();
    }

    public Color GetPathColorRGBA()
    {
        return WorldPath.GetRGBAfromPathColorType(pathColor);
    }
}
