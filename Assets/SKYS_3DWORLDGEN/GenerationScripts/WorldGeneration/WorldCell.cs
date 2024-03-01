using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Represents the smallest unit of the game world, encapsulating terrain,
/// environmental, and gameplay-related data. Functions as a building block
/// for the larger world map, enabling detailed and scalable world design.
/// </summary>

public class WorldCell
{
    public enum TYPE { EMPTY, EDGE, CORNER, OBSTACLE, SPAWN_POINT}
    public TYPE type = WorldCell.TYPE.EMPTY;

    WorldGeneration _generation;
    int _chunkCellIndex;
    WorldMaterialLibrary _materialLibrary;

    public WorldChunk chunkParent { get; private set; }
    public MeshQuad meshQuad { get; private set; }

    GameObject _debugCubeObject;
    float _defaultRelativeScale = 0.25f;
    float _debugCubeRelativeScale = 0.25f; // percentage of the WorldGeneration cellSize

    public Vector3[] vertices; // Corners of the cell  
    public Vector3 worldPosition;     // Center position of the cell
    public Vector3 normal; // Normal Direction of the cell

    public WorldCell(WorldChunk chunkParent, MeshQuad meshQuad)
    {
        this._generation = WorldGeneration.Instance;
        this.chunkParent = chunkParent;
        this._materialLibrary = WorldMaterialLibrary.Instance;
        this.meshQuad = meshQuad;

        // Set Position [[ parent position offset + center of corresponding quad ]]
        this.worldPosition = this.chunkParent.GetGroundWorldPosition() + meshQuad.GetCenterPosition();
        this.normal = meshQuad.faceNormal;
    }

    public void SetCellType(TYPE type)
    {
        this.type = type;
    }

    public WorldChunk GetChunk()
    {
        return this.chunkParent;
    }
}

