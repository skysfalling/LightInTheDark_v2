using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Darklight.ThirdDimensional.World
{
    public class ChunkMap
    {
        HashSet<Chunk> _chunks = new();
        Dictionary<Vector2Int, Chunk> _chunkMap = new();

        public bool Initialized { get; private set; }
        public Region WorldRegionParent { get; private set; }
        public CoordinateMap CoordinateMap { get; private set; }
        public HashSet<Chunk> AllChunks { get { return _chunks; } private set { } }

        public ChunkMap(Region worldRegion, CoordinateMap coordinateMap)
        {
            this.WorldRegionParent = worldRegion;
            this.CoordinateMap = coordinateMap;

            // [[ CREATE WORLD CHUNKS ]]
            foreach (Vector2Int position in coordinateMap.AllCoordinateValues)
            {
                Coordinate coordinate = coordinateMap.GetCoordinateAt(position);
                Chunk newChunk = new Chunk(this, coordinate);
                _chunks.Add(newChunk);
                _chunkMap[coordinate.Value] = newChunk;
            }

            Initialized = true;
        }

        public void UpdateMap()
        {
            foreach (Chunk chunk in _chunks)
            {
                Coordinate.TYPE type = (Coordinate.TYPE)CoordinateMap.GetCoordinateTypeAt(chunk.Coordinate.Value);

                switch (type)
                {
                    case Coordinate.TYPE.NULL:
                        break; // Allow default Perlin Noise
                    case Coordinate.TYPE.CLOSED:
                        chunk.SetGroundHeight(WorldGeneration.Settings.ChunkMaxHeight_inCellUnits);
                        break; // Set to max height
                    default:
                        chunk.SetGroundHeight(0); // Set to default 0
                        break;
                }
            }
        }


        public void GenerateChunkMeshes()
        {
            foreach (Chunk chunk in _chunks)
            {
                chunk.CreateChunkMesh();
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
            return GetChunkAt(worldCoord.Value);
        }

        public List<Chunk> GetChunksAtCoordinates(List<Coordinate> worldCoords)
        {
            if (!Initialized) { return new List<Chunk>(); }

            List<Chunk> chunks = new List<Chunk>();
            foreach (Coordinate worldCoord in worldCoords)
            {
                chunks.Add(GetChunkAt(worldCoord));
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