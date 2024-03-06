using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World = Darklight;
namespace Darklight.ThirdDimensional.World
{
    using WorldGen = WorldGeneration;

    public class Chunk
    {
        /// <summary>
        /// Defines World Chunks based on wall count / location
        /// </summary>
        public enum TYPE
        {
            /// <summary>No walls present.</summary>
            EMPTY,
            /// <summary>One sidewall present.</summary>
            WALL,
            /// <summary>Two parallel walls present, forming a hallway.</summary>
            HALLWAY,
            /// <summary>Two perpendicular walls present, forming a corner.</summary>
            CORNER,
            /// <summary>Three walls present, forming a dead end.</summary>
            DEADEND,
            /// <summary>Enclosed by walls on all four sides.</summary>
            CLOSED,
            /// <summary>Indicates a boundary limit, set by WorldCoordinateMap.</summary>
            BORDER,
            /// <summary>Indicates an exit point, set by WorldExit.</summary>
            EXIT
        }
        public enum FaceType { Front, Back, Left, Right, Top, Bottom }

        // [[ PRIVATE VARIABLES ]]
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkMesh _chunkMesh;
        CellMap _cellMap;
        TYPE _type;
        int _groundHeight = 2;

        // [[ PUBLIC ACCESS VARIABLES ]] 
        public int Width => WorldGeneration.Settings.ChunkWidth_inGameUnits;
        public ChunkMap ChunkMapParent { get; private set; }
        public Coordinate Coordinate => _coordinate;
        public CoordinateMap CoordinateMap => _coordinateMap;
        public GameObject ChunkObject { get; private set; }
        public ChunkMesh ChunkMesh => _chunkMesh;
        public CellMap CellMap => _cellMap;
        public int GroundHeight => _groundHeight;
        public TYPE Type => _type;
        public Color TypeColor { get; private set; } = Color.white;
        public Vector3 CenterPosition => Coordinate.ScenePosition;
        public Vector3 OriginPosition
        {
            get
            {
                Vector3 origin = CenterPosition;
                origin -= WorldGen.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                origin += WorldGen.Settings.CellSize_inGameUnits * new Vector3(0.5f, 0, 0.5f);
                return origin;
            }
        }
        public Vector3 GroundPosition
        {
            get
            {
                Vector3 groundPosition = CenterPosition;
                groundPosition += (GroundHeight * WorldGen.Settings.CellSize_inGameUnits) * Vector3Int.up;
                return groundPosition;
            }
        }
        public Vector3 ChunkMeshDimensions => WorldGen.Settings.ChunkVec3Dimensions_inCellUnits + new Vector3Int(0, GroundHeight, 0);

        public Chunk(ChunkMap chunkMap, Coordinate coordinate)
        {
            this.ChunkMapParent = chunkMap;
            this._coordinate = coordinate;

            // >> set perlin noise height
            Vector2Int perlinOffset = new Vector2Int((int)coordinate.ScenePosition.x, (int)coordinate.ScenePosition.z);
            this._groundHeight = PerlinNoise.CalculateHeightFromNoise(perlinOffset);

            // Create coordinate map
            this._coordinateMap = new CoordinateMap(this);
        }

        public void CreateChunkMesh()
        {
            // Create chunkMesh
            _chunkMesh = new ChunkMesh(this, GroundHeight, GroundPosition);

            // Create cellMap
            _cellMap = new CellMap(this, _chunkMesh);


            DetermineChunkType();
        }

        public void CreateChunkMeshObject(Region region)
        {
            if (this.ChunkObject != null)
            {
                WorldGen.DestroyGameObject(ChunkObject);
            }

            this.ChunkObject = WorldGen.CreateMeshObject($"Chunk {Coordinate.Value} " +
                $":: height {GroundHeight}", ChunkMesh.Mesh, region.GenerationParent.materialLibrary.DefaultGroundMaterial);
            this.ChunkObject.transform.parent = region.transform;
        }

        public void SetGroundHeight(int height)
        {
            this._groundHeight = height;
        }

        void DetermineChunkType()
        {
            // [[ ITERATE THROUGH CHUNK NEIGHBORS ]] 
            Dictionary<WorldDirection, Chunk> naturalNeighborMap = GetNaturalNeighborMap();
            foreach(WorldDirection direction in naturalNeighborMap.Keys.ToList())
            {
                Chunk neighborChunk = naturalNeighborMap[direction];
                if (neighborChunk != null && neighborChunk.GroundHeight != this.GroundHeight)
                {
                    BorderDirection? neighborBorder = CoordinateMap.GetBorderDirection(direction); // get chunk border
                    if (neighborBorder == null) continue;

                    CoordinateMap.CloseMapBorder((BorderDirection)neighborBorder); // close the chunk border
                }
            }

            // ========================================================

            // [[ DETERMINE TYPE FROM BORDERS ]]
            Dictionary<BorderDirection, bool> activeBorderMap = CoordinateMap.ActiveBorderMap;

            // Count active borders directly from the dictionary
            int activeBorderCount = activeBorderMap.Count(kv => kv.Value == true);

            // Determine type based on active edge count and their positions
            switch (activeBorderCount)
            {
                case 4:
                    SetType(TYPE.CLOSED); break;
                case 3:
                    SetType(TYPE.DEADEND); break;
                case 2:
                    // Check for parallel edges
                    if (activeBorderMap[BorderDirection.NORTH] && activeBorderMap[BorderDirection.SOUTH]) 
                        { SetType(TYPE.HALLWAY); break; }
                    if (activeBorderMap[BorderDirection.EAST] && activeBorderMap[BorderDirection.WEST]) 
                        { SetType(TYPE.HALLWAY); break; }

                    // Otherwise chunk is in corner
                    SetType(TYPE.CORNER); break;
                case 1:
                    SetType(TYPE.WALL); break;
                case 0:
                    SetType(TYPE.EMPTY); break;
            }
        }

