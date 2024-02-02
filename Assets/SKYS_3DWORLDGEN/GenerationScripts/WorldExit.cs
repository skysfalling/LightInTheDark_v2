using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WorldDirection { West, East, North, South }

// =================================================================
//      WORLD EXIT CLASS
// ========================================================
[System.Serializable]
public class WorldExit
{
    WorldCoordinate _coordinate;
    WorldCoordinate _pathConnection;

    public WorldDirection edgeDirection;
    public int edgeIndex;

    public WorldCoordinate Coordinate
    {
        get { return _coordinate; }
        set { _coordinate = value; }
    }

    public WorldCoordinate PathConnectionCoordinate
    {
        get { return _pathConnection; }
        set { _pathConnection = value; }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(WorldExit))]
public class WorldExitDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var directionRect = new Rect(position.x, position.y, position.width / 2, position.height);
        var indexRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, position.height);

        // Draw the direction field
        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("edgeDirection"), GUIContent.none);

        // << DRAW CUSTOM INDEX SLIDER >>>
        SerializedProperty edgeIndexProp = property.FindPropertyRelative("edgeIndex");
        int maxIndex = WorldGeneration.PlayZoneArea.x - 1;
        edgeIndexProp.intValue = EditorGUI.IntSlider(indexRect, GUIContent.none, edgeIndexProp.intValue, 0, maxIndex);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif


