using UnityEditor;
using UnityEngine;

namespace Darklight.Unity.Backend
{
    public class AsyncTaskQueenProfiler : EditorWindow
    {
        public GameObject selectedGameObject;

        [MenuItem("Window/AsyncTaskQueen Profiler")]
        public static void ShowWindow()
        {
            GetWindow<AsyncTaskQueenProfiler>("AsyncTaskQueen Profiler");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            selectedGameObject = Selection.activeGameObject;
            Repaint();
        }

        private void OnGUI()
        {
            if (selectedGameObject == null)
            {
                EditorGUILayout.LabelField("No GameObject selected.");
                return;
            }

            AsyncTaskQueen taskQueen = selectedGameObject.GetComponent<AsyncTaskQueen>();

            if (taskQueen == null)
            {
                EditorGUILayout.LabelField("No AsyncTaskQueen found in the selected GameObject.");
                return;
            }

            EditorGUILayout.LabelField("AsyncTaskQueen Name: " + taskQueen.Name);

            foreach (var profilerData in taskQueen.ProfilerData)
            {
                EditorGUILayout.LabelField("Bot ID: " + profilerData.guidId);
                EditorGUILayout.LabelField("Bot Name: " + profilerData.name);
                EditorGUILayout.LabelField("Execution Time: " + profilerData.executionTime + " ms");
            }
        }
    }
}