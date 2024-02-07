 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    public bool mapInitialized { get; private set; }

    public void InitializeChunkMap()
    {
        GetChunkMap();
        mapInitialized = true;
    }
    public void ResetChunkMap()
    {
        // Reset Path Colors
        foreach (WorldChunk chunk in GetChunkMap())
        {
            chunk.groundHeight = 0;
            chunk.pathColor = WorldPath.PathColor.CLEAR;
            chunk.zoneColor = WorldZone.ZoneColor.CLEAR;
        }
    }

    #region == CHUNK MAP ======================================== ////
    private static List<WorldChunk> ChunkMap = new List<WorldChunk>();
    private static Dictionary<WorldCoordinate, WorldChunk> ChunkCoordMap = new();
    public static List<WorldChunk> GetChunkMap(bool forceReset = false)
    {
        if (forceReset == false && ChunkMap != null && ChunkMap.Count > 0) return ChunkMap;

        ChunkMap = new();
        ChunkCoordMap = new();

        List<WorldCoordinate> coordMap = WorldCoordinateMap.GetCoordinateMap();
        foreach (WorldCoordinate coord in coordMap)
        {
            WorldChunk newChunk = new WorldChunk(coord);

            ChunkMap.Add(newChunk);
            ChunkCoordMap[coord] = newChunk;
        }

        return ChunkMap;
    }
    public static WorldChunk GetChunkAtCoordinate(WorldCoordinate coord) 
    {
        GetChunkMap();

        WorldChunk value;
        ChunkCoordMap.TryGetValue(coord, out value);

        return value; 
    }
    public static List<WorldChunk> GetChunksAtCoordinates(List<WorldCoordinate> coords)
    {
        GetChunkMap();

        List<WorldChunk> chunks = new List<WorldChunk>();
        foreach (WorldCoordinate c in coords)
        {
            chunks.Add(ChunkCoordMap[c]);
        }
        return chunks;
    }
    #endregion




}
