using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    private SerializedObject serializedWorldGen;
    private bool toggleBoundaries;

    GUIStyle centeredStyle;

    private void OnEnable()
    {
        // Cache the SerializedObject
        serializedWorldGen = new SerializedObject(target);
        WorldGeneration.InitializeRandomSeed();
    }

    public override void OnInspectorGUI()
    {
        serializedWorldGen.Update(); // Always start with this call

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        #region GENERATION PARAMETERS ===========================================
        // Store the current state to check for changes later
        EditorGUI.BeginChangeCheck();

        SerializedProperty gameSeedProperty = serializedWorldGen.FindProperty("gameSeed");
        EditorGUILayout.PropertyField(gameSeedProperty);
        EditorGUILayout.LabelField("Encoded Seed", WorldGeneration.CurrentSeed.ToString());
        EditorGUILayout.LabelField("============///");


        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical();

        //DarklightEditor.CreateIntegerControl("Cell Size", WorldGeneration.CellWidth_inWorldSpace, 1, 8, (value) => WorldGeneration.CellWidth_inWorldSpace = value);
        //DarklightEditor.CreateIntegerControl("Chunk Width", WorldGeneration.ChunkWidth_inCells, 1, 10, (value) => WorldGeneration.ChunkWidth_inCells = value);
        //DarklightEditor.CreateIntegerControl("Chunk Depth", WorldGeneration.ChunkDepth_inCells, 1, 10, (value) => WorldGeneration.ChunkDepth_inCells = value);

        DarklightEditor.CreateIntegerControl("Playable Area", WorldGeneration.PlayRegionWidth_inChunks, 1, 10, (value) => WorldGeneration.PlayRegionWidth_inChunks = value);
        DarklightEditor.CreateIntegerControl("Boundary Offset", WorldGeneration.PlayRegionBoundaryOffset, 1, 10, (value) => WorldGeneration.PlayRegionBoundaryOffset = value);
        DarklightEditor.CreateIntegerControl("Max Ground Height", WorldGeneration.RegionMaxGroundHeight, 1, 10, (value) => WorldGeneration.RegionMaxGroundHeight = value);

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        #endregion

        WorldGeneration worldGen = (WorldGeneration)target;

        if (GUILayout.Button("Create Regions"))
        {
            worldGen.CreateRegions();
        }

        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedWorldGen.ApplyModifiedProperties();

            WorldGeneration.InitializeRandomSeed(gameSeedProperty.stringValue);

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }

        DrawDefaultInspector();
    }

    void OnSceneGUI()
    {
        WorldGeneration worldGen = (WorldGeneration)target;
        Transform transform = worldGen.transform;

        DarklightGizmos.DrawWireSquare_withLabel("World Generation", transform.position, WorldGeneration.GetWorldWidth_inWorldSpace());
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Chunk", transform.position, WorldGeneration.GetChunkWidth_inWorldSpace());
        //DarklightEditor.DrawWireRectangle_withWidthLabel("World Cell", transform.position, WorldGeneration.CellWidth_inWorldSpace);


        if (worldGen.worldRegions.Count > 0)
        {
            foreach (WorldRegion region in worldGen.worldRegions)
            {
                if (region != null && region.IsInitialized())
                {
                    DarklightGizmos.DrawWireSquare_withLabel($"World Region {region.regionCoordinate}", region.centerPosition, WorldGeneration.GetFullRegionWidth_inWorldSpace());
                }
            }
        }
    }


}