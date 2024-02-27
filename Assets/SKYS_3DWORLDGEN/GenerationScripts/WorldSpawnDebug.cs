using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpawnDebug : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    WorldChunkMap _worldChunkMap;
    WorldSpawnMap _worldSpawnMap;

    public WorldChunk.TYPE gizmoType;
    public List<WorldChunk> gizmoSelectedTypeChunks;

    public void OnDrawGizmosSelected()
    {
        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _worldChunkMap = FindObjectOfType<WorldChunkMap>();
        _worldSpawnMap = FindObjectOfType<WorldSpawnMap>();

        if (_worldGeneration == null) return;
        if (_worldGeneration.generation_finished == false) return;
        if (_worldSpawnMap.initialized)
        {
            gizmoSelectedTypeChunks = _worldSpawnMap.GetAllChunksOfType(gizmoType);

            // << CHOOSE COLOR >>
            switch (gizmoType)
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

            foreach (WorldChunk chunk in gizmoSelectedTypeChunks)
            {
                Gizmos.DrawCube(chunk.groundPosition, WorldGeneration.GetChunkVec3Dimensions_inCells());
            }


        }
    }

}
