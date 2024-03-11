using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Unity.Backend;
using UnityEngine;
using UnityEngine.UIElements;

namespace Darklight.World.Generation
{
	public class ChunkGeneration : TaskQueen
	{
		HashSet<Chunk> _chunks = new();
		Dictionary<Vector2Int, Chunk> _chunkMap = new();

		public bool Initialized { get; private set; }
		public bool CreateObjects { get; private set; } = false;
		public RegionBuilder RegionParent { get; private set; }
		CoordinateMap _coordinateMap;
		public HashSet<Chunk> AllChunks { get { return _chunks; } private set { } }

		public void Initialize(RegionBuilder worldRegion, CoordinateMap coordinateMap)
		{
			base.Initialize("ChunkGenerationAsync");
			RegionParent = worldRegion;
			_coordinateMap = coordinateMap;
		}

		public void Reset()
		{
			Initialized = false;
			_coordinateMap = null;
			_chunks.Clear();
			_chunkMap.Clear();
		}

		public async Task GenerationSequence()
		{

			while (_coordinateMap.Initialized == false)
			{
				await Task.Delay(1000);
			}


			TaskBot task1 = new TaskBot("Creating Chunks", async () =>
			{
				Console.Log(this, $"Creating {_coordinateMap.AllCoordinateValues.Count} Chunks");

				// [[ CREATE WORLD CHUNKS ]]
				foreach (Vector2Int position in _coordinateMap.AllCoordinateValues)
				{
					Coordinate coordinate = _coordinateMap.GetCoordinateAt(position);
					Chunk newChunk = new Chunk(this, coordinate);
					_chunks.Add(newChunk);
					_chunkMap[coordinate.ValueKey] = newChunk;
				}

				await Task.Yield();
			});
			Enqueue(task1);

			TaskBot task2 = new TaskBot("Creating Meshes", async () =>
			{
				Console.Log(this, $"Creating {_chunks.Count} Meshes");

				foreach (Chunk chunk in _chunks)
				{
					ChunkMesh newMesh = chunk.CreateChunkMesh();
				}

				await Task.Yield();
			});
			Enqueue(task2);

			if (WorldBuilder.Instance == null)
			{
				TaskBot task3 = new TaskBot("Creating Objects", async () =>
				{
					Console.Log(this, $"Creating {_chunks.Count} Objects");
					foreach (Chunk chunk in _chunks)
					{
						await Task.Run(() => chunk.ChunkMesh != null);

						GameObject newObject = RegionParent.CreateMeshObject($"Chunk {chunk.Coordinate.ValueKey}", chunk.ChunkMesh.Mesh);
						Console.Log(this, $"\tNewChunkMeshObject : {newObject}");
					}

					await Task.Yield();
				});
				Enqueue(task3);

			}


			ExecuteAllTasks();
			Console.Log(this, "Initialization Sequence Completed");
			Initialized = true;

		}


		public void UpdateMap()
		{
			foreach (Chunk chunk in _chunks)
			{
				Coordinate.TYPE type = (Coordinate.TYPE)_coordinateMap.GetCoordinateTypeAt(chunk.Coordinate.ValueKey);

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
			if (!Initialized || !_coordinateMap.AllCoordinateValues.Contains(position)) { return null; }
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