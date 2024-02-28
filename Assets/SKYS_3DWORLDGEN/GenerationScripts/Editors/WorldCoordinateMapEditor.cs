#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CoordinateMap))]
public class WorldCoordinateMapEditor : Editor
{
    SerializedObject serializedCoordinateMap;
    private GUIStyle h1Style;
    private GUIStyle h2Style;
    private GUIStyle worldStyle;

    private void OnEnable()
    {
        serializedCoordinateMap = new SerializedObject(target);

        // Initialize GUIStyle for H1 header
        h1Style = new GUIStyle()
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        h1Style.normal.textColor = Color.grey;

        // Initialize GUIStyle for H2 header
        h2Style = new GUIStyle(h1Style) // Inherit from H1 and override as needed
        {
            fontSize = 14,
            fontStyle = FontStyle.Italic
        };
        h2Style.normal.textColor = Color.grey;

        worldStyle = new GUIStyle()
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        worldStyle.normal.textColor = Color.black;
    }

    public override void OnInspectorGUI()
    {
        serializedCoordinateMap.Update();
        CoordinateMap worldCoordMap = (CoordinateMap)target;
        WorldRegionMap worldMap = worldCoordMap.GetComponent<WorldRegionMap>();


        EditorGUILayout.LabelField($"World Coordinate Map Initialized => {CoordinateMap.coordMapInitialized}");
        EditorGUILayout.Space();

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        //======================================================== ////
        // WORLD PATHS
        //======================================================== ////
        EditorGUILayout.LabelField("Exit Paths", h1Style);
        EditorGUILayout.Space();
        #region World Exit Paths List

        SerializedProperty worldExitPathsProperty = serializedCoordinateMap.FindProperty("worldExitPaths");
        if (worldExitPathsProperty != null)
        {
            for (int i = 0; i < worldExitPathsProperty.arraySize; i++)
            {
                SerializedProperty exitProperty = worldExitPathsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(exitProperty, new GUIContent($"World Exit Path {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Exit Path"))
            {
                worldCoordMap.CreateWorldExitPath();
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (worldExitPathsProperty.arraySize > 0)
                {
                    worldExitPathsProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        #endregion

        //======================================================== ////
        // WORLD ZONES
        //======================================================== ////
        EditorGUILayout.LabelField("World Zones", h1Style);
        EditorGUILayout.Space();

        // Display each WorldZone with a foldout
        SerializedProperty worldZonesProperty = serializedCoordinateMap.FindProperty("worldZones");
        if (worldZonesProperty != null)
        {
            for (int i = 0; i < worldZonesProperty.arraySize; i++)
            {
                SerializedProperty zoneProperty = worldZonesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(zoneProperty, new GUIContent($"World Zone {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Zone"))
            {
                worldCoordMap.CreateWorldZone();
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (worldZonesProperty.arraySize > 0)
                {
                    worldZonesProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }

        //======================================================== ////
        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedCoordinateMap.ApplyModifiedProperties();

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }
}
#endif