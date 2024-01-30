 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCellMap : MonoBehaviour
{
    public static WorldCellMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public bool initialized = false;
    WorldGeneration _worldGeneration;
    WorldPathfinder _worldPathfinder;
    List<WorldCell> _worldCells = new List<WorldCell>();
    Dictionary<WorldCell, List<WorldCell>> _cellNeighborMap = new Dictionary<WorldCell, List<WorldCell>>();
    public void InitializeCellMap()
    {
        initialized = false;

        _worldGeneration = WorldGeneration.Instance;
        _worldPathfinder = _worldGeneration.GetComponent<WorldPathfinder>();
        _worldCells = _worldGeneration.GetCells();
        _cellNeighborMap.Clear();

        // SET CELL NEIGHBORS
        foreach (WorldCell cell in _worldCells)
        {
            List<WorldCell> neighbors = SetCellNeighbors(cell);
            _cellNeighborMap[cell] = neighbors;
        }

        // SET CELL TYPES
        foreach (WorldCell cell in _worldCells)
        {
            SetCellType(cell);
        }

        initialized = true;
    }

    public void Reset()
    {
        _worldCells.Clear();
        _cellNeighborMap.Clear();
        initialized = false;
    }

    public void ClearAllCellDebugs()
    {
        foreach (WorldCell cell in _worldCells)
        {
            Destroy(cell.GetDebugCube());
            cell.RemoveDebugCube();
        }
    }

    public void ClearCellPathDebugs(List<WorldCell> path)
    {
        foreach (WorldCell cell in _worldCells)
        {
            Destroy(cell.GetDebugCube());
            cell.RemoveDebugCube();
        }
    }

    #region === INDIVIDUAL CELLS ======================================================..///
    private WorldCell.TYPE SetCellType(WorldCell cell)
    {
        WorldCell.TYPE cellType = WorldCell.TYPE.EMPTY;

        // CHECK FOR EDGE
        if (_cellNeighborMap[cell].Count < 4)
        {
            cellType = WorldCell.TYPE.EDGE;
        }
        // EDGE CORNERS
        else if (_cellNeighborMap[cell].Count == 4)
        {
            // Count how many neighbors are also edges
            int edgeNeighborCount = 0;
            foreach (WorldCell neighbor in _cellNeighborMap[cell])
            {
                if (_cellNeighborMap[neighbor].Count < 4)
                {
                    edgeNeighborCount++;
                }
            }

            // If at least two neighbors are edges, it's an edge corner
            if (edgeNeighborCount >= 2)
            {
                cellType = WorldCell.TYPE.CORNER;
            }
        }

        // SET TYPE
        cell.SetCellType(cellType);

        return cellType;
    }

    private List<WorldCell> SetCellNeighbors(WorldCell cell)
    {
        List<WorldCell> neighbors = new List<WorldCell>(new WorldCell[4]);
        float cellSize = _worldGeneration.cellSize; // Assuming 'cellSize' is a public field in WorldGeneration

        // Calculate neighbor positions
        Vector3 leftPosition = cell.position + new Vector3(-cellSize, 0, 0);
        Vector3 rightPosition = cell.position + new Vector3(cellSize, 0, 0);
        Vector3 forwardPosition = cell.position + new Vector3(0, 0, cellSize);
        Vector3 backwardPosition = cell.position + new Vector3(0, 0, -cellSize);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldCells.Find(c => c.position == leftPosition);     // Left
        neighbors[1] = _worldCells.Find(c => c.position == rightPosition);    // Right
        neighbors[2] = _worldCells.Find(c => c.position == forwardPosition);  // Forward
        neighbors[3] = _worldCells.Find(c => c.position == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public List<WorldCell> GetCellNeighbors(WorldCell cell)
    {
        if (!initialized) return new List<WorldCell>();
        return _cellNeighborMap[cell];
    }

    public WorldCell FindClosestCellTo(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldCell closestCell = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldCell cell in _worldGeneration.GetCells())
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
            //Debug.Log("Closest cell found at: " + closestCell.position);
            return closestCell;
        }

        return null;
    }

    public float GetDistance(WorldCell cellA, WorldCell cellB)
    {
        // Implement the heuristic. Here's an example using Euclidean distance
        float distX = Mathf.Abs(cellA.position.x - cellB.position.x);
        float distY = Mathf.Abs(cellA.position.y - cellB.position.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
    #endregion


    #region === CELL PATHS ==================================================..//
    public List<WorldCell> GetPath(WorldCell cellStart,  WorldCell cellEnd)
    {
        return _worldPathfinder.FindPath(cellStart, cellEnd);
    }

    public void DrawPath(List<WorldCell> path)
    {
        foreach (WorldCell cell in path)
        {
            cell.ShowDebugCube();
        }
    }
    #endregion


    #region === CELL GROUPS ==================================================..//


    #endregion
}
