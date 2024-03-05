using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Path
    {
        float _pathRandomness = 0.5f;
        List<Vector2Int> _positions = new();

        public Vector2Int StartPosition { get; private set; }
        public Vector2Int EndPosition { get; private set; }
        public List<Vector2Int> AllPositions => _positions;

        public Path(CoordinateMap coordinateMap, Vector2Int start, Vector2Int end, float pathRandomness = 0.5f)
        {
            this.StartPosition = start;
            this.EndPosition = end;
            this._pathRandomness = pathRandomness;

            _positions = Pathfinder.FindPath(coordinateMap, this.StartPosition, this.EndPosition, _pathRandomness);
        }
    }
}


