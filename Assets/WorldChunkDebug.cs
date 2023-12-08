using UnityEngine;
public class WorldChunkDebug : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    public WorldChunk selected_worldChunk;

    // Start is called before the first frame update
    void Start()
    {
        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
    }

    public void SelectWorldChunk(WorldChunk chunk)
    {
        selected_worldChunk = chunk;
    }

    private void OnDrawGizmosSelected()
    {
        if (_worldGeneration == null) return;
        if (_worldGeneration.generation_finished == false) return;
        if (selected_worldChunk == null) return;

        if (_worldGeneration.GetChunks().Count > 0)
        {
            switch (selected_worldChunk.type)
            {
                case WorldChunk.TYPE.CLOSED:
                    Gizmos.color = Color.black;
                    break;
                case WorldChunk.TYPE.DEADEND:
                    Gizmos.color = Color.red;
                    break;
                case WorldChunk.TYPE.HALLWAY:
                case WorldChunk.TYPE.CORNER:
                    Gizmos.color = Color.yellow;
                    break;
                case WorldChunk.TYPE.WALL:
                    Gizmos.color = Color.green;
                    break;
                case WorldChunk.TYPE.EMPTY:
                    Gizmos.color = Color.blue;
                    break;
                default:
                    Gizmos.color = Color.grey;
                    break;
            }

            Gizmos.DrawCube(selected_worldChunk.position, _worldGeneration.fullsize_chunkDimensions);

            Gizmos.color = Color.grey;
            foreach (WorldChunk chunk in _worldChunkMap.GetChunkNeighbors(selected_worldChunk))
            {
                Gizmos.DrawCube(chunk.position, _worldGeneration.fullsize_chunkDimensions);
            }

        }
    }

    public string GetChunkStats(WorldChunk chunk)
    {
        if (_worldGeneration == null || !_worldGeneration.generation_finished) return "[ WORLD GENERATION ] is not available.";
        if (chunk == null) return "[ WORLD CHUNK ] is not available.";
        if (chunk.initialized == false) return "[ WORLD CHUNK ] is not initialized.";


        string str_out = $"[ WORLD CHUNK ] : {chunk.position}\n";
        str_out += $"\t>> chunk_type : {chunk.type}\n";
        str_out += $"\t>> Total Cell Count : {chunk.localCells.Count}\n";
        str_out += $"\t    -- Empty Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EMPTY).Count}\n";
        str_out += $"\t    -- Edge Cells : {chunk.GetCellsOfType(WorldCell.TYPE.EDGE).Count}\n";
        str_out += $"\t    -- Corner Cells : {chunk.GetCellsOfType(WorldCell.TYPE.CORNER).Count}\n";


        return str_out;
    }
}