        public void SetType(TYPE newType)
        {
            _type = newType;
            switch (newType)
            {
                case TYPE.CLOSED: TypeColor = Color.black; break;
                case TYPE.DEADEND: TypeColor = Color.red; break;
                case TYPE.HALLWAY: TypeColor = Color.yellow; break;
                case TYPE.CORNER: TypeColor = Color.blue; break;
                case TYPE.WALL: TypeColor = Color.green; break;
                case TYPE.EMPTY: TypeColor = Color.white; break;
            }
        }

        public Dictionary<WorldDirection, Chunk> GetNaturalNeighborMap()
        {
            Dictionary<WorldDirection, Chunk> neighborMap = new();

            List<WorldDirection> naturalNeighborDirections = new List<WorldDirection> { WorldDirection.NORTH, WorldDirection.SOUTH, WorldDirection.EAST, WorldDirection.WEST };
            foreach (WorldDirection direction in naturalNeighborDirections)
            {
                Vector2Int neighborCoordinateValue = CoordinateMap.CalculateNeighborCoordinateValue(Coordinate.Value, direction);
                neighborMap[direction] = ChunkMapParent.GetChunkAt(neighborCoordinateValue);
            }

            return neighborMap;
        }

        // ================== SPAWN OBJECTS ============= >>


        public List<Cell> FindSpace(EnvironmentObject envObj)
        {
            Dictionary<int, List<Cell>> availableSpace = new Dictionary<int, List<Cell>>();
            int spaceIndex = 0;

            /*
            foreach (Cell startCell in localCells)
            {
                if (IsSpaceAvailable(startCell, envObj))
                {
                    availableSpace[spaceIndex] = GetCellsInArea(startCell, envObj.space);
                    spaceIndex++;
                }
            }
            */

            // Get Random Available Space
            if (availableSpace.Count > 0)
            {
                return availableSpace[UnityEngine.Random.Range(0, availableSpace.Keys.Count)];
            }

            // Return Empty
            return new List<Cell>();
        }

        public bool IsSpaceAvailable(Cell startCell, EnvironmentObject envObj)
        {
            List<Cell> cellsInArea = GetCellsInArea(startCell, envObj.space);
            List<Cell.TYPE> requiredTypes = envObj.spawnCellTypeRequirements;

            // Check Cell Count ( Also Area Size )
            int spawnAreaSize = envObj.space.x * envObj.space.y;
            if (cellsInArea.Count != spawnAreaSize) { return false; }

            // Check Validity of Cell Types
            foreach (Cell cell in cellsInArea)
            {
                if (!requiredTypes.Contains(cell.Type)) { return false; }
            }

            string cellAreaList = "";
            foreach (Cell cell in cellsInArea)
            {
                cellAreaList += $"{cell.Position} {cell.Type}\n";
            }

            /*
            Debug.Log($"{prefix} SpaceAvailableAt {startCell.position}\n" +
                $"\tEnvObj Prefab : {envObj.prefab.name}\n" +
                $"\tSpace Needed : {envObj.space} {spawnAreaSize}\n" +
                $"\tFound Space Count : {cellsInArea.Count}\n" +
                $"\tRequired Type : {requiredTypes[0]}\n" +
                $"\tCell Area : {cellAreaList}");
            */

            return true;
        }

        // NOTE :: This is specifically starting with the top left cell.
        private List<Cell> GetCellsInArea(Cell startCell, Vector2Int space)
        {
            List<Cell> areaCells = new List<Cell>();
            int cellSize = WorldGen.Settings.CellSize_inGameUnits;

            for (int x = 0; x < space.x; x++)
            {
                for (int z = 0; z < space.y; z++)
                {
                    Cell cell = GetCellAt(startCell.Position.x + (x * cellSize), startCell.Position.z + (z * cellSize));
                    if (cell != null && !areaCells.Contains(cell))
                    {
                        areaCells.Add(cell);
                    }
                }
            }

            return areaCells;
        }

        private Cell GetCellAt(float x, float z)
        {
            //Debug.Log($"{prefix} GetCellAt {x} {z}\n");

            /*
            foreach (Cell cell in localCells)
            {
                if (cell.Position.x == x && cell.Position.z == z) { return cell; }
            }
            */

            return null;
        }

        public void MarkArea(List<Cell> area, Cell.TYPE markType)
        {
            foreach (Cell cell in area) { cell.SetCellType(markType); }
        }

        // ================= HELPER FUNCTIONS ============================== >>
    }
}
