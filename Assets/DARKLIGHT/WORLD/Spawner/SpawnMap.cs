using UnityEngine;
namespace Darklight.World.Generation
{
    using Darklight.Bot;
    using Builder;
    using Darklight.World.Map;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public class SpawnMap : TaskBotQueen, ITaskEntity
    {
        Builder.RegionBuilder _regionBuilder;
        public GameObject playerTravelerObject;

        public async void Start()
        {

            await Awaitable.WaitForSecondsAsync(3f);

            await base.Initialize();
            //await ExecutionSequence();
        }

        /*
            public override async Awaitable ExecutionSequence()
            {
                await base.ExecutionSequence();
                TaskBotConsole.Log(this, "SpawnMap Initialization Started");
                List<Chunk> allChunks = _regionBuilder.ChunkBuilder.AllChunks.ToList();
                TaskBotConsole.Log(this, "Detected Region Generation");
                TaskBotConsole.Log(this, $"Found {_regionBuilder.ChunkBuilder.AllChunks.Count} Chunks");

                /* 
                TODO : Scan the region for chunks of a certain type & match with library of prefabs

                */


        /*
        // Track all of the cells
        foreach (Chunk chunk in spawnChunks)
        {
            cellMaps[chunk] = chunk.CellMap;

            HashSet<Cell> topFaceCells = chunk.CellMap.ChunkFaceMap[Chunk.FaceType.Top];
            foreach (Cell cell in topFaceCells)
            {
                if (cell.Coordinate.Type == Coordinate.TYPE.NULL)
                {
                    allSpawnCells.TryAdd(chunk, new HashSet<Cell>());
                    allSpawnCells[chunk].Add(cell);
                }
            }
            TaskBotConsole.Log(this, $"Found {allSpawnCells[chunk].Count} Spawn Cells in Chunk {chunk.Coordinate.ValueKey}");
        }

        await Awaitable.WaitForSecondsAsync(1f);
        Debug.Log($"Spawn Chunk Count {spawnChunks.Count}");

        Chunk spawnChunk = spawnChunks[UnityEngine.Random.Range(0, spawnChunks.Count - 1)];
        Cell spawnCell = spawnChunk.CellMap.AllCells.ElementAt(UnityEngine.Random.Range(0, spawnChunk.CellMap.AllCells.Count - 1));

        if (spawnCell == null)
        {
            TaskBotConsole.Log(this, "Spawn Cell not found");
            return;
        }
        else
        {
            TaskBotConsole.Log(this, "Spawn Cell found");
            spawnCell.SetCellType(Cell.TYPE.SPAWN_POINT);
        }

        // Spawn Player
        GameObject player = Instantiate(playerTravelerObject, spawnCell.Position, Quaternion.identity);
        player.GetComponent<Traveler>().InitializeAtCell(spawnCell);

    }
*/


        /*
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
                */
    }
}