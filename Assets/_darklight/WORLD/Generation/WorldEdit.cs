using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace Darklight.World.Generation
{
#if UNITY_EDITOR
    using UnityEditor;
    using DarklightEditor = Darklight.Unity.CustomInspectorGUI;
    using EditMode = WorldEdit.EditMode;
    using WorldView = WorldEdit.WorldView;
    using RegionView = WorldEdit.RegionView;
    using ChunkView = WorldEdit.ChunkView;
    using CellView = WorldEdit.CellView;
    using CoordinateMapView = WorldEdit.CoordinateMapView;
    using ChunkMapView = WorldEdit.ChunkMapView;
    using CellMapView = WorldEdit.CellMapView;
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

        public WorldBuilder worldBuilder => GetComponent<WorldBuilder>();
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


}


