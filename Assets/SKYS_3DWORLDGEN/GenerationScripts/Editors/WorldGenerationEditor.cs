using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    private SerializedObject serializedWorldGen;
    private bool toggleBoundaries;

    private void OnEnable()
    {
        // Cache the SerializedObject
        serializedWorldGen = new SerializedObject(target);
        WorldGeneration.InitializeRandomSeed();
    }

    public override void OnInspectorGUI()
    {
        serializedWorldGen.Update(); // Always start with this call

        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        SerializedProperty gameSeedProperty = serializedWorldGen.FindProperty("gameSeed");
        EditorGUILayout.PropertyField(gameSeedProperty);

        EditorGUILayout.LabelField("============");
        EditorGUILayout.LabelField("CurrentSeed", WorldGeneration.CurrentSeed.ToString());


        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedWorldGen.ApplyModifiedProperties();

            WorldGeneration.InitializeRandomSeed(gameSeedProperty.stringValue);

            WorldGeneration worldGen = (WorldGeneration)target;

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
    }
}