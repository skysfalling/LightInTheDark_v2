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

        WorldCoordinateMap worldCoordMap = (WorldCoordinateMap)target;
        worldCoordMap.InitializeWorldExits();

        if (worldExitPathsProperty != null)
        {
            for (int i = 0; i < worldExitPathsProperty.arraySize; i++)
            {
                SerializedProperty exitProperty = worldExitPathsProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(exitProperty, new GUIContent($"World Exit Path {i}"), true);

                // Additional custom UI elements can be added here
            }

            // Optionally, add buttons for adding/removing WorldExit objects from the list
            if (GUILayout.Button("Add New WorldExitPath"))
            {
                worldExitPathsProperty.arraySize++;
            }
        }


        serializedCoordinateMap.ApplyModifiedProperties(); // Apply changes to the serialized object
    }
}
#endif