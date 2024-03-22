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
    using Darklight.World.Builder;

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
        #region [[ STATIC INSTANCE ]] ---- >> 
        /// <summary> A singleton instance of the WorldGenerationSystem class. </summary>
        public static WorldGenerationSystem Instance;
        public static string Prefix => "< WORLD GENERATION SYSTEM >";

        /// <summary> Destroy GameObject in Play and Edit mode </summary>
        public static void DestroyGameObject(GameObject gameObject)
        {
            // Check if we are running in the Unity Editor
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                // Use DestroyImmediate if in edit mode and not playing
                DestroyImmediate(gameObject);
                return;
            }
            else
#endif
            {
                // Use Destroy in play mode or in a build
                Destroy(gameObject);
            }
        }

        #endregion

        #region [[ GENERATION SETTINGS ]] ---- >> 
        /// <summary> Contains settings used during the world generation process. </summary>
        [SerializeField] private GenerationSettings _settings = new GenerationSettings();
        public GenerationSettings Settings => _settings;
        #endregion

        #region ---- (( INSTANTIATE OBJECTS ))
        public HashSet<GameObject> InstantiatedObjects { get; private set; } = new HashSet<GameObject>();

        public Task<GameObject> CreateGameObjectAt(string name, GridMap2D.Coordinate coordinate)
        {
            GameObject newObject = new GameObject($"{name} :: {coordinate.PositionKey}");
            newObject.transform.parent = this.transform;
            newObject.transform.position = coordinate.GetPositionInScene();

            InstantiatedObjects.Add(newObject);
            return Task.FromResult(newObject);
        }
        #endregion

        #region ---- (( DATA HANDLING ))
        public GridMap2D<Generation.Region> RegionGridMap { get; private set; } = null;
        #endregion

        #region --------------- UNITY MAIN ----))
        public Material defaultMaterial;

        public override void Awake()
        {
            base.Awake();
            _ = Initialize();
        }
        #endregion

        #region --------------- TASK BOT QUEEN --))
        public override async Task Initialize()
        {
            // << Singelton Instance >> 
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            // << INIT >> -------------------------- //
            this.Name = "WorldGenerationSystem";
            _settings.Initialize();
            await base.Initialize();
            RegionGridMap = new GridMap2D<Generation.Region>(transform, UnitSpace.REGION); // Assign GridMap to this Transform
            RegionGridMap.Initialize(_settings);
            Debug.Log($"{Prefix} Initialized");
            await RegionGridMap.InitializeDataMap(); // Initialize Data



            // [[ ADD BOT CLONES TO EXECUTION QUEUE ]]
            // Enqueue a TaskBot clone for each position
            await EnqueueClones("CreateRegionBuilders", RegionGridMap.PositionKeys, position =>
            {
                Generation.Region region = RegionGridMap.DataMap[position];
                return new TaskBot(this, $"CreateRegion {position}", async () =>
                {
                    GameObject newObject = await CreateGameObjectAt("RegionBuilder", region.coordinateValue);
                    Builder.RegionBuilder regionBuilder = newObject.AddComponent<Builder.RegionBuilder>();
                    await regionBuilder.Initialize();
                });
                // i love my little bots <3 
            });
        }
        #endregion

        public override void Reset()
        {
            base.Reset();
            TaskBotConsole.Reset();
            RegionGridMap.Reset();

            foreach (GameObject obj in InstantiatedObjects)
            {
                DestroyGameObject(obj);
            }
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
                CustomInspectorGUI.CreateEnumLabel(ref gridMap2DView, "Grid View");
            }
        }

        /// <summary>
        /// Enables the Editor to handle an event in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            WorldGenerationSystem worldGenSystem = (WorldGenerationSystem)target;
            SceneGUI_DrawGridMap2D(worldGenSystem.RegionGridMap, gridMap2DView, (coordinate) =>
            {

                Debug.Log($"Selected Coordinate: {coordinate.PositionKey}");
            });
        }

        public void SceneGUI_DrawGridMap2D(GridMap2D gridMap2D, GridMap2DView gridMap2DView, System.Action<GridMap2D.Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = CustomGUIStyles.CenteredStyle;
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
    }

#endif
    #endregion
}
