using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    WorldGeneration worldGeneration;
    WorldCellMap cellMap;

    // Start is called before the first frame update
    void Start()
    {
        worldGeneration = FindObjectOfType<WorldGeneration>();
        cellMap = FindObjectOfType<WorldCellMap>();
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
                WorldGeneration.Cell closestCell = cellMap.FindClosestCell(hit.point);
                // Add your logic here for what to do with the closest cell
                Debug.Log("Closest cell found at position: " + closestCell.position);
            }
        }
    }
}
