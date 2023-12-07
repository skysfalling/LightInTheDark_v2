using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    WorldGeneration worldGeneration;
    WorldCellMap cellMap;

    WorldChunkMap chunkMap;
    WorldChunkDebug chunkDebug;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = FindObjectOfType<WorldGeneration>();
        cellMap = FindObjectOfType<WorldCellMap>();

        chunkMap = FindObjectOfType<WorldChunkMap>();
        chunkDebug = FindObjectOfType<WorldChunkDebug>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0)) // 0 for left mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                WorldChunk closestChunk = chunkMap.FindClosestChunk(hit.point);
                chunkDebug.SelectWorldChunk(closestChunk);
            }
        }
    }
}
