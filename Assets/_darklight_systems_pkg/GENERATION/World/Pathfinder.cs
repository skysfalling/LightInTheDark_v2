using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight.ThirdDimensional.World
{
    public class Pathfinder
    {
        public Pathfinder() { }

        public static List<Vector2Int> FindPath(CoordinateMap coordinateMap, Vector2Int startCoord, Vector2Int endCoord, List<Coordinate.TYPE> validTypes,  float pathRandomness = 0)
        {
            // A* Pathfinding implementation
            // gCost is the known cost from the starting node
            // hCost is the estimated distance to the end node
            // fCost is gCost + hCost


            // Helper function to calculate validity of values
            bool IsCoordinateValidForPathfinding(Vector2Int candidate)
            {
                // Check Types
                if (coordinateMap.AllCoordinateValues.Contains(candidate))
                {
                    Coordinate.TYPE candidateType = (Coordinate.TYPE)coordinateMap.GetCoordinateTypeAt(candidate);
                    if (validTypes.Contains(candidateType))
                    {
                        return true;
                    }
                }
                return false;
            }

            // Initialize Random Seed :: IMPORTANT To keep the same results per seed
            WorldGeneration.InitializeSeedRandom();

            // Store all possible positions from the coordinate map
            List<Vector2Int> positions = coordinateMap.AllCoordinateValues;
            // Initialize the open set with the start coordinate
            List<Vector2Int> openSet = new List<Vector2Int> { startCoord };
            // Initialize the closed set as an empty collection of Vector2Int
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

            // Initialize costs for all coordinates to infinity, except the start coordinate
            Dictionary<Vector2Int, float> gCost = new Dictionary<Vector2Int, float>();
            Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
            foreach (Vector2Int pos in coordinateMap.AllCoordinateValues)
            {
                gCost[pos] = float.MaxValue;
            }
            gCost[startCoord] = 0;

            // Initialize the heuristic costs
            Dictionary<Vector2Int, float> fCost = new Dictionary<Vector2Int, float>();
            foreach (Vector2Int pos in positions)
            {
                fCost[pos] = float.MaxValue;
            }
            fCost[startCoord] = Vector2Int.Distance(startCoord, endCoord);

            while (openSet.Count > 0)
            {
                Vector2Int current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    Vector2Int candidate = openSet[i];
                    // Convert FCost and HCost checks to work with Vector2Int by accessing WorldCoordinate properties
                    if (fCost[candidate] <= fCost[current] && UnityEngine.Random.Range(0f, 1f) <= pathRandomness) // Apply randomness
                    {
                        current = openSet[i];
                    }
                }

                if (current == endCoord)
                {
                    // Path has been found
                    return RetracePath(startCoord, endCoord, parents);
                }

                openSet.Remove(current);
                closedSet.Add(current);


                // [[ ITERATE THROUGH NATURAL NEIGHBORS ]]
                foreach (Vector2Int pos in CoordinateMap.CalculateNaturalNeighborCoordinateValues(current))
                {
                    if (closedSet.Contains(pos) || IsCoordinateValidForPathfinding(pos) == false)
                        continue; // Skip non-traversable neighbors and those already evaluated

                    float tentativeGCost = gCost[current] + Vector2Int.Distance(current, pos);

                    if (tentativeGCost < gCost[pos])
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        parents[pos] = current;
                        gCost[pos] = tentativeGCost;
                        fCost[pos] = tentativeGCost + Vector2Int.Distance(pos, endCoord);

                        if (!openSet.Contains(pos))
                            openSet.Add(pos);
                    }
                }
            }

            // If we reach here, then there is no path
            return new List<Vector2Int>();
        }

        // Helper method to retrace path from end to start using parent references
        static List<Vector2Int> RetracePath(Vector2Int startCoord, Vector2Int endCoord, Dictionary<Vector2Int, Vector2Int> parents)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int currentCoord = endCoord;

            while (currentCoord != startCoord)
            {
                path.Add(currentCoord);
                currentCoord = parents[currentCoord]; // Move to the parent coordinate
            }
            path.Add(startCoord); // Add the start coordinate at the end
            path.Reverse(); // Reverse the list to start from the beginning

            return path;
        }




    }
}
