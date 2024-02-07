using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class WorldZone
{
    public enum ZoneColor { BLACK, WHITE, RED, YELLOW, GREEN, BLUE, CLEAR }
    public ZoneColor zoneColor = ZoneColor.GREEN;
    public static Color GetRGBAfromZoneColorType(ZoneColor pathColor)
    {
        switch (pathColor)
        {
            case ZoneColor.BLACK:
                return Color.black;
            case ZoneColor.WHITE:
                return Color.white;
            case ZoneColor.RED:
                return Color.red;
            case ZoneColor.YELLOW:
                return Color.yellow;
            case ZoneColor.GREEN:
                return Color.green;
            case ZoneColor.BLUE:
                return Color.blue;
            default:
                return Color.clear;
        }
    }

    WorldCoordinate _centerCoordinate;
    List<WorldCoordinate> _zoneCoordinates;

    public enum TYPE { FULL, NATURAL, HORIZONTAL, VERTICAL }
    public TYPE type = TYPE.FULL;
    public Vector2Int coord;


    public WorldZone( WorldCoordinate centerCoordinate, TYPE zoneType )
    {
        this._centerCoordinate = centerCoordinate;
        this.type = zoneType;
    }

    public void Initialize()
    {
        this._centerCoordinate = WorldCoordinateMap.GetCoordinate(coord);

        List<WorldCoordinate> affectedNeighbors = new();
        switch(this.type)
        {
            case TYPE.FULL:
                affectedNeighbors = WorldCoordinateMap.GetAllCoordinateNeighbors(_centerCoordinate);
                break;
            case TYPE.NATURAL:
                affectedNeighbors = WorldCoordinateMap.GetCoordinateNaturalNeighbors(_centerCoordinate);
                break;
            case TYPE.HORIZONTAL:
                affectedNeighbors.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.West));
                affectedNeighbors.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.East));
                break;
            case TYPE.VERTICAL:
                affectedNeighbors.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.North));
                affectedNeighbors.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.South));
                break;
        }

        _zoneCoordinates = new List<WorldCoordinate> { _centerCoordinate};
        _zoneCoordinates.AddRange(affectedNeighbors);

        string debugStr = "Set Zones";
        foreach(WorldCoordinate coordinate in _zoneCoordinates)
        {
            debugStr += $"\n coordinate : {coordinate.Coordinate} {coordinate.type} -> ZONE";

            coordinate.type = WorldCoordinate.TYPE.ZONE;

            WorldChunk chunk = WorldChunkMap.GetChunkAtCoordinate(coordinate);
            chunk.zoneColor = this.zoneColor;
        }

        Debug.Log(debugStr);
    }
}

// =================================================================
//      CUSTOM WORLD ZONE GUI
// ========================================================
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(WorldZone))]
public class WorldZoneDrawer : PropertyDrawer
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
        Rect colorRect = new Rect(position.x - halfWidth, position.y + verticalOffset, position.width, singleLineHeight);
        Rect typeRect = new Rect(position.x - halfWidth, position.y + (2 * verticalOffset), position.width, singleLineHeight);
        Rect coordXRect = new Rect(position.x - halfWidth, position.y + (3 * verticalOffset), position.width, singleLineHeight);
        Rect coordYRect = new Rect(position.x - halfWidth, position.y + (4 * verticalOffset), position.width, singleLineHeight);


        EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("zoneColor"), new GUIContent("Zone Color"));

        // Draw the "ZoneType" field
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), new GUIContent("Zone Type"));

        SerializedProperty coordProp = property.FindPropertyRelative("coord");
        int x = EditorGUI.IntSlider(coordXRect, new GUIContent("Coord X"), coordProp.vector2IntValue.x, 0, WorldGeneration.PlayZoneArea.x);
        int y = EditorGUI.IntSlider(coordYRect, new GUIContent("Coord Y"), coordProp.vector2IntValue.y, 0, WorldGeneration.PlayZoneArea.y);
        coordProp.vector2IntValue = new Vector2Int(x, y);

        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed by adding the height of three controls and the spacing between them
        return (EditorGUIUtility.singleLineHeight * 6) + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
