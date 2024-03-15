using System;
using System.Collections;
using System.Linq;
using Darklight.World.Generation;
using UnityEngine;
using Darklight.World.Settings;
using System.Threading.Tasks;
using Darklight.Bot;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Map
{
    public class CoordinateMapMonoBehaviour : TaskQueen
    {
        [SerializeField]
        public CustomGenerationSettings customSettings;
        public CoordinateMap coordinateMap;
        public Traveler travelerObject;

        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = transform.position; // Start at center
                origin -= customSettings.WorldWidth * new Vector3(0.5f, 0, 0.5f);
                origin += customSettings.RegionWidth * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }

        private new void Awake()
        {
            base.Awake();
            _ = SetCoordinateMap();

        }

        public async Task SetCoordinateMap()
        {
            coordinateMap = new CoordinateMap(transform, OriginPosition, customSettings.WorldWidth, customSettings.RegionWidth);
            await coordinateMap.InitializeDefaultMap();
            travelerObject.InitializeAtCoordinate(coordinateMap, coordinateMap.GetRandomCoordinateValueOfType(Coordinate.TYPE.NULL));
            await Task.CompletedTask;
        }

        void OnDrawGizmos()
        {
            if (coordinateMap == null || !coordinateMap.Initialized)
            {
                return;
            }

            // Draw all coordinates
            foreach (Coordinate coordinate in coordinateMap.AllCoordinates)
            {
                Gizmos.color = Color.white; // Default color for regular coordinates
                Vector3 position = coordinate.ScenePosition;

                // Adjust colors and sizes for different types
                switch (coordinate.Type)
                {
                    case Coordinate.TYPE.BORDER:
                        Gizmos.color = Color.red;
                        break;
                    case Coordinate.TYPE.EXIT:
                        Gizmos.color = Color.green;
                        break;
                    case Coordinate.TYPE.ZONE:
                        Gizmos.color = Color.blue;
                        break;
                    case Coordinate.TYPE.PATH:
                        Gizmos.color = Color.yellow;
                        break;
                        // Add more cases as needed
                }

                // Draw a small cube at each coordinate
                Gizmos.DrawCube(position, Vector3.one * coordinateMap.CoordinateSize * 0.1f); // Adjust the size as necessary
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CoordinateMapMonoBehaviour))]
    public class CoordinateMapBehaviourEditor : UnityEditor.Editor
    {
        private CoordinateMapMonoBehaviour mapScript;
        private CoordinateMap coordinateMap;
        private static bool showGenerationSettingsFoldout = true;

        private void OnEnable()
        {
            mapScript = (CoordinateMapMonoBehaviour)target;
            coordinateMap = mapScript.coordinateMap;
            _ = mapScript.SetCoordinateMap();
        }
        public override void OnInspectorGUI()
        {
            mapScript = (CoordinateMapMonoBehaviour)target;
            coordinateMap = mapScript.coordinateMap;

            // Detect changes to the serialized properties
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            SerializedProperty customWorldGenSettingsProperty = serializedObject.FindProperty("customSettings");
            showGenerationSettingsFoldout = EditorGUILayout.Foldout(showGenerationSettingsFoldout, "Custom World Generation Settings", true);
            if (showGenerationSettingsFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();

                UnityEditor.Editor editor = CreateEditor(mapScript.customSettings);
                editor.OnInspectorGUI();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.Update();

            if (EditorGUI.EndChangeCheck())
            {
                // If something changed, apply the changes and update the camera position
                serializedObject.ApplyModifiedProperties();
                _ = mapScript.SetCoordinateMap();
            }
        }
    }
#endif
}
