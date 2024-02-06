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

    // == CHUNK MAP ======================================== ////
    private static List<WorldChunk> ChunkMap = new List<WorldChunk>();
    private static Dictionary<WorldCoordinate, WorldChunk> ChunkCoordMap = new();

    public static List<WorldChunk> GetChunkMap()
    {
        if (ChunkMap != null && ChunkMap.Count > 0) return ChunkMap;

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
        return ChunkCoordMap[coord]; 
    }

    // == GAME INTIALIZATION ===============================================================
    [HideInInspector] public bool initialized = false;



    #region == DETERMINE CHUNK HEIGHTS ==================>>

    #endregion


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
        if (chunk == null) return "[ WORLD CHUNK ] is not available.";
        //if (chunk._initialized == false) return "[ WORLD CHUNK ] is not initialized.";


        string str_out = $"[ WORLD CHUNK ] : {chunk.coordinate}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";

        return str_out;
    }
}
