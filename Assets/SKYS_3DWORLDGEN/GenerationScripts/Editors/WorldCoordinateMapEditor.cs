#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCoordinateMap))]
public class WorldCoordinateMapEditor : Editor
{
    SerializedObject serializedCoordinateMap;
    SerializedProperty worldExitPathsProperty;

    private void OnEnable()
    {
        serializedCoordinateMap = new SerializedObject(target);
        worldExitPathsProperty = serializedCoordinateMap.FindProperty("worldExitPaths");
    }

    public override void OnInspectorGUI()
    {
        serializedCoordinateMap.Update();

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        // Display each WorldExitPath with a foldout
        if (worldExitPathsProperty != null)
        {
            EditorGUILayout.LabelField("World Exit Paths", EditorStyles.boldLabel); // Optional: Add a section label

            for (int i = 0; i < worldExitPathsProperty.arraySize; i++)
            {
                SerializedProperty exitProperty = worldExitPathsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(exitProperty, new GUIContent($"World Exit Path {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Exit Path"))
            {
                worldExitPathsProperty.arraySize++;
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (worldExitPathsProperty.arraySize > 0)
                {
                    worldExitPathsProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedCoordinateMap.ApplyModifiedProperties();

            // Get your WorldCoordinateMap component
            WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;

            // Call your initialization method here
            worldCoordMap.InitializeWorldExits();

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
        worldCoordMap.InitializeWorldExits();
    }
}
#endif