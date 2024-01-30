using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    WorldCellMap _cellMap;
    WorldEntityManager _entityManager;
    
    List<WorldCell> _movePath = new();
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
        if (_currentCell == null) { _currentCell = _cellMap.FindClosestCellTo(transform.position); }

        if (_movePath.Count > 0)
        {
            _currentCell = _movePath[0];
            _movePath.RemoveAt(0);
        }
    }

    public void SetTargetCell(WorldCell cell)
    {
        _targetCell = cell;
        CreatePathTo(_targetCell);
    }

    public void SetTargetCell(Vector3 worldPos)
    {
        _targetCell = WorldCellMap.Instance.FindClosestCellTo(worldPos);
        CreatePathTo(_targetCell);
    }

    public void CreatePathTo(WorldCell targetCell)
    {
        if (_currentCell == null) return;
        if (targetCell == null) return;

        // Clear old path
        _cellMap.ClearCellPathDebugs(_movePath);

        _targetCell = targetCell;
        _movePath = WorldPathfinder.Instance.FindPath(_currentCell, _targetCell);
        _cellMap.DrawPath(_movePath);
    }
}
