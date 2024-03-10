using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using UnityEngine;
using UnityEngine.UIElements;

namespace Darklight.World.Generation
{
    public class ChunkGeneration : AsyncTaskQueen, ITaskQueen
    {
        HashSet<Chunk> _chunks = new();
        Dictionary<Vector2Int, Chunk> _chunkMap = new();

        public bool Initialized { get; private set; }
        public RegionBuilder RegionParent { get; private set; }
        public CoordinateMap CoordinateMap { get; private set; }
        public HashSet<Chunk> AllChunks { get { return _chunks; } private set { } }

        public void Initialize(RegionBuilder worldRegion, CoordinateMap coordinateMap)
        {
            base.Initialize("ChunkGenerationAsync");
            RegionParent = worldRegion;
            CoordinateMap = coordinateMap;
            Initialized = true;

            _ = InitializationSequence();
        }

        async Task InitializationSequence()
        {

            base.NewTaskBot("Wait for Coordinate Map", async () =>
            {
                await Task.Run(() => CoordinateMap.Initialized);
                await Task.Yield();
            });


            base.NewTaskBot("Creating Chunks", async () =>
            {
                asyncTaskConsole.Log(this, $"Creating {CoordinateMap.AllCoordinateValues.Count} Chunks");

                // [[ CREATE WORLD CHUNKS ]]
                foreach (Vector2Int position in CoordinateMap.AllCoordinateValues)
                {
                    Coordinate coordinate = CoordinateMap.GetCoordinateAt(position);
                    Chunk newChunk = new Chunk(this, coordinate);
                    _chunks.Add(newChunk);
                    _chunkMap[coordinate.ValueKey] = newChunk;
                }

                await Task.Yield();
            });

            base.NewTaskBot("Creating Meshes", async () =>
            {
                asyncTaskConsole.Log(this, $"Creating {_chunks.Count} Meshes");

                foreach (Chunk chunk in _chunks)
                {
                    ChunkMesh newMesh = chunk.CreateChunkMesh();
                    asyncTaskConsole.Log(this, $"\tNewChunkMeshObject : {newMesh}");
                }

                await Task.Yield();
            });

            base.NewTaskBot("Creating Objects", async () =>
            {
                asyncTaskConsole.Log(this, $"Creating {_chunks.Count} Objects");


                foreach (Chunk chunk in _chunks)
                {
                    await Task.Run(() => chunk.ChunkMesh != null);

                    GameObject newObject = RegionParent.CreateChunkMeshObject(chunk.ChunkMesh, $"Chunk {chunk.Coordinate.ValueKey}");
                    asyncTaskConsole.Log(this, $"\tNewChunkMeshObject : {newObject}");
                }

                await Task.Yield();
            });

            await base.ExecuteAllBotsInQueue();
            Debug.Log("Initialization Sequence Completed");

        }


        public void UpdateMap()
        {
            foreach (Chunk chunk in _chunks)
            {
                Coordinate.TYPE type = (Coordinate.TYPE)CoordinateMap.GetCoordinateTypeAt(chunk.Coordinate.ValueKey);

                switch (type)
                {
                    case Coordinate.TYPE.NULL:
                    case Coordinate.TYPE.BORDER:
                        break; // Allow default Perlin Noise
                    case Coordinate.TYPE.CLOSED:
                        chunk.SetGroundHeight(WorldBuilder.Settings.ChunkMaxHeight_inCellUnits);
                        break; // Set to max height
                    default:
                        chunk.SetGroundHeight(0); // Set to default 0
                        break;
                }
            }
        }

        public Chunk GetChunkAt(Vector2Int position)
        {
            if (!Initialized || !CoordinateMap.AllCoordinateValues.Contains(position)) { return null; }
            return _chunkMap[position];
        }

        public Chunk GetChunkAt(Coordinate worldCoord)
        {
            if (!Initialized || worldCoord == null) { return null; }
            return GetChunkAt(worldCoord.ValueKey);
        }

        public List<Chunk> GetChunksAtCoordinateValues(List<Vector2Int> values)
        {
            if (!Initialized) { return new List<Chunk>(); }

            List<Chunk> chunks = new List<Chunk>();
            foreach (Vector2Int value in values)
            {
                chunks.Add(GetChunkAt(value));
            }

            return chunks;
        }

        public void ResetAllChunkHeights()
        {
            foreach (Chunk chunk in AllChunks)
            {
                chunk.SetGroundHeight(0);
            }
        }

        public void SetChunksToHeight(List<Chunk> worldChunk, int chunkHeight)
        {
            foreach (Chunk chunk in worldChunk)
            {
                chunk.SetGroundHeight(chunkHeight);
            }
        }

        public void SetChunksToHeightFromPositions(List<Vector2Int> positions, int chunkHeight)
        {
            foreach (Vector2Int pos in positions)
            {
                Chunk chunk = GetChunkAt(pos);
                if (chunk != null)
                {
                    chunk.SetGroundHeight(chunkHeight);
                }
            }
        }

        public void SetChunksToHeightFromPath(Path path, float heightAdjustChance = 1f)
        {
            int startHeight = GetChunkAt(path.StartPosition).GroundHeight;
            int endHeight = GetChunkAt(path.EndPosition).GroundHeight;

            // Calculate height difference
            int endpointHeightDifference = endHeight - startHeight;
            int currHeightLevel = startHeight; // current height level starting from the startHeight
            int heightLeft = endpointHeightDifference; // initialize height left

            // Iterate through the chunks
            for (int i = 0; i < path.AllPositions.Count; i++)
            {
                Chunk currentChunk = GetChunkAt(path.AllPositions[i]);

                // Assign start/end chunk heights & CONTINUE
                if (i == 0) { currentChunk.SetGroundHeight(startHeight); continue; }
                else if (i == path.AllPositions.Count - 1) { currentChunk.SetGroundHeight(endHeight); continue; }
                else
                {
                    // Determine heightOffset 
                    int heightOffset = 0;

                    // Determine the direction of the last & next chunk in path
                    Chunk previousChunk = GetChunkAt(path.AllPositions[i - 1]);
                    Chunk nextChunk = GetChunkAt(path.AllPositions[i + 1]);
                    WorldDirection? lastChunkDirection = currentChunk.Coordinate.GetWorldDirectionOfNeighbor(previousChunk.Coordinate);
                    WorldDirection? nextChunkDirection = currentChunk.Coordinate.GetWorldDirectionOfNeighbor(nextChunk.Coordinate);
                    if (lastChunkDirection != null && nextChunkDirection != null)
                    {
                        // if previous chunk is direct opposite of next chunk, allow for change in the current chunk
                        if (currentChunk.Coordinate.GetNeighborInOppositeDirection((WorldDirection)nextChunkDirection) == previousChunk.Coordinate)
                        {
                            // Valid transition chunk
                            if (heightLeft > 0) { heightOffset = 1; } // if height left is greater
                            else if (heightLeft < 0) { heightOffset = -1; } // if height left is less than 0
                            else { heightOffset = 0; } // if height left is equal to 0
                        }
                    }

                    // Set the new height level
                    currHeightLevel += heightOffset;
                    currentChunk.SetGroundHeight(currHeightLevel);

                    // Recalculate heightLeft with the new current height level
                    heightLeft = endHeight - currHeightLevel;
                }
            }
        }
    }
}