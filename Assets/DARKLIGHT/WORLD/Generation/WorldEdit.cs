namespace Darklight.World.Generation
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Darklight.World.Builder;

	public class WorldEdit : MonoBehaviour
	{
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

		public WorldBuilder worldBuilder => GetComponent<WorldBuilder>();
		public RegionBuilder selectedRegion;
		public Chunk selectedChunk;
		public Cell selectedCell;

		public void SelectRegion(RegionBuilder region)
		{
			selectedRegion = region;

			//Debug.Log("Selected Region: " + selectedRegion.Coordinate.Value);

			Darklight.CustomInspectorGUI.FocusSceneView(region.Coordinate.ScenePosition);

			editMode = EditMode.REGION;
		}

		public void SelectChunk(Chunk chunk)
		{
			selectedChunk = chunk;

			//Debug.Log("Selected Chunk: " + chunk.Coordinate.Value);

			Darklight.CustomInspectorGUI.FocusSceneView(chunk.Coordinate.ScenePosition);

			//editMode = EditMode.CHUNK;
		}

		public void SelectCell(Cell cell)
		{
			selectedCell = cell;

			//Debug.Log("Selected Cell: " + cell.Coordinate.Value);

			Darklight.CustomInspectorGUI.FocusSceneView(cell.Position);

			//editMode = EditMode.CELL;
		}
	}


}


