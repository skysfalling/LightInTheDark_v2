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


[System.Serializable]
public class WorldCell
{
    public enum TYPE { EMPTY, EDGE, CORNER, OBSTACLE, SPAWN_POINT}
    public TYPE type = WorldCell.TYPE.EMPTY;

    WorldGeneration _generation;
    WorldChunk _chunkParent;
    int _chunkCellIndex;
    WorldMaterialLibrary _materialLibrary;

    GameObject _debugCubeObject;
    float _debugCubeRelativeScale = 0.75f; // percentage of the WorldGeneration cellSize
    public Vector3[] vertices; // Corners of the cell
    public Vector3 position;     // Center position of the cell

    public WorldCell(WorldChunk chunkParent, int chunkCellIndex, Vector3[] vertices)
    {
        this._generation = WorldGeneration.Instance;
        this._chunkParent = chunkParent;
        this._chunkCellIndex = chunkCellIndex;
        this._materialLibrary = WorldMaterialLibrary.Instance;

        this.vertices = vertices;
        position = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
    }

    public void SetCellType(TYPE type)
    {
        this.type = type;
    }

    public WorldChunk GetChunk()
    {
        return this._chunkParent;
    }

    public void CreateDebugCube()
    {
        _debugCubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _debugCubeObject.transform.position = position + (Vector3.up * _generation.cellSize);
        _debugCubeObject.transform.localScale = Vector3.one * (_generation.cellSize * _debugCubeRelativeScale);
        _debugCubeObject.GetComponent<MeshRenderer>().material = _materialLibrary.GetMaterialOfCellType(type);
    }

    public void ShowDebugCube()
    {
        if (this._debugCubeObject == null) { CreateDebugCube(); }
        this._debugCubeObject.SetActive(true);
    }

    public void HideDebugCube()
    {
        if (this._debugCubeObject == null) { CreateDebugCube(); }
        this._debugCubeObject.SetActive(false);
    }
}

