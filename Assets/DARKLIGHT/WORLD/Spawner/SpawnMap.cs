using UnityEngine;
namespace Darklight.World.Generation
{
    using Darklight.Bot;
    using Builder;
    using Darklight.World.Map;


    public class SpawnMap : TaskQueen, ITaskEntity
    {
        public RegionBuilder regionBuilder;

        public async void Start()
        {
            await base.Initialize();
            await InitializationSequence();
        }

        public override async Awaitable InitializationSequence()
        {
            await base.InitializationSequence();
            while (regionBuilder.Initialized == false)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
            }

            TaskBotConsole.Log(this, "Detected Initialized Region");
        }

        public void OnDrawGizmos()
        {
            if (regionBuilder == null) return;

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
}