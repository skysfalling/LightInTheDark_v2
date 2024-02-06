using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WorldCoordinateMap), typeof(WorldChunkMap), typeof(WorldCellMap))]
public class WorldMap : MonoBehaviour
{
    public void InitializeWorldMap()
    {
        StartCoroutine(InitializeWorldMapRoutine());
    }

    public IEnumerator InitializeWorldMapRoutine()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        worldCoordinateMap.InitializeCoordinateMap();
        yield return new WaitUntil(() => worldCoordinateMap.mapInitialized);
        //Debug.Log($"World Coordinate Map Initialized :: {WorldCoordinateMap.GetCoordinateMap().Count} Coordinates");

        worldChunkMap.InitializeChunkMap();
        yield return new WaitUntil(() => worldChunkMap.mapInitialized);
        //Debug.Log($"World Chunk Map Initialized :: {WorldChunkMap.GetChunkMap().Count} Chunks");

        worldChunkMap.InitializeZones();
        yield return new WaitUntil(() => worldChunkMap.zonesInitialized);
        //Debug.Log($"World Zones Initialized :: {worldChunkMap.zones.Count} Zones");

        worldCoordinateMap.InitializeWorldExitPaths();
        yield return new WaitUntil(() => worldCoordinateMap.pathsInitialized);
        //Debug.Log($"World Exit Paths Initialized :: {worldCoordinateMap.worldExitPaths.Count} Paths");

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WorldMap worldMap = (WorldMap)target;
        WorldCoordinateMap worldCoordinateMap = worldMap.GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = worldMap.GetComponent<WorldChunkMap>();


        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Initialize Zones"))
            {
                worldChunkMap.InitializeZones();
            }
            if (GUILayout.Button("Initialize World Exit Paths"))
            {
                worldCoordinateMap.InitializeWorldExitPaths();
            }
            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }
}



#endif
