using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCoordinate
{
    public enum TYPE { NULL, BORDER, EXIT, PATH, ZONE, CLOSED }
    public TYPE type;
    public WorldDirection borderEdgeDirection;
    public Color debugColor = Color.clear;

    public Vector2Int Coordinate { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector3 WorldPosition
    {
        get
        {
            return new Vector3(Position.x, 0, Position.y);
        }
        private set { }
    }

    public WorldCoordinate(Vector2Int coord)
    {
        Coordinate = coord;

        // Calculate position
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2 half_FullWorldSize = realFullWorldSize * 0.5f;
        Vector2 newPos = new Vector2(coord.x * realChunkAreaSize.x, coord.y * realChunkAreaSize.y);
        newPos -= Vector2.one * half_FullWorldSize;
        newPos += Vector2.one * realChunkAreaSize * 0.5f;

        Position = newPos;
        WorldPosition = new Vector3(Position.x, 0, Position.y);
    }
}

