using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Interaction
{
    public class WorldCellCursor : MonoBehaviour
    {
        public enum CURSOR_TYPE { HOVERED_OVER, SELECTED }

        Cell _hoverCursorCell;
        Cell _selectedCursorCell;
        Dictionary<Cell, GameObject> _activeCursors = new();

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


        void SetHoverCursorCell(Cell cell)
        {
            if (_selectedCursorCell == _hoverCursorCell) { return; }
            if (_hoverCursorCell != null) { RemoveCursorAt(_hoverCursorCell); }

            _hoverCursorCell = cell;
            CreateCursorAt(_hoverCursorCell, CURSOR_TYPE.HOVERED_OVER);

            // Move transform to cell
            transform.position = _hoverCursorCell.Position;
        }

        private void SetSelectedCursorCell(Cell cell)
        {
            if (_selectedCursorCell != null) { RemoveCursorAt(_selectedCursorCell); }

            _selectedCursorCell = cell;
            CreateCursorAt(_selectedCursorCell, CURSOR_TYPE.SELECTED);
        }


        #region == CURSOR CREATION ===================================== >>>>>
        void CreateCursorAt(Cell cell, CURSOR_TYPE type)
        {
            RemoveCursorAt(cell);

            // Create New Cursor
            GameObject cursor = Instantiate(cursorPrefab, cell.Position, Quaternion.identity);
            cursor.transform.localScale = Vector3.one * localScaleMultiplier;
            cursor.name = $"{cursorPrefab.name} :: {type}";
            _activeCursors[cell] = cursor;
        }

        void RemoveCursorAt(Cell cell)
        {
            if (_activeCursors.ContainsKey(cell))
            {
                Destroy(_activeCursors[cell]);
                _activeCursors.Remove(cell);
            }
        }
        #endregion

    }
}

