using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class Path
    {
        public Vector2Int start { get; private set; }
        public Vector2Int end { get; private set; }
        public List<Vector2Int> positions { get; private set; }
        float _pathRandomness = 0.5f;
        bool _initialized = false;

        public Path(CoordinateMap coordinateMap, Vector2Int start, Vector2Int end, float pathRandomness = 0.5f)
        {
            this.start = start;
            this.end = end;
            this._pathRandomness = pathRandomness;

            positions = Pathfinder.FindPath(coordinateMap, this.start, this.end, _pathRandomness);
        }
    }
}


