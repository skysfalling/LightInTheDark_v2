using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkDebug : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    public WorldChunk selected_worldChunk;

    // Start is called before the first frame update
    void Start()
    {
        _worldGeneration = GetComponentInParent<WorldGeneration>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectWorldChunk(WorldChunk chunk)
    {
        selected_worldChunk = chunk;
    }

    private void OnDrawGizmos()
    {
        if (selected_worldChunk != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(selected_worldChunk.position, _worldGeneration.fullsize_chunkDimensions);
        }
    }
}
