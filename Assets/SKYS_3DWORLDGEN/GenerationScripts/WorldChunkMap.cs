 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    // >> SINGLETON INSTANCE ================== >>
    public static WorldChunkMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // >> COORDINATE MAP ================================ >>
    static List<Coordinate> CoordinateMap = new List<Coordinate>();
    public class Coordinate
    {
        public WorldChunk.TYPE ChunkType = WorldChunk.TYPE.EMPTY;
        public WorldDirection borderEdgeDirection;

        // ASTAR PATHFINDING
        public Vector2 Position { get; set; }
        public Vector3 WorldPosition { get; set; }
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        public Coordinate Parent { get; set; }

        public Coordinate(Vector2 position)
        {
            Position = position;
            WorldPosition = new Vector3(position.x, 0, position.y);

            GCost = float.MaxValue;
            HCost = 0;
            Parent = null;
        }
        public Coordinate(Vector2 position, WorldChunk.TYPE type)
        {
            Position = position;
            WorldPosition = new Vector3(position.x, 0, position.y);

            ChunkType = type;

            GCost = float.MaxValue;
            HCost = 0;
            Parent = null;
        }
    }

    public static List<Coordinate> GetCoordinateMap(bool forceReset = false)
    {
        if (!forceReset && CoordinateMap != null && CoordinateMap.Count > 0) return CoordinateMap;

        List<Coordinate> coordMap = new List<Coordinate>();
        Vector2Int realChunkAreaSize = WorldGeneration.GetRealChunkAreaSize();
        Vector2 realFullWorldSize = WorldGeneration.GetRealFullWorldSize();
        Vector2 half_FullWorldSize = realFullWorldSize * 0.5f;

        int xCoordCount = Mathf.CeilToInt(realFullWorldSize.x / realChunkAreaSize.x);
        int yCoordCount = Mathf.CeilToInt(realFullWorldSize.y / realChunkAreaSize.y);

        for (int x = 0; x < xCoordCount; x++)
        {
            for (int y = 0; y < yCoordCount; y++)
            {
                Vector2 newPos = new Vector2(x * realChunkAreaSize.x, y * realChunkAreaSize.y);
                newPos -= Vector2.one * half_FullWorldSize;
                newPos += Vector2.one * realChunkAreaSize * 0.5f;

                Coordinate newCoord = new Coordinate(newPos);

                // Check if the position is on the border
                if (x == 0 || y == 0 || x == xCoordCount - 1 || y == yCoordCount - 1)
                {
                    newCoord.ChunkType = WorldChunk.TYPE.BORDER;
                    if ( x == 0 ) { newCoord.borderEdgeDirection = WorldDirection.West; }
                    else if ( y == 0 ) { newCoord.borderEdgeDirection = WorldDirection.South; }
                    else if ( x == xCoordCount - 1 ) { newCoord.borderEdgeDirection = WorldDirection.East; }
                    else if ( y == yCoordCount - 1 ) { newCoord.borderEdgeDirection = WorldDirection.North; }
                }

                coordMap.Add( newCoord );
            }
        }

        CoordinateMap = coordMap;
        Debug.Log("New Coordinate Map " + CoordinateMap.Count);

        return CoordinateMap;
    }

    public static List<Vector2> GetCoordinateMapPositions()
    {
        List<Coordinate> coordMap = GetCoordinateMap();
        List<Vector2> coordMapPositions = new List<Vector2>();

        foreach(Coordinate coord in coordMap) { coordMapPositions.Add(coord.Position); }
        return coordMapPositions;
    }
    public static List<Coordinate> GetCoordinatesOnBorder(WorldDirection edgeDirection)
    {
        List<Coordinate> coordinatesOnEdge = new List<Coordinate>();
        List<Coordinate> coordMap = GetCoordinateMap();
        foreach (Coordinate coord in coordMap)
        {
            if (coord.borderEdgeDirection == edgeDirection)
            {
                coordinatesOnEdge.Add(coord);
            }
        }
        return coordinatesOnEdge;
    }

    public static Coordinate GetWorldExitCoordinate(WorldExit worldExit)
    {
        WorldDirection direction = worldExit.edgeDirection;
        int index = worldExit.edgeIndex;

        List<Coordinate> borderCoords = GetCoordinatesOnBorder(direction);
        if (borderCoords.Count > index)
        {
            return borderCoords[index];
        }
        return null;
    }

    // == GAME INTIALIZATION ===============================================================
    public bool initialized = false;
    WorldGeneration _worldGen;
    List<WorldChunk> _worldChunks = new List<WorldChunk>();
    Dictionary<WorldChunk, List<WorldChunk>> _chunkMap = new Dictionary<WorldChunk, List<WorldChunk>>();

    public void InitializeChunkMap()
    {
        initialized = false;

        _worldGen = WorldGeneration.Instance;
        _worldChunks = _worldGen.GetChunks();
        _chunkMap.Clear();

        // << Initialize Chunks >>
        foreach (WorldChunk chunk in _worldChunks)
        {
            List<WorldChunk> neighbors = SetChunkNeighbors(chunk);
            _chunkMap[chunk] = neighbors;
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
        _chunkMap.Clear();
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
        if (!_chunkMap.ContainsKey(chunk)) { return new List<WorldChunk>(); }

        return _chunkMap[chunk];
    }
    #endregion


    #region == COORDINATE PATHFINDING =================================///
    // A* Pathfinding implementation
    // - gCost is the known cost from the starting node
    // - hCost is the estimated distance to the end node
    // - fCost is gCost + hCost

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

    private void OnDrawGizmos()
    {
        List<Coordinate> coordMap = GetCoordinateMap();
        Vector3 realChunkDimensions = WorldGeneration.GetRealChunkDimensions();

        // << DRAW CHUNK MAP >>
        foreach (Coordinate coord in coordMap)
        {
            // Draw Chunks
            Vector3 chunkHeightOffset = realChunkDimensions.y * Vector3.down * 0.5f;
            if (coord.ChunkType == WorldChunk.TYPE.BORDER) { 
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
            }
            else if (coord.ChunkType == WorldChunk.TYPE.EXIT)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(coord.WorldPosition + chunkHeightOffset, realChunkDimensions);
            }

            // Draw Coordinates
            Gizmos.color = Color.black;
            Gizmos.DrawCube(coord.WorldPosition, Vector3.one);
        }
    }
}
