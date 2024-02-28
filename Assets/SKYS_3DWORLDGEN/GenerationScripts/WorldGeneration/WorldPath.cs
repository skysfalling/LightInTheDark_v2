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
    DebugColor _pathColor = DebugColor.WHITE;
    public static Color GetRGBAFromDebugColor(DebugColor pathColor)
    {
        switch (pathColor)
        {
            case DebugColor.BLACK:
                return Color.black;
            case DebugColor.WHITE:
                return Color.white;
            case DebugColor.RED:
                return Color.red;
            case DebugColor.YELLOW:
                return Color.yellow;
            case DebugColor.GREEN:
                return Color.green;
            case DebugColor.BLUE:
                return Color.blue;
            default:
                return Color.clear;
        }
    }

    public static DebugColor GetRandomPathColor()
    {
        // Get all values of the PathColor enum
        Array values = Enum.GetValues(typeof(DebugColor));

        // Select a random index
        int randomIndex = UnityEngine.Random.Range(0, values.Length);

        // Return the random PathColor
        return (DebugColor)values.GetValue(randomIndex);
    }


    Vector2Int _startCoordinate;
    Vector2Int _endCoordinate;

    int _startHeight;
    int _endHeight;

    List<Coordinate> _pathCoords = new List<Coordinate>();
    List<WorldChunk> _pathChunks = new List<WorldChunk>();
    float _pathRandomness = 0;
    bool _initialized = false;

    public WorldPath(Coordinate startCoord, int startHeight, Coordinate endCoord, int endHeight, DebugColor pathColor, float pathRandomness = 0)
    {
        this._startCoordinate = startCoord.NormalizedCoordinate;
        this._endCoordinate = endCoord.NormalizedCoordinate;

        this._startHeight = startHeight;
        this._endHeight = endHeight;

        this._pathColor = pathColor;
        this._pathRandomness = pathRandomness;
        Initialize();
    }

    public void Initialize()
    {
        // Check that all coords are valid
        foreach (Coordinate coord in _pathCoords)
        {
            if (coord.type != Coordinate.TYPE.PATH)
            {
                _initialized = false;
                return;
            }
        }

        // Initialize
        if (_initialized || !CoordinateMap.coordMapInitialized) return;
        _initialized = false;

        // << CREATE PATH >>
        _pathCoords = CoordinateMap.FindWorldCoordinatePath(_startCoordinate, _endCoordinate, _pathRandomness);
        List<Coordinate.TYPE> types = CoordinateMap.GetCoordinateTypesFromList(_pathCoords);
        bool typesAreValid = true;
        foreach (Coordinate.TYPE type in types)
        {
            // If Coordinate Type does not match
            if (type != Coordinate.TYPE.PATH && type != Coordinate.TYPE.NULL)
            {
                typesAreValid = false;
                Debug.Log($"Path contains invalid types");
                break;
            }
        }

        if (typesAreValid)
        {
            //Debug.Log($"Found Valid Path from {_startCoordinate} -> {_endCoordinate}");

            _pathChunks = WorldChunkMap.GetChunksAtCoordinates(_pathCoords);

            WorldChunk startChunk = WorldChunkMap.GetChunkAt(_startCoordinate);
            WorldChunk endChunk = WorldChunkMap.GetChunkAt(_endCoordinate);
            DeterminePathChunkHeights(_startHeight, _endHeight);

            // Set Coordinate Path Type
            CoordinateMap.SetMapCoordinatesToType(_pathCoords, Coordinate.TYPE.PATH, GetRGBAFromDebugColor(_pathColor));

            _initialized = true;
        }
    }

    public void Reset()
    {
        if (!_initialized || !CoordinateMap.coordMapInitialized) return;

        // Reset Coordinate Path Type
        if (_pathCoords != null && _pathCoords.Count > 0)
        {
            _pathCoords.Clear();

            _initialized = false;
        }
    }


    public bool IsInitialized() 
    { 
        return _initialized; 
    }

    public List<Coordinate> GetPathCoordinates()
    {
        if (!_initialized) { return new List<Coordinate>(); }
        return _pathCoords;
    }

    public List<WorldChunk> GetPathChunks()
    {
        if (!_initialized) { return new List<WorldChunk>(); }
        return _pathChunks;
    }

    public void DeterminePathChunkHeights(int startHeight, int endHeight, float heightAdjustChance = 1f)
    {
        if (WorldChunkMap.chunkMapInitialized == false || _pathChunks.Count == 0) return;


        // Calculate height difference
        int endpointHeightDifference = endHeight - startHeight;
        int currHeightLevel = startHeight; // current height level starting from the startHeight
        int heightLeft = endpointHeightDifference; // initialize height left

        // Iterate through the chunks
        for (int i = 0; i < _pathChunks.Count; i++)
        {
            // Assign start/end chunk heights & CONTINUE
            if (i == 0) { _pathChunks[i].SetGroundHeight(startHeight); continue; }
            else if (i == _pathChunks.Count - 1) { _pathChunks[i].SetGroundHeight(endHeight); continue; }
            else
            {

                // Determine heightOffset 
                int heightOffset = 0;

                // Determine the direction of the last & next chunk in path
                WorldChunk lastChunk = _pathChunks[i - 1];
                WorldChunk nextChunk = _pathChunks[i + 1];
                WorldDirection? lastChunkDirection = _pathChunks[i].worldCoordinate.GetDirectionOfNeighbor(lastChunk.worldCoordinate);
                WorldDirection? nextChunkDirection = _pathChunks[i].worldCoordinate.GetDirectionOfNeighbor(nextChunk.worldCoordinate);
                if (lastChunkDirection != null && nextChunkDirection != null)
                {
                    if (_pathChunks[i].worldCoordinate.GetNeighborInOppositeDirection((WorldDirection)nextChunkDirection) == lastChunk.worldCoordinate)
                    {
                        // Valid transition chunk
                        if (heightLeft > 0) { heightOffset = 1; } // if height left is greater
                        else if (heightLeft < 0) { heightOffset = -1; } // if height left is less than 0
                        else { heightOffset = 0; } // if height left is equal to 0
                    }

                }

                // Set the new height level
                currHeightLevel += heightOffset;
                _pathChunks[i].SetGroundHeight(currHeightLevel);

                // Recalculate heightLeft with the new current height level
                heightLeft = endHeight - currHeightLevel;
            }
        }
    }

}


