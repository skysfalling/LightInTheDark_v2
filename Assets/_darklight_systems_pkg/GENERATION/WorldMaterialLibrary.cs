using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    public class WorldMaterialLibrary : MonoBehaviour
    {
        public static WorldMaterialLibrary Instance;
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        [Header("World Cell Default")]
        public Material cell_empty;
        public Material cell_edge;
        public Material cell_corner;
        public Material cell_obstacle;
        public Material cell_spawnpoint;

        [Header("World Cell Cursor")]
        public Material cursor_selected;
        public Material cursor_hoverOver;

        [Space(10), Header("Materials")]
        public Material chunkMaterial; // Assign a material in the inspector
        public Material borderChunkMaterial; // Assign a material in the inspector

        public Material GetMaterialOfCellType(WorldCell.TYPE type)
        {
            switch (type)
            {
                case WorldCell.TYPE.EMPTY: return cell_empty;
                case WorldCell.TYPE.EDGE: return cell_edge;
                case WorldCell.TYPE.CORNER: return cell_corner;
                case WorldCell.TYPE.OBSTACLE: return cell_obstacle;
                case WorldCell.TYPE.SPAWN_POINT: return cell_spawnpoint;
                default: return null;
            }
        }
    }
}

