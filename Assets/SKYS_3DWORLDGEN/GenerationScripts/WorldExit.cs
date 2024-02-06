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
    bool _initialized;

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
    public int exitHeight;

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
            WorldChunkMap.GetChunkAtCoordinate(_coordinate).chunkHeight = exitHeight;

        }
        IsInitialized();
    }

    public bool IsInitialized() {

        // Check if exits and path are initialized
        if (_coordinate != null) 
        { 
            _initialized = true; 
        }
        else { _initialized = false; }

        return _initialized; 
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
        float verticalOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Calculate rects for each field
        var halfWidth = position.width / 2 - 2;
        Rect directionRect = new Rect(position.x - halfWidth, position.y + verticalOffset, position.width, singleLineHeight);
        Rect indexRect = new Rect(position.x - halfWidth, directionRect.y + verticalOffset, position.width, singleLineHeight);
        Rect exitHeightRect = new Rect(position.x - halfWidth, indexRect.y + verticalOffset, position.width, singleLineHeight);


        // Draw the "Border Direction" field
        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("borderDirection"), new GUIContent("borderDirection"));

        // Draw the "Border Index" slider
        SerializedProperty borderIndexProp = property.FindPropertyRelative("borderIndex");
        int maxIndex = Mathf.Max(0, WorldGeneration.PlayZoneArea.x - 1);
        borderIndexProp.intValue = EditorGUI.IntSlider(indexRect, new GUIContent("borderIndex"), borderIndexProp.intValue, 0, maxIndex);

        // Draw the "Chunk Height" slider
        SerializedProperty exitHeightProp = property.FindPropertyRelative("exitHeight");
        int maxHeight = Mathf.Max(0, WorldGeneration.MaxChunkHeight);
        exitHeightProp.intValue = EditorGUI.IntSlider(exitHeightRect, new GUIContent("exitHeight"), exitHeightProp.intValue, 0, maxHeight);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed by adding the height of three controls and the spacing between them
        return (EditorGUIUtility.singleLineHeight * 5) + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif

