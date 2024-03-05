using System;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Data
{
    [System.Serializable]
    public class WorldData
    {
        public GenerationSettings Settings;

    }

    [System.Serializable]
    public class CoordinateData
    {
        public int typeID; // Coordinate Type
        public int X;
        public int Y;

        public CoordinateData(Coordinate coord)
        {
            typeID = (int)coord.type;
            X = coord.Value.x;
            Y = coord.Value.y;
        }
    }

    [System.Serializable]
    public class CoordinateMapData
    {
        public CoordinateData[][] CoordinateMap;

        public CoordinateMapData(CoordinateMap map)
        {

        }
    }




    [System.Serializable]
    public class WorldPathData
    {
        public Vector2Int[] path;

        public WorldPathData(Path path)
        {
            this.path = path.positions.ToArray();
        }
    }
}

