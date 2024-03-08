using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation
{
    public class Entity : MonoBehaviour
    {
        bool _active;
        Region _regionParent;
        Chunk _currentChunk;
        Coordinate _currentCoordinate;
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

