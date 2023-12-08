using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        selected_worldChunk.SetChunkType();

        Debug.Log($"Selected Chunk {chunk.position} is TYPE : {chunk.type}");
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
}
