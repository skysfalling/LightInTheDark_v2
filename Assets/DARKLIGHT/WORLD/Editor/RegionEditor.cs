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
        void Start()
        {
            SelectRegion(this.GetComponent<RegionBuilder>());
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RegionEditor))]
    public class RegionEditorGUI : WorldEditorGUI
    {
        private SerializedObject _serializedObject;
        private RegionEditor _regionEditor;


        public override void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _regionEditor = (RegionEditor)target;
        }

        public override void OnInspectorGUI()
        {
            // Make sure region is selected
            if (_regionEditor.selectedRegion == null)
            {
                _regionEditor.SelectRegion(_regionEditor.GetComponent<RegionBuilder>());
            }

            base.OnInspectorGUI();
        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
        }
    }
#endif
}