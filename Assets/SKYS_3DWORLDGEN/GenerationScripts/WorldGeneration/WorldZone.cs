using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class WorldZone
{
    public DebugColor zoneColor = DebugColor.GREEN;
    public static Color GetRGBAfromDebugColor(DebugColor zoneColor)
    {
        switch (zoneColor)
        {
            case DebugColor.BLACK:
                return Color.black;
            case DebugColor.WHITE:
                return Color.white;
            case DebugColor.RED:
                return Color.red;
            case DebugColor.YELLOW:
                return Color.yellow;
            case DebugColor.GREEN:
                return Color.green;
            case DebugColor.BLUE:
                return Color.blue;
            default:
                return Color.clear;
        }
    }

    public enum TYPE { FULL, NATURAL, HORIZONTAL, VERTICAL }

    bool _initialized = false;
    TYPE _zoneType;
    WorldCoordinate _centerCoordinate;
    Vector2Int _coordinateVector;
    int _zoneHeight;
    List<WorldCoordinate> _zoneCoordinates = new List<WorldCoordinate>();


    // PUBLIC INSPECTOR VARIABLES
    public TYPE zoneType = TYPE.FULL;
    public Vector2Int coordinateVector = Vector2Int.one;
    public int zoneHeight = 0;

    public WorldZone( WorldCoordinate centerCoordinate, TYPE zoneType )
    {
        this.coordinateVector = centerCoordinate.Coordinate;
        this.zoneType = zoneType;

        Initialize();
    }

    public void Initialize()
    {
        if (_initialized ) { return; }
        _initialized = false;

        // Update private variables
        _coordinateVector = coordinateVector;
        _centerCoordinate = WorldCoordinateMap.GetCoordinateAt(coordinateVector);
        _zoneType = zoneType;
        _zoneHeight = zoneHeight;

        // Get affected neighbors
        List<WorldCoordinate> neighborsInZone = new();
        switch(_zoneType)
        {
            case TYPE.FULL:
                neighborsInZone = WorldCoordinateMap.GetAllCoordinateNeighbors(_centerCoordinate);
                break;
            case TYPE.NATURAL:
                neighborsInZone = WorldCoordinateMap.GetCoordinateNaturalNeighbors(_centerCoordinate);
                break;
            case TYPE.HORIZONTAL:
                neighborsInZone.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.WEST));
                neighborsInZone.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.EAST));
                break;
            case TYPE.VERTICAL:
                neighborsInZone.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.NORTH));
                neighborsInZone.Add(WorldCoordinateMap.GetCoordinateNeighborInDirection(_centerCoordinate, WorldDirection.SOUTH));
                break;
        }

        // Assign Zone Coordinates
        _zoneCoordinates = new List<WorldCoordinate> { _centerCoordinate };
        _zoneCoordinates.AddRange(neighborsInZone);

        // Assign Chunk Heights
        WorldChunkMap.SetChunksToHeightFromCoordinates(_zoneCoordinates, zoneHeight);

        // Assign Zone TYPE
        WorldCoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, WorldCoordinate.TYPE.ZONE, GetRGBAfromDebugColor(zoneColor));

        _initialized = true;
        Debug.Log($"Initialized WORLD ZONE : {_coordinateVector} : height {zoneHeight}");
    }

    public void Reset()
    {
        _initialized = false;
    }

    public bool IsInitialized() {

        // Check private variables
        if ( _centerCoordinate.Coordinate != coordinateVector
            || _coordinateVector != coordinateVector
            || _zoneHeight != zoneHeight
            || _zoneType != zoneType
            || _zoneCoordinates.Count == 0)
        {
            _initialized = false;
        }
        else if (_zoneCoordinates.Count > 0)
        {
            foreach (WorldCoordinate coord in _zoneCoordinates)
            {
                if (WorldCoordinateMap.GetCoordinateAt(coord.Coordinate).type != WorldCoordinate.TYPE.ZONE)
                {
                    _initialized = false;
                }
            }
        }
        else
        {
            _initialized = true;
        }

        Debug.Log($">> World Zones isInitialized? : {_initialized}" +
            $"\n_centerCoordinate {_centerCoordinate.Coordinate}" +
            $"\n_coordinateVector {_coordinateVector}" +
            $"\n_zoneHeight {_zoneHeight}" +
            $"\n_zoneType {_zoneType}" +
            $"\n_zoneCoordinates {_zoneCoordinates.Count}");

        return _initialized; 
    }


    public List<WorldCoordinate> GetZoneCoordinates() { return _zoneCoordinates; }
    public List<WorldChunk> GetZoneChunks()
    {
        if (_zoneCoordinates == null || _zoneCoordinates.Count == 0) { return new List<WorldChunk>(); }
        return WorldChunkMap.GetChunksAtCoordinates(_zoneCoordinates);
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
        Rect chunkHeightRect = new Rect(position.x - halfWidth, position.y + (5 * verticalOffset), position.width, singleLineHeight);


        EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("zoneColor"), new GUIContent("Zone Color"));

        // Draw the "ZoneType" field
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("zoneType"), new GUIContent("Zone Type"));

        // Coordinate Vector
        SerializedProperty coordProp = property.FindPropertyRelative("coordinateVector");
        int x = EditorGUI.IntSlider(coordXRect, new GUIContent("Coord X"), coordProp.vector2IntValue.x, 0, WorldGeneration.PlayZoneArea.x);
        int y = EditorGUI.IntSlider(coordYRect, new GUIContent("Coord Y"), coordProp.vector2IntValue.y, 0, WorldGeneration.PlayZoneArea.y);
        coordProp.vector2IntValue = new Vector2Int(x, y);

        // Chunk Height
        SerializedProperty chunkHeightProp = property.FindPropertyRelative("zoneHeight");
        chunkHeightProp.intValue = EditorGUI.IntSlider(chunkHeightRect, new GUIContent("Zone Height"), chunkHeightProp.intValue, 0, WorldGeneration.PlayZoneArea.x);

        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed by adding the height of three controls and the spacing between them
        return (EditorGUIUtility.singleLineHeight * 8) + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
