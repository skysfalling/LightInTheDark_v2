using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UIElements;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WorldCoordinateMap), typeof(WorldChunkMap), typeof(WorldCellMap))]
public class WorldMap : MonoBehaviour
{
    Coroutine initializationCoroutine = null;

    public void InitializeWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
    }

    public IEnumerator InitializeWorldMapRoutine()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        yield return null;
    }

    public void ResetWorldMap()
    {
        if (initializationCoroutine != null) {
            StopCoroutine(initializationCoroutine);
        }

        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        worldCoordinateMap.ResetCoordinateMap();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    private bool showWorldCoordinateMap = true;
    private bool showWorldChunkMap = true;
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WorldMap worldMap = (WorldMap)target;
        WorldCoordinateMap worldCoordinateMap = worldMap.GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = worldMap.GetComponent<WorldChunkMap>();

        // Control the size of each box representing a coordinate
        float mapGUIBoxSize = 10f;
        int mapWidth = WorldGeneration.GetFullWorldArea().x;
        int mapHeight = WorldGeneration.GetFullWorldArea().y;

        // Begin a scroll view to handle maps that won't fit in the inspector window
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mapWidth * (mapGUIBoxSize * 1.5f)), GUILayout.Height(mapHeight * (mapGUIBoxSize * 1.5f)));

        // Create a flexible layout to start drawing the map
        GUILayout.BeginVertical();
        for (int y = 0; y < mapHeight; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < mapWidth; x++)
            {
                WorldCoordinate worldCoord = WorldCoordinateMap.GetCoordinate(new Vector2Int(x, y));
                if (worldCoord != null)
                {
                    // Draw a box for each type of coordinate with different colors
                    Color color = GetColorForCoordinateType(worldCoord.type);
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
                    EditorGUI.DrawRect(rect, color);
                }
                else
                {
                    // Draw a default box for null coordinates
                    GUILayout.Box("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();


        /*
        showWorldCoordinateMap = EditorGUILayout.Foldout(showWorldCoordinateMap, "World Coordinate Map Details");
        if (showWorldCoordinateMap)
        {
            EditorGUILayout.LabelField($"Initialized: {worldCoordinateMap.mapInitialized}");
            EditorGUILayout.LabelField($"World Paths Initialized: {worldCoordinateMap.pathsInitialized}");
            EditorGUILayout.LabelField($"Count: {worldCoordinateMap.worldExitPaths.Count}");
            // Add more details or actions here
        }

        showWorldChunkMap = EditorGUILayout.Foldout(showWorldChunkMap, "World Chunk Map Details");
        if (showWorldChunkMap)
        {
            EditorGUILayout.LabelField($"Initialized: {worldChunkMap.mapInitialized}");
            EditorGUILayout.LabelField($"World Zones Initialized: {worldChunkMap.zonesInitialized}");
            EditorGUILayout.LabelField($"Count: {worldChunkMap.zones.Count}");
            // Add more details or actions here
        }
        */

        // ================================================= >>

        if (GUILayout.Button("Update"))
        {

        }

        if (GUILayout.Button("Reset"))
        {
            worldMap.ResetWorldMap();
        }


        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            //worldMap.InitializeWorldMap();
            EditorUtility.SetDirty(target);
        }
    }



    private Color GetColorForCoordinateType(WorldCoordinate.TYPE type)
    {
        return type switch
        {
            WorldCoordinate.TYPE.NULL => Color.grey,
            WorldCoordinate.TYPE.PATH => Color.yellow,
            WorldCoordinate.TYPE.ZONE => Color.green,
            WorldCoordinate.TYPE.BORDER => Color.red,
            WorldCoordinate.TYPE.CLOSED => Color.black,
            _ => Color.white,
        };
    }

}
#endif
