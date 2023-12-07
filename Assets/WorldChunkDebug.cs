using System.Collections;
using System.Collections.Generic;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectWorldChunk(WorldChunk chunk)
    {
        selected_worldChunk = chunk;
        selected_worldChunk.DetermineChunkType();

    }

    private void OnDrawGizmos()
    {
        if (selected_worldChunk != null)
        {


            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(selected_worldChunk.position, _worldGeneration.fullsize_chunkDimensions);

            Gizmos.color = Color.red;
            foreach (WorldChunk chunk in _worldChunkMap.neighborMap[selected_worldChunk])
            {
                Gizmos.DrawCube(chunk.position, _worldGeneration.fullsize_chunkDimensions);
            }

        }
    }
}
