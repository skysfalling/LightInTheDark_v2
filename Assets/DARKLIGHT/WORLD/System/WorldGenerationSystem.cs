using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World
{
    using Debug = UnityEngine.Debug;

    using Darklight.Bot;
    using Settings;
    using Generation;
    using Map;
    using Builder;
    using UnityEditor.SearchService;

    #region (( SPATIAL ENUMS))
    /// <summary>
    /// Represents the spatial scope for operations or elements within the world generation context.
    /// </summary>
    public enum UnitSpace { WORLD, REGION, CHUNK, CELL, GAME }
    /// <summary>
    /// Defines cardinal and intercardinal directions for world layout and neighbor identification.
    /// </summary>
    public enum WorldDirection { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
    /// <summary> Specifies the directions for borders relative to a given region or chunk. </summary>
    public enum EdgeDirection { WEST, NORTH, EAST, SOUTH }
    #endregion

    public class WorldGenerationSystem : TaskQueen, ITaskEntity
    {
        #region [[ STATIC INSTANCE ]] ---- >> 
        /// <summary> A singleton instance of the WorldGenerationSystem class. </summary>
        public static WorldGenerationSystem Instance;

        #endregion

        #region [[ GENERATION SETTINGS ]] ---- >> 
        static GenerationSettings _settings = new GenerationSettings();

        /// <summary> Contains settings used during the world generation process. </summary>
        public static GenerationSettings Settings => _settings;

        /// <summary> Override the default generation settings. </summary>
        public static void OverrideSettings(CustomGenerationSettings customSettings)
        {
            if (customSettings == null) { _settings = new GenerationSettings(); return; }
            _settings = new GenerationSettings(customSettings);
        }
        #endregion

        #region [[ RANDOM SEED ]] ---- >> 
        public static string Seed { get { return Settings.Seed; } }
        public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
        public static void InitializeRandomSeed()
        {
            UnityEngine.Random.InitState(EncodedSeed);
        }
        #endregion

        string _prefix = "< WORLD GENERATION SYSTEM > ";
        public CustomGenerationSettings customWorldGenSettings; // Settings Scriptable Object
        public Material defaultMaterial;
        public GridMap2D regionGrid = new GridMap2D();


        public Vector3 CenterPosition { get { return transform.position; } }
        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = CenterPosition; // Start at center
                origin -= Settings.WorldWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }

        // ================================================================== FUNCTIONS
        public override void Awake()
        {
            // >> set singleton instance
            if (Instance == null) Instance = this;
            else { Destroy(this); }

            InitializeRandomSeed();

            // >>  awake base TaskQueen
            base.Awake();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGenerationSystem))]
    public class WorldGenerationSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            SceneGUI_DrawGridMap2D(worldGenSystem.regionGrid, (coordinate) =>
            {
                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
        }

        public void SceneGUI_DrawGridMap2D(GridMap2D gridMap2D, System.Action<GridMap2D.Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            Color coordinateColor = Color.black;

            foreach (GridMap2D.Coordinate gridCoordinate in gridMap2D.CoordinateValues)
            {
                Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.PositionKey}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                Darklight.CustomGizmos.DrawButtonHandle(gridCoordinate.GetPositionInScene(), Vector3.up, gridCoordinate.Size * 0.25f, coordinateColor, () =>
                {
                    onCoordinateSelect?.Invoke(gridCoordinate); // Invoke the action if the button is clicked
                }, Handles.CubeHandleCap);

                coordinateColor = Color.white;
            }

            /*

            // Draw Coordinates
            if (coordinateMap != null && coordinateMap.Initialized && coordinateMap.AllCoordinateValues.Count > 0)
            {
                foreach (Vector2Int coordinateValue in coordinateMap.AllCoordinateValues)
                {
                    Coordinate coordinate = coordinateMap.GetCoordinateAt(coordinateValue);

                    // Draw Custom View
                    switch (mapView)
                    {
                        case WorldEditor.CoordinateMapView.GRID_ONLY:
                            break;
                        case WorldEditor.CoordinateMapView.COORDINATE_VALUE:
                            Darklight.CustomGizmos.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = Color.white;
                            break;
                        case WorldEditor.CoordinateMapView.COORDINATE_TYPE:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;
                            Darklight.CustomGizmos.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = coordinate.TypeColor;
                            break;
                        case WorldEditor.CoordinateMapView.ZONE_ID:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;

                            if (coordinate.Type == Coordinate.TYPE.ZONE)
                            {
                                Zone zone = coordinateMap.GetZoneFromCoordinate(coordinate);
                                if (zone != null)
                                {
                                    Darklight.CustomGizmos.DrawLabel($"{zone.ID}", coordinate.ScenePosition, coordLabelStyle);
                                }
                            }

                            break;
                    }

                    // Draw Selection Rectangle
                    Darklight.CustomGizmos.DrawButtonHandle(coordinate.ScenePosition, Vector3.up, coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
                    {
                        onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                    }, Handles.RectangleHandleCap);
                }
            }
            */

        }
    }



#endif
}
