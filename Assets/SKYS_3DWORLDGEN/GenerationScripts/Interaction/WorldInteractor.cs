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
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;
    WorldEnvironment _worldEnvironment;

    [Header("World Cursor")]
    public Transform worldCursor; // related transform to the cursor
    public WorldCell currCursorCell = null;

    [Header("Select Entity")]
    public Entity selectedEntity;

    public void Start()
    {
        _worldGeneration = WorldGeneration.Instance;
        //_worldCellMap = WorldCellMap.Instance;
        _worldSpawnMap = WorldSpawnMap.Instance;
        _worldEnvironment = WorldEnvironment.Instance;
    }

}
