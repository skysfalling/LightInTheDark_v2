using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace Darklight.ThirdDimensional.World
{
#if UNITY_EDITOR
    using UnityEditor;
    using EditMode = WorldEdit.EditMode;
    using WorldView = WorldEdit.WorldView;
    using RegionView = WorldEdit.RegionView;
    using CoordinateMapView = WorldEdit.CoordinateMapView;
    using ChunkMapView = WorldEdit.ChunkMapView;
    using Unity.Properties;
#endif

    public class WorldEdit : MonoBehaviour
    {
        public enum EditMode { WORLD, REGION, CHUNK, CELL }
        public EditMode editMode = EditMode.WORLD;

        // World View
        public enum WorldView { COORDINATE_MAP, ALL_REGION_COORDINATES };
        public WorldView worldView = WorldView.COORDINATE_MAP;

        // Region View
        public enum RegionView { OUTLINE, COORDINATE_MAP, CHUNK_MAP}
        public RegionView regionView = RegionView.COORDINATE_MAP;

        // Coordinate Map
        public enum CoordinateMapView { GRID_ONLY, VALUE, TYPE }
        public CoordinateMapView coordinateMapView = CoordinateMapView.TYPE;

        // Chunk Map
        public enum ChunkMapView { TYPE, HEIGHT }
        public ChunkMapView chunkMapView = ChunkMapView.TYPE;

        public WorldGeneration worldGeneration => GetComponent<WorldGeneration>();
        public Region selectedRegion;
        public Chunk selectedChunk;
        public Cell selectedCell;

        public void SelectRegion(Region region)
        {
            selectedRegion = region;

            //Debug.Log("Selected Region: " + selectedRegion.Coordinate.Value);

            //CustomEditorLibrary.FocusSceneView(region.Coordinate.ScenePosition, WorldGeneration.Settings.RegionFullWidth_inGameUnits * 2);

            editMode = EditMode.REGION;
        }

        public void SelectChunk(Chunk chunk)
        {
            selectedChunk = chunk;

            //Debug.Log("Selected Chunk: " + chunk.Coordinate.Value);

            //CustomEditorLibrary.FocusSceneView(chunk.Coordinate.ScenePosition, WorldGeneration.Settings.ChunkWidth_inGameUnits * 2);

            editMode = EditMode.CHUNK;
        }
    }

#if UNITY_EDITOR
   
    [CustomEditor(typeof(WorldEdit))]
    public class WorldEditGUI : UnityEditor.Editor
    {
        private SerializedObject _serializedObject;

        private WorldEdit _worldEditScript;

        private void OnEnable()
        {
            // Cache the SerializedObject
            _serializedObject = new SerializedObject(target);
            _worldEditScript = (WorldEdit)target;

            _worldEditScript.editMode = EditMode.WORLD;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            // [[ EDITOR VIEW ]]
            CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.editMode, "Edit Mode");

            EditorGUILayout.Space();

            switch (_worldEditScript.editMode)
            {
                case EditMode.WORLD:

                    CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.worldView, "World View");
                    CoordinateMapInspector( _worldEditScript.worldGeneration.CoordinateMap);
                    break;
                case EditMode.REGION:
                    if (_worldEditScript.selectedRegion == null) { break; }

                    CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.regionView, "Region View");
                    CoordinateMapInspector( _worldEditScript.selectedRegion.CoordinateMap);
                    ChunkMapInspector(_worldEditScript.selectedRegion.ChunkMap);
                    break;
                case EditMode.CHUNK:
                    if (_worldEditScript.selectedChunk == null) { break; }

                    CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.regionView, "Chunk View");
                    CoordinateMapInspector( _worldEditScript.selectedChunk.CoordinateMap);
                    break;
            }

            _serializedObject.ApplyModifiedProperties();

            void CoordinateMapInspector(CoordinateMap coordinateMap)
            {
                EditorGUILayout.LabelField("Coordinate Map", Darklight.CustomEditorLibrary.Header2Style);
                EditorGUILayout.Space(10);
                if (coordinateMap == null)
                {
                    EditorGUILayout.LabelField("No Coordinate Map is Selected. Try Initializing the World Generation");
                    return;
                }

                // >> select debug view
                CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.coordinateMapView, "Coordinate Map View");
            }

            void ChunkMapInspector(ChunkMap chunkMap)
            {
                EditorGUILayout.LabelField("Chunk Map", Darklight.CustomEditorLibrary.Header2Style);
                EditorGUILayout.Space(10);
                if (chunkMap == null)
                {
                    EditorGUILayout.LabelField("No Chunk Map is Selected. Try Initializing the World Generation");
                    return;
                }

                // >> select debug view
                CustomEditorLibrary.DrawLabeledEnumPopup(ref _worldEditScript.chunkMapView, "Chunk Map View");

            }
        }



        // ==================== SCENE GUI =================================================== ////


        protected void OnSceneGUI()
        {

            // >> draw world generation bounds
            WorldGeneration worldGeneration = _worldEditScript.worldGeneration;
            CustomGizmoLibrary.DrawWireSquare_withLabel("World Generation", worldGeneration.CenterPosition, WorldGeneration.Settings.WorldWidth_inGameUnits, Color.black, CustomEditorLibrary.CenteredStyle);

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
            }

            void DrawWorldEditorGUI()
            {
                WorldGeneration worldGeneration = _worldEditScript.worldGeneration;

                // [[ DRAW DEFAULT SIZE GUIDE ]]
                if (worldGeneration == null || worldGeneration.CoordinateMap == null)
                {
                    GUIStyle labelStyle = CustomEditorLibrary.BoldStyle;
                    CustomGizmoLibrary.DrawWireSquare_withLabel("Origin Region", worldGeneration.OriginPosition, WorldGeneration.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
                    CustomGizmoLibrary.DrawWireSquare_withLabel("World Chunk Size", worldGeneration.OriginPosition, WorldGeneration.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
                    CustomGizmoLibrary.DrawWireSquare_withLabel("World Cell Size", worldGeneration.OriginPosition, WorldGeneration.Settings.CellSize_inGameUnits, Color.black, labelStyle);
                }
                // [[ DRAW COORDINATE MAP ]]
                else if (_worldEditScript.worldView == WorldView.COORDINATE_MAP)
                {
                    DrawCoordinateMap(worldGeneration.CoordinateMap, _worldEditScript.coordinateMapView,(coordinate) =>
                    {
                        Region selectedRegion = _worldEditScript.worldGeneration.RegionMap[coordinate.Value];
                        _worldEditScript.SelectRegion(selectedRegion);
                    });
                }
                // [[ DRAW ALL CHILD REGION COORDINATES ]]
                else if (_worldEditScript.worldView == WorldView.ALL_REGION_COORDINATES)
                {
                    if (worldGeneration.AllRegions.Count > 0)
                    {
                        foreach (Region region in worldGeneration.AllRegions)
                        {
                            DrawRegion(region, RegionView.COORDINATE_MAP);
                        }
                    }
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

            }

            Repaint();
        }



        void DrawRegion(Region region, RegionView type)
        {
            if (region == null || region.CoordinateMap == null ) { return; }

            CoordinateMap coordinateMap = region.CoordinateMap;
            ChunkMap chunkMap = region.ChunkMap;
            GUIStyle regionLabelStyle = CustomEditorLibrary.CenteredStyle;

            // [[ DRAW GRID ONLY ]]
            if (type == RegionView.OUTLINE)
            {
                CustomGizmoLibrary.DrawLabel($"{region.Coordinate.Value}", region.Position, regionLabelStyle);
                CustomGizmoLibrary.DrawButtonHandle(region.Position, Quaternion.LookRotation(Vector3.up), WorldGeneration.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
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

        void DrawCoordinateMap(CoordinateMap coordinateMap, CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
        {
            GUIStyle coordLabelStyle = CustomEditorLibrary.CenteredStyle;
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
                        case CoordinateMapView.VALUE:
                            CustomGizmoLibrary.DrawLabel($"{coordinate.Value}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = Color.white;
                            break;
                        case CoordinateMapView.TYPE:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;
                            CustomGizmoLibrary.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
                            coordinateColor = coordinate.TypeColor;
                            break;
                    }

                    // Draw Selection Rectangle
                    CustomGizmoLibrary.DrawButtonHandle(coordinate.ScenePosition, Quaternion.LookRotation(Vector3.up), coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
                    {
                        onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
                    }, Handles.RectangleHandleCap);
                }
            }
        }

        void DrawCoordinateNeighbors(Coordinate coordinate)
        {
            if (coordinate.Initialized)
            {
                List<Coordinate> natural_neighbors = coordinate.GetValidNaturalNeighbors();

                foreach (Coordinate neighbor in natural_neighbors)
                {
                    WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                    Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.25f;

                    CustomGizmoLibrary.DrawArrow(coordinate.ScenePosition, direction, Color.red);
                }

                List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
                foreach (Coordinate neighbor in diagonal_neighbors)
                {
                    WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                    Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.25f;

                    CustomGizmoLibrary.DrawArrow(coordinate.ScenePosition, direction, Color.yellow);
                }
            }
        }

        void DrawChunkMap(ChunkMap chunkMap, ChunkMapView mapView)
        {
            GUIStyle chunkLabelStyle = CustomEditorLibrary.CenteredStyle;
            Color chunkColor = Color.black;

            // Draw Coordinates
            if (chunkMap != null && chunkMap.Initialized)
            {
                foreach (Chunk chunk in chunkMap.AllChunks)
                {
                    // Draw Custom View
                    switch (mapView)
                    {
                        case ChunkMapView.TYPE:

                            CustomGizmoLibrary.DrawFilledSquareAt(chunk.CenterPosition, WorldGeneration.Settings.ChunkWidth_inGameUnits, Vector3.up, Color.grey);

                            // Draw Selection Rectangle
                            CustomGizmoLibrary.DrawButtonHandle(chunk.CenterPosition, Quaternion.LookRotation(Vector3.up), WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.475f, chunkColor, () =>
                            {
                                _worldEditScript.SelectChunk(chunk);
                            }, Handles.CubeHandleCap);

                            break;
                        case ChunkMapView.HEIGHT:

                            // Draw Height Label
                            CustomGizmoLibrary.DrawLabel($"{chunk.GroundHeight}", chunk.CenterPosition + (Vector3.up * WorldGeneration.Settings.CellSize_inGameUnits), chunkLabelStyle);

                            // Draw Selection Rectangle
                            CustomGizmoLibrary.DrawButtonHandle(chunk.CenterPosition, Quaternion.LookRotation(Vector3.up), WorldGeneration.Settings.ChunkWidth_inGameUnits * 0.475f, chunkColor, () =>
                            {
                                _worldEditScript.SelectChunk(chunk);
                            }, Handles.RectangleHandleCap);

                        break;
                    }


                }
            }
        }

    }


#endif


}


