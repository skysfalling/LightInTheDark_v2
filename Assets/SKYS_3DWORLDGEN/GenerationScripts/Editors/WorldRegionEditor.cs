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
        EditorGUILayout.LabelField("Origin Coordinate Position:", region.originCoordinatePosition.ToString());
        EditorGUILayout.LabelField("Region Initialized:", region.IsInitialized().ToString());

        if (region.coordinateMap != null)
        {
            EditorGUILayout.LabelField("Coordinate Map Initialized:", region.coordinateMap.coordMapInitialized.ToString());
        }


        EditorGUILayout.Space(25);
        if (GUILayout.Button("Initialize Coordinate Map"))
        {
            region.Initialize(region.regionCoordinate);
        }
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = region.GetComponentInParent<WorldGeneration>();
        Transform transform = region.transform;

        DarklightGizmos.DrawWireSquare_withLabel("World Generation", worldGen.transform.position, WorldGeneration.GetWorldWidth_inWorldSpace(), Color.black);
        DarklightGizmos.DrawWireSquare_withLabel("World Region", transform.position, WorldGeneration.GetFullRegionWidth_inWorldSpace(), Color.blue);
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Chunk", transform.position, WorldGeneration.GetChunkWidth_inWorldSpace());
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Cell", transform.position, WorldGeneration.CellWidth_inWorldSpace);

        if (region != null && region.coordinateMap != null)
        {
            if (region.coordinateMap.coordinates != null)
            {
                foreach (Coordinate coord in region.coordinateMap.coordinates)
                {
                    DarklightGizmos.DrawWireSquare_withLabel($"{coord.LocalCoordinate} \n {coord.GetAllValidNeighbors().Count} neighbors", 
                        coord.WorldPosition, WorldGeneration.GetChunkWidth_inWorldSpace(), Color.green);
                }
            }
        }

        DarklightGizmos.DrawWireSquare_withLabel($"origin", region.originCoordinatePosition, 10, Color.red);

    }
}
