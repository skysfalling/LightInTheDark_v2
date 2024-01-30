using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPathfinder : MonoBehaviour
{
    WorldCellMap worldCellMap;
    [HideInInspector] public WorldCell startCell;
    [HideInInspector] public WorldCell endCell;

    private void Start()
    {
    }

    public void SelectClosestCellAsStart(Vector3 worldPosition)
    {
        worldCellMap = FindObjectOfType<WorldCellMap>();
        startCell = worldCellMap.FindClosestCell(worldPosition);
    }

    public void SelectClosestCellAsEnd(Vector3 worldPosition)
    {
        worldCellMap = FindObjectOfType<WorldCellMap>();
        endCell = worldCellMap.FindClosestCell(worldPosition);
    }
}
