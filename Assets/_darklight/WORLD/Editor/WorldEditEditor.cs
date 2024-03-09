using UnityEngine;
using System.Collections.Generic;
using static Darklight.World.Generation.WorldEdit;


namespace Darklight.World.Generation.CustomEditor
{
    using DarklightEditor = Darklight.Unity.CustomInspectorGUI;
    using DarklightGizmos = Darklight.Unity.CustomGizmos;
    using UnityCustomEditor = UnityEditor.CustomEditor;

#if UNITY_EDITOR
	using UnityEditor;

    [UnityCustomEditor(typeof(WorldEdit))]
    public class WorldEditEditor : Editor
    {
        private SerializedObject _serializedWorldEditObject;
        private WorldEdit _worldEditScript;
        private WorldBuilder _worldBuilderScript;

        private bool coordinateMapFoldout;

        private void OnEnable()
        {
            // Cache the SerializedObject
            _serializedWorldEditObject = new SerializedObject(target);
            _worldEditScript = (WorldEdit)target;
            _worldBuilderScript = _worldEditScript.GetComponent<WorldBuilder>();
            _worldEditScript.editMode = EditMode.WORLD;
        }

#region [[ INSPECTOR GUI ]] ================================================================= 

        public override void OnInspectorGUI()
        {
            _serializedWorldEditObject.Update();

            WorldBuilder worldGeneration = _worldEditScript.worldGeneration;


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

            _serializedWorldEditObject.ApplyModifiedProperties();

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
                EditorGUILayout.LabelField("Chunk Map", DarklightEditor.Header2Style);
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
                EditorGUILayout.LabelField("Cell Map", DarklightEditor.Header2Style);
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

#endregion


        // ==================== SCENE GUI =================================================== ////


        protected void OnSceneGUI()
        {

            // >> draw world generation bounds
            WorldBuilder worldGeneration = _worldEditScript.worldGeneration;
            DarklightGizmos.DrawWireSquare_withLabel("World Generation", worldGeneration.CenterPosition, WorldBuilder.Settings.WorldWidth_inGameUnits, Color.black, DarklightEditor.CenteredStyle);

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
                WorldBuilder worldGeneration = _worldEditScript.worldGeneration;

                // [[ DRAW DEFAULT SIZE GUIDE ]]
                if (worldGeneration == null || worldGeneration.CoordinateMap == null)
                {
                    GUIStyle labelStyle = DarklightEditor.BoldStyle;
                    DarklightGizmos.DrawWireSquare_withLabel("Origin Region", worldGeneration.OriginPosition, WorldBuilder.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
                    DarklightGizmos.DrawWireSquare_withLabel("World Chunk Size", worldGeneration.OriginPosition, WorldBuilder.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
                    DarklightGizmos.DrawWireSquare_withLabel("World Cell Size", worldGeneration.OriginPosition, WorldBuilder.Settings.CellSize_inGameUnits, Color.black, labelStyle);
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
                DarklightGizmos.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
                DarklightGizmos.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldBuilder.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
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
                    DarklightGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
                    {
                        _worldEditScript.SelectChunk(chunk);
                    }, Handles.RectangleHandleCap);

                    break;
                case ChunkView.TYPE:
                    chunkLabelStyle.normal.textColor = chunk.TypeColor;
                    DarklightGizmos.DrawLabel($"{chunk.Type.ToString()[0]}", chunk.CenterPosition, chunkLabelStyle);

                    DarklightGizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
                    {
                        _worldEditScript.SelectChunk(chunk);
                    }, Handles.RectangleHandleCap);
                    break;
                case ChunkView.HEIGHT:
                    DarklightGizmos.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

                    DarklightGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
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
                    DarklightGizmos.DrawButtonHandle(cell.Position, cell.Normal, cell.Size * 0.475f, Color.black, () =>
                    {
                        _worldEditScript.SelectCell(cell);
                    }, Handles.RectangleHandleCap);
                    break;
                case CellView.TYPE:
                    // Draw Face Type Label
                    DarklightGizmos.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
                    DarklightGizmos.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
                    break;
                case CellView.FACE:
                    // Draw Face Type Label
                    DarklightGizmos.DrawLabel($"{cell.FaceType}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
                    break;
            }
        }

        void DrawCoordinateMap(CoordinateMap coordinateMap, CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = DarklightEditor.CenteredStyle;
            Color coordinateColor = Color.black;

            // Draw Coordinates
            if (coordinateMap != null && coordinateMap.Initialized && coordinateMap.AllCoordinateValues.Count > 0)
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
                            DarklightGizmos.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = Color.white;
                            break;
                        case CoordinateMapView.COORDINATE_TYPE:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;
                            DarklightGizmos.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = coordinate.TypeColor;
                            break;
                        case CoordinateMapView.ZONE_ID:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;

                            if (coordinate.Type == Coordinate.TYPE.ZONE)
                            {
                                Zone zone = coordinateMap.GetZoneFromCoordinate(coordinate);
                                if (zone != null)
                                {
                                    DarklightGizmos.DrawLabel($"{zone.ID}", coordinate.ScenePosition, coordLabelStyle);
                                }
                            }

                            break;
                    }

                    // Draw Selection Rectangle
                    DarklightGizmos.DrawButtonHandle(coordinate.ScenePosition, Vector3.up, coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
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