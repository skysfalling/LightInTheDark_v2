using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// This is to be used as a default accessor to the world generation values

namespace Darklight.World.Generation
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Traveler : MonoBehaviour
    {
        bool _active = false;
        WorldBuilder _worldGeneration => WorldBuilder.Instance;
        Region _parentRegion;
        Coordinate _currentCoordinate;

        // [[ INSPECTOR VARIABLES ]]
        public GameObject modelPrefab;
        GameObject _modelObject;

        public GameObject ModelObject => _modelObject;

        public void InitializeAt(Region region, Coordinate coordinate)
        {
            _parentRegion = region;
            _currentCoordinate = coordinate;
        }

        public void SpawnModel()
        {
            if (modelPrefab == null) return;
            if (_modelObject != null)
            {
                DestroyModel();
            }
            _modelObject = Instantiate(modelPrefab, transform.position, Quaternion.identity, transform);
            _modelObject.hideFlags = HideFlags.DontSave;
        }

        public void DestroyModel()
        {
            DestroyGameObject(_modelObject);
        }

        public static void DestroyGameObject(GameObject gameObject)
        {
            // Check if we are running in the Unity Editor
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                // Use DestroyImmediate if in edit mode and not playing
                DestroyImmediate(gameObject);
                return;
            }
            else
#endif
            {
                // Use Destroy in play mode or in a build
                Destroy(gameObject);
            }
        }
    }
}