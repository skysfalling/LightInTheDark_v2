using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// Handles all player interactions.
/// </summary>

public class WorldInteractor : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    WorldCellMap _worldCellMap;
    WorldChunkMap _worldChunkMap;

    [Header("World Cursor")]
    public Transform worldCursor; // related transform to the cursor
    public WorldCell currCursorCell = null;

    public void Start()
    {
        _worldGeneration = WorldGeneration.Instance;
        _worldCellMap = WorldCellMap.Instance;
        _worldChunkMap = WorldChunkMap.Instance;
    }

    public void SelectClosestCell(Vector3 worldPos)
    {
        // Hide previous chunk parent
        if (currCursorCell != null)
        {
            _worldChunkMap.HideChunkCells(currCursorCell.GetChunk());
        }

        currCursorCell = _worldCellMap.FindClosestCell(worldPos);
        currCursorCell.SetDebugRelativeScale(1);

        _worldChunkMap.ShowChunkCells(currCursorCell.GetChunk());

        worldCursor.position = currCursorCell.position;

        //Debug.Log("Selected cell " + currCursorCell.position);

    }
}
