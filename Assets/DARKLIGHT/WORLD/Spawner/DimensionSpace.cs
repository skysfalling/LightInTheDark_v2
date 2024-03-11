using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using Unity.VisualScripting;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Generation
{
    public class DimensionSpace : MonoBehaviour
    {
        WorldBuilder builder = WorldBuilder.Instance;
        public UnitSpace UnitSpace = UnitSpace.CHUNK;
        public Vector3 dimensionSpace = Vector3.one;
        public GameObject PreviewObject { get; private set;}

        public void SetPreviewObject(GameObject obj)
        {
            PreviewObject = obj;
        }
    }

#if UNITY_EDITOR
[CustomEditor(typeof(DimensionSpace))]
public class DimensionSpacerEditor : Editor
{
    private Editor _previewEditor;
	private DimensionSpace _dimensionSpaceScript;
	void OnEnable()
    {
	    _dimensionSpaceScript = (DimensionSpace)target;
    }

    public override void OnInspectorGUI()
    {

        EditorGUILayout.BeginHorizontal();
		GameObject previewObject = _dimensionSpaceScript.PreviewObject;
        if (previewObject != null)
        {
            if (_previewEditor == null)
                _previewEditor = Editor.CreateEditor(previewObject);

   			 _previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false)), GUIStyle.none);
        }

        EditorGUILayout.BeginVertical();
	    base.OnInspectorGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();


    }

    private void OnDestroy()
    {
        // Clean up the editor when the window is closed
        if (_previewEditor != null)
        {
            DestroyImmediate(_previewEditor);
        }
    }
}

#endif

}

