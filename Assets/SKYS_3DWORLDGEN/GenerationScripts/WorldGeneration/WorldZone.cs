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

    public enum TYPE { FULL, NATURAL_CROSS, DIAGONAL_CROSS, HORIZONTAL, VERTICAL }

    bool _initialized = false;
    TYPE _zoneType;
    Coordinate _centerCoordinate;
    Vector2Int _coordinateVector;
    int _zoneHeight;
    List<Coordinate> _zoneCoordinates = new List<Coordinate>();


    // PUBLIC INSPECTOR VARIABLES
    public TYPE zoneType = TYPE.FULL;
    public Vector2Int coordinateVector = Vector2Int.one;
    public int zoneHeight = 0;

    public WorldZone( Coordinate centerCoordinate, TYPE zoneType )
    {
        this.coordinateVector = centerCoordinate.localPosition;
        this.zoneType = zoneType;

        Initialize();
    }

    public void Initialize()
    {
        if (_initialized) { return; }
        _initialized = false;

        // Update private variables
        _coordinateVector = coordinateVector;
        //_centerCoordinate = CoordinateMap.GetCoordinateAt(coordinateVector);
        _zoneType = zoneType;
        _zoneHeight = zoneHeight;

        // Get affected neighbors
        List<Coordinate> neighborsInZone = new();
        switch(_zoneType)
        {
            case TYPE.FULL:
                neighborsInZone = _centerCoordinate.GetAllValidNeighbors();
                break;
            case TYPE.NATURAL_CROSS:
                neighborsInZone = _centerCoordinate.GetValidNaturalNeighbors();
                break;
            case TYPE.DIAGONAL_CROSS:
                neighborsInZone = _centerCoordinate.GetValidDiagonalNeighbors();
                break;
            case TYPE.HORIZONTAL:
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.WEST));
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.EAST));
                break;
            case TYPE.VERTICAL:
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.NORTH));
                neighborsInZone.Add(_centerCoordinate.GetNeighborInDirection(WorldDirection.SOUTH));
                break;
        }

        // Assign Zone Coordinates
        _zoneCoordinates = new List<Coordinate> { _centerCoordinate };
        _zoneCoordinates.AddRange(neighborsInZone);

        // Assign Chunk Heights
        //WorldChunkMap.SetChunksToHeightFromCoordinates(_zoneCoordinates, zoneHeight);

        // Assign Zone TYPE
        //CoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, Coordinate.TYPE.ZONE, GetRGBAfromDebugColor(zoneColor));

        _initialized = true;
        //Debug.Log($"Initialized WORLD ZONE : {_coordinateVector} : height {zoneHeight}");
    }

    public void Reset()
    {
        _initialized = false;
    }

    public bool IsInitialized() {

        // Check private variables
        if ( _centerCoordinate == null
            || _centerCoordinate.localPosition != coordinateVector
            || _coordinateVector != coordinateVector
            || _zoneHeight != zoneHeight
            || _zoneType != zoneType
            || _zoneCoordinates.Count == 0)
        {
            _initialized = false;
        }
        else if (_zoneCoordinates.Count > 0)
        {
            /*
            foreach (Coordinate coord in _zoneCoordinates)
            {
                if (CoordinateMap.GetCoordinateAt(coord.LocalCoordinate).type != Coordinate.TYPE.ZONE)
                {
                    _initialized = false;
                }
            }
            */
        }
        else
        {
            _initialized = true;
        }

        /*
        Debug.Log($">> World Zones isInitialized? : {_initialized}" +
            $"\n_centerCoordinate {_centerCoordinate.Coordinate}" +
            $"\n_coordinateVector {_coordinateVector}" +
            $"\n_zoneHeight {_zoneHeight}" +
            $"\n_zoneType {_zoneType}" +
            $"\n_zoneCoordinates {_zoneCoordinates.Count}");
        */

        return _initialized; 
    }


    public List<Coordinate> GetZoneCoordinates() { return _zoneCoordinates; }
    /*
    public List<WorldChunk> GetZoneChunks()
    {
        if (_zoneCoordinates == null || _zoneCoordinates.Count == 0) { return new List<WorldChunk>(); }
        return WorldChunkMap.GetChunksAtCoordinates(_zoneCoordinates);
    }
    */
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
        int x = EditorGUI.IntSlider(coordXRect, new GUIContent("Coord X"), coordProp.vector2IntValue.x, 0, WorldGeneration.PlayRegionWidth_inChunks);
        int y = EditorGUI.IntSlider(coordYRect, new GUIContent("Coord Y"), coordProp.vector2IntValue.y, 0, WorldGeneration.PlayRegionWidth_inChunks);
        coordProp.vector2IntValue = new Vector2Int(x, y);

        // Chunk Height
        SerializedProperty chunkHeightProp = property.FindPropertyRelative("zoneHeight");
        chunkHeightProp.intValue = EditorGUI.IntSlider(chunkHeightRect, new GUIContent("Zone Height"), chunkHeightProp.intValue, 0, WorldGeneration.PlayRegionWidth_inChunks);

        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate the total height needed by adding the height of three controls and the spacing between them
        return (EditorGUIUtility.singleLineHeight * 8) + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
