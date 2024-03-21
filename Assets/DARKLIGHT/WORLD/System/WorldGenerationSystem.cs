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
    using Bot;
    using Settings;
    using Map;
    using System.Threading.Tasks;
    using Darklight.World.Generation;

    #region (( GLOBAL SPATIAL ENUMS ))
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

    public class WorldGenerationSystem : TaskBotQueen, ITaskEntity
    {
        #region [[ STATIC METHODS ]] ---- >> 
        /// <summary> A singleton instance of the WorldGenerationSystem class. </summary>
        public static WorldGenerationSystem Instance
        {
            get
            {
                if (Instance == null)
                {
                    Instance = FindFirstObjectByType<WorldGenerationSystem>();
                }
                return Instance;
            }
            private set { Instance = value; }
        }
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

        #region [[ GENERATION DATA ]] ---- >>

        #endregion

        #region [[ RANDOM SEED ]] ---- >> 
        public static string Seed { get { return Settings.Seed; } }
        public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
        public static void InitializeRandomSeed()
        {
            UnityEngine.Random.InitState(EncodedSeed);
        }
        #endregion

        #region --------------- UNITY MAIN --))

        public string prefix => "< WORLD GENERATION SYSTEM >";
        public Material defaultMaterial;
        public GridMap2D<Region> RegionGrid { get; private set; } = new GridMap2D<Region>();

        public override void Awake()
        {
            // >> set singleton instance
            if (Instance == null) Instance = this;
            else { Destroy(this); }

            // >>  awaken base TaskQueen
            base.Awake();
        }
        #endregion

        #region --------------- TASKS --))

        async Task CreateRegions()
        {
            RegionGrid.InitializeDataMap();
            await Task.CompletedTask;
        }

        #endregion

        #region --------------- TASK BOT QUEEN --))
        public override async Task Initialize()
        {
            this.Name = "WorldGenerationSystem";
            WorldGenerationSystem.InitializeRandomSeed();
            await base.Initialize();
            Debug.Log($"{prefix} Initialized");

            // [[ INSTANTIATE REGIONS ]]
            await CreateRegions();
        }
        #endregion




        public override void Reset()
        {
            base.Reset();
        }
    }

    #region==== CUSTOM UNITY EDITOR ================== )) 
#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGenerationSystem))]
    public class WorldGenerationSystemEditor : TaskBotQueenEditor
    {
        SerializedObject _serializedObject;
        WorldGenerationSystem _worldGenSystem;
        public enum GridMap2DView { SIZE_GRID, COORD_POSITION, COORD_FLAG, BORDER_DIRECTIONS, CORNER_DIRECTIONS, ZONE_ID }
        public static GridMap2DView gridMap2DView = GridMap2DView.BORDER_DIRECTIONS;

        public static bool showGridMapFoldout = true;

        public override void OnEnable()
        {
            base.OnEnable();
            _serializedObject = new SerializedObject(target);
            _worldGenSystem = (WorldGenerationSystem)target;

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
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
            SceneGUI_DrawGridMap2D(worldGenSystem.RegionGrid, gridMap2DView, (coordinate) =>
            {

                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
        }

        public void SceneGUI_DrawGridMap2D(GridMap2D gridMap2D, GridMap2DView gridMap2DView, System.Action<GridMap2D.Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            Color coordinateColor = Color.white;

            Darklight.CustomGizmos.DrawWireSquare(gridMap2D.CenterPosition, gridMap2D.MapWidth * gridMap2D.CoordinateSize, coordinateColor, Vector3.up);


            // << BORDER DIRECTIONS >>
            if (gridMap2DView == GridMap2DView.BORDER_DIRECTIONS)
            {
                // << DRAW BORDERS ONLY >>
                Dictionary<EdgeDirection, GridMap2D.Border> borders = gridMap2D.MapBorders;
                if (borders == null || borders.Count == 0) return;

                foreach (EdgeDirection edgeDirection in borders.Keys)
                {
                    GridMap2D.Border selectedBorder = borders[edgeDirection];
                    foreach (Vector2Int position in selectedBorder.Positions)
                    {
                        GridMap2D.Coordinate coordinate = gridMap2D.GetCoordinateAt(position);
                        if (coordinate == null) continue;
                        coordinateColor = coordinate.GetFlagColor(coordinate.CurrentFlag);
                        Darklight.CustomGizmos.DrawLabel($"{selectedBorder.Direction}", coordinate.GetPositionInScene(), coordLabelStyle);
                        Darklight.CustomGizmos.DrawButtonHandle(coordinate.GetPositionInScene(), Vector3.up, coordinate.Size * 0.45f, coordinateColor, () =>
                        {
                            onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                        }, Handles.RectangleHandleCap);
                    }
                }
                return;
            }

            // << CORNER DIRECTIONS >>
            if (gridMap2DView == GridMap2DView.CORNER_DIRECTIONS)
            {
                Dictionary<(EdgeDirection, EdgeDirection), Vector2Int> corners = gridMap2D.MapCorners;
                if (corners == null || corners.Count == 0) return;

                foreach ((EdgeDirection, EdgeDirection) directionTuple in corners.Keys)
                {
                    Vector2Int position = corners[directionTuple];
                    GridMap2D.Coordinate coordinate = gridMap2D.GetCoordinateAt(position);
                    if (coordinate == null) continue;
                    coordinateColor = coordinate.GetFlagColor(coordinate.CurrentFlag);
                    Darklight.CustomGizmos.DrawLabel($"{directionTuple.Item1} - {directionTuple.Item2}", coordinate.GetPositionInScene(), coordLabelStyle);
                    Darklight.CustomGizmos.DrawButtonHandle(coordinate.GetPositionInScene(), Vector3.up, coordinate.Size * 0.45f, coordinateColor, () =>
                    {
                        onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                    }, Handles.RectangleHandleCap);
                }
            }



            // << DRAW ALL COORDINATES >>
            foreach (Vector2Int position in gridMap2D.PositionKeys)
            {
                GridMap2D.Coordinate gridCoordinate = gridMap2D.GetCoordinateAt(position);
                if (gridCoordinate == null) continue;

                switch (gridMap2DView)
                {
                    case GridMap2DView.SIZE_GRID:
                        break;
                    case GridMap2DView.COORD_POSITION:
                        coordinateColor = Color.white;
                        Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.PositionKey}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        break;
                    case GridMap2DView.COORD_FLAG:
                        coordinateColor = gridCoordinate.GetFlagColor(gridCoordinate.CurrentFlag); // Get the color for the current flag (type of coordinate)
                        coordLabelStyle.normal.textColor = coordinateColor;
                        Darklight.CustomGizmos.DrawLabel($"{gridCoordinate.CurrentFlag}", gridCoordinate.GetPositionInScene(), coordLabelStyle);
                        break;
                    case GridMap2DView.ZONE_ID:
                        coordinateColor = gridCoordinate.GetFlagColor(gridCoordinate.CurrentFlag); ;
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
    #endregion
}
