using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

// =================================================================
//      WORLD EXIT
// ========================================================
[System.Serializable]
public class WorldExit
{
    bool _initialized = false;
    WorldChunk _chunk;
    WorldDirection _borderDirection;
    int _borderIndex;
    int _exitHeight;

    // Coordinate on border
    public WorldCoordinate worldCoordinate;

    // Connecting Neighbor that is in the playArea
    public WorldCoordinate pathConnection;

    // == INSPECTOR VALUES >>
    public WorldDirection borderDirection = WorldDirection.WEST;
    public int borderIndex = 0;
    public int exitHeight = 0;

    public WorldExit(WorldDirection direction, int index)
    {
        borderDirection = direction;
        borderIndex = index;

        Initialize();
    }

    public void Initialize()
    {
        if (_initialized || !WorldCoordinateMap.coordMapInitialized || !WorldChunkMap.chunkMapInitialized) return;

        _borderDirection = borderDirection;
        _borderIndex = borderIndex;
        _exitHeight = exitHeight;

        worldCoordinate = WorldCoordinateMap.GetCoordinateAtWorldExit(borderDirection, borderIndex);
        pathConnection = WorldCoordinateMap.GetNeighborInOppositeDirection(worldCoordinate, borderDirection);

        _chunk = WorldChunkMap.GetChunkAt(worldCoordinate);
        _chunk.SetGroundHeight(exitHeight);

        //Debug.Log($"WORLDEXIT : Initialized at {worldCoordinate.Coordinate} with exitHeight {exitHeight}");
        _initialized = true;
    }

    public void Reset()
    {
        WorldCoordinateMap.SetMapCoordinateToType(worldCoordinate, WorldCoordinate.TYPE.BORDER);
        _initialized = false;
    }

    public bool IsInitialized()
    {
        if (_borderDirection != borderDirection
            || _borderIndex != borderIndex
            || _exitHeight != exitHeight) 
        { _initialized = false; }

        return _initialized;
    }
}

// =================================================================
//      WORLD EXIT PATH
// ========================================================
[System.Serializable]
public class WorldExitPath
{
    WorldPath _worldPath;
    bool _initialized = false;

    public DebugColor pathColor = DebugColor.YELLOW;
    [Range(0, 1)] public float pathRandomness = 0f;
    public WorldExit startExit;
    public WorldExit endExit;

    int _startHeight;
    int _endHeight;

    WorldCoordinate _pathStart;
    WorldCoordinate _pathEnd;
    float _pathRandomness;

    public WorldExitPath(WorldExit startExit, WorldExit endExit)
    {
        this.startExit = startExit;
        this.endExit = endExit;

        _startHeight = startExit.exitHeight;
        _endHeight = endExit.exitHeight;

        this.pathColor = WorldPath.GetRandomPathColor();
        this.pathRandomness = 1;
    }

    public void EditorUpdate()
    {
        startExit.Initialize();
        endExit.Initialize();

        if (!startExit.IsInitialized() || !endExit.IsInitialized()) { Reset(); }
        if (_initialized) { return; }
        _initialized = false;

        // Update private variables
        _pathStart = startExit.pathConnection;
        _pathEnd = endExit.pathConnection;

        _startHeight = startExit.exitHeight;
        _endHeight = endExit.exitHeight;

        _pathRandomness = pathRandomness;

        // Get new World Path
        _worldPath = new WorldPath(_pathStart, _startHeight, _pathEnd, _endHeight, pathColor, pathRandomness);

        // Update Exit Values
        if (_worldPath.IsInitialized() && WorldChunkMap.chunkMapInitialized)
        {
            startExit.worldCoordinate.debugColor = WorldPath.GetRGBAFromDebugColor(pathColor);
            endExit.worldCoordinate.debugColor = WorldPath.GetRGBAFromDebugColor(pathColor);

            WorldCoordinateMap.SetMapCoordinateToType(startExit.worldCoordinate, WorldCoordinate.TYPE.EXIT);
            WorldCoordinateMap.SetMapCoordinateToType(endExit.worldCoordinate, WorldCoordinate.TYPE.EXIT);

            WorldChunkMap.GetChunkAt(startExit.worldCoordinate).SetGroundHeight(_startHeight);
            WorldChunkMap.GetChunkAt(endExit.worldCoordinate).SetGroundHeight(_endHeight);


            _initialized = true;
        }
    }

    public void Reset(bool forceReset = false)
    {
        if (!_initialized) return;
        if (WorldCoordinateMap.coordMapInitialized == false) { _initialized = false; return; }
        if (WorldChunkMap.chunkMapInitialized == false) { _initialized = false; return; }

        startExit.Reset();
        endExit.Reset();
        _worldPath.Reset();

        _initialized = false;
    }

    public bool IsInitialized()
    {
        if (_pathStart != startExit.pathConnection
        || _pathEnd != endExit.pathConnection
        || _startHeight != startExit.exitHeight
        || _endHeight != endExit.exitHeight
        || _pathRandomness != pathRandomness)
        {
            Reset();
            _initialized = false;
        }

        return _initialized;
    }

    public List<WorldCoordinate> GetPathCoordinates()
    {
        return _worldPath.GetPathCoordinates();
    }

    public List<WorldChunk> GetPathChunks()
    {
        return _worldPath.GetPathChunks();
    }

    public Color GetPathColorRGBA()
    {
        return WorldPath.GetRGBAFromDebugColor(pathColor);
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

