using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPathfinder : MonoBehaviour
{
    public static WorldPathfinder Instance;
    public void Awake()
    {
        Instance = this;
    }

    WorldCellMap _worldCellMap;


    public void Start()
    {
        _worldCellMap = WorldCellMap.Instance;
    }

    // A* Pathfinding implementation
    // - gCost is the known cost from the starting node
    // - hCost is the estimated distance to the end node
    // - fCost is gCost + hCost

    public List<WorldCell> FindPath(WorldCell startCell, WorldCell endCell)
    {
        // INITIALIZE SETS
        List<WorldCell> openSet = new List<WorldCell>();
        HashSet<WorldCell> closedSet = new HashSet<WorldCell>();
        openSet.Add(startCell);

        while (openSet.Count > 0)
        {

            // << GET BEST OPTION OF OPEN SET >>
            WorldCell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].astar_fCost <= currentCell.astar_fCost) { continue; }
                if (openSet[i].astar_hCost < currentCell.astar_hCost ) { continue; }

                currentCell = openSet[i];
            }

            // << ADD VALID CELL TO CLOSED SET >>
            openSet.Remove(currentCell);
            closedSet.Add(currentCell);
            if (currentCell == endCell)
            {
                // We found the path, retrace steps from endCell to startCell
                return RetracePath(startCell, endCell);
            }

            // << GET ALL NEIGHBORS OF CURRENT CELL >>
            foreach (WorldCell neighbor in _worldCellMap.GetCellNeighbors(currentCell))
            {
                // Skip invalid neighbors of current cell
                if (closedSet.Contains(neighbor) || neighbor.type != WorldCell.TYPE.EMPTY)
                {
                    continue;
                }

                // Set movement cost for valid neighbor
                float newMovementCostToNeighbor = currentCell.astar_gCost + _worldCellMap.GetDistance(currentCell, neighbor);
                if (newMovementCostToNeighbor < neighbor.astar_gCost || !openSet.Contains(neighbor))
                {
                    neighbor.astar_gCost = newMovementCostToNeighbor; // cost from starting node
                    neighbor.astar_hCost = _worldCellMap.GetDistance(neighbor, endCell); // distance from end node
                    neighbor.astar_parent = currentCell;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<WorldCell>(); // Return an empty path if there is no path
    }

    List<WorldCell> RetracePath(WorldCell startCell, WorldCell endCell)
    {
        List<WorldCell> path = new List<WorldCell>();
        WorldCell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.astar_parent;
        }
        path.Reverse(); // The path is constructed from endCell to startCell, so we reverse it
        return path;
    }

}
