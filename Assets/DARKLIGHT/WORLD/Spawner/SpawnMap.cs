using UnityEngine;
namespace Darklight.World.Generation
{
    using Darklight.Bot;
    using Builder;
    using Darklight.World.Map;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    public class SpawnMap : TaskQueen, ITaskEntity
    {
        RegionBuilder _regionBuilder;
        RegionBuilder RegionBuilder => GetComponent<RegionBuilder>();
        CoordinateMap CoordinateMap => RegionBuilder.CoordinateMap;
        Dictionary<Chunk, CellMap> cellMaps = new();
        Dictionary<Chunk, HashSet<Cell>> spawnCells = new();

        public Traveler playerTraveler;

        public async void Start()
        {
            _regionBuilder = RegionBuilder;
            await base.Initialize();
            await InitializationSequence();
        }

        public override async Awaitable InitializationSequence()
        {
            await base.InitializationSequence();
            while (RegionBuilder.GenerationFinished == false)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
            }

            HashSet<Chunk> allChunks = _regionBuilder.ChunkBuilder.AllChunks;
            TaskBotConsole.Log(this, "Detected Region Generation");
            TaskBotConsole.Log(this, $"Found {RegionBuilder.ChunkBuilder.AllChunks.Count} Chunks");

            TaskBot MapSpawnFacesBot = new TaskBot(this, "MapSpawnFacesBot", async () =>
            {
                foreach (Chunk chunk in allChunks)
                {
                    cellMaps[chunk] = chunk.CellMap;
                    spawnCells[chunk] = chunk.CellMap.ChunkFaceMap[Chunk.FaceType.Top];
                    TaskBotConsole.Log(this, $"Found {spawnCells[chunk].Count} Spawn Cells in Chunk {chunk.Coordinate.ValueKey}");
                }
                await Task.CompletedTask;
            }, true);
            await ExecuteBot(MapSpawnFacesBot);
        }

        void DrawCell(Cell cell, WorldEditor.CellView type)
        {
            GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;

            switch (type)
            {
                case WorldEditor.CellView.OUTLINE:
                    // Draw Selection Rectangle
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

        void DrawCells(HashSet<Cell> cells, WorldEditor.CellMapView mapView)
        {
            if (cells == null) return;

            GUIStyle cellLabelStyle = Darklight.CustomInspectorGUI.CenteredStyle;
            foreach (Cell cell in cells)
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
}