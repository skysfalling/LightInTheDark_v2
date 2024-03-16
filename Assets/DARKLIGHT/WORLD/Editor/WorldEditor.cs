using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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
		public enum WorldView { COORDINATE_MAP, FULL_COORDINATE_MAP, };
		public enum RegionView { OUTLINE, COORDINATE_MAP, CHUNK_MAP, CELL_MAP }
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
	[CustomEditor(typeof(WorldEditor), true)]
	public class WorldEditorGUI : UnityEditor.Editor
	{
		private SerializedObject _serializedObject;
		private WorldEditor _worldEditScript;

		public virtual void OnEnable()
		{
			// Cache the SerializedObject
			_serializedObject = new SerializedObject(target);
			_worldEditScript = (WorldEditor)target;
		}

		#region [[ INSPECTOR GUI ]] ================================================================= 

		public static void CoordinateMapInspectorGUI(CoordinateMap coordinateMap, WorldEditor.CoordinateMapView mapView)
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
			CustomInspectorGUI.DrawLabeledEnumPopup(ref mapView, "Coordinate Map View");
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

		public static void ChunkMapInspectorGUI(ChunkBuilder chunkMap)
		{
			EditorGUILayout.LabelField("Chunk Map", Darklight.CustomInspectorGUI.Header2Style);
			EditorGUILayout.Space(10);
			if (chunkMap == null)
			{
				EditorGUILayout.LabelField("No Chunk Map is Selected. Try Initializing the World Generation");
				return;
			}

			// >> select debug view
			//Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.chunkMapView, "Chunk Map View");
		}

		public static void CellMapInspectorGUI(CellMap cellMap)
		{
			EditorGUILayout.LabelField("Cell Map", Darklight.CustomInspectorGUI.Header2Style);
			EditorGUILayout.Space(10);
			if (cellMap == null)
			{
				EditorGUILayout.LabelField("No Cell Map is Selected. Try starting the World Generation");
				return;
			}

			// >> select debug view
			//Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.cellMapView, "Cell Map View");
		}

		#endregion


		#region [[ SCENE GUI ]]

		void DrawWorldEditorGUI(WorldBuilder worldBuilder)
		{
			// [[ DRAW DEFAULT SIZE GUIDE ]]
			if (worldBuilder == null || worldBuilder.CoordinateMap == null)
			{
				GUIStyle labelStyle = Darklight.CustomInspectorGUI.BoldStyle;
				Darklight.CustomGizmos.DrawWireSquare_withLabel("Origin Region", worldBuilder.OriginPosition, WorldBuilder.Settings.RegionFullWidth_inGameUnits, Color.red, labelStyle);
				Darklight.CustomGizmos.DrawWireSquare_withLabel("World Chunk Size", worldBuilder.OriginPosition, WorldBuilder.Settings.ChunkWidth_inGameUnits, Color.black, labelStyle);
				Darklight.CustomGizmos.DrawWireSquare_withLabel("World Cell Size", worldBuilder.OriginPosition, WorldBuilder.Settings.CellSize_inGameUnits, Color.black, labelStyle);
			}
			// [[ DRAW COORDINATE MAP ]]
			else if (_worldEditScript.worldView == WorldEditor.WorldView.COORDINATE_MAP)
			{

				DrawCoordinateMap(worldBuilder.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) =>
				{
					try
					{
						RegionBuilder selectedRegion = worldBuilder.RegionMap[coordinate.ValueKey];
						_worldEditScript.SelectRegion(selectedRegion);
					}
					catch (System.Exception e)
					{
						Debug.LogError("An error occurred while selecting the region: " + e.Message);
					}
				});
			}
		}

		#endregion

		// ==== DRAW WORLD UNITS ====================================================================================================
		public static void DrawRegion(RegionBuilder region, WorldEditor.RegionView type)
		{
			if (region == null || region.CoordinateMap == null) { return; }

			CoordinateMap coordinateMap = region.CoordinateMap;
			ChunkBuilder chunkBuilder = region.ChunkBuilder;
			GUIStyle regionLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			// [[ DRAW GRID ONLY ]]
			if (type == WorldEditor.RegionView.OUTLINE)
			{
				CustomGizmos.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
				CustomGizmos.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldBuilder.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
				{
					//_worldEditScript.SelectRegion(region);
				}, Handles.RectangleHandleCap);
			}
			// [[ DRAW COORDINATE MAP ]]
			else if (type == WorldEditor.RegionView.COORDINATE_MAP)
			{
				DrawCoordinateMap(coordinateMap, WorldEditor.CoordinateMapView.COORDINATE_VALUE, (coordinate) =>
				{

				});
			}
			// [[ DRAW CHUNK MAP ]]
			else if (type == WorldEditor.RegionView.CHUNK_MAP)
			{
				DrawChunkMap(chunkBuilder, WorldEditor.ChunkMapView.HEIGHT);
			}
			// [[ DRAW CHUNK MAP ]]
			else if (type == WorldEditor.RegionView.CELL_MAP)
			{
				foreach (Chunk chunk in chunkBuilder.AllChunks)
				{
					DrawCellMap(chunk.CellMap, WorldEditor.CellMapView.TYPE);
				}
			}
		}

		public static void DrawChunk(Chunk chunk, WorldEditor.ChunkView type)
		{
			GUIStyle chunkLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case WorldEditor.ChunkView.OUTLINE:

					// Draw Selection Rectangle
					Darklight.CustomGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
					{
						//_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);

					break;
				case WorldEditor.ChunkView.TYPE:
					chunkLabelStyle.normal.textColor = chunk.TypeColor;
					Darklight.CustomGizmos.DrawLabel($"{chunk.Type.ToString()[0]}", chunk.CenterPosition, chunkLabelStyle);

					Darklight.CustomGizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
					{
						//_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.ChunkView.HEIGHT:
					Darklight.CustomGizmos.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

					Darklight.CustomGizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
					{
						//_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.ChunkView.COORDINATE_MAP:

					//DrawCoordinateMap(chunk.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) => {});

					break;
				case WorldEditor.ChunkView.CELL_MAP:

					DrawCellMap(chunk.CellMap, WorldEditor.CellMapView.TYPE);
					break;
			}
		}

		public static void DrawCell(Cell cell, WorldEditor.CellView type)
		{
			GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case WorldEditor.CellView.OUTLINE:
					// Draw Selection Rectangle
					Darklight.CustomGizmos.DrawButtonHandle(cell.Position, cell.Normal, cell.Size * 0.475f, Color.black, () =>
					{
					}, Handles.RectangleHandleCap);
					break;
				case WorldEditor.CellView.TYPE:
					// Draw Face Type Label
					Darklight.CustomGizmos.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					Darklight.CustomGizmos.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
					break;
				case WorldEditor.CellView.FACE:
					// Draw Face Type Label
					Darklight.CustomGizmos.DrawLabel($"{cell.FaceType}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					break;
			}
		}

		public static void DrawCoordinateMap(CoordinateMap coordinateMap, WorldEditor.CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
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

		public static void DrawChunkMap(ChunkBuilder chunkMap, WorldEditor.ChunkMapView mapView)
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

		public static void DrawCellMap(CellMap cellMap, WorldEditor.CellMapView mapView)
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
