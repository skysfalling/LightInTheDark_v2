using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCellCursor : MonoBehaviour
{
    public WorldCell hoverCursorCell;
    public WorldCell selectedCursorCell;

    public GameObject cursorPrefab;
    Dictionary<WorldCell, GameObject> activeCursors = new();

    public Material selected_material;
    public Material hoverOver_material;


    #region == Mouse Input ======================================= ///////
    public void MouseHoverInput(Vector3 worldPosition)
    {
        UpdateHoverCursorCell(WorldCellMap.Instance.FindClosestCellTo(worldPosition));
    }

    public void MouseSelectInput(Vector3 worldPosition)
    {
        selectedCursorCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        CreateCursorAt(selectedCursorCell);
    }
    #endregion

    void UpdateHoverCursorCell(WorldCell cell)
    {
        if (WorldCellMap.Instance.initialized && cell != null)
        {
            if (hoverCursorCell != null)
            {
                WorldCellMap.Instance.HideCellNeighbors(hoverCursorCell);
            }

            hoverCursorCell = cell;
            this.transform.position = cell.position;
            WorldCellMap.Instance.ShowCellNeighbors(cell);
        }
    }

    void CreateCursorAt(WorldCell cell)
    {
        GameObject newCursor = Instantiate(cursorPrefab, cell.position, Quaternion.identity);
        activeCursors[cell] = newCursor;

        newCursor.GetComponent<MeshRenderer>().material = WorldMaterialLibrary.Instance.GetMaterialOfCellType(cell.type);
    }
}
