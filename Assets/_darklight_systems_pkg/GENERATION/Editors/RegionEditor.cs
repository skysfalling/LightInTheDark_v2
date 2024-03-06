using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Editor
{
    using WorldGen = WorldGeneration;

    [CustomEditor(typeof(Region))]
    public class RegionEditor : UnityEditor.Editor
    {
        private SerializedObject _serializedObject;
        private Region _region;

        GUIStyle titleHeaderStyle;
        GUIStyle centeredStyle;
        GUIStyle rightAlignedStyle;
        GUIStyle h1Style;
        GUIStyle h2Style;
        GUIStyle pStyle;

        // Editor View
        public enum RegionEditorView { REGION, CHUNK, CELL }
        static RegionEditorView editorViewSpace = RegionEditorView.REGION;

        // Coordinate Map
        enum CoordinateMapDebug { NONE, COORDINATE, TYPE, EDITOR }
        static CoordinateMapDebug coordinateMapDebugType = CoordinateMapDebug.TYPE;
        Coordinate selectedCoordinate = null;

        // Chunk Map
        enum ChunkMapDebug { NONE, COORDINATE_TYPE, CHUNK_TYPE, CHUNK_HEIGHT }
        static ChunkMapDebug chunkMapDebugType = ChunkMapDebug.COORDINATE_TYPE;
        Chunk selectedChunk = null;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _region = (Region)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            #region STYLES ======================= >>>>
            titleHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };

            centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            rightAlignedStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            h1Style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40,
            };

            h2Style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40,
            };

            pStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
            };
            pStyle.margin.left = 20;
            #endregion ================================== ////


            // [[ EDITOR VIEW ]]
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Editor View Space  =>");
            editorViewSpace = (RegionEditorView)EditorGUILayout.EnumPopup(editorViewSpace);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // [[ REGION VIEW ]] ============================================================ \\\\
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Region", h1Style);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Region Coordinate:", _region.Coordinate.Value.ToString());
            EditorGUILayout.LabelField("Center Position:", _region.CenterPosition.ToString());
            EditorGUILayout.LabelField("Origin Coordinate Position:", _region.OriginPosition.ToString());
            EditorGUILayout.LabelField("Region Initialized:", _region.Initialized.ToString());

            EditorGUILayout.EndVertical(); // ======================================= ////

            switch (editorViewSpace)
            {
                case RegionEditorView.REGION:
                    DrawCoordinateMapInspector();
                    break;
                case RegionEditorView.CHUNK:
                    DrawChunkMapInspector();
                    break;
                case RegionEditorView.CELL:
                    DrawCellMapInspector();
                    break;
            }

            _serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        void DrawCoordinateMapInspector()
        {
            // [[ COORDINATE MAP ]]
            CoordinateMap coordinateMap = _region.CoordinateMap;

            EditorGUILayout.LabelField("Region Coordinate Map", h2Style);
            EditorGUILayout.Space(10);

            // >> initialize button
            if (_region.CoordinateMap == null)
            {
                return;
            }

            // >> select debug view
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug View  =>");
            coordinateMapDebugType = (CoordinateMapDebug)EditorGUILayout.EnumPopup(coordinateMapDebugType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // TYPE >> show coordinate type data
            if (coordinateMapDebugType == CoordinateMapDebug.TYPE)
            {
                EditorGUILayout.LabelField($"NULL TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.NULL).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"BORDER TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.BORDER).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"CLOSED TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.CLOSED).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"EXIT TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.EXIT).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"PATH TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.PATH).Count}", rightAlignedStyle);
                EditorGUILayout.LabelField($"ZONE TYPE : {coordinateMap.GetAllPositionsOfType(Coordinate.TYPE.ZONE).Count}", rightAlignedStyle);

            }
            // EDITOR >> edit the coordinates in the map
            else if (coordinateMapDebugType == CoordinateMapDebug.EDITOR)
            {
                #region Exits
                // << EXITS >> // ==================================================== \\\\
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Exits", h2Style);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Exit Count: {coordinateMap.Exits.Count}");

                // >> show exits
                if (coordinateMap.Exits.Count > 0)
                {
                    foreach (Vector2Int pos in coordinateMap.Exits)
                    {
                        EditorGUILayout.LabelField($">> Exit At: {pos}");
                    }
                }

                // >>>> create random exits
                if (GUILayout.Button("Generate Random Exits"))
                {
                    coordinateMap.GenerateRandomExits();
                }

                // >>>> convert selected to exit
                if (selectedCoordinate != null && GUILayout.Button("Convert Selected Coordinate to EXIT"))
                {
                    coordinateMap.ConvertCoordinateToExit(selectedCoordinate);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical(); // ==================================================== ////
                #endregion

                // << PATHS >>
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Paths", h2Style);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Path Count: {coordinateMap.Paths.Count}");

                // >>>> create paths
                if (coordinateMap.Exits.Count > 1)
                {
                    if (GUILayout.Button("Generate Paths"))
                    {
                        coordinateMap.GeneratePathsBetweenExits();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                // << ZONES >>
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Zones", h2Style);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Zone Count: {coordinateMap.Zones.Count}");

                // >>>> create zones
                if (GUILayout.Button("Generate Random Zones"))
                {
                    coordinateMap.GenerateRandomZones(1, 4);
                }

                // >>>> create zone at selected
                if (GUILayout.Button("Create Zone At Selected"))
                {
                    coordinateMap.CreateWorldZone(selectedCoordinate.Value, Zone.TYPE.FULL, 5);
                }


                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawChunkMapInspector()
        {
            ChunkMap chunkMap = _region.ChunkMap;

            EditorGUILayout.LabelField("Region Chunk Map", h2Style);
            EditorGUILayout.Space(10);

            // << DEBUG VIEW >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug View  =>");
            chunkMapDebugType = (ChunkMapDebug)EditorGUILayout.EnumPopup(chunkMapDebugType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // << SELECTED CHUNK >>
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected Chunk", h2Style);
            EditorGUILayout.Space(10);
            if (selectedChunk != null && selectedChunk.Coordinate != null)
            {
                EditorGUILayout.LabelField($"Coordinate Local Position:  {selectedChunk.Coordinate.Value}");
                EditorGUILayout.LabelField($"Chunk Ground Height:  {selectedChunk.GroundHeight}");
                EditorGUILayout.LabelField($"Chunk Type:  {selectedChunk.type}");
            }
            else
            {
                EditorGUILayout.LabelField($"Please Select a Chunk in the Scene View");
            }
            EditorGUILayout.EndVertical();
        }

        void DrawCellMapInspector()
        {
            EditorGUILayout.LabelField("Chunk Cells", h2Style);
            EditorGUILayout.Space(10);

            // << SELECTED CHUNK >>
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected Chunk", h2Style);
            EditorGUILayout.Space(10);
            if (selectedChunk != null && selectedChunk.Coordinate != null)
            {
                EditorGUILayout.LabelField($"Chunk Local Cells:  {selectedChunk.localCells.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Please Select a Chunk in the Scene View");
            }
            EditorGUILayout.EndVertical();
        }

        // ==================== SCENE GUI =================================================== ////


        protected void OnSceneGUI()
        {
            WorldGeneration worldGen = _region.GetComponentInParent<WorldGeneration>();
            Transform transform = _region.transform;

            GUIStyle labelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 12, // Example font size
            };

            DarklightGizmos.DrawWireSquare_withLabel("World Generation", worldGen.transform.position, WorldGen.Settings.WorldWidth_inGameUnits, Color.black, labelStyle);
            DarklightGizmos.DrawWireSquare_withLabel("World Region", transform.position, WorldGen.Settings.RegionFullWidth_inGameUnits, Color.blue, labelStyle);

            switch (editorViewSpace)
            {
                case RegionEditorView.REGION:
                    DrawRegionView();
                    break;
                case RegionEditorView.CHUNK:
                    DrawChunkView();
                    break;
                case RegionEditorView.CELL:
                    DrawCellView();
                    break;
            }
        }

        void DrawRegionView()
        {
            if (_region == null || _region.CoordinateMap == null) { return; }

            GUIStyle coordLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 12, // Example font size
                normal = new GUIStyleState { textColor = Color.blue } // Set the text color
            };

            // Draw Coordinates
            CoordinateMap coordinateMap = _region.CoordinateMap;
            if (coordinateMap.Initialized && coordinateMap.AllPositions.Count > 0)
            {
                foreach (Vector2Int position in coordinateMap.AllPositions)
                {
                    Coordinate coordinate = coordinateMap.GetCoordinateAt(position);

                    if (selectedCoordinate != null && position == selectedCoordinate.Value)
                    {
                        DrawCoordinateNeighbors(coordinateMap.GetCoordinateAt(position));
                        continue;
                    }

                    DarklightGizmos.DrawButtonHandle(coordinate.Position, Vector3.right * 90, WorldGen.Settings.CellSize_inGameUnits, Color.black, () =>
                    {
                        SelectCoordinate(coordinate);
                    });

                    switch (coordinateMapDebugType)
                    {
                        case CoordinateMapDebug.COORDINATE:
                            DarklightGizmos.DrawWireSquare(coordinate.Position, WorldGen.Settings.CellSize_inGameUnits, Color.blue);
                            DarklightGizmos.DrawLabel($"{coordinate.Value}", coordinate.Position - (Vector3.forward * WorldGen.Settings.CellSize_inGameUnits), coordLabelStyle);
                            break;
                        case CoordinateMapDebug.TYPE:
                        case CoordinateMapDebug.EDITOR:
                            coordLabelStyle.normal.textColor = coordinate.TypeColor;
                            DarklightGizmos.DrawWireSquare(coordinate.Position, WorldGen.Settings.CellSize_inGameUnits, coordinate.TypeColor);
                            DarklightGizmos.DrawLabel($"{coordinate.Type}", coordinate.Position - (Vector3.forward * WorldGen.Settings.CellSize_inGameUnits), coordLabelStyle);
                            break;
                    }
                }
            }
            else
            {
                DarklightGizmos.DrawWireSquare_withLabel($"region origin", _region.OriginPosition, 10, Color.red, coordLabelStyle);
            }
        }

        void DrawChunkView()
        {
            if (_region == null || _region.ChunkMap == null) { return; }
            GUIStyle chunkLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 12, // Example font size
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.black } // Set the text color
            };

            ChunkMap chunkMap = _region.ChunkMap;
            if (chunkMap.Initialized)
            {
                foreach (Chunk chunk in chunkMap.AllChunks)
                {
                    Color chunkDebugColor = Color.green;
                    string chunkDebugString = "Chunk";

                    switch (chunkMapDebugType)
                    {
                        case ChunkMapDebug.COORDINATE_TYPE:
                            chunkDebugColor = chunk.Coordinate.TypeColor;
                            chunkDebugString = $"{chunk.Coordinate.Type}";
                            break;
                        case ChunkMapDebug.CHUNK_TYPE:
                            chunkDebugColor = chunk.TypeColor;
                            chunkDebugString = $"{chunk.type}";
                            break;
                        case ChunkMapDebug.CHUNK_HEIGHT:
                            chunkDebugColor = Color.Lerp(Color.black, Color.white, (float)chunk.GroundHeight / (float)WorldGen.Settings.ChunkMaxHeight_inCellUnits);
                            chunkDebugString = $"{chunk.GroundHeight}";
                            break;
                    }


                    DarklightGizmos.DrawFilledSquareAt(chunk.CenterPosition, WorldGen.Settings.ChunkWidth_inGameUnits * 0.75f, Vector3.up, chunkDebugColor);
                    DarklightGizmos.DrawLabel(chunkDebugString, chunk.CenterPosition - (Vector3.forward * WorldGen.Settings.CellSize_inGameUnits * 2), chunkLabelStyle);

                    DarklightGizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.right * 90, WorldGen.Settings.CellSize_inGameUnits * 0.5f, Color.black, () =>
                    {
                        SelectChunk(chunk);
                    });

                }
            }
        }

        void DrawCellView()
        {
            if (_region == null || _region.ChunkMap == null) { return; }
            GUIStyle cellLabelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold, // Example style
                fontSize = 8, // Example font size
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.white } // Set the text color
            };

            ChunkMap chunkMap = _region.ChunkMap;
            if (chunkMap.Initialized)
            {
                foreach (Chunk chunk in chunkMap.AllChunks)
                {
                    Color chunkDebugColor = Color.white;
                    string chunkDebugString = "";

                    DarklightGizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.right * 90, WorldGen.Settings.CellSize_inGameUnits * 0.5f, Color.black, () =>
                    {
                        SelectChunk(chunk);
                    });

                }

                if (selectedChunk != null)
                {
                    foreach (WorldCell cell in selectedChunk.localCells)
                    {
                        DarklightGizmos.DrawFilledSquareAt(cell.Position, WorldGen.Settings.CellSize_inGameUnits * 0.75f, cell.MeshQuad.faceNormal, Color.grey);
                        DarklightGizmos.DrawLabel($"{cell.MeshQuad.faceCoord}", cell.Position, cellLabelStyle);
                    }
                }
            }
        }

        void SelectCoordinate(Coordinate coordinate)
        {
            selectedCoordinate = coordinate;

            Repaint();
        }

        void SelectChunk(Chunk chunk)
        {
            selectedChunk = chunk;

            Debug.Log("Selected Chunk: " + selectedChunk.Coordinate.Value);

            Repaint();
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
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGen.Settings.ChunkWidth_inGameUnits * 0.25f;

                    DarklightGizmos.DrawArrow(coordinate.Position, direction, Color.red);
                }

                List<Coordinate> diagonal_neighbors = coordinate.GetValidDiagonalNeighbors();
                foreach (Coordinate neighbor in diagonal_neighbors)
                {
                    WorldDirection neighborDirection = (WorldDirection)coordinate.GetWorldDirectionOfNeighbor(neighbor);
                    Vector2Int directionVector = CoordinateMap.GetDirectionVector(neighborDirection);
                    Vector3 direction = new Vector3(directionVector.x, 0, directionVector.y) * WorldGen.Settings.ChunkWidth_inGameUnits * 0.25f;

                    DarklightGizmos.DrawArrow(coordinate.Position, direction, Color.yellow);
                }
            }
        }
    }
}
