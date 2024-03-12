namespace Darklight.World.Editor
{
	using UnityEditor;
	using Generation;
	using Builder;
	using Map;
	/*
	using WorldE
	
	[CustomEditor(typeof(WorldEdit))]
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

			WorldBuilder worldBuilder = _worldBuilderScript;


			// [[ EDITOR VIEW ]]
			Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.editMode, "Edit Mode");

			EditorGUILayout.Space();

			switch (_worldEditScript.editMode)
			{
				case WorldEdit.EditMode.WORLD:

					EditorGUILayout.LabelField("World Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.worldView, "World View");

					// SHOW WORLD STATS


					CoordinateMapInspector(_worldBuilderScript.CoordinateMap);
					break;
				case WorldEdit.EditMode.REGION:
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
					ChunkMapInspector(_worldEditScript.selectedRegion.ChunkGeneration);
					break;
				case WorldEdit.EditMode.CHUNK:
					if (_worldEditScript.selectedChunk == null && worldBuilder.Initialized)
					{
						RegionBuilder originRegion = worldBuilder.RegionMap[Vector2Int.zero];
						_worldEditScript.SelectChunk(originRegion.ChunkGeneration.GetChunkAt(Vector2Int.zero));
						break;
					}

					EditorGUILayout.LabelField("Chunk Edit Mode", Darklight.CustomInspectorGUI.Header1Style);
					EditorGUILayout.Space(20);

					// SHOW CHUNK STATS



					Darklight.CustomInspectorGUI.DrawLabeledEnumPopup(ref _worldEditScript.chunkView, "Chunk View");
					//CoordinateMapInspector( _worldEditScript.selectedChunk);
					CellMapInspector(_worldEditScript.selectedChunk.CellMap);
					break;
				case WorldEdit.EditMode.CELL:
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

			void ChunkMapInspector(ChunkGeneration chunkMap)
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
				case WorldEdit.EditMode.WORLD:
					DrawWorldEditorGUI();
					break;
				case WorldEdit.EditMode.REGION:
					DrawRegionEditorGUI();
					break;
				case WorldEdit.EditMode.CHUNK:
					DrawChunkEditorGUI();
					break;
				case WorldEdit.EditMode.CELL:
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
				else if (_worldEditScript.worldView == WorldEdit.WorldView.COORDINATE_MAP)
				{

					DrawCoordinateMap(worldGeneration.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) =>
					{

						if (_worldEditScript.selectedCell == null) return;

						Cell selectedCell = _worldEditScript.selectedCell;

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
					if (region != selectedRegion) { DrawRegion(region, RegionView.OUTLINE); }
				}

			}

			void DrawChunkEditorGUI()
			{
				if (_worldEditScript.selectedChunk == null) return;

				Chunk selectedChunk = _worldEditScript.selectedChunk;
				DrawChunk(selectedChunk, _worldEditScript.chunkView);

				foreach (Chunk chunk in selectedChunk.GenerationParent.AllChunks)
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
		void DrawRegion(RegionBuilder region, RegionView type)
		{
			if (region == null || region.CoordinateMap == null) { return; }

			CoordinateMap coordinateMap = region.CoordinateMap;
			ChunkGeneration chunkMap = region.ChunkGeneration;
			GUIStyle regionLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			// [[ DRAW GRID ONLY ]]
			if (type == RegionView.OUTLINE)
			{
				Darklight.Gizmos.DrawLabel($"{region.Coordinate.ValueKey}", region.CenterPosition, regionLabelStyle);
				Darklight.Gizmos.DrawButtonHandle(region.CenterPosition, Vector3.up, WorldBuilder.Settings.RegionWidth_inGameUnits * 0.475f, Color.black, () =>
				{
					_worldEditScript.SelectRegion(region);
				}, Handles.RectangleHandleCap);
			}
			// [[ DRAW COORDINATE MAP ]]
			else if (type == RegionView.COORDINATE_MAP)
			{
				DrawCoordinateMap(coordinateMap, _worldEditScript.coordinateMapView, (coordinate) =>
				{
					Chunk selectedChunk = region.ChunkGeneration.GetChunkAt(coordinate);
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
			GUIStyle chunkLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case ChunkView.OUTLINE:

					// Draw Selection Rectangle
					Darklight.Gizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.black, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);

					break;
				case ChunkView.TYPE:
					chunkLabelStyle.normal.textColor = chunk.TypeColor;
					Darklight.Gizmos.DrawLabel($"{chunk.Type.ToString()[0]}", chunk.CenterPosition, chunkLabelStyle);

					Darklight.Gizmos.DrawButtonHandle(chunk.CenterPosition, Vector3.up, chunk.Width * 0.475f, chunk.TypeColor, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case ChunkView.HEIGHT:
					Darklight.Gizmos.DrawLabel($"{chunk.GroundHeight}", chunk.GroundPosition, chunkLabelStyle);

					Darklight.Gizmos.DrawButtonHandle(chunk.GroundPosition, Vector3.up, chunk.Width * 0.475f, Color.grey, () =>
					{
						_worldEditScript.SelectChunk(chunk);
					}, Handles.RectangleHandleCap);
					break;
				case ChunkView.COORDINATE_MAP:

					//DrawCoordinateMap(chunk.CoordinateMap, _worldEditScript.coordinateMapView, (coordinate) => {});

					break;
				case ChunkView.CELL_MAP:

					DrawCellMap(chunk.CellMap, _worldEditScript.cellMapView);

					break;
			}
		}

		void DrawCell(Cell cell, CellView type)
		{
			GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

			switch (type)
			{
				case CellView.OUTLINE:
					// Draw Selection Rectangle
					Darklight.Gizmos.DrawButtonHandle(cell.Position, cell.Normal, cell.Size * 0.475f, Color.black, () =>
					{
						_worldEditScript.SelectCell(cell);
					}, Handles.RectangleHandleCap);
					break;
				case CellView.TYPE:
					// Draw Face Type Label
					Darklight.Gizmos.DrawLabel($"{cell.Type.ToString()[0]}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					Darklight.Gizmos.DrawFilledSquareAt(cell.Position, cell.Size * 0.75f, cell.Normal, cell.TypeColor);
					break;
				case CellView.FACE:
					// Draw Face Type Label
					Darklight.Gizmos.DrawLabel($"{cell.FaceType}", cell.Position + (cell.Normal * cell.Size), cellLabelStyle);
					break;
			}
		}

		void DrawCoordinateMap(CoordinateMap coordinateMap, CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
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
						case CoordinateMapView.GRID_ONLY:
							break;
						case CoordinateMapView.COORDINATE_VALUE:
							Darklight.Gizmos.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = Color.white;
							break;
						case CoordinateMapView.COORDINATE_TYPE:
							coordLabelStyle.normal.textColor = coordinate.TypeColor;
							Darklight.Gizmos.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = coordinate.TypeColor;
							break;
						case CoordinateMapView.ZONE_ID:
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

		void DrawChunkMap(ChunkGeneration chunkMap, ChunkMapView mapView)
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

			GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
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
	*/
}