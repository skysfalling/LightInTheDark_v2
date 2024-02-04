using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PathColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE }

[System.Serializable]
public class WorldPath
{
    PathColor _color;
    WorldCoordinate _startCoordinate;
    WorldCoordinate _endCoordinate;
    List<WorldCoordinate> _pathCoords;
    bool _isValid;
    [HideInInspector] public bool initialized = false;

    public static float PathRandomness;
    public WorldPath(WorldCoordinate startCoord, WorldCoordinate endCoord, PathColor color)
    {
        this._startCoordinate = startCoord;
        this._endCoordinate = endCoord;
        this._color = color;

        Initialize();

        initialized = true;
    }

    public void Initialize()
    {
        _pathCoords = WorldCoordinateMap.FindCoordinatePath(this._startCoordinate, this._endCoordinate);
        foreach (WorldCoordinate c in _pathCoords)
        {
            c.pathColor = GetPathColor(_color);
            c.type = WorldCoordinate.TYPE.PATH;
        }

        initialized = true;
    }

    public Color GetPathColor(PathColor color)
    {
        switch (color)
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
        }
        return Color.black;
    }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!initialized) { Initialize(); }

        return _pathCoords;
    }
}

[System.Serializable]
public class WorldExitPath
{
    public PathColor pathColor = PathColor.YELLOW;
    public WorldExit startExit;
    public WorldExit endExit;
    WorldPath _worldPath;
    [HideInInspector] public bool initialized = false;

    public void Initialize()
    {
        if (startExit == null) { return; }
        if (endExit == null) { return; }

        startExit.Initialize();
        endExit.Initialize();
        _worldPath = new WorldPath(startExit.PathConnectionCoord, endExit.PathConnectionCoord, pathColor);

        initialized = true;
    }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        if (!initialized) { Initialize(); }

        return _worldPath.GetPathCoordinates();
    }
}
