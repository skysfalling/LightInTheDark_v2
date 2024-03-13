using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Darklight.World;
using Darklight.World.Generation;
using Darklight.World.Builder;
using Darklight.World.Editor;
using Darklight.World.Map;
using System;
using Darklight.Bot;
using Darklight.Bot.Editor;
#endif

namespace Darklight.World
{
	using Generation;
	using Builder;
	using Editor;
	using Map;
	using System;
	using Darklight.Bot;

	public class WorldEditor : MonoBehaviour
	{
		#region [[ EDITOR SETTINGS ]] ------------------- //
		public enum EditMode { WORLD, REGION, CHUNK, CELL }
		public EditMode editMode = EditMode.WORLD;

		// World View
		public enum WorldView { COORDINATE_MAP, FULL_COORDINATE_MAP, };
		public WorldView worldView = WorldView.COORDINATE_MAP;

		// Region View
		public enum RegionView { OUTLINE, COORDINATE_MAP, CHUNK_MAP }
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
		#endregion

		public WorldBuilder worldBuilder => GetComponentInParent<WorldBuilder>();
		public RegionBuilder selectedRegion;
		public Chunk selectedChunk;
		public Cell selectedCell;

		public void SelectRegion(RegionBuilder region)
		{
			selectedRegion = region;

			//Debug.Log("Selected Region: " + selectedRegion.Coordinate.ValueKey);

			Darklight.CustomInspectorGUI.FocusSceneView(region.Coordinate.ScenePosition);

			editMode = EditMode.REGION;
		}

		public void SelectChunk(Chunk chunk)
		{
			selectedChunk = chunk;

			//Debug.Log("Selected Chunk: " + chunk.Coordinate.ValueKey);

			Darklight.CustomInspectorGUI.FocusSceneView(chunk.Coordinate.ScenePosition);

			editMode = EditMode.CHUNK;
		}

		public void SelectCell(Cell cell)
		{
			selectedCell = cell;

			//Debug.Log("Selected Cell: " + cell.Position);

			Darklight.CustomInspectorGUI.FocusSceneView(cell.Position);

			editMode = EditMode.CELL;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(WorldEditor))]
	public class WorldEditorGUI : TaskQueenEditor
	{
		private SerializedObject _serializedWorldEditObject;
		private WorldEditor _worldEditScript;
		private WorldBuilder _worldBuilderScript;

		private bool coordinateMapFoldout;

		public override void OnEnable()
		{
			// Cache the SerializedObject
			_serializedWorldEditObject = new SerializedObject(target);
			_worldEditScript = (WorldEditor)target;
			_worldBuilderScript = _worldEditScript.GetComponentInParent<WorldBuilder>();
			_worldEditScript.editMode = WorldEditor.EditMode.WORLD;
		}

		#region [[ INSPECTOR GUI ]] ================================================================= 

		public override void OnInspectorGUI()
		{
			_serializedWorldEditObject.Update();

			WorldBuilder worldBuilder = _worldBuilderScript;


			// [[ EDITOR VIEW ]]
			Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.editMode, "Edit Mode");

			EditorGUILayout.Space();

