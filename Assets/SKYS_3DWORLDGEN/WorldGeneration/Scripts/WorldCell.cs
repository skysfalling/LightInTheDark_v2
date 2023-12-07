using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCell
{
    public enum Type { EMPTY, EDGE, CORNER }
    public Type type;

    WorldChunk chunkParent;
    int chunkIndex;
    public Vector3[] vertices; // Corners of the cell
    public Vector3 position;     // Center position of the cell


    public WorldCell(WorldChunk chunkParent, int index, Vector3[] vertices)
    {
        this.vertices = vertices;
        this.chunkIndex = index;
        this.chunkParent = chunkParent;
        position = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
    }

    public void SetCellType(Type type)
    {
        this.type = type;
    }
}

