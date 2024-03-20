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
    public enum Direction { NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST }
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

        // ================================================================== FUNCTIONS ==================== 
        public override void Awake()
        {
            // >> set singleton instance
            if (Instance == null) Instance = this;
            else { Destroy(this); }

            InitializeRandomSeed();

            // >>  awake base TaskQueen
            base.Awake();
        }

        public override void Reset()
        {
            base.Reset();
        }
    }


    // ==================================================== CUSTOM UNITY EDITOR ==================
#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGenerationSystem))]
    public class WorldGenerationSystemEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        WorldGenerationSystem _worldGenSystem;
        public enum GridMap2DView { GRID_ONLY, COORDINATE_VALUE, COORDINATE_TYPE, BORDERS_ONLY, ZONE_ID }
        static GridMap2DView gridMap2DView;
        static bool showGridMapFoldout;

        public void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _worldGenSystem = (WorldGenerationSystem)target;

            _worldGenSystem.Reset();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            showGridMapFoldout = EditorGUILayout.Foldout(showGridMapFoldout, "Region Grid Map");
            if (showGridMapFoldout)
            {
                Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref gridMap2DView, "Grid View");
            }

        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            SceneGUI_DrawGridMap2D(worldGenSystem.regionGrid, gridMap2DView, (coordinate) =>
            {
                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
        }

        public void SceneGUI_DrawGridMap2D(GridMap2D gridMap2D, GridMap2DView gridMap2DView, System.Action<GridMap2D.Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            Color coordinateColor = Color.white;

            foreach (Vector2Int position in gridMap2D.PositionKeys)
            {
                GridMap2D.Coordinate coordinate = gridMap2D.GetCoordinateAt(position);
                switch (gridMap2DView)
                {
                    case GridMap2DView.GRID_ONLY:
                        break;
                    case GridMap2DView.COORDINATE_VALUE:
                        Darklight.CustomGizmos.DrawLabel($"{coordinate.PositionKey}", coordinate.GetPositionInScene(), coordLabelStyle);
                        coordinateColor = Color.white;
                        break;
                    case GridMap2DView.COORDINATE_TYPE:
                        coordinateColor = coordinate.CurrentFlagColor;
                        coordLabelStyle.normal.textColor = coordinateColor;
                        Darklight.CustomGizmos.DrawLabel($"{coordinate.CurrentFlag}", coordinate.GetPositionInScene(), coordLabelStyle);
                        break;
                    case GridMap2DView.ZONE_ID:
                        coordinateColor = coordinate.CurrentFlagColor;
                        coordLabelStyle.normal.textColor = coordinateColor;
                        if (coordinate.CurrentFlag == GridMap2D.Coordinate.Flag.ZONE)
                        {
                            // TODO : Implement Zone ID
                            /*
                            Zone zone = coordinateMap.GetZoneFromCoordinate(gridCoordinate);
                            if (zone != null)
                            {
                                Darklight.CustomGizmos.DrawLabel($"{zone.ID}", gridCoordinate.ScenePosition, coordLabelStyle);
                            }
                            */
                        }
                        break;
                    case GridMap2DView.BORDERS_ONLY:
                        coordinateColor = coordinate.CurrentFlagColor;
                        coordLabelStyle.normal.textColor = coordinateColor;
                        if (coordinate.CurrentFlag == GridMap2D.Coordinate.Flag.BORDER)
                        {
                            EdgeDirection? edgeDirection = GridMap2D.DetermineBorderEdge(coordinate.PositionKey, coordinate.ParentGrid.MapWidth);
                            Darklight.CustomGizmos.DrawLabel($"{edgeDirection}", coordinate.GetPositionInScene(), coordLabelStyle);
                        }
                        break;
                }

                Darklight.CustomGizmos.DrawButtonHandle(coordinate.GetPositionInScene(), Vector3.up, coordinate.Size * 0.45f, coordinateColor, () =>
                {
                    onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                }, Handles.RectangleHandleCap);
            }
        }
    }

#endif
}