			switch (_worldEditScript.editMode)
			{
				case WorldEditor.EditMode.WORLD:

					EditorGUILayout.LabelField("World Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.worldView, "World View");

					// SHOW WORLD STATS


					CoordinateMapInspector(_worldBuilderScript.CoordinateMap);
					break;
				case WorldEditor.EditMode.REGION:
					if (_worldEditScript.selectedRegion == null && worldBuilder.AllRegions.Count > 0)
					{
						_worldEditScript.SelectRegion(worldBuilder.RegionMap[Vector2Int.zero]);
						break;
					}

					EditorGUILayout.LabelField("Region Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					// SHOW REGION STATS
					RegionBuilder selectedRegion = _worldEditScript.selectedRegion;
					EditorGUILayout.LabelField($"Coordinate ValueKey => {selectedRegion.Coordinate.ValueKey}", Darklight.CustomInspectorGUI.LeftAlignedStyle);

					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.regionView, "Region View");
					CoordinateMapInspector(_worldEditScript.selectedRegion.CoordinateMap);
					ChunkMapInspector(_worldEditScript.selectedRegion.ChunkBuilder);
					break;
				case WorldEditor.EditMode.CHUNK:
					if (_worldEditScript.selectedChunk == null && worldBuilder.Initialized)
					{
						RegionBuilder originRegion = worldBuilder.RegionMap[Vector2Int.zero];
						_worldEditScript.SelectChunk(originRegion.ChunkBuilder.GetChunkAt(Vector2Int.zero));
						break;
					}

					EditorGUILayout.LabelField("Chunk Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					// SHOW CHUNK STATS



					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.chunkView, "Chunk View");
					//CoordinateMapInspector( _worldEditScript.selectedChunk);
					CellMapInspector(_worldEditScript.selectedChunk.CellMap);
					break;
				case WorldEditor.EditMode.CELL:
					if (_worldEditScript.selectedCell == null)
					{
						break;
					}

					EditorGUILayout.LabelField("Cell Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					// SHOW CELL STATS

					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.cellView, "Cell View");
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
					//DarklightEditor.DrawLabeledEnumPopup(ref _worldEditScript.coordinateMapView, "Coordinate Map View");
					EditorGUILayout.LabelField($"Unit Space => {coordinateMap.UnitSpace}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Initialized => {coordinateMap.Initialized}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Max Coordinate Value => {coordinateMap.MaxCoordinateValue}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Coordinate Count => {coordinateMap.AllCoordinates.Count}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Exit Count => {coordinateMap.Exits.Count}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Path Count => {coordinateMap.Paths.Count}", Darklight.CustomInspectorGUI.LeftAlignedStyle);
					EditorGUILayout.LabelField($"Zone Count => {coordinateMap.Zones.Count}", Darklight.CustomInspectorGUI.LeftAlignedStyle);

					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
			}

			void ChunkMapInspector(ChunkBuilder chunkMap)
			{
				EditorGUILayout.LabelField("Chunk Map", Darklight.CustomInspectorGUI.Header2Style);
				EditorGUILayout.Space(10);
				if (chunkMap == null)
				{
					EditorGUILayout.LabelField("No Chunk Map is Selected. Try Initializing the World Generation");
					return;
				}

				// >> select debug view
				Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.chunkMapView, "Chunk Map View");
			}

