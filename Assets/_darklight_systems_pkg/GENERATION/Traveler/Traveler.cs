using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

        // This is to be used as a default accessor to the world generation values

namespace Darklight.ThirdDimensional.Generation
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Traveler : MonoBehaviour
    {
        bool _active = false;
        WorldGeneration _worldGeneration => WorldGeneration.Instance;
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

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Traveler))]
    public class TravelerEditor : UnityEditor.Editor
    {
        SerializedObject _travelerObject;
        Traveler _travelerScript;
        SerializedProperty _modelProperty;

        private void OnEnable() {
            _travelerScript = (Traveler)target;
            _travelerObject = new SerializedObject(target);

            _travelerScript.SpawnModel();

            // Subscribe to selection change event
            Selection.selectionChanged += OnSelectionChanged;
        }

        // New method to handle selection changes
        private void OnSelectionChanged()
        {
            if (_travelerScript == null)
            {
                Selection.selectionChanged -= OnSelectionChanged;
                return;
            }

            // Call the existing logic to check if the object or its children are selected
            if (!IsObjectOrChildSelected(_travelerScript.gameObject))
            {
                // If not, destroy the model
                _travelerScript.DestroyModel();
                
                // Unsubscribe to avoid memory leaks
                Selection.selectionChanged -= OnSelectionChanged;
            }
        }

        public override void OnInspectorGUI()
        {

            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            // Check if any changes were made in the Inspector
            if (EditorGUI.EndChangeCheck())
            {
                // If there were changes, apply them to the serialized object
                _travelerObject.ApplyModifiedProperties();

                // Optionally, mark the target object as dirty to ensure the changes are saved
                EditorUtility.SetDirty(target);
            }

        }

        private bool IsObjectOrChildSelected(GameObject obj)
        {
            // Check if the direct object is selected
            if (Selection.activeGameObject == obj)
            {
                return true;
            }

            // Check if any of the selected objects is a child of the inspected object
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject.transform.IsChildOf(obj.transform))
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endif
}


