using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PathColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }

[System.Serializable]
public class WorldPath
{
    PathColor _color = PathColor.CLEAR;
    WorldCoordinate _startCoordinate;
    WorldCoordinate _endCoordinate;
    List<WorldCoordinate> _pathCoords;
    bool _isValid;
    float _pathRandomness = 0;
    bool _initialized = false;

    public WorldPath(WorldCoordinate startCoord, WorldCoordinate endCoord, PathColor color, float pathRandomness = 0)
    {
        this._startCoordinate = startCoord;
        this._endCoordinate = endCoord;
        this._color = color;
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
        foreach (WorldCoordinate c in _pathCoords)
        {
            c.type = WorldCoordinate.TYPE.PATH;
        }

        _initialized = true;
    }

    public bool IsInitialized() { return _initialized; }

    public Color GetPathColorRGBA()
    {
        switch (this._color)
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

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!_initialized) { Initialize(); }

        return _pathCoords;
    }
}

[System.Serializable]
public class WorldExitPath
{
    WorldPath _worldPath;
    bool _initialized = false;

    public PathColor pathColor = PathColor.YELLOW;
    [Range(0, 1)] public float pathRandomness = 0f;
    public WorldExit startExit;
    public WorldExit endExit;

    public void Initialize()
    {
        if (startExit == null) { return; }
        if (endExit == null) { return; }

        startExit.Initialize();
        endExit.Initialize();
        _worldPath = new WorldPath(startExit.PathConnectionCoord, endExit.PathConnectionCoord, pathColor, pathRandomness);

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
        switch (this.pathColor)
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
}
