using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    WorldGeneration worldGeneration;
    public WorldGeneration.Cell currentCell;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = GameObject.FindObjectOfType<WorldGeneration>();
        SetCell(worldGeneration.GetChunks()[0].cells[3]);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCell(WorldGeneration.Cell cell)
    {
        currentCell = cell;
        transform.position = cell.position;
    }
}
