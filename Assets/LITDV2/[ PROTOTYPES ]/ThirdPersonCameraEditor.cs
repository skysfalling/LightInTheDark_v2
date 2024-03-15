using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Darklight.Game.CameraController
{
    [CustomEditor(typeof(ThirdPersonCamera))]
    public class ThirdPersonCameraEditor : Editor
    {
        ThirdPersonCamera cameraScript;

        public void OnEnable()
        {
            cameraScript = (ThirdPersonCamera)target;
        }
        
        public override void OnInspectorGUI()
        {
            if (!cameraScript.Initialized && GUILayout.Button("Initialize"))
            {
                cameraScript.Initialize();
            }
            else if (cameraScript.Initialized && GUILayout.Button("Reset"))
            {
                cameraScript.ResetCamera();
            }

            DrawDefaultInspector();

            // Detect changes to the serialized properties
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            if (EditorGUI.EndChangeCheck())
            {
                // If something changed, apply the changes and update the camera position
                serializedObject.ApplyModifiedProperties();
                cameraScript.Initialize();
            }
        }
    }
}
