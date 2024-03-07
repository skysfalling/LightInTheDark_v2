using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation
{
    public class Path
    {
        List<Vector2Int> _positions = new();

        public Vector2Int StartPosition { get; private set; }
        public Vector2Int EndPosition { get; private set; }
        public List<Vector2Int> AllPositions => _positions;

        public Path(CoordinateMap coordinateMap, Vector2Int start, Vector2Int end, List<Coordinate.TYPE> validTypes, float pathRandomness = 0.5f)
        {
            this.StartPosition = start;
            this.EndPosition = end;

            _positions = Pathfinder.FindPath(coordinateMap, this.StartPosition, this.EndPosition, validTypes, pathRandomness);
        }
    }
}


