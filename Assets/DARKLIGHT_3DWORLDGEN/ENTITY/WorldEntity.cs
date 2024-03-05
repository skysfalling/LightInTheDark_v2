using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Entity
{
    public class Entity : MonoBehaviour
    {
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
            _entityManager = WorldEntityManager.Instance;
            InvokeRepeating("TickUpdate", _entityManager.tickSpeed, _entityManager.tickSpeed);
        }

        // Update is called once per frame
        void Update()
        {
            if (_currentCell != null)
            {
                transform.position = Vector3.Lerp(transform.position, _currentCell.worldPosition, moveSpeed * Time.deltaTime);
            }
        }

        void TickUpdate()
        {
            //if (_currentCell == null) { _currentCell = _cellMap.FindClosestCellTo(transform.position); }

            // if still following path .. update
            if (_currPathIndex < _movePath.Count)
            {
                _currentCell = _movePath[_currPathIndex];

                _currPathIndex++;
            }
            else
            {
                _currPathIndex = 0;
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
            //_targetCell = WorldCellMap.Instance.FindClosestCellTo(worldPos);
            //SetMovePathTo(_targetCell);
        }

        public void SetMovePathTo(WorldCell targetCell)
        {
            if (_currentCell == null) return;
            if (targetCell == null) return;
            _currPathIndex = 0;

            _targetCell = targetCell;
            //_movePath = WorldPathfinder.Instance.FindPath(_currentCell, _targetCell);
        }
    }
}

