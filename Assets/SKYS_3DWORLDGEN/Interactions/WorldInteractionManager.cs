using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInteractionManager : MonoBehaviour
{
    WorldGeneration worldGeneration;
    WorldCellMap cellMap;
    public Transform worldCursor;
    public WorldCell selectedCell = null;

    public void Start()
    {
        worldGeneration = FindObjectOfType<WorldGeneration>();
        cellMap = FindObjectOfType<WorldCellMap>();
    }

    public void SelectClosestCell(Vector3 worldPos)
    {
        selectedCell = cellMap.FindClosestCell(worldPos);
        Debug.Log("Selected cell " + selectedCell.position);
        worldCursor.position = selectedCell.position;
    }
}
