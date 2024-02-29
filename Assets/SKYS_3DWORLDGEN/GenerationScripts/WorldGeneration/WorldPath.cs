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

    /*
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
                WorldDirection? lastChunkDirection = _pathChunks[i].worldCoordinate.GetWorldDirectionOfNeighbor(lastChunk.worldCoordinate);
                WorldDirection? nextChunkDirection = _pathChunks[i].worldCoordinate.GetWorldDirectionOfNeighbor(nextChunk.worldCoordinate);
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
    */

}


