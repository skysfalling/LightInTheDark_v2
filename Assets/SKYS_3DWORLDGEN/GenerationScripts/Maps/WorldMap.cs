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
    public void UpdateWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();

        worldCoordinateMap.UpdateCoordinateMap();
        worldChunkMap.UpdateChunkMap();
    }

    public void ResetWorldMap()
    {
        WorldCoordinateMap worldCoordinateMap = GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = GetComponent<WorldChunkMap>();
        WorldCellMap worldCellMap = GetComponent<WorldCellMap>();

        worldCoordinateMap.DestroyCoordinateMap();
        worldChunkMap.DestroyChunkMap();
        worldCellMap.Reset();

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldMap))]
public class WorldMapEditor : Editor
{
    private bool showWorldCoordinateMap = true;
    private bool showWorldChunkMap = true;
    private Vector2 scrollPosition;

    public void OnEnable()
    {
        WorldMap worldMap = (WorldMap)target;
        worldMap.ResetWorldMap();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WorldMap worldMap = (WorldMap)target;
        WorldCoordinateMap worldCoordinateMap = worldMap.GetComponent<WorldCoordinateMap>();
        WorldChunkMap worldChunkMap = worldMap.GetComponent<WorldChunkMap>();

        WorldGeneration worldGeneration = FindObjectOfType<WorldGeneration>();

        // ================================================= >>

        UpdateGUIWorldMap();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            worldGeneration.StartGeneration();
        }

        if (GUILayout.Button("Update Map"))
        {
            worldMap.UpdateWorldMap();
        }

        if (GUILayout.Button("Full Reset"))
        {
            worldMap.ResetWorldMap();
        }
        EditorGUILayout.EndHorizontal();

        // ================================================= >>

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }
    }

    private void UpdateGUIWorldMap()
    {
        // Control the size of each box representing a coordinate
        float mapGUIBoxSize = 10f;
        int mapWidth = WorldGeneration.GetFullWorldArea().x;
        int mapHeight = WorldGeneration.GetFullWorldArea().y;

        // Begin a scroll view to handle maps that won't fit in the inspector window
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mapWidth * (mapGUIBoxSize * 1.5f)), GUILayout.Height(mapHeight * (mapGUIBoxSize * 1.5f)));

        // Create a flexible layout to start drawing the map
        GUILayout.BeginVertical();
        for (int y = mapHeight - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < mapWidth; x++)
            {
                if (WorldCoordinateMap.coordMapInitialized)
                {
                    WorldCoordinate worldCoord = WorldCoordinateMap.GetCoordinateAt(new Vector2Int(x, y));
                    if (worldCoord != null)
                    {
                        // Draw a box for each type of coordinate with different colors
                        Color color = GetColorForCoordinateType(worldCoord.type);
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
                        EditorGUI.DrawRect(rect, color);
                        continue; // continue to next coordinate
                    }
                }

                // Draw a default box for null coordinates
                GUILayout.Box("", GUILayout.Width(mapGUIBoxSize), GUILayout.Height(mapGUIBoxSize));
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
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
