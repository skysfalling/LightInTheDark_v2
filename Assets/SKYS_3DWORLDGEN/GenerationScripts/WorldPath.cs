using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class WorldPath
{
    WorldCoordinate _startCoordinate;
    WorldCoordinate _endCoordinate;
    List<WorldCoordinate> _path;
    bool _isValid;

    public static float PathRandomness;

    public Color pathColor = Color.yellow;

    public WorldPath(WorldCoordinate startCoord, WorldCoordinate endCoord)
    {
        this._startCoordinate = startCoord;
        this._endCoordinate = endCoord;
        _path = WorldCoordinateMap.FindCoordinatePath(startCoord, endCoord);
        foreach (WorldCoordinate c in _path)
        {
            c.type = WorldCoordinate.TYPE.PATH;
        }
    }


}

[System.Serializable]
public class WorldExitPath
{
    public WorldExit startExit;
    public WorldExit endExit;
    WorldPath _path;

    public WorldExitPath(WorldExit startExit, WorldExit endExit)
    {
        this.startExit = startExit;
        this.endExit = endExit;
        _path = new WorldPath(startExit.PathConnectionCoord, endExit.PathConnectionCoord);
    }
}
