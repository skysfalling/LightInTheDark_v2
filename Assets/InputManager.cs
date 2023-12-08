using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    WorldGeneration _worldGeneration;
    WorldCellMap _cellMap;
    WorldChunkMap _chunkMap;
    WorldChunkDebug _chunkDebug;

    // Start is called before the first frame update
    void Start()
    {
        _worldGeneration = FindObjectOfType<WorldGeneration>();
        _cellMap = FindObjectOfType<WorldCellMap>();

        _chunkMap = FindObjectOfType<WorldChunkMap>();
        _chunkDebug = FindObjectOfType<WorldChunkDebug>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0)) // 0 for left mouse button
        {
            if (_worldGeneration == null || _cellMap == null || _chunkMap == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (_chunkMap == null) return;
                WorldChunk closestChunk = _chunkMap.FindClosestChunk(hit.point);
                _chunkDebug.SelectWorldChunk(closestChunk);
            }
        }
    }
}
