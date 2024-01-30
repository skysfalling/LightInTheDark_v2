using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPathfinder : MonoBehaviour
{
    WorldCellMap _worldCellMap;
    public void Start()
    {
        _worldCellMap = WorldCellMap.Instance;
    }

    // A* Pathfinding implementation
    public List<WorldCell> FindPath(WorldCell startCell, WorldCell endCell)
    {
        // The set of nodes to be evaluated
        List<WorldCell> openSet = new List<WorldCell>();
        // Nodes already evaluated
        HashSet<WorldCell> closedSet = new HashSet<WorldCell>();
        // Start by adding the start cell to the open set
        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            WorldCell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                // Check if this path to neighbor is better than the one previously known.
                // This is where you might check for things like terrain costs, etc.
                if (openSet[i].astar_fCost < currentCell.astar_fCost || openSet[i].astar_fCost == currentCell.astar_fCost && openSet[i].astar_hCost < currentCell.astar_hCost)
                {
                    currentCell = openSet[i];
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            if (currentCell == endCell)
            {
                // We found the path, retrace steps from endCell to startCell
                return RetracePath(startCell, endCell);
            }

            foreach (WorldCell neighbor in _worldCellMap.GetCellNeighbors(currentCell))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCostToNeighbor = currentCell.astar_gCost + _worldCellMap.GetDistance(currentCell, neighbor);
                if (newMovementCostToNeighbor < neighbor.astar_gCost || !openSet.Contains(neighbor))
                {
                    neighbor.astar_gCost = newMovementCostToNeighbor;
                    neighbor.astar_hCost = _worldCellMap.GetDistance(neighbor, endCell);
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
