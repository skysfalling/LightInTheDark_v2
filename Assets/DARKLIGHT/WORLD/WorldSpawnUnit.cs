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
		public Vector3Int dimensions => new Vector3Int(x, y, z);
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(WorldSpawnUnit), true)]
	public class WorldSpawnUnitEditor : UnityEditor.Editor
	{
		private UnityEditor.Editor previewEditor;
		private WorldSpawnUnit spawnUnitScript;
		private void OnEnable()
		{
			// This method is called when the editor is created and whenever the selection changes.
			spawnUnitScript = (WorldSpawnUnit)target;
			previewEditor = null; // Reset the preview editor to ensure it updates when the selection changes.
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			spawnUnitScript = (WorldSpawnUnit)target;
			// Check and create a preview editor for the modelPrefab if one does not exist or if the prefab has changed.
			if (previewEditor == null || spawnUnitScript.modelPrefab != previewEditor.target)
			{
				// Clean up the old preview editor if it exists
				if (previewEditor != null)
				{
					previewEditor = null;
				}
				if (spawnUnitScript.modelPrefab != null) // Ensure there is a prefab to create an editor for.
				{
					previewEditor = CreateEditor(spawnUnitScript.modelPrefab);
				}
			}

			// If a preview editor exists, draw it.
			if (previewEditor != null)
			{
				previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(true)), GUIStyle.none);
			}

			DrawDefaultInspector();
			serializedObject.ApplyModifiedProperties();
		}
	}
#endif

}

