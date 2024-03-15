using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ThirdPersonCamera))]
public class ThirdPersonCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ThirdPersonCamera cameraScript = (ThirdPersonCamera)target;

        serializedObject.Update();

        // Detect changes to the serialized properties
        EditorGUI.BeginChangeCheck();

        // Draw the default inspector
        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            // If something changed, apply the changes and update the camera position
            serializedObject.ApplyModifiedProperties();
            cameraScript.SetToEditorValues();
        }
    }
}
