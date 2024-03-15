namespace Darklight.World
{
	using UnityEngine;
	using UnityEditor;

	[CreateAssetMenu(fileName = "NewSpawnUnit", menuName = "World/New Spawn Unit", order = 1)]
	public class WorldSpawnUnit : ScriptableObject
	{
		public UnitSpace UnitSpace = UnitSpace.CHUNK;
		public GameObject modelPrefab;
		[Range(1, 5)]
		public int x, y, z = 0;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(WorldSpawnUnit), true)]
	public class WorldSpawnUnitEditor : UnityEditor.Editor
	{
		private UnityEditor.Editor _previewEditor;
		private WorldSpawnUnit _spawnUnitScript;
		public GameObject previewObject;

		void OnEnable()
		{
			_spawnUnitScript = (WorldSpawnUnit)target;
			Repaint();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_spawnUnitScript = (WorldSpawnUnit)target;
			EditorGUI.BeginChangeCheck();

			if (_spawnUnitScript.modelPrefab == null)
			{
				previewObject = null;
				_previewEditor = null;
			}
			else if (_spawnUnitScript.modelPrefab != previewObject)
			{
				_previewEditor = null;
				previewObject = _spawnUnitScript.modelPrefab;
			}


			EditorGUILayout.BeginHorizontal();
			if (_previewEditor == null)
			{
				_previewEditor = CreateEditor(previewObject);
			}
			_previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false)), GUIStyle.none);


			EditorGUILayout.BeginVertical();
			base.OnInspectorGUI();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();


				Repaint();
			}
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

