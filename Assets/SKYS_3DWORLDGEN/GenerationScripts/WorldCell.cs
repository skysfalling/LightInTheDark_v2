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

    [HideInInspector] public float astar_fCost;
    [HideInInspector] public float astar_gCost;
    [HideInInspector] public float astar_hCost;
    [HideInInspector] public WorldCell astar_parent;

    GameObject _debugCubeObject;
    float _defaultRelativeScale = 0.25f;
    float _debugCubeRelativeScale = 0.25f; // percentage of the WorldGeneration cellSize
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
        float relativeSize = _generation.cellSize * _debugCubeRelativeScale;

        this._debugCubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this._debugCubeObject.transform.parent = WorldCellMap.Instance.transform;
        this._debugCubeObject.transform.position = position + (Vector3.up * relativeSize * 0.5f); // adjust height offset
        this._debugCubeObject.transform.localScale = Vector3.one * relativeSize; // adjust scale
        this._debugCubeObject.GetComponent<MeshRenderer>().material = _materialLibrary.GetMaterialOfCellType(type); // set material
    }

    public GameObject GetDebugCube()
    {
        if (this._debugCubeObject != null) { return this._debugCubeObject; }
        else { return null; }
    }

    public void ShowDebugCube()
    {
        if (this._debugCubeObject == null) { CreateDebugCube(); }
        this._debugCubeObject.SetActive(true);
        this._debugCubeObject.GetComponent<MeshRenderer>().material = _materialLibrary.GetMaterialOfCellType(type); // set material
    }

    // Note : Cannot Destroy debugCube from non - MonoBehaviour class
    public void HideDebugCube()
    {
        if (this._debugCubeObject == null) { return; }
        this._debugCubeObject.SetActive(false);
        ResetDebugRelativeScale();
    }

    public void RemoveDebugCube()
    {
        this._debugCubeObject = null;
        ResetDebugRelativeScale();
    }

    public void SetDebugRelativeScale(float relativeScale)
    {
        _debugCubeRelativeScale = relativeScale;
    }

    public void ResetDebugRelativeScale()
    {
        _debugCubeRelativeScale = _defaultRelativeScale;
    }
}

