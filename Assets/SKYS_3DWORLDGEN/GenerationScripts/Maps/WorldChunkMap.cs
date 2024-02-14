using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkMap : MonoBehaviour
{
    // >> SINGLETON INSTANCE ================== >>
    public static WorldChunkMap Instance;
    public void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public static bool chunkMapInitialized { get; private set; }
    public static bool chunkMeshInitialized { get; private set; }

    public static List<WorldChunk> ChunkList { get; private set; }
    public static Dictionary<WorldCoordinate, WorldChunk> WorldCoordChunkMap { get; private set; }
    public static Dictionary<Vector2Int, WorldChunk> CoordinateChunkMap { get; private set; }

    // == HANDLE CHUNK MAP =================================== ///
    public IEnumerator InitializeChunkMap()
    {
        yield return new WaitUntil(() => WorldCoordinateMap.coordMapInitialized);

        List<WorldChunk> newChunkList = new();
        Dictionary<WorldCoordinate, WorldChunk> newWorldCoordChunkMap = new();
        Dictionary<Vector2Int, WorldChunk> newCoordinateChunkMap = new();

        // Create Chunks at each World Coordinate
        foreach (WorldCoordinate worldCoord in WorldCoordinateMap.CoordinateList)
        {
            WorldChunk newChunk = new WorldChunk(worldCoord);

            newChunkList.Add(newChunk);
            newWorldCoordChunkMap[worldCoord] = newChunk;
            newCoordinateChunkMap[worldCoord.Coordinate] = newChunk;
        }

        ChunkList = newChunkList;
        WorldCoordChunkMap = newWorldCoordChunkMap;
        CoordinateChunkMap = newCoordinateChunkMap;

        chunkMapInitialized = true;
    }

    public void DestroyChunkMap()
    {
        ChunkList = new();
        WorldCoordChunkMap = new();
        CoordinateChunkMap = new();

        chunkMapInitialized = false;
    }

    public void UpdateChunkMap()
    {
        if (chunkMapInitialized) return;

        StartCoroutine(InitializeChunkMap()); // Make sure chunk map is initialized
    }

    public void InitializeChunkMesh()
    {
        StartCoroutine(InitializeChunkMeshRoutine());
    }

     IEnumerator InitializeChunkMeshRoutine()
     {

        foreach (WorldChunk chunk in ChunkList)
        {
            // Generate individual chunk
            chunk.GenerateChunkMesh();
            yield return new WaitUntil(() => chunk.generation_finished);
        }

        chunkMeshInitialized = true;
     }

    #region == GET CHUNKS ======================================== ////

    public static WorldChunk GetChunkAt(Vector2Int coordinate)
    {
        if (!chunkMapInitialized) { return null; }

        // Use the dictionary for fast lookups
        if (CoordinateChunkMap.TryGetValue(coordinate, out WorldChunk foundChunk))
        {
            return foundChunk;
        }
        return null;
    }

    public static WorldChunk GetChunkAt(WorldCoordinate worldCoord)
    {
        if (!chunkMapInitialized || worldCoord == null) { return null; }

        // Use the dictionary for fast lookups
        if (WorldCoordChunkMap.TryGetValue(worldCoord, out WorldChunk foundChunk))
        {
            return foundChunk;
        }
        return null;
    }

    public static List<WorldChunk> GetChunksAtCoordinates(List<WorldCoordinate> worldCoords)
    {
        if (!chunkMapInitialized) { return new List<WorldChunk>(); }

        List<WorldChunk> chunks = new List<WorldChunk>();
        foreach (WorldCoordinate worldCoord in worldCoords)
        {
            chunks.Add(GetChunkAt(worldCoord));
        }

        return chunks;
    }
    #endregion


    #region == SET CHUNKS =====================================////

    public static void SetChunksToHeight(List<WorldChunk> worldChunk, int chunkHeight)
    {
        foreach (WorldChunk chunk in worldChunk)
        {
            chunk.SetGroundHeight(chunkHeight);
        }
    }

    public static void SetChunksToHeightFromCoordinates(List<WorldCoordinate> worldCoords, int chunkHeight)
    {
        foreach (WorldCoordinate coord in worldCoords)
        {
            WorldChunk chunk = GetChunkAt(coord);
            if (chunk != null)
            {
                chunk.SetGroundHeight(chunkHeight);
            }
        }
    }

    #endregion
}
