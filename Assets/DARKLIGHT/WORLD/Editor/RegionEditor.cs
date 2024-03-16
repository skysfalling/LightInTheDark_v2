namespace Darklight.World.Editor
{
    using Darklight.World.Generation;
    using Bot;
    using Builder;
    using Bot.Editor;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class RegionEditor : WorldEditor
    {
        public RegionBuilder regionBuilder => GetComponent<RegionBuilder>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RegionEditor))]
    public class RegionEditorGUI : WorldEditorGUI
    {
        private SerializedObject _serializedObject;
        private RegionEditor _regionEditorScript;
        private RegionBuilder _regionBuilder;

        public override void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _regionEditorScript = (RegionEditor)target;
            _regionBuilder = _regionEditorScript.regionBuilder;
        }

        public override void OnInspectorGUI()
        {
            _regionBuilder = _regionEditorScript.regionBuilder;
            _serializedObject.Update();
            if (_regionBuilder == null)
            {
                EditorGUILayout.HelpBox("RegionBuilder not found", MessageType.Error);
            }
            else
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty("regionView"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty("coordinateMapView"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty("chunkMapView"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty("chunkView"));


            }
            _serializedObject.ApplyModifiedProperties(); // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        public void OnSceneGUI()
        {
            if (_regionBuilder)
            {
                DrawRegion(_regionBuilder, _regionEditorScript);
            }
        }
    }
#endif
}