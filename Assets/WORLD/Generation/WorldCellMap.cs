using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCellMap : MonoBehaviour
{
    
    WorldGeneration worldGeneration;
    List<WorldGeneration.Chunk> worldChunks = new List<WorldGeneration.Chunk>();
    List<WorldGeneration.Cell> worldCells = new List<WorldGeneration.Cell>();
    Dictionary<WorldGeneration.Cell, List<WorldGeneration.Cell>> worldCellMap = new Dictionary<WorldGeneration.Cell, List<WorldGeneration.Cell>>();

    List<GameObject> generatedWallPrefabs = new List<GameObject>();
    public GameObject wallPrefab;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = GetComponentInParent<WorldGeneration>();
    }

    public void InitializeCellMap()
    {
        worldChunks = worldGeneration.GetChunks();
        worldCells = worldGeneration.GetCells();
        worldCellMap.Clear();

        // SET CELL NEIGHBORS
        foreach (WorldGeneration.Cell cell in worldCells)
        {
            List<WorldGeneration.Cell> neighbors = GetCellNeighbors(cell);
            worldCellMap[cell] = neighbors;
        }

        // SET CELL TYPES
        foreach (WorldGeneration.Cell cell in worldCells)
        {
            SetCellType(cell);
        }

        // SPAWN ASSETS
        foreach (WorldGeneration.Cell cell in worldCells)
        {
            if (cell.type != WorldGeneration.Cell.Type.EMPTY)
            {
                GameObject newAsset = Instantiate(wallPrefab, cell.position, Quaternion.identity);
                newAsset.transform.parent = worldGeneration._worldGenerationObject.transform;
            }
        }
    }

    private List<WorldGeneration.Cell> GetCellNeighbors(WorldGeneration.Cell cell)
    {
        List<WorldGeneration.Cell> neighbors = new List<WorldGeneration.Cell>(new WorldGeneration.Cell[4]);
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

    private WorldGeneration.Cell.Type SetCellType(WorldGeneration.Cell cell)
    {
        WorldGeneration.Cell.Type cellType = WorldGeneration.Cell.Type.EMPTY;

        // CHECK FOR EDGE
        if (worldCellMap[cell].Count < 4)
        {
            cellType = WorldGeneration.Cell.Type.EDGE;
        }
        // EDGE CORNERS
        else if (worldCellMap[cell].Count == 4)
        {
            // Count how many neighbors are also edges
            int edgeNeighborCount = 0;
            foreach (WorldGeneration.Cell neighbor in worldCellMap[cell])
            {
                if (worldCellMap[neighbor].Count < 4)
                {
                    edgeNeighborCount++;
                }
            }

            // If at least two neighbors are edges, it's an edge corner
            if (edgeNeighborCount >= 2)
            {
                cellType = WorldGeneration.Cell.Type.CORNER;
            }
        }

        // SET TYPE
        cell.type = cellType;

        return cellType;
    }

    public WorldGeneration.Cell FindClosestCell(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldGeneration.Cell closestCell = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldGeneration.Cell cell in worldGeneration.GetCells())
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

    private void OnDrawGizmos()
    {
        foreach (WorldGeneration.Cell cell in worldCells)
        {
            Gizmos.color = Color.white;

            if (cell.type == WorldGeneration.Cell.Type.EDGE) { Gizmos.color = Color.red; }
            if (cell.type == WorldGeneration.Cell.Type.CORNER) { Gizmos.color = Color.yellow; }



            Gizmos.DrawCube(cell.position, Vector3.one);
        }
    }
}
