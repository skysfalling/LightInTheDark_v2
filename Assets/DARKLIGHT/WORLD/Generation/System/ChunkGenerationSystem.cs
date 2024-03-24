using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Darklight;
using Darklight.Bot;
using Darklight.World.Generation;
using Darklight.World.Generation.Unit;
using Darklight.World.Map;
using Darklight.World.Settings;

using UnityEngine;
using UnityEngine.UIElements;

namespace Darklight.World.Generation.System
{
	public class ChunkGenerationSystem : TaskBotQueen, ITaskEntity
	{
		public static HashSet<GameObject> InstantiatedObjects { get; private set; } = new HashSet<GameObject>();
		public Region RegionParent { get; private set; }
		public GridMap2D<Chunk> GridMap => RegionParent.ChunkGridMap2D;

		public async void Initialize(Region regionParent, bool startGeneration = false)
		{
			await base.Initialize();
			RegionParent = regionParent;


			// << CREATE GENERATION SEQUENCE >>  
			List<TaskBot> generationBots = new List<TaskBot> {
				new TaskBot(this, "CreateAllChunkMesh", CreateAllChunkMesh),
				new TaskBot(this, "CreateAllChunkObjs", CreateAllsChunkObjs)
			};
			await EnqueueList(generationBots);

			if (startGeneration) { await ExecuteAllTasks(); }
		}

		async Task CreateAllChunkMesh()
		{
			//TaskBotConsole.Log(this, $"Creating {_chunks.Count} Meshes");
			foreach (Chunk chunk in GridMap.DataValues)
			{
				ChunkMesh newMesh = chunk.CreateChunkMesh();
				//TaskBotConsole.Log(this, $"\tNewChunkMesh : {newMesh}");
			}
			await Task.CompletedTask;
		}

		async Task CreateAllsChunkObjs()
		{
			//TaskBotConsole.Log(this, $"Creating {_chunks.Count} Meshes");
			foreach (Chunk chunk in GridMap.DataValues)
			{

				chunk.ChunkObject = new GameObject($"Chunk {chunk.PositionKey} :: Height {chunk.GroundHeight}");
				chunk.ChunkObject.transform.position = Vector3.zero;
				chunk.ChunkObject.transform.parent = transform;
				chunk.ChunkObject.AddComponent<MeshRenderer>().material = WorldGenerationSystem.Instance.defaultMaterial;
				chunk.ChunkObject.AddComponent<MeshFilter>().sharedMesh = chunk.ChunkMesh.Mesh;

				InstantiatedObjects.Add(chunk.ChunkObject);
				//TaskBotConsole.Log(this, $"\tNewChunkMeshObject : {chunk.ChunkObject}");
			}
			await Task.CompletedTask;
		}

		/*
				public async Task GenerationSequence()
				{
					/// STAGE 1 : [[ CREATE CHUNKS ]]
					TaskBot createChunkData = new TaskBot(this, "CreatingChunks [task1]", CreateAllChunkData());
					await Enqueue(createChunkData);

					// STAGE 2 : [[ CREATE CHUNK MESH]]
					TaskBot createMeshTask = new TaskBot(this, "CreatingChunkMesh [task2]", CreateAllChunkMesh());
					await Enqueue(createMeshTask);

					/// STAGE 3 : [[ CREATE CHUNK OBJECT ]]
					//TaskBot createChunkObject = new TaskBot(this, "CreatingChunkMeshObjs [task2]", CreateAllsChunkObjs());
					//await Enqueue(createChunkObject);

					await ExecuteAllTasks();
					//TaskBotConsole.Log(this, "Initialization Sequence Completed");
					GenerationFinished = true;
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
							Direction? lastChunkDirection = currentChunk.Coordinate.GetWorldDirectionOfNeighbor(previousChunk.Coordinate);
							Direction? nextChunkDirection = currentChunk.Coordinate.GetWorldDirectionOfNeighbor(nextChunk.Coordinate);
							if (lastChunkDirection != null && nextChunkDirection != null)
							{
								// if previous chunk is direct opposite of next chunk, allow for change in the current chunk
								if (currentChunk.Coordinate.GetNeighborInOppositeDirection((Direction)nextChunkDirection) == previousChunk.Coordinate)
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
				*/

		public override void Reset()
		{
			base.Reset();
			TaskBotConsole.Reset();

			// Destroy Instantiated Objects
			foreach (GameObject gameObject in InstantiatedObjects)
			{
				WorldGenerationSystem.DestroyWithEditorContext(gameObject);
			}
		}
	}
}