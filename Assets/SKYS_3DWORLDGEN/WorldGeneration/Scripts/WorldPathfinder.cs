using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPathfinder : MonoBehaviour
{
    WorldCellMap worldCellMap;
    public WorldCell startCell;
    public WorldCell endCell;

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

    private void OnDrawGizmos()
    {
        if (startCell != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(startCell.position, Vector3.one);
        }

        if (endCell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(endCell.position, Vector3.one);
        }
    }
}
