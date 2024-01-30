using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCellCursor : MonoBehaviour
{
    public enum CURSOR_TYPE { HOVERED_OVER , SELECTED }

    WorldCell _hoverCursorCell;
    WorldCell _selectedCursorCell;
    Dictionary<WorldCell, GameObject> _activeCursors = new();

    public GameObject cursorPrefab;
    [Range(0.1f, 10f)] public float localScaleMultiplier;



    #region == Mouse Input ======================================= ///////
    public void MouseHoverInput(Vector3 worldPosition)
    {
        WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        UpdateHoverCursorCell(closestCell);
    }

    public void MouseSelectInput(Vector3 worldPosition)
    {
        _selectedCursorCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        CreateCursorAt(_selectedCursorCell, CURSOR_TYPE.SELECTED);
    }
    #endregion

    void UpdateHoverCursorCell(WorldCell cell)
    {
        if (WorldCellMap.Instance.initialized && cell != null)
        {
            if (_hoverCursorCell != null)
            {
                WorldCellMap.Instance.HideCellNeighbors(_hoverCursorCell);
            }

            _hoverCursorCell = cell;
            this.transform.position = cell.position;
            CreateCursorAt(_hoverCursorCell, CURSOR_TYPE.HOVERED_OVER);

            WorldCellMap.Instance.ShowCellNeighbors(cell);
        }
    }

    void CreateCursorAt(WorldCell cell, CURSOR_TYPE type)
    {
        GameObject newCursor = Instantiate(cursorPrefab, cell.position, Quaternion.identity);
        newCursor.transform.parent = this.transform;
        newCursor.transform.localScale = Vector3.one * localScaleMultiplier;
        _activeCursors[cell] = newCursor;

        WorldMaterialLibrary.Instance.SetCursorMaterial(newCursor, type);
    }
}
