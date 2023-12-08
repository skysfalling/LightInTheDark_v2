using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    WorldGeneration worldGeneration;
    public WorldCell currentCell;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = GameObject.FindObjectOfType<WorldGeneration>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCell(WorldCell cell)
    {
        currentCell = cell;
        transform.position = cell.position;
    }
}
