using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WorldDirection { West, East, North, South }

// =================================================================
//      WORLD EXIT
// ========================================================
[System.Serializable]
public class WorldExit
{
    WorldDirection _borderDirection;
    int _borderIndex;
    int _exitHeight;
    bool _initialized = false;

    // Coordinate on border
    public WorldCoordinate Coordinate
    {
        get { return WorldCoordinateMap.GetCoordinateAtWorldExit(this); }
    }

    // Connecting Neighbor that is in the playArea
    public WorldCoordinate PathConnectionCoord
    {
        get { return WorldCoordinateMap.GetWorldExitPathConnection(this); }
    }

    public WorldChunk Chunk
    {
        get { return WorldChunkMap.GetChunkAt(Coordinate); }
    }

    // == INSPECTOR VALUES >>
    public WorldDirection borderDirection = WorldDirection.West;
    public int borderIndex = 0;
    public int exitHeight = 0;

    public WorldExit(WorldDirection direction, int index)
    {
        borderDirection = direction;
        borderIndex = index;
        UpdateValues();
    }

    void UpdateValues()
    {
        _borderDirection = borderDirection;
        _borderIndex = borderIndex;
        _exitHeight = exitHeight;
        Chunk.SetGroundHeight(_exitHeight);
        _initialized = true;
    }

    public bool IsInitialized()
    {
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

    public WorldPath.PathColor pathColor = WorldPath.PathColor.YELLOW;
    [Range(0, 1)] public float pathRandomness = 0f;
    public WorldExit startExit;
    public WorldExit endExit;

    WorldCoordinate _pathStart;
    WorldCoordinate _pathEnd;
    float _pathRandomness;

    public WorldExitPath(WorldExit startExit, WorldExit endExit)
    {
        this.startExit = startExit;
        this.endExit = endExit;
        this.pathColor = WorldPath.GetRandomPathColor();
    }

    public void EditorUpdate()
    {
        // Update Exits first
        bool newStart = startExit.IsInitialized();
        bool newEnd = endExit.IsInitialized();
        if (newStart || newEnd) { Reset(true); }

        if (_initialized) { return; }

        // Update private variables
        _pathStart = startExit.PathConnectionCoord;
        _pathEnd = endExit.PathConnectionCoord;
        _pathRandomness = pathRandomness;

        // Get new World Path
        _worldPath = new WorldPath(_pathStart, _pathEnd, pathColor, pathRandomness);

        // Determine Path Chunk Heights
        if (_worldPath.IsInitialized() && WorldChunkMap.chunkMapInitialized)
        {
            _worldPath.DeterminePathChunkHeights(startExit.exitHeight, endExit.exitHeight);
            _initialized = true;
        }
    }

    public void Reset(bool forceReset = false)
    {
        if (!_initialized) return;

        // Check if values are incorrectly initialized
        if (_pathStart != startExit.PathConnectionCoord 
            || _pathEnd != endExit.PathConnectionCoord
            || _pathRandomness != pathRandomness
            || forceReset)
        {
            _worldPath.Reset();
            _initialized = false;
        }
    }

    public bool IsInitialized()
    {
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
        return WorldPath.GetRGBAfromPathColorType(pathColor);
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

