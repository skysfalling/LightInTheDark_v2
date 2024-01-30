using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaterialLibrary : MonoBehaviour
{
    public static WorldMaterialLibrary Instance;
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    [Header("World Cell Debug")]
    public Material cell_empty;
    public Material cell_edge;
    public Material cell_corner;
    public Material cell_obstacle;
    public Material cell_spawnpoint;

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
