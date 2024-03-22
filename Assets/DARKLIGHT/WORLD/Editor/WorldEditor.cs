using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World
{
	using Generation;
	using Builder;
	using Map;
	using System;
	using Darklight.Bot;
	using System.Linq;
	using System.Collections.Generic;

	public class WorldEditor : MonoBehaviour
	{
		#region [[ EDITOR SETTINGS ]] ------------------- //
		public enum EditMode { WORLD, REGION, CHUNK, CELL }
		public enum WorldView { COORDINATE_MAP, FULL_COORDINATE_MAP, };
		public enum RegionView { OUTLINE, COORDINATE_MAP, CHUNK_MAP }
		public enum ChunkView { OUTLINE, TYPE, HEIGHT, COORDINATE_MAP, CELL_MAP }
		public enum CellView { OUTLINE, TYPE, FACE }
		public enum CoordinateMapView { GRID_ONLY, COORDINATE_VALUE, COORDINATE_TYPE, ZONE_ID }
		public enum ChunkMapView { TYPE, HEIGHT }
		public enum CellMapView { TYPE, FACE }
		public EditMode editMode = EditMode.WORLD;
		public WorldView worldView = WorldView.COORDINATE_MAP;
		public RegionView regionView = RegionView.COORDINATE_MAP;
		public ChunkView chunkView = ChunkView.COORDINATE_MAP;
		public CellView cellView = CellView.OUTLINE;
		public CoordinateMapView coordinateMapView = CoordinateMapView.COORDINATE_TYPE;
		public ChunkMapView chunkMapView = ChunkMapView.TYPE;
		public CellMapView cellMapView = CellMapView.TYPE;
		#endregion

		public Region selectedRegion;
		public Chunk selectedChunk;
		public Cell selectedCell;

		public bool showNeighbors = false;

		public void SelectRegion(Region region)
		{
			selectedRegion = region;
			if (selectedRegion.coordinateValue != null)
			{
				Debug.Log("Selected Region: " + selectedRegion.coordinateValue);
				Darklight.CustomInspectorGUI.FocusSceneView(region.coordinateValue.GetPositionInScene());
			}
			else { Debug.Log("Selected Region: " + region.positionKey); }

			editMode = EditMode.REGION;
			regionView = RegionView.COORDINATE_MAP;
			coordinateMapView = CoordinateMapView.COORDINATE_TYPE;
			chunkMapView = ChunkMapView.TYPE;
		}

		public void SelectChunk(Chunk chunk)
		{
			selectedChunk = chunk;

			//Debug.Log("Selected Chunk: " + chunk.Coordinate.ValueKey);

			//Darklight.CustomInspectorGUI.FocusSceneView(chunk.Coordinate.ScenePosition);

			editMode = EditMode.CHUNK;
			chunkView = ChunkView.HEIGHT;
			coordinateMapView = CoordinateMapView.COORDINATE_TYPE;
			cellMapView = CellMapView.TYPE;
		}

		public void SelectCell(Cell cell)
		{
			selectedCell = cell;

			Debug.Log("Selected Cell: " + cell.Position);

			Darklight.CustomInspectorGUI.FocusSceneView(cell.Position);

			editMode = EditMode.CELL;
		}
	}
	/*
	#if UNITY_EDITOR
		[CustomEditor(typeof(WorldEditor), true)]
		public class WorldEditorGUI : UnityEditor.Editor
		{
			private SerializedObject _serializedObject;
			private WorldEditor _editorScript;
			private WorldBuilder _worldBuilder;
			private RegionBuilder _regionBuilder;

			EdgeDirection splitBorderDirection = EdgeDirection.NORTH; // default

			public virtual void OnEnable()
			{
				// Cache the SerializedObject
				_serializedObject = new SerializedObject(target);
				_editorScript = (WorldEditor)target;

				_worldBuilder = _editorScript.GetComponent<WorldBuilder>();
			}

			#region [[ INSPECTOR GUI ]] ================================================================= 
			public override void OnInspectorGUI()
			{
				_serializedObject = new SerializedObject(target);
				_serializedObject.Update();
				_editorScript = (WorldEditor)target;

				_editorScript.editMode = (WorldEditor.EditMode)EditorGUILayout.EnumPopup("Edit Mode", _editorScript.editMode);
				switch (_editorScript.editMode)
				{
					case WorldEditor.EditMode.WORLD:
						if (_worldBuilder != null)
						{
							_editorScript.worldView = (WorldEditor.WorldView)EditorGUILayout.EnumPopup("World View", _editorScript.worldView);
							CoordinateMapInspectorGUI(_worldBuilder.CoordinateMap, _editorScript);
						}
						break;
					case WorldEditor.EditMode.REGION:
						if (_editorScript.selectedRegion != null)
						{
							_editorScript.regionView = (WorldEditor.RegionView)EditorGUILayout.EnumPopup("Region View", _editorScript.regionView);
							if (_editorScript.regionView == WorldEditor.RegionView.COORDINATE_MAP)
							{
								CoordinateMapInspectorGUI(_editorScript.selectedRegion.CoordinateMap, _editorScript);
							}
							else if (_editorScript.regionView == WorldEditor.RegionView.CHUNK_MAP)
							{
								_editorScript.chunkView = WorldEditor.ChunkView.HEIGHT;
								ChunkMapInspectorGUI(_editorScript.selectedRegion.ChunkBuilder);
							}
						}
						break;
					case WorldEditor.EditMode.CHUNK:
						if (_editorScript.selectedChunk != null)
						{
							_editorScript.chunkView = (WorldEditor.ChunkView)EditorGUILayout.EnumPopup("Chunk View", _editorScript.chunkView);
							_editorScript.showNeighbors = EditorGUILayout.Toggle("Show Neighbors", _editorScript.showNeighbors);

							if (_editorScript.chunkView == WorldEditor.ChunkView.COORDINATE_MAP)
							{
								CoordinateMapInspectorGUI(_editorScript.selectedChunk.CoordinateMap, _editorScript);
							}
							else if (_editorScript.chunkView == WorldEditor.ChunkView.CELL_MAP)
							{
								_editorScript.cellView = WorldEditor.CellView.TYPE;
								CellMapInspectorGUI(_editorScript.selectedChunk.CellMap);
							}

							if (GUILayout.Button("Generate Chunk Mesh"))
							{
								_editorScript.selectedChunk.ChunkMesh.Recalculate();
								_editorScript.selectedChunk.ChunkBuilderParent.DestroyGameObject(_editorScript.selectedChunk.ChunkObject);
								_editorScript.selectedChunk.ChunkBuilderParent.CreateChunkObject(_editorScript.selectedChunk);

								if (_editorScript.showNeighbors)
								{
									foreach (Chunk neighbor in _editorScript.selectedChunk.GetNaturalNeighborMap().Values.ToList())
									{
										if (neighbor == null) { continue; }
										neighbor.ChunkMesh.Recalculate();
										_editorScript.selectedChunk.ChunkBuilderParent.DestroyGameObject(neighbor.ChunkObject);
										_editorScript.selectedChunk.ChunkBuilderParent.CreateChunkObject(neighbor);
									}
								}
							}


							EditorGUILayout.Space(10);
							splitBorderDirection = (EdgeDirection)EditorGUILayout.EnumPopup("Border Direction", splitBorderDirection);
							if (GUILayout.Button("Split Edge"))
							{
								//_editorScript.selectedChunk.ChunkMesh.ExtrudeQuad(Chunk.FaceDirection.TOP, Vector2Int.zero);
								HashSet<Vector2Int> borderValues = _editorScript.selectedChunk.CoordinateMap.BorderValuesMap[splitBorderDirection];

								foreach (Vector2Int borderValue in borderValues)
								{
									_editorScript.selectedChunk.ChunkMesh.SplitQuad(Chunk.FaceDirection.TOP, borderValue, splitBorderDirection);
								}

								_editorScript.selectedChunk.ChunkMesh.Recalculate();
								_editorScript.selectedChunk.ChunkBuilderParent.DestroyGameObject(_editorScript.selectedChunk.ChunkObject);
								_editorScript.selectedChunk.ChunkBuilderParent.CreateChunkObject(_editorScript.selectedChunk);
							}
						}
						break;
					case WorldEditor.EditMode.CELL:
						_editorScript.cellView = (WorldEditor.CellView)EditorGUILayout.EnumPopup("Cell View", _editorScript.cellView);

						if (GUILayout.Button("Generate Cell Mesh"))
						{
							_editorScript.selectedCell.CreateCellMeshObject();
						}
						break;
				}

			}


			public virtual void CoordinateMapInspectorGUI(CoordinateMap coordinateMap, WorldEditor worldEditor)
			{
				EditorGUILayout.Space(10);
				if (coordinateMap == null)
				{
					EditorGUILayout.LabelField("No Coordinate Map is Selected. Try Initializing the World Generation");
					return;
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space(10);
				EditorGUILayout.BeginVertical();

				// >> select debug view
				CustomInspectorGUI.CreateEnumLabel(ref worldEditor.coordinateMapView, "Coordinate Map View");
				EditorGUILayout.LabelField($"Unit Space => {coordinateMap.UnitSpace}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Initialized => {coordinateMap.Initialized}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Max Coordinate Value => {coordinateMap.MaxCoordinateValue}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Coordinate Count => {coordinateMap.AllCoordinates.Count}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Exit Count => {coordinateMap.Exits.Count}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Path Count => {coordinateMap.Paths.Count}", Darklight.CustomGUIStyles.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Zone Count => {coordinateMap.Zones.Count}", Darklight.CustomGUIStyles.LeftAlignedStyle);

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}

			public virtual void ChunkMapInspectorGUI(ChunkBuilder chunkMap)
			{
				EditorGUILayout.LabelField("Chunk Map", Darklight.CustomGUIStyles.Header2Style);
				EditorGUILayout.Space(10);
				if (chunkMap == null)
				{
					EditorGUILayout.LabelField("No Chunk Map is Selected. Try Initializing the World Generation");
					return;
				}

				// >> select debug view
				Darklight.CustomInspectorGUI.CreateEnumLabel(ref _editorScript.chunkMapView, "Chunk Map View");
			}

			public virtual void CellMapInspectorGUI(CellMap cellMap)
			{
				EditorGUILayout.LabelField("Cell Map", Darklight.CustomGUIStyles.Header2Style);
				EditorGUILayout.Space(10);
				if (cellMap == null)
				{
					EditorGUILayout.LabelField("No Cell Map is Selected. Try starting the World Generation");
					return;
				}

				// >> select debug view
				Darklight.CustomInspectorGUI.CreateEnumLabel(ref _editorScript.cellMapView, "Cell Map View");
			}

			#endregion


			#region [[ SCENE GUI ]]
			/// <summary>
			/// Enables the Editor to handle an event in the scene view.
			/// </summary>
			public virtual void OnSceneGUI()
			{
				_editorScript = (WorldEditor)target;


				switch (_editorScript.editMode)
				{
					case WorldEditor.EditMode.WORLD:
						if (_worldBuilder != null)
							DrawWorldSceneGUI(_worldBuilder);
						break;
					case WorldEditor.EditMode.REGION:
						if (_editorScript.selectedRegion != null)
							DrawRegionSceneGUI(_editorScript.selectedRegion, _editorScript);
						else if (_regionBuilder != null)
							_editorScript.SelectRegion(_regionBuilder);
						break;
					case WorldEditor.EditMode.CHUNK:
						if (_editorScript.selectedChunk != null)
							DrawChunkSceneGUI(_editorScript.selectedChunk, _editorScript);
						if (_editorScript.showNeighbors)
						{
							foreach (Chunk neighbor in _editorScript.selectedChunk.GetNaturalNeighborMap().Values.ToList())
							{
								if (neighbor == null) { continue; }
								DrawChunkSceneGUI(neighbor, _editorScript);
							}
						}
						else if (_regionBuilder != null)
							_editorScript.SelectChunk(_regionBuilder.ChunkBuilder.AllChunks.First());
						break;
					case WorldEditor.EditMode.CELL:
						if (_editorScript.selectedCell != null)
							DrawCellSceneGUI(_editorScript.selectedCell, _editorScript);
						else if (_regionBuilder != null)
							_editorScript.SelectCell(_regionBuilder.ChunkBuilder.AllChunks.First().CellMap.AllCells.First());
						break;
				}
			}


			void DrawWorldSceneGUI(WorldBuilder worldBuilder)
			{
				// [[ DRAW DEFAULT SIZE GUIDE ]]
				if (worldBuilder == null)
				{
					GUIStyle labelStyle = Darklight.CustomGUIStyles.BoldStyle;
					Darklight.CustomGizmos.DrawWireSquare_withLabel("Origin Region", worldBuilder.OriginPosition, WorldBuilder.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
					Darklight.CustomGizmos.DrawWireSquare_withLabel("World Chunk Size", worldBuilder.OriginPosition, WorldBuilder.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
					Darklight.CustomGizmos.DrawWireSquare_withLabel("World Cell Size", worldBuilder.OriginPosition, WorldBuilder.Settings.CellSize_inGameUnits, Color.black, labelStyle);
				}
				// [[ DRAW COORDINATE MAP ]]
				else if (_editorScript.worldView == WorldEditor.WorldView.COORDINATE_MAP)
				{
					DrawCoordinateMap(worldBuilder.CoordinateMap, _editorScript.coordinateMapView, (coordinate) =>
					{
						RegionBuilder selectedRegion = worldBuilder.RegionMap[coordinate.ValueKey];
						_editorScript.SelectRegion(selectedRegion);
					});
				}
			}

			#endregion

			// ==== DRAW WORLD UNITS ====================================================================================================
			public void DrawRegionSceneGUI(RegionBuilder region, WorldEditor editor)
			{
				if (region == null || region.CoordinateMap == null) { return; }

				CoordinateMap coordinateMap = region.CoordinateMap;
				ChunkBuilder chunkBuilder = region.ChunkBuilder;
				GUIStyle regionLabelStyle = Darklight.CustomGUIStyles.CenteredStyle;
				Darklight.CustomGizmos.DrawWireSquare_withLabel("Region", region.CenterPosition, RegionBuilder.Settings.RegionFullWidth_inGameUnits, Color.black, CustomGUIStyles.BoldStyle);

				// [[ DRAW GRID ONLY ]]
				if (editor.regionView == WorldEditor.RegionView.OUTLINE)
				{
					if (region.Coordinate != null)
					{
						CustomGizmos.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
					}
					CustomGizmos.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldBuilder.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
					{
						editor.SelectRegion(region);
					}, Handles.RectangleHandleCap);
				}
				// [[ DRAW COORDINATE MAP ]]
				else if (editor.regionView == WorldEditor.RegionView.COORDINATE_MAP)
				{
					DrawCoordinateMap(coordinateMap, editor.coordinateMapView, (coordinate) =>
					{
						editor.SelectChunk(chunkBuilder.GetChunkAt(coordinate));
					});
				}
				// [[ DRAW CHUNK MAP ]]
				else if (editor.regionView == WorldEditor.RegionView.CHUNK_MAP)
				{
					DrawChunkMap(chunkBuilder, editor);
				}
			}

			public void DrawChunkSceneGUI(Chunk chunk, WorldEditor editor)
			{
				if (chunk == null || chunk.CoordinateMap == null) { return; }
				GUIStyle chunkLabelStyle = Darklight.CustomGUIStyles.CenteredStyle;
				Darklight.CustomGizmos.DrawWireSquare_withLabel("Region", chunk.ChunkBuilderParent.RegionParent.CenterPosition, WorldBuilder.Settings.RegionFullWidth_inGameUnits, Color.black, CustomGUIStyles.BoldStyle);

				switch (editor.chunkView)
				{
					case WorldEditor.ChunkView.OUTLINE:

						// Draw Selection Rectangle
						Darklight.CustomGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
						{
							editor.SelectChunk(chunk);
						}, Handles.RectangleHandleCap);

						break;
					case WorldEditor.ChunkView.TYPE:
						chunkLabelStyle.normal.textColor = chunk.TypeColor;
						Darklight.CustomGizmos.DrawLabel($"{chunk.Type.ToString()}", chunk.GroundPosition, chunkLabelStyle);

						Darklight.CustomGizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
						{
							editor.SelectChunk(chunk);
						}, Handles.RectangleHandleCap);
						break;
					case WorldEditor.ChunkView.HEIGHT:
						Darklight.CustomGizmos.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

						Darklight.CustomGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
						{
							editor.SelectChunk(chunk);
						}, Handles.RectangleHandleCap);
						break;
					case WorldEditor.ChunkView.COORDINATE_MAP:

						DrawCoordinateMap(chunk.CoordinateMap, editor.coordinateMapView, (coordinate) =>
						{
							Cell cell = chunk.CellMap.GetCellAtCoordinate(coordinate);
							_editorScript.SelectCell(cell);
						});

						break;
					case WorldEditor.ChunkView.CELL_MAP:

						DrawCellMap(chunk.CellMap, editor);
						break;
				}
			}

			public void DrawCellSceneGUI(Cell cell, WorldEditor editor)
			{
				GUIStyle cellLabelStyle = Darklight.CustomGUIStyles.CenteredStyle;

				switch (editor.cellView)
				{
					case WorldEditor.CellView.OUTLINE:
						// Draw Selection Rectangle
						//Darklight.CustomGizmos.DrawWireSquare(cell.Position, cell.Size, Color.black, cell.Normal);
						Darklight.CustomGizmos.DrawButtonHandle(cell.Position, Vector3.up, cell.Size * 0.475f, cell.TypeColor, () =>
						{
							editor.SelectCell(cell);
						}, Handles.RectangleHandleCap);
						break;
					case WorldEditor.CellView.TYPE:
						// Draw Face Type Label
						//Darklight.CustomGizmos.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
						//Darklight.CustomGizmos.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
						Darklight.CustomGizmos.DrawButtonHandle(cell.Position, Vector3.up, cell.Size * 0.475f, cell.TypeColor, () =>
						{
							editor.SelectCell(cell);
						}, Handles.RectangleHandleCap);
						break;
					case WorldEditor.CellView.FACE:
						Darklight.CustomGizmos.DrawButtonHandle(cell.Position, Vector3.up, cell.Size * 0.475f, cell.TypeColor, () =>
						{
							editor.SelectCell(cell);
						}, Handles.RectangleHandleCap);
						//Darklight.CustomGizmos.DrawLabel($"{cell.FaceType} {cell.FaceCoord}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
						break;
				}
			}

			/// <summary>
			/// Draw a CoordinateMap with selectable coordinates
			/// </summary>
			/// <param name="coordinateMap"></param>
			/// <param name="mapView"></param>
			/// <param name="onCoordinateSelect"></param> <summary>
			/// 
			/// </summary>
			/// <param name="coordinateMap"></param>
			/// <param name="mapView"></param>
			/// <param name="onCoordinateSelect"></param>
			public void DrawCoordinateMap(CoordinateMap coordinateMap, WorldEditor.CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
			{
				GUIStyle coordLabelStyle = Darklight.CustomGUIStyles.CenteredStyle;
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
			}

			public void DrawChunkMap(ChunkBuilder chunkMap, WorldEditor editor)
			{
				GUIStyle chunkLabelStyle = Darklight.CustomGUIStyles.CenteredStyle;
				Color chunkColor = Color.black;

				// Draw Chunks
				if (chunkMap != null && chunkMap.Initialized)
				{
					foreach (Chunk chunk in chunkMap.AllChunks)
					{
						// Draw Custom View
						switch (editor.chunkMapView)
						{
							case WorldEditor.ChunkMapView.TYPE:
								DrawChunkSceneGUI(chunk, editor);
								break;
							case WorldEditor.ChunkMapView.HEIGHT:
								DrawChunkSceneGUI(chunk, editor);
								break;
						}
					}
				}
			}

			public void DrawCellMap(CellMap cellMap, WorldEditor editor)
			{
				if (cellMap == null) return;

				GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
				foreach (Cell cell in cellMap.AllCells)
				{
					// Draw Custom View
					switch (editor.cellMapView)
					{
						case WorldEditor.CellMapView.TYPE:
							if (cell.Type == Cell.TYPE.SPAWN_POINT)
							{
								DrawCellSceneGUI(cell, editor);
							}
							break;
						case WorldEditor.CellMapView.FACE:
							DrawCellSceneGUI(cell, editor);
							break;
					}
				}
			}

}
#endif

			*/

}
