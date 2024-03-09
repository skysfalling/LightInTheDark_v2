using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Generation
{
    // this will the basic class used for any object that is spawned in the world
    // this does not include items, travelers, or entities
    [RequireComponent(typeof(DimensionSpace))]
    public class WorldObject : MonoBehaviour
    {
        public GameObject modelPrefab;
        public DimensionSpace DimensionSpace => GetComponent<DimensionSpace>();

        public void Start()
        {
            // if the model prefab is not null, then we will spawn the model
            if (modelPrefab != null)
            {
                // spawn the model
                GameObject model = Instantiate(modelPrefab, transform.position, Quaternion.identity);
                model.transform.SetParent(transform);
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(WorldObject))]
    public class WorldObjectEditor : Editor
    {
        private SerializedObject _serializedWorldObject =>  new SerializedObject(target);
        private WorldObject _worldObjectScript => (WorldObject) target;
        private DimensionSpace _dimensionSpaceScript => _worldObjectScript.GetComponent<DimensionSpace>();
        void OnEnable()
        {
            Debug.Log($"WorldObjectEditor model {_worldObjectScript.modelPrefab.name} loaded");
            
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck(); // Begin change check

            DrawDefaultInspector();

            GameObject objectModel = _worldObjectScript.modelPrefab;
            if (objectModel != null)
            {
                _dimensionSpaceScript.SetPreviewObject(objectModel);
                Repaint();
            }

            // Check if any changes were made in the Inspector
            if (EditorGUI.EndChangeCheck())
            {
                // If there were changes, apply them to the serialized object
                _serializedWorldObject.ApplyModifiedProperties();


                // Optionally, mark the target object as dirty to ensure the changes are saved
                EditorUtility.SetDirty(target);
            }
        }
    }

#endif

}