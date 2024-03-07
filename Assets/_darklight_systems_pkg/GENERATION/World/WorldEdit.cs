using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace Darklight.ThirdDimensional.Generation
{
#if UNITY_EDITOR
    using UnityEditor;
    using DarklightEditor = DarklightEditor;
    using EditMode = WorldEdit.EditMode;
    using WorldView = WorldEdit.WorldView;
    using RegionView = WorldEdit.RegionView;
    using ChunkView = WorldEdit.ChunkView;
    using CellView = WorldEdit.CellView;
    using CoordinateMapView = WorldEdit.CoordinateMapView;
    using ChunkMapView = WorldEdit.ChunkMapView;
    using CellMapView = WorldEdit.CellMapView;
    using Unity.Properties;
    using System.Linq;
#endif

    public class WorldEdit : MonoBehaviour
    {
        public enum EditMode { WORLD, REGION, CHUNK, CELL }
        public EditMode editMode = EditMode.WORLD;

        // World View
        public enum WorldView { COORDINATE_MAP, FULL_COORDINATE_MAP,  };
        public WorldView worldView = WorldView.COORDINATE_MAP;

        // Region View
        public enum RegionView { OUTLINE, COORDINATE_MAP, CHUNK_MAP}
        public RegionView regionView = RegionView.COORDINATE_MAP;

        // Chunk View
        public enum ChunkView { OUTLINE, TYPE, HEIGHT, COORDINATE_MAP, CELL_MAP }
        public ChunkView chunkView = ChunkView.COORDINATE_MAP;

        // Cell View
        public enum CellView { OUTLINE, TYPE, FACE }
        public CellView cellView = CellView.OUTLINE;

        // Coordinate Map
        public enum CoordinateMapView { GRID_ONLY, COORDINATE_VALUE, COORDINATE_TYPE, ZONE_ID }
        public CoordinateMapView coordinateMapView = CoordinateMapView.COORDINATE_TYPE;

        // Chunk Map
        public enum ChunkMapView { TYPE, HEIGHT }
        public ChunkMapView chunkMapView = ChunkMapView.TYPE;

        // Cell Map
        public enum CellMapView { TYPE, FACE }
        public CellMapView cellMapView = CellMapView.TYPE;

        public WorldGeneration worldGeneration => GetComponent<WorldGeneration>();
        public Region selectedRegion;
        public Chunk selectedChunk;
        public Cell selectedCell;

        public void SelectRegion(Region region)
        {
            selectedRegion = region;

            //Debug.Log("Selected Region: " + selectedRegion.Coordinate.Value);

            DarklightEditor.FocusSceneView(region.Coordinate.ScenePosition);

            editMode = EditMode.REGION;
        }

        public void SelectChunk(Chunk chunk)
        {
            selectedChunk = chunk;

            //Debug.Log("Selected Chunk: " + chunk.Coordinate.Value);

            DarklightEditor.FocusSceneView(chunk.Coordinate.ScenePosition);

            //editMode = EditMode.CHUNK;
        }

        public void SelectCell(Cell cell)
        {
            selectedCell = cell;

            //Debug.Log("Selected Cell: " + cell.Coordinate.Value);

            DarklightEditor.FocusSceneView(cell.Position);

            //editMode = EditMode.CELL;
        }
    }

#if UNITY_EDITOR
   
    [CustomEditor(typeof(WorldEdit))]
    public class WorldEditGUI : UnityEditor.Editor
    {
        private SerializedObject _serializedObject;
        private WorldEdit _worldEditScript;
        private WorldGeneration _worldGenerationScript;

        private bool coordinateMapFoldout;

        private async void OnEnable()
        {
            // Cache the SerializedObject
            _serializedObject = new SerializedObject(target);
            _worldEditScript = (WorldEdit)target;
            _worldGenerationScript = _worldEditScript.GetComponent<WorldGeneration>();

            _worldEditScript.editMode = EditMode.WORLD;
            await _worldGenerationScript.InitializeAsync();
        }

        private void OnDisable() {
            _worldGenerationScript.ResetGeneration();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            WorldGeneration worldGeneration = _worldEditScript.worldGeneration;


            // [[ EDITOR VIEW ]]
            DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.editMode, "Edit Mode");

            EditorGUILayout.Space();

            switch (_worldEditScript.editMode)
            {
                case EditMode.WORLD:

                    EditorGUILayout.LabelField("World Edit Mode", DarklightEditor.Header1Style);
                    EditorGUILayout.Space(20);

                    DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.worldView, "World View");

                    // SHOW WORLD STATS


                    CoordinateMapInspector( _worldEditScript.worldGeneration.CoordinateMap);
                    break;
                case EditMode.REGION:
                    if (_worldEditScript.selectedRegion == null && worldGeneration.AllRegions.Count > 0) 
                    {
                        _worldEditScript.SelectRegion(worldGeneration.RegionMap[Vector2Int.zero]);
                        break;
                    }

                    EditorGUILayout.LabelField("Region Edit Mode", DarklightEditor.Header1Style);
                    EditorGUILayout.Space(20);

                    // SHOW REGION STATS
                    Region selectedRegion = _worldEditScript.selectedRegion;
                    EditorGUILayout.LabelField($"Coordinate ValueKey => {selectedRegion.Coordinate.ValueKey}", DarklightEditor.LeftAlignedStyle);

                    DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.regionView, "Region View");
                    CoordinateMapInspector( _worldEditScript.selectedRegion.CoordinateMap);
                    ChunkMapInspector(_worldEditScript.selectedRegion.ChunkMap);
                    break;
                case EditMode.CHUNK:
                    if (_worldEditScript.selectedChunk == null && worldGeneration.Initialized)
                    {
                        Region originRegion = worldGeneration.RegionMap[Vector2Int.zero];
                        _worldEditScript.SelectChunk(originRegion.ChunkMap.GetChunkAt(Vector2Int.zero));
                        break;
                    }

                    EditorGUILayout.LabelField("Chunk Edit Mode", DarklightEditor.Header1Style);
                    EditorGUILayout.Space(20);

                    // SHOW CHUNK STATS



                    DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.chunkView, "Chunk View");
                    CoordinateMapInspector( _worldEditScript.selectedChunk.CoordinateMap);
                    CellMapInspector(_worldEditScript.selectedChunk.CellMap);
                    break;
                case EditMode.CELL:
                    if (_worldEditScript.selectedCell == null)
                    {
                        break;
                    }

                    EditorGUILayout.LabelField("Cell Edit Mode", DarklightEditor.Header1Style);
                    EditorGUILayout.Space(20);

                    // SHOW CELL STATS

                    DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.cellView, "Cell View");
                    break;
            }

            _serializedObject.ApplyModifiedProperties();

            void CoordinateMapInspector(CoordinateMap coordinateMap)
            {

                coordinateMapFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(coordinateMapFoldout, "Coordinate Map Inspector");
                EditorGUILayout.Space(10);
                if (coordinateMapFoldout)
                {
                    if (coordinateMap == null)
                    {
                        EditorGUILayout.LabelField("No Coordinate Map is Selected. Try Initializing the World Generation");
                        return;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    // >> select debug view
                    DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.coordinateMapView, "Coordinate Map View");
                    EditorGUILayout.LabelField($"Unit Space => {coordinateMap.UnitSpace}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Initialized => {coordinateMap.Initialized}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Max Coordinate Value => {coordinateMap.MaxCoordinateValue}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Coordinate Count => {coordinateMap.AllCoordinates.Count}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Exit Count => {coordinateMap.Exits.Count}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Path Count => {coordinateMap.Paths.Count}", DarklightEditor.LeftAlignedStyle);
                    EditorGUILayout.LabelField($"Zone Count => {coordinateMap.Zones.Count}", DarklightEditor.LeftAlignedStyle);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ChunkMapInspector(ChunkMap chunkMap)
            {
                EditorGUILayout.LabelField("Chunk Map", Darklight.DarklightEditor.Header2Style);
                EditorGUILayout.Space(10);
                if (chunkMap == null)
                {
                    EditorGUILayout.LabelField("No Chunk Map is Selected. Try Initializing the World Generation");
                    return;
                }

                // >> select debug view
                DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.chunkMapView, "Chunk Map View");
            }

            void CellMapInspector(CellMap cellMap)
            {
                EditorGUILayout.LabelField("Cell Map", Darklight.DarklightEditor.Header2Style);
                EditorGUILayout.Space(10);
                if (cellMap == null)
                {
                    EditorGUILayout.LabelField("No Cell Map is Selected. Try starting the World Generation");
                    return;
                }

                // >> select debug view
                DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.cellMapView, "Cell Map View");
            }
        }



        // ==================== SCENE GUI =================================================== ////


        protected void OnSceneGUI()
        {

            // >> draw world generation bounds
            WorldGeneration worldGeneration = _worldEditScript.worldGeneration;
            CustomGizmoLibrary.DrawWireSquare_withLabel("World Generation", worldGeneration.CenterPosition, WorldGeneration.Settings.WorldWidth_inGameUnits, Color.black, DarklightEditor.CenteredStyle);

            switch (_worldEditScript.editMode)
            {
                case EditMode.WORLD:
                    DrawWorldEditorGUI();
                    break;
                case EditMode.REGION:
                    DrawRegionEditorGUI();
                    break;
                case EditMode.CHUNK:
                    DrawChunkEditorGUI();
                    break;
                case EditMode.CELL:
                    DrawCellEditorGUI();
                    break;
            }

            void DrawWorldEditorGUI()
            {
                WorldGeneration worldGeneration = _worldEditScript.worldGeneration;

                // [[ DRAW DEFAULT SIZE GUIDE ]]
                if (worldGeneration == null || worldGeneration.CoordinateMap == null)
                {
                    GUIStyle labelStyle = DarklightEditor.BoldStyle;
                    CustomGizmoLibrary.DrawWireSquare_withLabel("Origin Region", worldGeneration.OriginPosition, WorldGeneration.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
                    CustomGizmoLibrary.DrawWireSquare_withLabel("World Chunk Size", worldGeneration.OriginPosition, WorldGeneration.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
                    CustomGizmoLibrary.DrawWireSquare_withLabel("World Cell Size", worldGeneration.OriginPosition, WorldGeneration.Settings.CellSize_inGameUnits, Color.black, labelStyle);
                }
                // [[ DRAW COORDINATE MAP ]]
                else if (_worldEditScript.worldView == WorldView.COORDINATE_MAP)
                {
                    DrawCoordinateMap(worldGeneration.CoordinateMap, _worldEditScript.coordinateMapView,(coordinate) =>
                    {
                        Region selectedRegion = _worldEditScript.worldGeneration.RegionMap[coordinate.ValueKey];
                        _worldEditScript.SelectRegion(selectedRegion);
                    });
                }
            }

            void DrawRegionEditorGUI()
            {
                if (_worldEditScript.selectedRegion == null) return;


                Region selectedRegion = _worldEditScript.selectedRegion;
                DrawRegion(selectedRegion, _worldEditScript.regionView);


                foreach (Region region in worldGeneration.AllRegions)
                {
                    if (region != selectedRegion) { DrawRegion(region, RegionView.OUTLINE); }
                }

            }

            void DrawChunkEditorGUI()
            {
                if (_worldEditScript.selectedChunk == null) return;

                Chunk selectedChunk = _worldEditScript.selectedChunk;
                DrawChunk(selectedChunk, _worldEditScript.chunkView);

                foreach (Chunk chunk in selectedChunk.ChunkMapParent.AllChunks)
                {
                    if (chunk != selectedChunk) { DrawChunk(chunk, ChunkView.OUTLINE); }
                }
            }

            void DrawCellEditorGUI()
            {
                if (_worldEditScript.selectedCell == null) return;

                Cell selectedCell = _worldEditScript.selectedCell;
                

            }

            Repaint();
        }


        // ==== DRAW WORLD UNITS ====================================================================================================
        void DrawRegion(Region region, RegionView type)
        {
            if (region == null || region.CoordinateMap == null ) { return; }

            CoordinateMap coordinateMap = region.CoordinateMap;
            ChunkMap chunkMap = region.ChunkMap;
            GUIStyle regionLabelStyle = DarklightEditor.CenteredStyle;

            // [[ DRAW GRID ONLY ]]
            if (type == RegionView.OUTLINE)
            {
                CustomGizmoLibrary.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
                CustomGizmoLibrary.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldGeneration.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
                {
                    _worldEditScript.SelectRegion(region);
                }, Handles.RectangleHandleCap);
            }
            // [[ DRAW COORDINATE MAP ]]
            else if (type == RegionView.COORDINATE_MAP)
            {
                DrawCoordinateMap(coordinateMap, _worldEditScript.coordinateMapView, (coordinate) => {
                    Chunk selectedChunk = region.ChunkMap.GetChunkAt(coordinate);
                    _worldEditScript.SelectChunk(selectedChunk);
                });
            }
            // [[ DRAW CHUNK MAP ]]
            else if (type == RegionView.CHUNK_MAP)
            {
                DrawChunkMap(chunkMap, _worldEditScript.chunkMapView);
            }
        }

        void DrawChunk(Chunk chunk, ChunkView type)
        {
            GUIStyle chunkLabelStyle = DarklightEditor.CenteredStyle;

            switch (type)
            {
                case ChunkView.OUTLINE:

                    // Draw Selection Rectangle
                    CustomGizmoLibrary.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
                    {
                        _worldEditScript.SelectChunk(chunk);
                    }, Handles.RectangleHandleCap);

                    break;
                case ChunkView.TYPE:
                    chunkLabelStyle.normal.textColor = chunk.TypeColor;
                    CustomGizmoLibrary.DrawLabel($"{chunk.Type.ToString()[0]}", chunk.CenterPosition, chunkLabelStyle);

                    CustomGizmoLibrary.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
                    {
                        _worldEditScript.SelectChunk(chunk);
                    }, Handles.RectangleHandleCap);
                    break;
                case ChunkView.HEIGHT:
                    CustomGizmoLibrary.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

                    CustomGizmoLibrary.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
                    {
                        _worldEditScript.SelectChunk(chunk);
                    }, Handles.RectangleHandleCap);
                    break;
                case ChunkView.COORDINATE_MAP:

                    DrawCoordinateMap(chunk.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) => {});

                    break;
                case ChunkView.CELL_MAP:

                    DrawCellMap(chunk.CellMap, _worldEditScript.cellMapView);

                    break;
            }
        }

        void DrawCell(Cell cell, CellView type)
        {
            GUIStyle cellLabelStyle = DarklightEditor.CenteredStyle;

            switch (type)
            {
                case CellView.OUTLINE:
                    // Draw Selection Rectangle
                    CustomGizmoLibrary.DrawButtonHandle(cell.Position, cell.Normal, cell.Size * 0.475f, Color.black, () =>
                    {
                        _worldEditScript.SelectCell(cell);
                    }, Handles.RectangleHandleCap);
                    break;
                case CellView.TYPE:
                    // Draw Face Type Label
                    CustomGizmoLibrary.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
                    CustomGizmoLibrary.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
                    break;
                case CellView.FACE:
                    // Draw Face Type Label
                    CustomGizmoLibrary.DrawLabel($"{cell.FaceType}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
                    break;
            }
        }

        void DrawCoordinateMap(CoordinateMap coordinateMap, CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = DarklightEditor.CenteredStyle;
            Color coordinateColor = Color.black;

            // Draw Coordinates
            if (coordinateMap != null && coordinateMap.Initialized && coordinateMap.AllCoordinateValues.ToList().Count > 0)
            {
                foreach (Vector2Int coordinateValue in coordinateMap.AllCoordinateValues)
                {
                    Coordinate coordinate = coordinateMap.GetCoordinateAt(coordinateValue);

                    // Draw Custom View
                    switch (mapView)
                    {
                        case CoordinateMapView.GRID_ONLY:
                            break;
                        case CoordinateMapView.COORDINATE_VALUE:
                            CustomGizmoLibrary.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = Color.white;
                            break;
                        case CoordinateMapView.COORDINATE_TYPE:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;
                            CustomGizmoLibrary.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = coordinate.TypeColor;
                            break;
                        case CoordinateMapView.ZONE_ID:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;

                            if (coordinate.Type == Coordinate.TYPE.ZONE)
                            {
                                Zone zone = coordinateMap.GetZoneFromCoordinate(coordinate);
                                if (zone != null)
                                {
                                    CustomGizmoLibrary.DrawLabel($"{zone.ID}", coordinate.ScenePosition, coordLabelStyle);
                                }
                            }

                            break;
                    }

                    // Draw Selection Rectangle
                    CustomGizmoLibrary.DrawButtonHandle(coordinate.ScenePosition, Vector3.up, coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
                    {
                        onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                    }, Handles.RectangleHandleCap);
                }
            }
        }

        void DrawChunkMap(ChunkMap chunkMap, ChunkMapView mapView)
        {
            GUIStyle chunkLabelStyle = DarklightEditor.CenteredStyle;
            Color chunkColor = Color.black;

            // Draw Chunks
            if (chunkMap != null && chunkMap.Initialized)
            {
                foreach (Chunk chunk in chunkMap.AllChunks)
                {
                    // Draw Custom View
                    switch (mapView)
                    {
                        case ChunkMapView.TYPE:
                            DrawChunk(chunk, ChunkView.TYPE);
                            break;
                        case ChunkMapView.HEIGHT:
                            DrawChunk(chunk, ChunkView.HEIGHT);
                            break;
                    }
                }
            }
        }

        void DrawCellMap(CellMap cellMap, CellMapView mapView)
        {
            if (cellMap == null) return;
            
            GUIStyle cellLabelStyle = DarklightEditor.CenteredStyle;
            foreach (Cell cell in cellMap.AllCells)
            {
                // Draw Custom View
                switch (mapView)
                {
                    case CellMapView.TYPE:
                        DrawCell(cell, CellView.TYPE);
                        break;
                    case CellMapView.FACE:
                        DrawCell(cell, CellView.FACE);
                        break;
                }
            }
        }

    }


#endif


}


