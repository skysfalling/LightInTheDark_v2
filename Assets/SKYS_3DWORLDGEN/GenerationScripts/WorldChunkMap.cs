 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    public static WorldChunkMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public bool initialized = false;
    WorldGeneration _worldGen;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    Dictionary<WorldChunk, List<WorldChunk>> _chunkNeighborMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    public void InitializeChunkMap()
    {
        initialized = false;

        _worldGen = GetComponentInParent<WorldGeneration>();
        _worldChunks = _worldGen.GetChunks();
        _chunkNeighborMap.Clear();

        // << Initialize Chunks >>
        foreach (WorldChunk chunk in _worldChunks)
        {
            // Set Neighbors
            List<WorldChunk> neighbors = SetChunkNeighbors(chunk);
            _chunkNeighborMap[chunk] = neighbors;

            // Initialize
            chunk.Initialize();
        }

        /*
        // << Find Golden Path >>
        if (_worldGen.worldExits.Count > 1)
        {
            List<WorldChunk> pathA = FindCoordinatePath(_worldGen.worldExits[0], _worldGen.worldExits[1]);
            if (pathA.Count > 1) { foreach (WorldChunk chunk in pathA) { chunk.isOnGoldenPath = true; } }
        }
        */


        initialized = true;
    }

    public void Reset()
    {
        _worldChunks.Clear();
        _chunkNeighborMap.Clear();
        initialized = false;
    }

    #region == INDIVIDUAL CHUNK NEIGHBORS ==============>>
    private List<WorldChunk> SetChunkNeighbors(WorldChunk chunk)
    {
        List<WorldChunk> neighbors = new List<WorldChunk>(new WorldChunk[4]);
        int chunkWidth = WorldGeneration.GetRealChunkAreaSize().x;
        int chunkLength = WorldGeneration.GetRealChunkAreaSize().y;

        // Calculate neighbor positions
        Vector2 leftPosition = chunk.coordinate + new Vector2(-chunkWidth, 0);
        Vector2 rightPosition = chunk.coordinate + new Vector2(chunkWidth, 0);
        Vector2 forwardPosition = chunk.coordinate + new Vector2(0, chunkLength);
        Vector2 backwardPosition = chunk.coordinate + new Vector2(0, -chunkLength);

        // Find and assign neighbors in the specific order [Left, Right, Forward, Backward]
        neighbors[0] = _worldChunks.Find(c => c.coordinate == leftPosition);     // Left
        neighbors[1] = _worldChunks.Find(c => c.coordinate == rightPosition);    // Right
        neighbors[2] = _worldChunks.Find(c => c.coordinate == forwardPosition);  // Forward
        neighbors[3] = _worldChunks.Find(c => c.coordinate == backwardPosition); // Backward

        // Remove null entries if a neighbor is not found
        neighbors.RemoveAll(item => item == null);

        return neighbors;
    }

    public List<WorldChunk> GetAllChunkNeighbors(WorldChunk chunk) 
    {
        if (!_chunkNeighborMap.ContainsKey(chunk)) { return new List<WorldChunk>(); }

        return _chunkNeighborMap[chunk];
    }
    #endregion

    #region == WORLD EDGE CHUNKS ==========================>>
    // Method to retrieve an edge chunk
    public WorldChunk GetEdgeChunk(WorldDirection edgeDirection, int index)
    {
        // This list will hold the chunks on the specified edge
        List<WorldChunk> edgeChunks = new List<WorldChunk>();

        // Identify chunks on the specified edge
        foreach (WorldChunk chunk in WorldGeneration.Instance.GetBorderChunks())
        {
            if (IsOnSpecifiedEdge(chunk, edgeDirection))
            {
                edgeChunks.Add(chunk);
            }
        }

        // Ensure the index is within bounds
        if (index >= 0 && index < edgeChunks.Count)
        {
            return edgeChunks[index];
        }

        // Return null or throw an exception if no chunk is found at the index
        return null;
    }

    private bool IsOnSpecifiedEdge(WorldChunk chunk, WorldDirection edgeDirection)
    {
        Vector2 halfSize_playArea = (Vector2)WorldGeneration.PlayZoneArea * 0.5f;

        // Define the boundaries for each direction
        float westBoundaryX = -halfSize_playArea.x;
        float eastBoundaryX = halfSize_playArea.x;
        float northBoundaryY = -halfSize_playArea.y;
        float southBoundaryY = halfSize_playArea.y;

        switch (edgeDirection)
        {
            case WorldDirection.West:
                return chunk.coordinate.x == westBoundaryX;
            case WorldDirection.East:
                return chunk.coordinate.x == eastBoundaryX;
            case WorldDirection.North:
                return chunk.coordinate.y == northBoundaryY;
            case WorldDirection.South:
                return chunk.coordinate.y == southBoundaryY;
            default:
                return false;
        }
    }
    #endregion

    #region == COORDINATE PATHFINDING =================================///
    // A* Pathfinding implementation
    // - gCost is the known cost from the starting node
    // - hCost is the estimated distance to the end node
    // - fCost is gCost + hCost

    public class Coordinate
    {
        public Vector2 Position { get; set; }
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        public Coordinate Parent { get; set; }

        public Coordinate(Vector2 position)
        {
            Position = position;
            GCost = float.MaxValue;
            HCost = 0;
            Parent = null;
        }
    }

    public List<Coordinate> FindCoordinatePath(Coordinate startCoordinate, Coordinate endCoordinate)
    {
        List<Coordinate> openSet = new List<Coordinate>();
        HashSet<Coordinate> closedSet = new HashSet<Coordinate>();
        openSet.Add(startCoordinate);

        while (openSet.Count > 0)
        {
            Coordinate currentCoordinate = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost <= currentCoordinate.FCost && openSet[i].HCost < currentCoordinate.HCost)
                {
                    currentCoordinate = openSet[i];
                }
            }

            openSet.Remove(currentCoordinate);
            closedSet.Add(currentCoordinate);

            if (currentCoordinate == endCoordinate)
            {
                return RetracePath(startCoordinate, endCoordinate);
            }

            foreach (Coordinate neighbor in GetAllCoordinateNeighbors(currentCoordinate))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCostToNeighbor = currentCoordinate.GCost + GetCoordinateDistance(currentCoordinate, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetCoordinateDistance(neighbor, endCoordinate);
                    neighbor.Parent = currentCoordinate;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Coordinate>(); // Return an empty path if there is no path
    }

    List<Coordinate> RetracePath(Coordinate startCoordinate, Coordinate endCoordinate)
    {
        List<Coordinate> path = new List<Coordinate>();
        Coordinate currentCoordinate = endCoordinate;

        while (currentCoordinate != startCoordinate)
        {
            path.Add(currentCoordinate);
            currentCoordinate = currentCoordinate.Parent;
        }
        path.Reverse();
        return path;
    }

    public List<Coordinate> GetAllCoordinateNeighbors(Coordinate coordinate)
    {
        List<Coordinate> neighbors = new List<Coordinate>();

        // List of potential neighbor positions (north, south, east, west)
        Vector2[] potentialNeighbors = {
        new Vector2(coordinate.Position.x, coordinate.Position.y + 1), // North
        new Vector2(coordinate.Position.x, coordinate.Position.y - 1), // South
        new Vector2(coordinate.Position.x + 1, coordinate.Position.y), // East
        new Vector2(coordinate.Position.x - 1, coordinate.Position.y)  // West
    };

        foreach (Vector2 neighborPos in potentialNeighbors)
        {
            // Here you would typically check if the neighbor is within the bounds of your world
            // and if it is traversable (not an obstacle or otherwise inaccessible)
            // For simplicity, we'll assume all generated positions are valid and add them directly
            neighbors.Add(new Coordinate(neighborPos));
        }

        return neighbors;
    }

    public float GetCoordinateDistance(Coordinate a, Coordinate b)
    {
        return Vector2.Distance(a.Position, b.Position);
    }

    public static Coordinate GetWorldExitCoordinate(WorldExit worldExit)
    {
        WorldDirection direction = worldExit.edgeDirection;
        int index = worldExit.edgeIndex;

        // Assuming these values are accessible in the current context
        int fullWorldAreaWidth = WorldGeneration.GetFullWorldArea().x;
        int fullWorldAreaLength = WorldGeneration.GetFullWorldArea().y;
        float realChunkWidthSize = WorldGeneration.GetRealChunkAreaSize().x;
        float realChunkHeightSize = WorldGeneration.GetRealChunkAreaSize().y;

        Vector2 position = Vector2.zero;
        switch (direction)
        {
            case WorldDirection.North:
                position = new Vector2((index - fullWorldAreaWidth / 2) * realChunkWidthSize, (fullWorldAreaLength / 2) * realChunkHeightSize);
                break;
            case WorldDirection.South:
                position = new Vector2((index - fullWorldAreaWidth / 2) * realChunkWidthSize, -(fullWorldAreaLength / 2) * realChunkHeightSize);
                break;
            case WorldDirection.East:
                position = new Vector2((fullWorldAreaWidth / 2) * realChunkWidthSize, (index - fullWorldAreaLength / 2) * realChunkHeightSize);
                break;
            case WorldDirection.West:
                position = new Vector2(-(fullWorldAreaWidth / 2) * realChunkWidthSize, (index - fullWorldAreaLength / 2) * realChunkHeightSize);
                break;
        }

        return new Coordinate(position);
    }
    #endregion


    // == HELPER FUNCTIONS ==============>>
    public WorldChunk FindClosestChunk(Vector3 position)
    {
        float minDistance = float.MaxValue;
        WorldChunk closestChunk = null;

        // Iterate over each cell in WorldGeneration
        foreach (WorldChunk chunk in _worldGen.GetChunks())
        {
            float distance = Vector3.Distance(position, chunk.coordinate);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestChunk = chunk;
            }
        }

        if (closestChunk != null)
        {
            return closestChunk;
        }

        return null;
    }

    public float GetDistanceBetweenChunks(WorldChunk chunkA, WorldChunk chunkB)
    {
        return Vector2.Distance(chunkA.coordinate, chunkB.coordinate);
    }

    // == DEBUG FUNCTIONS ==============>>
    public void ShowChunkCells(WorldChunk chunk)
    {
        if (chunk == null) { return; }
        foreach (WorldCell cell in chunk.localCells)
        {
            cell.ShowDebugCube();
        }
    }

    public void HideChunkCells(WorldChunk chunk)
    {
        if (chunk == null) { return; }
        foreach (WorldCell cell in chunk.localCells)
        {
            //cell.HideDebugCube();
            Destroy(cell.GetDebugCube()); // Destroy the debug cube for efficiency
        }
    }

    public string GetChunkStats(WorldChunk chunk)
    {
        if (_worldGen == null) return "[ WORLD GENERATION ] is not available.";
        if (chunk == null) return "[ WORLD CHUNK ] is not available.";
        if (chunk.initialized == false) return "[ WORLD CHUNK ] is not initialized.";


        string str_out = $"[ WORLD CHUNK ] : {chunk.coordinate}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";


        return str_out;
    }
}
