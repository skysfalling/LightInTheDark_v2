namespace Darklight.Game.CameraController
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ThirdPersonCamera))]
    public class ThirdPersonCameraEditor : Editor
    {
        ThirdPersonCamera cameraScript;

        public override void OnInspectorGUI()
        {
            cameraScript = (ThirdPersonCamera)target;
            if (!cameraScript.Initialized && GUILayout.Button("Initialize"))
            {
                cameraScript.Initialize();
            }
            else if (cameraScript.Initialized && GUILayout.Button("Reset"))
            {
                cameraScript.ResetCamera();
            }

            // Detect changes to the serialized properties
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            serializedObject.Update();

            if (EditorGUI.EndChangeCheck())
            {
                // If something changed, apply the changes and update the camera position
                serializedObject.ApplyModifiedProperties();
                cameraScript.SetToEditorValues();
            }
        }
    }
}
