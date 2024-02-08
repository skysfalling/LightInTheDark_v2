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
    public static List<WorldChunk> ChunkList { get; private set; }
    public static Dictionary<WorldCoordinate, WorldChunk> WorldCoordChunkMap { get; private set; }
    public static Dictionary<Vector2Int, WorldChunk> CoordinateChunkMap { get; private set; }

    // == HANDLE CHUNK MAP =================================== ///
    public void InitializeChunkMap()
    {
        if (WorldCoordinateMap.coordMapInitialized == false) return;
        if (chunkMapInitialized == true) return;

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

        // Reset Path Colors
        /*
        foreach (WorldChunk chunk in GetChunkMap())
        {
            chunk.groundHeight = 0;
            chunk.pathColor = WorldPath.PathColor.CLEAR;
            chunk.zoneColor = WorldZone.ZoneColor.CLEAR;
        }
        */

        chunkMapInitialized = false;
    }

    public void UpdateChunkMap()
    {
        InitializeChunkMap(); // Make sure chunk map is initialized

        // Determine path heights
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
        if (!chunkMapInitialized) { return null; }

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




}
