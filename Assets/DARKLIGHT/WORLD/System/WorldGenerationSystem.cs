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
    using System.Linq;

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
        public enum GridMap2DView
        {
            EMPTY_GRID, COORD_POSITION,
            ALL_FLAGS, BORDER_FLAGS, CORNER_FLAGS,
            ZONE_ID
        }
        static GridMap2DView gridMap2DView;
        static bool showGridMapFoldout;

        public void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _worldGenSystem = (WorldGenerationSystem)target;

        }

        public override void OnInspectorGUI()
        {
            /*
            DrawDefaultInspector();
            EditorGUILayout.Space();
            showGridMapFoldout = EditorGUILayout.Foldout(showGridMapFoldout, "Region Grid Map");
            if (showGridMapFoldout)
            {
                Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref gridMap2DView, "Grid View");
            }
            */
        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            /*
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            SceneGUI_DrawGridMap2D(worldGenSystem.regionGrid, gridMap2DView, (coordinate) =>
            {
                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
            */
        }

        public void SceneGUI_DrawGridMap2D(GridMap2D gridMap2D, GridMap2DView gridMap2DView, System.Action<GridMap2D.Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            Color coordinateColor = Color.white;

            foreach (Vector2Int position in gridMap2D.PositionKeys)
            {
                GridMap2D.Coordinate gridCoordinate = gridMap2D.GetCoordinateAt(position);
                if (gridCoordinate == null) continue;

                switch (gridMap2DView)
                {
                    case GridMap2DView.EMPTY_GRID:
                        break;
                    case GridMap2DView.COORD_POSITION:
                        Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.PositionKey}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        coordinateColor = Color.white;
                        break;
                    case GridMap2DView.ALL_FLAGS:
                        coordinateColor = gridCoordinate.GetCurrentFlagColor();
                        coordLabelStyle.normal.textColor = coordinateColor;
                        Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.CurrentFlag}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        break;
                    case GridMap2DView.ZONE_ID:
                        coordinateColor = gridCoordinate.GetCurrentFlagColor();
                        coordLabelStyle.normal.textColor = coordinateColor;
                        if (gridCoordinate.CurrentFlag == GridMap2D.Coordinate.Flag.ZONE)
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
                    case GridMap2DView.BORDER_FLAGS:
                        coordinateColor = gridCoordinate.GetCurrentFlagColor();
                        coordLabelStyle.normal.textColor = coordinateColor;
                        if (gridCoordinate.CurrentFlag == GridMap2D.Coordinate.Flag.BORDER)
                        {
                            EdgeDirection? edgeDirection = GridMap2D.DetermineBorderEdge(gridCoordinate.PositionKey, gridCoordinate.ParentGrid.MapWidth);
                            Darklight.CustomGizmos.DrawLabel($"{edgeDirection}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        }
                        break;
                    case GridMap2DView.CORNER_FLAGS:
                        coordinateColor = gridCoordinate.GetCurrentFlagColor();
                        coordLabelStyle.normal.textColor = coordinateColor;
                        Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.CurrentFlag}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        Darklight.CustomGizmos.DrawButtonHandle(gridCoordinate.GetPositionInScene(), Vector3.up, gridCoordinate.Size * 0.45f, coordinateColor, () =>
                        {
                            onCoordinateSelect?.Invoke(gridCoordinate); // Invoke the action if the button is clicked
                        }, Handles.RectangleHandleCap);
                        break;
                }
            }

        }

        void SceneGUI_DrawGridCoordinate(GridMap2D.Coordinate coordinate)
        {
            GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            Color coordinateColor = Color.white;



        }
    }

#endif
}
