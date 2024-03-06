using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World = Darklight;
namespace Darklight.ThirdDimensional.World
{
    using WorldGen = WorldGeneration;

    public class Chunk
    {
        public enum FaceType { Front, Back, Left, Right, Top, Bottom }

        // [[ PRIVATE VARIABLES ]]
        Coordinate _coordinate;
        CoordinateMap _coordinateMap;
        ChunkMesh _chunkMesh;
        CellMap _cellMap;
        int _groundHeight = 2;

        // [[ PUBLIC ACCESS VARIABLES ]] 
        public ChunkMap ChunkMapParent { get; private set; }
        public Coordinate Coordinate => _coordinate;
        public CoordinateMap CoordinateMap => _coordinateMap;
        public GameObject ChunkObject { get; private set; }
        public ChunkMesh ChunkMesh => _chunkMesh;
        public CellMap CellMap => _cellMap;
        public int GroundHeight => _groundHeight;
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
        public TYPE type;
        public bool generation_finished { get; private set; }

        // Active Edges
        bool _northEdgeActive;
        bool _southEdgeActive;
        bool _eastEdgeActive;
        bool _westEdgeActive;
        public List<Cell> localCells = new List<Cell>();

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


            //DetermineChunkEdges();
            //SetChunkType();
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

        #region ================ INITIALIZE WORLD CHUNK ============================= >>

        void DetermineChunkEdges()
        {
            // Initialize all edges as active
            _northEdgeActive = true;
            _southEdgeActive = true;
            _eastEdgeActive = true;
            _westEdgeActive = true;

            // Define the edge positions
            float northEdgeZ = float.MinValue;
            float southEdgeZ = float.MaxValue;
            float eastEdgeX = float.MinValue;
            float westEdgeX = float.MaxValue;

            // Find the edge positions
            foreach (Cell cell in localCells)
            {
                if (cell.Position.z > northEdgeZ) northEdgeZ = cell.Position.z;
                if (cell.Position.z < southEdgeZ) southEdgeZ = cell.Position.z;
                if (cell.Position.x > eastEdgeX) eastEdgeX = cell.Position.x;
                if (cell.Position.x < westEdgeX) westEdgeX = cell.Position.x;
            }

            // Check each cell
            foreach (Cell cell in localCells)
            {
                if (cell.Type == Cell.TYPE.EMPTY)
                {
                    if (cell.Position.z == northEdgeZ) _northEdgeActive = false;
                    if (cell.Position.z == southEdgeZ) _southEdgeActive = false;
                    if (cell.Position.x == eastEdgeX) _eastEdgeActive = false;
                    if (cell.Position.x == westEdgeX) _westEdgeActive = false;
                }
            }

            /*
            // Log the active edges
            Debug.Log($"North Edge Active: {NorthEdgeActive}\n " +
                      $"South Edge Active: {SouthEdgeActive}\n " +
                      $"East Edge Active: {EastEdgeActive}\n " +
                      $"West Edge Active: {WestEdgeActive}\n ");
            */
        }
        void SetChunkType()
        {
            // Get Edge Count
            int activeEdgeCount = 0;
            if (_northEdgeActive) { activeEdgeCount++; }
            if (_southEdgeActive) { activeEdgeCount++; }
            if (_eastEdgeActive) { activeEdgeCount++; }
            if (_westEdgeActive) { activeEdgeCount++; }

            // Set Type
            if (activeEdgeCount == 4) { type = TYPE.CLOSED; return; }
            if (activeEdgeCount == 3) { type = TYPE.DEADEND; return; }
            if (activeEdgeCount == 2)
            {
                // Check for parallel edges
                if (_northEdgeActive && _southEdgeActive) { type = TYPE.HALLWAY; return; }
                if (_eastEdgeActive && _westEdgeActive) { type = TYPE.HALLWAY; return; }
                type = TYPE.CORNER;
            }
            if (activeEdgeCount == 1) { type = TYPE.WALL; return; }
            if (activeEdgeCount == 0) { type = TYPE.EMPTY; return; }
        }

        #endregion

        // ================ CREATE & INITIALIZE WORLD CELLS ============================== >>



        // ================== SPAWN OBJECTS ============= >>
        public List<Cell> FindSpace(EnvironmentObject envObj)
        {
            Dictionary<int, List<Cell>> availableSpace = new Dictionary<int, List<Cell>>();
            int spaceIndex = 0;

            foreach (Cell startCell in localCells)
            {
                if (IsSpaceAvailable(startCell, envObj))
                {
                    availableSpace[spaceIndex] = GetCellsInArea(startCell, envObj.space);
                    spaceIndex++;
                }
            }

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

            foreach (Cell cell in localCells)
            {
                if (cell.Position.x == x && cell.Position.z == z) { return cell; }
            }

            return null;
        }

        public void MarkArea(List<Cell> area, Cell.TYPE markType)
        {
            foreach (Cell cell in area) { cell.SetCellType(markType); }
        }

        // ================= HELPER FUNCTIONS ============================== >>
    }
}
