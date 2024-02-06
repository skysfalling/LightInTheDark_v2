#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldChunkMap))]
public class WorldChunkMapEditor : Editor
{
    SerializedObject serializedObject;
    SerializedProperty zonesProperty;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target);
        zonesProperty = serializedObject.FindProperty("zones");

        WorldChunkMap map = (WorldChunkMap)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        // Display each WorldExitPath with a foldout
        if (zonesProperty != null)
        {
            EditorGUILayout.LabelField("World Zones", EditorStyles.boldLabel); // Optional: Add a section label

            for (int i = 0; i < zonesProperty.arraySize; i++)
            {
                SerializedProperty zoneProperty = zonesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(zoneProperty, new GUIContent($"World Zone {i}"), true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Zone"))
            {
                zonesProperty.arraySize++;
            }
            if (GUILayout.Button("Remove Last"))
            {
                if (zonesProperty.arraySize > 0)
                {
                    zonesProperty.arraySize--;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // Check if any changes were made in the Inspector
        if (EditorGUI.EndChangeCheck())
        {
            // If there were changes, apply them to the serialized object
            serializedObject.ApplyModifiedProperties();

            // Optionally, mark the target object as dirty to ensure the changes are saved
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        WorldChunkMap map = (WorldChunkMap)target;

        if (map.mapInitialized == false) { return; }

        List<WorldChunk> chunkMap = WorldChunkMap.GetChunkMap();
        if (chunkMap.Count > 0)
        {
            foreach (WorldChunk chunk in chunkMap)
            {
                Color zoneColorRGBA = WorldZone.GetRGBAfromZoneColorType(chunk.zoneColor);
                DrawRectangleArea(chunk.GroundPosition, WorldGeneration.GetRealChunkAreaSize(), zoneColorRGBA);

                Color pathColorRGBA = WorldPath.GetRGBAfromPathColorType(chunk.pathColor);
                DrawRectangleArea(chunk.GroundPosition, WorldGeneration.GetRealChunkAreaSize(), pathColorRGBA);
            }
        }
    }

    private void DrawRectangleArea(Vector3 worldPos, Vector2Int area, Color fillColor)
    {
        Handles.color = fillColor;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(worldPos, area), fillColor, Color.clear);
    }

    private Vector3[] GetRectangleVertices(Vector3 center, Vector2 area)
    {
        Vector2 halfArea = area * 0.5f;
        Vector3[] vertices = new Vector3[4]
        {
            center + new Vector3(-halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, -halfArea.y),
            center + new Vector3(halfArea.x, 0, halfArea.y),
            center + new Vector3(-halfArea.x, 0, halfArea.y)
        };

        return vertices;
    }
}
#endif