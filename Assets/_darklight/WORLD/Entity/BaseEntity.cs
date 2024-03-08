using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.World.Generation.Entity
{
    public class BaseEntity : MonoBehaviour
    {
        bool _active;
        Generation.Region _regionParent;
        Generation.Chunk _currentChunk;
        Generation.Coordinate _currentCoordinate;
        GameObject _modelObject;

        public void Initialize(string name, GameObject modelPrefab, Region region, Chunk chunk)
        {
            this.gameObject.name = $"_entity({name})";
            _regionParent = region;
            _currentChunk = chunk;
            _currentCoordinate = chunk.Coordinate;

            // create model as child transform
            _modelObject = Instantiate(modelPrefab, transform);

            this.transform.position = _currentChunk.GroundPosition;
        }
    }
}

