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
    TYPE _storedZoneType = TYPE.FULL;
    WorldCoordinate _centerCoordinate;
    List<WorldCoordinate> _zoneCoordinates = new List<WorldCoordinate>();
    bool _initialized = false;


    // PUBLIC INSPECTOR VARIABLES
    public TYPE zoneType = TYPE.FULL;
    public Vector2Int coordinateVector = Vector2Int.one;
    public int zoneHeight = 0;

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
        if (WorldCoordinateMap.coordMapInitialized == false) { return; }

        // Reassign the coordinate types
        if (_zoneCoordinates.Count > 0) {
            // Check for treason ...
            foreach (WorldCoordinate coord in _zoneCoordinates)
            {
                if ((coord.type != WorldCoordinate.TYPE.ZONE 
                    || WorldChunkMap.GetChunkAt(coord).groundHeight != zoneHeight))
                {
                    _initialized = false;
                }
            }
        }

        // << INITIALIZE >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        if ( _initialized ) { return; }
        _initialized = false;

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
        WorldCoordinateMap.SetMapCoordinatesToType(_zoneCoordinates, WorldCoordinate.TYPE.ZONE, GetRGBAfromDebugColor(zoneColor));

        // Assign Chunk Heights
        WorldChunkMap.SetChunksToHeightFromCoordinates(_zoneCoordinates, zoneHeight);

        _initialized = true;
    }

    public void Reset()
    {
        if (WorldCoordinateMap.coordMapInitialized == false || !_initialized) { return; }

        // IF VALUES CHANGED
        if (_centerCoordinate == null || _centerCoordinate.Coordinate != coordinateVector 
            || zoneType != _storedZoneType )
        {
            // Reset
            _zoneCoordinates.Clear();

            // Update private variables
            _centerCoordinate = WorldCoordinateMap.GetCoordinateAt(coordinateVector);
            this._storedZoneType = zoneType;

            _initialized = false;

        }
    }

    public List<WorldCoordinate> GetZoneCoordinates() { return _zoneCoordinates; }
    public List<WorldChunk> GetZoneChunks() 
    { 
        if (_zoneCoordinates == null || _zoneCoordinates.Count ==0 ) { return new List<WorldChunk>(); }
        return WorldChunkMap.GetChunksAtCoordinates(_zoneCoordinates);
    }
    public bool IsInitialized() {
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