			void CellMapInspector(CellMap cellMap)
			{
				EditorGUILayout.LabelField("Cell Map", Darklight.CustomInspectorGUI.Header2Style);
				EditorGUILayout.Space(10);
				if (cellMap == null)
				{
					EditorGUILayout.LabelField("No Cell Map is Selected. Try starting the World Generation");
					return;
				}

				// >> select debug view
				Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.cellMapView, "Cell Map View");
			}
		}

		#endregion


		// ==================== SCENE GUI =================================================== ////

		protected void OnSceneGUI()
		{

			// >> draw world generation bounds
			WorldBuilder worldGeneration = _worldEditScript.worldBuilder;
			Darklight.Gizmos.DrawWireSquare_withLabel("World Generation", worldGeneration.CenterPosition, WorldBuilder.Settings.WorldWidth_inGameUnits, Color.black, Darklight.CustomInspectorGUI.CenteredStyle);

			switch (_worldEditScript.editMode)
			{
				case WorldEditor.EditMode.WORLD:
					DrawWorldEditorGUI();
					break;
				case WorldEditor.EditMode.REGION:
					DrawRegionEditorGUI();
					break;
				case WorldEditor.EditMode.CHUNK:
					DrawChunkEditorGUI();
					break;
				case WorldEditor.EditMode.CELL:
					DrawCellEditorGUI();
					break;
			}

			void DrawWorldEditorGUI()
			{
				WorldBuilder worldGeneration = _worldEditScript.worldBuilder;

				// [[ DRAW DEFAULT SIZE GUIDE ]]
				if (worldGeneration == null || worldGeneration.CoordinateMap == null)
				{
					GUIStyle labelStyle = Darklight.CustomInspectorGUI.BoldStyle;
					Darklight.Gizmos.DrawWireSquare_withLabel("Origin Region", worldGeneration.OriginPosition, WorldBuilder.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
					Darklight.Gizmos.DrawWireSquare_withLabel("World Chunk Size", worldGeneration.OriginPosition, WorldBuilder.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
					Darklight.Gizmos.DrawWireSquare_withLabel("World Cell Size", worldGeneration.OriginPosition, WorldBuilder.Settings.CellSize_inGameUnits, Color.black, labelStyle);
				}
				// [[ DRAW COORDINATE MAP ]]
				else if (_worldEditScript.worldView == WorldEditor.WorldView.COORDINATE_MAP)
				{

					DrawCoordinateMap(worldGeneration.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) =>
					{
						try
						{
							RegionBuilder selectedRegion = _worldEditScript.worldBuilder.RegionMap[coordinate.ValueKey];
							_worldEditScript.SelectRegion(selectedRegion);
						}
						catch (System.Exception e)
						{
							Debug.LogError("An error occurred while selecting the region: " + e.Message);
						}
					});
				}
			}

			void DrawRegionEditorGUI()
			{
				if (_worldEditScript.selectedRegion == null) return;


				RegionBuilder selectedRegion = _worldEditScript.selectedRegion;
				DrawRegion(selectedRegion, _worldEditScript.regionView);


				foreach (RegionBuilder region in worldGeneration.AllRegions)
				{
					if (region != selectedRegion) { DrawRegion(region, WorldEditor.RegionView.OUTLINE); }
				}

			}

			void DrawChunkEditorGUI()
			{
				if (_worldEditScript.selectedChunk == null) return;

				Chunk selectedChunk = _worldEditScript.selectedChunk;
				DrawChunk(selectedChunk, _worldEditScript.chunkView);

				foreach (Chunk chunk in selectedChunk.GenerationParent.AllChunks)
				{
					if (chunk != selectedChunk) { DrawChunk(chunk, WorldEditor.ChunkView.OUTLINE); }
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
		void DrawRegion(RegionBuilder region, WorldEditor.RegionView type)
		{
			if (region == null || region.CoordinateMap == null) { return; }

			CoordinateMap coordinateMap = region.CoordinateMap;
			ChunkBuilder chunkMap = region.ChunkBuilder;
			GUIStyle regionLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			// [[ DRAW GRID ONLY ]]
			if (type == WorldEditor.RegionView.OUTLINE)
			{
				Darklight.Gizmos.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
				Darklight.Gizmos.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldBuilder.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
				{
					_worldEditScript.SelectRegion(region);
				}, Handles.RectangleHandleCap);
			}
			// [[ DRAW COORDINATE MAP ]]
			else if (type == WorldEditor.RegionView.COORDINATE_MAP)
			{
				DrawCoordinateMap(coordinateMap, _worldEditScript.coordinateMapView, (coordinate) =>
				{
					Chunk selectedChunk = region.ChunkBuilder.GetChunkAt(coordinate);
					_worldEditScript.SelectChunk(selectedChunk);
				});
			}
			// [[ DRAW CHUNK MAP ]]
			else if (type == WorldEditor.RegionView.CHUNK_MAP)
			{
				DrawChunkMap(chunkMap, _worldEditScript.chunkMapView);
			}
		}

		void DrawChunk(Chunk chunk, WorldEditor.ChunkView type)
		{
			GUIStyle chunkLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case WorldEditor.ChunkView.OUTLINE:

					// Draw Selection Rectangle
					Darklight.Gizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);

					break;
				case WorldEditor.ChunkView.TYPE:
					chunkLabelStyle.normal.textColor = chunk.TypeColor;
					Darklight.Gizmos.DrawLabel($"{chunk.Type.ToString()[0]}", chunk.CenterPosition, chunkLabelStyle);

					Darklight.Gizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.ChunkView.HEIGHT:
					Darklight.Gizmos.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

					Darklight.Gizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.ChunkView.COORDINATE_MAP:

					//DrawCoordinateMap(chunk.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) => {});

					break;
				case WorldEditor.ChunkView.CELL_MAP:

					DrawCellMap(chunk.CellMap, _worldEditScript.cellMapView);

					break;
			}
		}

		void DrawCell(Cell cell, WorldEditor.CellView type)
		{
			GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case WorldEditor.CellView.OUTLINE:
					// Draw Selection Rectangle
					Darklight.Gizmos.DrawButtonHandle(cell.Position, cell.Normal, cell.Size * 0.475f, Color.black, () =>
					{
						_worldEditScript.SelectCell(cell);
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.CellView.TYPE:
					// Draw Face Type Label
					Darklight.Gizmos.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					Darklight.Gizmos.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
					break;
				case WorldEditor.CellView.FACE:
					// Draw Face Type Label
					Darklight.Gizmos.DrawLabel($"{cell.FaceType}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					break;
			}
		}

		void DrawCoordinateMap(CoordinateMap coordinateMap, WorldEditor.CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
		{
			GUIStyle coordLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
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
							Darklight.Gizmos.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = Color.white;
							break;
						case WorldEditor.CoordinateMapView.COORDINATE_TYPE:
							coordLabelStyle.normal.textColor = coordinate.TypeColor;
							Darklight.Gizmos.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = coordinate.TypeColor;
							break;
						case WorldEditor.CoordinateMapView.ZONE_ID:
							coordLabelStyle.normal.textColor = coordinate.TypeColor;

							if (coordinate.Type == Coordinate.TYPE.ZONE)
							{
								Zone zone = coordinateMap.GetZoneFromCoordinate(coordinate);
								if (zone != null)
								{
									Darklight.Gizmos.DrawLabel($"{zone.ID}", coordinate.ScenePosition, coordLabelStyle);
								}
							}

							break;
					}

					// Draw Selection Rectangle
					Darklight.Gizmos.DrawButtonHandle(coordinate.ScenePosition, Vector3.up, coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
					{
						onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
					}, Handles.RectangleHandleCap);
				}
			}
		}

		void DrawChunkMap(ChunkBuilder chunkMap, WorldEditor.ChunkMapView mapView)
		{
			GUIStyle chunkLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
			Color chunkColor = Color.black;

			// Draw Chunks
			if (chunkMap != null && chunkMap.Initialized)
			{
				foreach (Chunk chunk in chunkMap.AllChunks)
				{
					// Draw Custom View
					switch (mapView)
					{
						case WorldEditor.ChunkMapView.TYPE:
							DrawChunk(chunk, WorldEditor.ChunkView.TYPE);
							break;
						case WorldEditor.ChunkMapView.HEIGHT:
							DrawChunk(chunk, WorldEditor.ChunkView.HEIGHT);
							break;
					}
				}
			}
		}

		void DrawCellMap(CellMap cellMap, WorldEditor.CellMapView mapView)
		{
			if (cellMap == null) return;

			GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
			foreach (Cell cell in cellMap.AllCells)
			{
				// Draw Custom View
				switch (mapView)
				{
					case WorldEditor.CellMapView.TYPE:
						DrawCell(cell, WorldEditor.CellView.TYPE);
						break;
					case WorldEditor.CellMapView.FACE:
						DrawCell(cell, WorldEditor.CellView.FACE);
						break;
				}
			}
		}

	}
#endif
}
