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
        //WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        //if (closestCell == _selectedCursorCell) { return; }
        //SetHoverCursorCell(closestCell);
    }

    public void MouseSelectInput(Vector3 worldPosition)
    {
        //WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        //SetSelectedCursorCell(closestCell);
    }
    #endregion


    void SetHoverCursorCell(WorldCell cell)
    {
        if (_selectedCursorCell == _hoverCursorCell) { return; }
        if (_hoverCursorCell != null) { RemoveCursorAt(_hoverCursorCell); }

        _hoverCursorCell = cell;
        CreateCursorAt(_hoverCursorCell, CURSOR_TYPE.HOVERED_OVER);

        // Move transform to cell
        transform.position = _hoverCursorCell.position;
    }

    private void SetSelectedCursorCell(WorldCell cell)
    {
        if (_selectedCursorCell != null) { RemoveCursorAt(_selectedCursorCell); }

        _selectedCursorCell = cell;
        CreateCursorAt(_selectedCursorCell, CURSOR_TYPE.SELECTED);
    }


    #region == CURSOR CREATION ===================================== >>>>>
    void CreateCursorAt(WorldCell cell, CURSOR_TYPE type)
    {
        RemoveCursorAt(cell);

        // Create New Cursor
        GameObject cursor = Instantiate(cursorPrefab, cell.position, Quaternion.identity);
        cursor.transform.localScale = Vector3.one * localScaleMultiplier;
        cursor.name = $"{cursorPrefab.name} :: {type}";
        _activeCursors[cell] = cursor;
        WorldMaterialLibrary.Instance.SetCursorMaterial(cursor, type);
    }

    void RemoveCursorAt(WorldCell cell)
    {
        if (_activeCursors.ContainsKey(cell))
        {
            Destroy(_activeCursors[cell]);
            _activeCursors.Remove(cell);
        }
    }
    #endregion

}
