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

    public WorldDirection borderDirection;
    public int borderIndex;

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

        // Calculate the height for each property field, considering a small space between them
        float propertyHeight = EditorGUIUtility.singleLineHeight;
        float spaceBetween = EditorGUIUtility.standardVerticalSpacing;

        // Adjust position rect for the first property
        Rect directionRect = new Rect(position.x, position.y, position.width, propertyHeight);
        Rect indexRect = new Rect(position.x, directionRect.y + propertyHeight + spaceBetween, position.width, propertyHeight);


        // Draw the direction field
        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("borderDirection"), new GUIContent("Border Direction"));

        // << DRAW CUSTOM INDEX SLIDER >>>
        SerializedProperty edgeIndexProp = property.FindPropertyRelative("borderIndex");
        int maxIndex = WorldGeneration.PlayZoneArea.x - 1;
        edgeIndexProp.intValue = EditorGUI.IntSlider(indexRect, GUIContent.none, edgeIndexProp.intValue, 0, maxIndex);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed for the custom layout
        float totalHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2 - EditorGUIUtility.standardVerticalSpacing + 2f; // For two properties, plus any extra padding
        return totalHeight;
    }
}
#endif


