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
        // Get Valid Path
        _pathCoords = WorldCoordinateMap.FindCoordinatePath(this._startCoordinate, this._endCoordinate, _pathRandomness);
        if (_pathCoords.Count == 0 ) { 
            _isValid = false; 
            _initialized = false; 
            return; 
        }

        _isValid = true;

        // Set Coordinate Path Type
        foreach (WorldCoordinate coord in _pathCoords)
        {
            coord.type = WorldCoordinate.TYPE.PATH;
        }

        // Set Chunk Path Color
        _pathChunks = WorldChunkMap.GetChunksAtCoordinates(_pathCoords);
        foreach(WorldChunk chunk in _pathChunks)
        {
            chunk.pathColor = _pathColor;
        }

        _initialized = true;
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

    public WorldExitPath(WorldExit startExit,  WorldExit endExit)
    {
        this.startExit = startExit;
        this.endExit = endExit;
        this.pathColor = WorldPath.GetRandomPathColor();

        Initialize();
    }

    public void Initialize()
    {
        if (startExit == null) { return; }
        if (endExit == null) { return; }

        startExit.Initialize();
        endExit.Initialize();

        _worldPath = new WorldPath(startExit.PathConnectionCoord, endExit.PathConnectionCoord, pathColor, pathRandomness);

        if (_worldPath.IsInitialized())
        {
            _worldPath.DeterminePathChunkHeights(startExit.exitHeight, endExit.exitHeight);
        }

        IsInitialized();
    }

    public bool IsInitialized() {

        /*
        Debug.Log($"Initialize WorldExitPath : {_initialized}" +
            $"\n StartExit : {startExit.IsInitialized()}" +
            $"\n EndExit : {endExit.IsInitialized()}" +
            $"\n WorldPath : {_worldPath.IsInitialized()}");
        */

        // Check if exits and path are initialized
        if (startExit.IsInitialized() && endExit.IsInitialized() && _worldPath.IsInitialized()) { _initialized = true; }
        else { _initialized = false; }

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
