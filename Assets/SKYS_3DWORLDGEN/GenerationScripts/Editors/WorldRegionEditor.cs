using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldRegion))]
public class WorldRegionEditor : Editor
{
    private SerializedObject serializedObject;
    private WorldRegion region;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target);
        region = (WorldRegion)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Region Coordinate:", region.regionCoordinate.ToString());
        EditorGUILayout.LabelField("Center Position:", region.centerPosition.ToString());
        EditorGUILayout.LabelField("Initialized:", region.IsInitialized().ToString());

        DrawCoordinates();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    public void DrawCoordinates()
    {


    }


    void OnSceneGUI()
    {
        WorldGeneration worldGen = region.GetComponentInParent<WorldGeneration>();
        Transform transform = region.transform;

        DarklightGizmos.DrawWireSquare_withLabel("World Generation", worldGen.transform.position, WorldGeneration.GetWorldWidth_inWorldSpace());
        DarklightGizmos.DrawWireSquare_withLabel("World Region", transform.position, WorldGeneration.GetFullRegionWidth_inWorldSpace());
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Chunk", transform.position, WorldGeneration.GetChunkWidth_inWorldSpace());
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Cell", transform.position, WorldGeneration.CellWidth_inWorldSpace);


        if (region != null && region.coordinateMap != null)
        {
            if (region.coordinateMap.coordinates != null)
            {
                foreach (Coordinate coord in region.coordinateMap.coordinates)
                {
                    DarklightGizmos.DrawWireSquare_withLabel($"{coord.LocalCoordinate}", coord.WorldPosition, WorldGeneration.GetChunkWidth_inWorldSpace());
                }
            }
        }
    }
}
