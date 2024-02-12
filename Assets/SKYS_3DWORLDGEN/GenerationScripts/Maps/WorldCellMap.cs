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
    Dictionary<WorldCell, List<WorldCell>> _cellFullNeighborMap = new Dictionary<WorldCell, List<WorldCell>>();
    Dictionary<WorldCell, List<WorldCell>> _cellNaturalNeighborMap = new Dictionary<WorldCell, List<WorldCell>>();


    public void InitializeCellMap()
    {
        initialized = false;

        _worldGeneration = WorldGeneration.Instance;
        _worldPathfinder = _worldGeneration.GetComponent<WorldPathfinder>();
        //_worldCells = _worldGeneration.GetCells();
        _cellFullNeighborMap.Clear();

        // SET CELL NEIGHBORS
        foreach (WorldCell cell in _worldCells)
        {
            SetCellNeighbors(cell);
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
        _cellFullNeighborMap.Clear();
        initialized = false;
        Debug_DestroyWorldCells();
    }

    #region == Debug Management ========================================== ///

    // =========== SHOW CELL DEBUGS =============================================== >>>>>>>>>>

    public void Debug_ShowCellList(List<WorldCell> list)
    {
        foreach (WorldCell cell in list)
        {
            cell.ShowDebugCube();
        }
    }

    public void Debug_ShowCellNeighbors(WorldCell cell)
    {
        if (cell == null || initialized != true) { return; }
        Debug_ShowCellList(GetAllCellNeighbors(cell));
    }

    public void Debug_ShowChunkLocalCells(WorldChunk chunk)
    {
        if (chunk == null || initialized != true) { return; }
        Debug_ShowCellList(chunk.localCells);
    }

    // =========== DESTROY CELL DEBUGS =============================================== >>>>>>>>>>

    public void Debug_DestroyWorldCells()
    {
        foreach (WorldCell cell in _worldCells)
        {
            Debug_DestroyCell(cell);
        }
    }

    public void Debug_DestroyCellList(List<WorldCell> list)
    {
        foreach (WorldCell cell in list)
        {
            Debug_DestroyCell(cell);
        }
    }

    public void Debug_DestroyChunkLocalCells(WorldChunk chunk)
    {
        Debug_DestroyCellList(chunk.localCells);
    }

    public void Debug_DestroyCellNeighbors(WorldCell cell)
    {
        Debug_DestroyCell(cell);
        Debug_DestroyCellList(GetAllCellNeighbors(cell));
    }

    public void Debug_DestroyCell(WorldCell cell)
    {
        Destroy(cell.GetDebugCube());
        cell.RemoveDebugCube();
    }
    #endregion

    #region === INDIVIDUAL CELL MANAGEMENT ======================================================..///
    private WorldCell.TYPE SetCellType(WorldCell cell)
    {
        WorldCell.TYPE cellType = WorldCell.TYPE.EMPTY;

        // CHECK FOR EDGE
        if (_cellNaturalNeighborMap[cell].Count < 4)
        {
            cellType = WorldCell.TYPE.EDGE;
        }
        // CHECK FOR EDGE CORNERS
        else if (_cellNaturalNeighborMap[cell].Count == 4)
        {
            // Count how many neighbors are also edges
            int edgeNeighborCount = 0;
            foreach (WorldCell neighbor in _cellNaturalNeighborMap[cell])
            {
                if (_cellNaturalNeighborMap[neighbor].Count < 4)
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

    private void SetCellNeighbors(WorldCell cell)
    {
        List<WorldCell> naturalNeighbors = new List<WorldCell>(new WorldCell[4]);
        List<WorldCell> diagonalNeighbors = new List<WorldCell>(new WorldCell[4]);

        float cellSize = WorldGeneration.CellSize; // Assuming 'cellSize' is a public field in WorldGeneration

        // Calculate natural neighbor positions
        Vector3 leftPosition = cell.position + new Vector3(-cellSize, 0, 0);
        Vector3 rightPosition = cell.position + new Vector3(cellSize, 0, 0);
        Vector3 forwardPosition = cell.position + new Vector3(0, 0, cellSize);
        Vector3 backwardPosition = cell.position + new Vector3(0, 0, -cellSize);
        naturalNeighbors[0] = _worldCells.Find(c => c.position == leftPosition);        // Left
        naturalNeighbors[1] = _worldCells.Find(c => c.position == rightPosition);       // Right
        naturalNeighbors[2] = _worldCells.Find(c => c.position == forwardPosition);     // Forward
        naturalNeighbors[3] = _worldCells.Find(c => c.position == backwardPosition);    // Backward


        // Calculate diagonal neighbor positions
        Vector3 forwardLeftPosition = cell.position + new Vector3(-cellSize, 0, cellSize);
        Vector3 forwardRightPosition = cell.position + new Vector3(cellSize, 0, cellSize);
        Vector3 backwardLeftPosition = cell.position + new Vector3(-cellSize, 0, -cellSize);
        Vector3 backwardRightPosition = cell.position + new Vector3(cellSize, 0, -cellSize);
        diagonalNeighbors[0] = _worldCells.Find(c => c.position == forwardLeftPosition); // Forward-Left
        diagonalNeighbors[1] = _worldCells.Find(c => c.position == forwardRightPosition);// Forward-Right
        diagonalNeighbors[2] = _worldCells.Find(c => c.position == backwardLeftPosition);// Backward-Left
        diagonalNeighbors[3] = _worldCells.Find(c => c.position == backwardRightPosition);// Backward-Right

        // Remove null entries if a neighbor is not found
        naturalNeighbors.RemoveAll(item => item == null);
        diagonalNeighbors.RemoveAll(item => item == null);

        // Store Neighbors in map
        _cellNaturalNeighborMap[cell] = naturalNeighbors;
        _cellFullNeighborMap[cell] = new List<WorldCell>(naturalNeighbors);
        _cellFullNeighborMap[cell].AddRange(diagonalNeighbors);
    }

    public List<WorldCell> GetAllCellNeighbors(WorldCell cell)
    {
        if (!initialized || !_cellFullNeighborMap.ContainsKey(cell)) return new List<WorldCell>();
        return _cellFullNeighborMap[cell];
    }

    public List<WorldCell> GetNaturalCellNeighbors(WorldCell cell)
    {
        if (!initialized || !_cellNaturalNeighborMap.ContainsKey(cell)) return new List<WorldCell>();
        return _cellNaturalNeighborMap[cell];
    }

    public WorldCell FindClosestCellTo(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldCell closestCell = null;

        /*
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
        */

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


    #endregion


    #region === CELL GROUPS ==================================================..//

    #endregion
}
