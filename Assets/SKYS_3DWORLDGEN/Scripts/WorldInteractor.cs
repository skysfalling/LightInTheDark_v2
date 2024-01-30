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

    [Header("World Cursor")]
    public Transform worldCursor; // related transform to the cursor
    public WorldCell currCursorCell = null;

    public void Start()
    {
        _worldGeneration = WorldGeneration.Instance;
        _worldCellMap = WorldCellMap.Instance;
    }

    public void SelectClosestCell(Vector3 worldPos)
    {
        currCursorCell = _worldCellMap.FindClosestCell(worldPos);
        currCursorCell.ShowDebugCube();

        worldCursor.position = currCursorCell.position;

        //Debug.Log("Selected cell " + currCursorCell.position);

    }
}
