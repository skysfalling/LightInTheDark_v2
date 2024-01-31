using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    WorldCellMap _cellMap;
    WorldEntityManager _entityManager;
    
    List<WorldCell> _affectedPath = new List<WorldCell>();
    List<WorldCell> _movePath = new();
    int _currPathIndex = 0;
    WorldCell _currentCell = null;
    WorldCell _targetCell = null;

    [Header("Attributes")]
    public int moveSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        _cellMap = WorldCellMap.Instance;
        _entityManager = WorldEntityManager.Instance;
        InvokeRepeating("TickUpdate", _entityManager.tickSpeed, _entityManager.tickSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentCell != null)
        {
            transform.position = Vector3.Lerp(transform.position, _currentCell.position, moveSpeed * Time.deltaTime);
        }
    }

    void TickUpdate()
    {
        if (_cellMap.initialized == false) { return; }
        if (_currentCell == null) { _currentCell = _cellMap.FindClosestCellTo(transform.position); }

        // if still following path .. update
        if (_currPathIndex < _movePath.Count)
        {
            _currentCell = _movePath[_currPathIndex];
            _cellMap.Debug_ShowCellList(_movePath);

            _currPathIndex++;
        }
        else 
        {
            _currPathIndex = 0;
            _cellMap.Debug_DestroyCellList(_movePath);
            _movePath = new List<WorldCell>();
        }
    }

    public void SetTargetCell(WorldCell cell)
    {
        _targetCell = cell;
        SetMovePathTo(_targetCell);
    }

    public void SetTargetCell(Vector3 worldPos)
    {
        _targetCell = WorldCellMap.Instance.FindClosestCellTo(worldPos);
        SetMovePathTo(_targetCell);
    }

    public void SetMovePathTo(WorldCell targetCell)
    {
        if (_currentCell == null) return;
        if (targetCell == null) return;
        _currPathIndex = 0;
        _cellMap.Debug_DestroyCellList(_movePath);

        _targetCell = targetCell;
        _movePath = WorldPathfinder.Instance.FindPath(_currentCell, _targetCell);
        _cellMap.Debug_ShowCellList(_movePath);
    }

    void ClearPath()
    {

    }
}
