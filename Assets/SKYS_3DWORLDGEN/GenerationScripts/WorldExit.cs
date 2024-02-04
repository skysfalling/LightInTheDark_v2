using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;


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
    // == INITIALIZE COORDINATES >>
    WorldCoordinate _coordinate; // Coordinate on border
    WorldCoordinate _pathConnection; // Connecting Neighbor that is in the playArea

    public WorldCoordinate Coordinate
    {
        get { return _coordinate; }
        set { _coordinate = value; }
    }

    public WorldCoordinate PathConnectionCoord
    {
        get { return _pathConnection; }
        set { _pathConnection = value; }
    }

    // == EXIT VALUES >>
    public WorldDirection borderDirection;
    public int borderIndex;
    public WorldExit(WorldDirection borderDirection, int index)
    {
        this.borderDirection = borderDirection;
        this.borderIndex = index;
        this.Initialize();
    }

    public void Initialize()
    {
        _coordinate = WorldCoordinateMap.GetCoordinateAtWorldExit(this);
        if (_coordinate != null)
        {
            _coordinate.type = WorldCoordinate.TYPE.EXIT;
            _pathConnection = WorldCoordinateMap.GetWorldExitPathConnection(this);
        }
    }
}

// =================================================================
//      CUSTOM WORLD EXIT GUI
// ========================================================
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

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

        // Calculate rects for each field
        var halfWidth = position.width / 2 - 2;
        Rect directionRect = new Rect(position.x, position.y, halfWidth, singleLineHeight);
        Rect indexRect = new Rect(position.x, position.y + singleLineHeight + verticalSpacing, halfWidth, singleLineHeight);

        // Draw the "Border Direction" field
        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("borderDirection"), GUIContent.none);

        // Draw the "Border Index" slider
        SerializedProperty borderIndexProp = property.FindPropertyRelative("borderIndex");
        int maxIndex = Mathf.Max(0, WorldGeneration.PlayZoneArea.x - 1); // Ensure maxIndex is at least 0
        borderIndexProp.intValue = EditorGUI.IntSlider(indexRect, GUIContent.none, borderIndexProp.intValue, 0, maxIndex);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed by adding the height of two controls and the spacing between them
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif

