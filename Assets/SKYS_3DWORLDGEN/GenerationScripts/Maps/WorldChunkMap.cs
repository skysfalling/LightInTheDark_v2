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
    public static Dictionary<Vector2Int, WorldChunk> CoordinateChunkMap { get; private set; }

    // == HANDLE CHUNK MAP =================================== ///
    public IEnumerator InitializeChunkMap()
    {

        if (chunkMapInitialized)
        {
            ResetAllChunkHeights();
            yield return null;
        }

        /*
        yield return new WaitUntil(() => CoordinateMap.coordMapInitialized);

        List<WorldChunk> newChunkList = new();
        Dictionary<Vector2Int, WorldChunk> newCoordinateChunkMap = new();

        // Create Chunks at each World Coordinate
        foreach (Coordinate worldCoord in CoordinateMap.CoordinateList)
        {
            WorldChunk newChunk = new WorldChunk(worldCoord);

            newChunkList.Add(newChunk);
            newCoordinateChunkMap[worldCoord.LocalCoordinate] = newChunk;
        }
        */

        //ChunkList = newChunkList;
        //CoordinateChunkMap = newCoordinateChunkMap;

        chunkMapInitialized = true;
    }

    public void DestroyChunkMap()
    {
        ChunkList = new();
        CoordinateChunkMap = new();

        chunkMapInitialized = false;
    }

    public void UpdateChunkMap()
    {
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

    public static WorldChunk GetChunkAt(Coordinate worldCoord)
    {
        if (!chunkMapInitialized || worldCoord == null) { return null; }

        // Use the dictionary for fast lookups
        if (CoordinateChunkMap.TryGetValue(worldCoord.LocalPosition, out WorldChunk foundChunk))
        {
            return foundChunk;
        }
        return null;
    }

    public static List<WorldChunk> GetChunksAtCoordinates(List<Coordinate> worldCoords)
    {
        if (!chunkMapInitialized) { return new List<WorldChunk>(); }

        List<WorldChunk> chunks = new List<WorldChunk>();
        foreach (Coordinate worldCoord in worldCoords)
        {
            chunks.Add(GetChunkAt(worldCoord));
        }

        return chunks;
    }
    #endregion


    #region == SET CHUNKS =====================================////
    public static void ResetAllChunkHeights()
    {
        foreach (WorldChunk chunk in ChunkList)
        {
            chunk.SetGroundHeight(0);
        }
    }


    public static void SetChunksToHeight(List<WorldChunk> worldChunk, int chunkHeight)
    {
        foreach (WorldChunk chunk in worldChunk)
        {
            chunk.SetGroundHeight(chunkHeight);
        }
    }

    public static void SetChunksToHeightFromCoordinates(List<Coordinate> worldCoords, int chunkHeight)
    {
        foreach (Coordinate coord in worldCoords)
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
