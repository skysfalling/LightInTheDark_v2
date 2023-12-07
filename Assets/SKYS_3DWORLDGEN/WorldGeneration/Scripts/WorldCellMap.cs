 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCellMap : MonoBehaviour
{
    
    WorldGeneration worldGeneration;
    List<WorldCell> worldCells = new List<WorldCell>();
    Dictionary<WorldCell, List<WorldCell>> worldCellMap = new Dictionary<WorldCell, List<WorldCell>>();

    List<GameObject> generatedWallPrefabs = new List<GameObject>();
    public GameObject wallPrefab;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = GetComponentInParent<WorldGeneration>();
    }

    public void InitializeCellMap()
    {
        worldCells = worldGeneration.GetCells();
        worldCellMap.Clear();

        // SET CELL NEIGHBORS
        foreach (WorldCell cell in worldCells)
        {
            List<WorldCell> neighbors = GetCellNeighbors(cell);
            worldCellMap[cell] = neighbors;
        }

        // SET CELL TYPES
        foreach (WorldCell cell in worldCells)
        {
            SetCellType(cell);
        }

        // SPAWN ASSETS
        foreach (WorldCell cell in worldCells)
        {
            if (cell.type != WorldCell.Type.EMPTY)
            {
                GameObject newAsset = Instantiate(wallPrefab, cell.position, Quaternion.identity);
                newAsset.transform.parent = worldGeneration._worldGenerationObject.transform;
            }
        }
    }

    private List<WorldCell> GetCellNeighbors(WorldCell cell)
    {
        List<WorldCell> neighbors = new List<WorldCell>(new WorldCell[4]);
        float cellSize = worldGeneration.cellSize; // Assuming 'cellSize' is a public field in WorldGeneration

        // Calculate neighbor positions
        Vector3 leftPosition = cell.position + new Vector3(-cellSize, 0, 0);
        Vector3 rightPosition = cell.position + new Vector3(cellSize, 0, 0);
        Vector3 forwardPosition = cell.position + new Vector3(0, 0, cellSize);
        Vector3 backwardPosition = cell.position + new Vector3(0, 0, -cellSize);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = worldCells.Find(c => c.position == leftPosition);     // Left
        neighbors[1] = worldCells.Find(c => c.position == rightPosition);    // Right
        neighbors[2] = worldCells.Find(c => c.position == forwardPosition);  // Forward
        neighbors[3] = worldCells.Find(c => c.position == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    private WorldCell.Type SetCellType(WorldCell cell)
    {
        WorldCell.Type cellType = WorldCell.Type.EMPTY;

        // CHECK FOR EDGE
        if (worldCellMap[cell].Count < 4)
        {
            cellType = WorldCell.Type.EDGE;
        }
        // EDGE CORNERS
        else if (worldCellMap[cell].Count == 4)
        {
            // Count how many neighbors are also edges
            int edgeNeighborCount = 0;
            foreach (WorldCell neighbor in worldCellMap[cell])
            {
                if (worldCellMap[neighbor].Count < 4)
                {
                    edgeNeighborCount++;
                }
            }

            // If at least two neighbors are edges, it's an edge corner
            if (edgeNeighborCount >= 2)
            {
                cellType = WorldCell.Type.CORNER;
            }
        }

        // SET TYPE
        cell.type = cellType;

        return cellType;
    }

    public WorldCell FindClosestCell(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldCell closestCell = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldCell cell in worldGeneration.GetCells())
        {
            float distance = Vector3.Distance(position, cell.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestCell = cell;
            }
        }

        if (closestCell != null)
        {
            Debug.Log("Closest cell found at: " + closestCell.position);
            return closestCell;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (WorldCell cell in worldCells)
        {
            Gizmos.color = Color.white;

            if (cell.type == WorldCell.Type.EDGE) { Gizmos.color = Color.red; }
            if (cell.type == WorldCell.Type.CORNER) { Gizmos.color = Color.yellow; }



            Gizmos.DrawCube(cell.position, Vector3.one);
        }
    }

}
