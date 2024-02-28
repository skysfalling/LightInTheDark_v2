using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldRegionEditorWindow : EditorWindow
{
    private WorldRegion selectedWorldRegion;

    [MenuItem("Window/DARKLIGHT/World Region Editor")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        GetWindow<WorldRegionEditorWindow>("World Region Editor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("World Region Inspector", EditorStyles.boldLabel);

        if (Selection.activeGameObject != null)
        {
            selectedWorldRegion = Selection.activeGameObject.GetComponent<WorldRegion>();
        }
        else
        {
            selectedWorldRegion = null;
        }

        if (selectedWorldRegion != null)
        {
            EditorGUILayout.LabelField("Region Coordinate:", selectedWorldRegion.regionCoordinate.ToString());
            EditorGUILayout.LabelField("Center Position:", selectedWorldRegion.centerPosition.ToString());
            EditorGUILayout.LabelField("Initialized:", selectedWorldRegion.IsInitialized().ToString());
        }
        else
        {
            EditorGUILayout.LabelField("No World Region selected.");
        }
    }

    private void OnSelectionChange()
    {
        Repaint();
    }
}
