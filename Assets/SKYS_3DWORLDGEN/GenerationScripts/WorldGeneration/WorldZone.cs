using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    public enum TYPE { FULL, NATURAL, HORIZONTAL, VERTICAL }
    TYPE _storedZoneType = TYPE.FULL;
    WorldCoordinate _centerCoordinate;
    List<WorldCoordinate> _zoneCoordinates = new List<WorldCoordinate>();
    bool _initialized = false;


    // INSPECTOR VARIABLES
    public TYPE zoneType = TYPE.FULL;
    public Vector2Int coordinateVector = Vector2Int.one;


    public WorldZone()
    {
        this._centerCoordinate = WorldCoordinateMap.GetCoordinateAt(coordinateVector);
        this.zoneType = TYPE.FULL;
        Update();
    }

    public WorldZone( WorldCoordinate centerCoordinate, TYPE zoneType )
    {
        this._centerCoordinate = centerCoordinate;
        this.coordinateVector = _centerCoordinate.Coordinate;
        this.zoneType = zoneType;
        Update();
    }

    public void Update()
    {
        // Reassign the coordinate types
        if (_zoneCoordinates.Count > 0) { WorldCoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, WorldCoordinate.TYPE.ZONE); }

        // << INITIALIZE >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        if ( _initialized ) { return; }
        _initialized = false;

        if (WorldCoordinateMap.coordMapInitialized == false) { return; }

        // Get affected neighbors
        List<WorldCoordinate> affectedNeighbors = new();
        switch(this._storedZoneType)
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

        _zoneCoordinates = new List<WorldCoordinate> { _centerCoordinate };
        _zoneCoordinates.AddRange(affectedNeighbors);

        // Assign Zone TYPE
        WorldCoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, WorldCoordinate.TYPE.ZONE);

        _initialized = true;
    }

    public void Reset()
    {
        if (WorldCoordinateMap.coordMapInitialized == false || !_initialized) { return; }

        // IF VALUES CHANGED
        if (_centerCoordinate == null || _centerCoordinate.Coordinate != coordinateVector 
            || zoneType != _storedZoneType)
        {
            WorldCoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, WorldCoordinate.TYPE.NULL);
            _zoneCoordinates.Clear();

            // Update private variables
            _centerCoordinate = WorldCoordinateMap.GetCoordinateAt(coordinateVector);
            this._storedZoneType = zoneType;
            _initialized = false;
        }
    }

    public List<WorldCoordinate> GetZoneCoordinates() { return _zoneCoordinates; }

    public bool IsInitialized() {

        if (_zoneCoordinates.Count > 0)
        {
            // Check for treason ...
            foreach (WorldCoordinate coord in _zoneCoordinates)
            {
                if (coord != null && coord.type != WorldCoordinate.TYPE.ZONE)
                {
                    _initialized = false;
                    Update();
                    break;
                }
            }
        }


        return _initialized; 
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
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("zoneType"), new GUIContent("Zone Type"));

        SerializedProperty coordProp = property.FindPropertyRelative("coordinateVector");
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