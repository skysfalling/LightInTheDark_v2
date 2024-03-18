using System.Collections;
namespace Darklight.World.Builder
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Darklight;
	using UnityEngine;
	using UnityEngine.UIElements;
	using Darklight.Bot;
	using Darklight.World.Generation;
	using Darklight.World.Settings;
	using Darklight.World.Map;
	using System;
	using Darklight.World.Editor;

	public class ChunkBuilder : TaskQueen, ITaskEntity
	{
		HashSet<ChunkData> _chunks = new();
		Dictionary<Vector2Int, ChunkData> _chunkMap = new();

		public bool Initialized { get; private set; }
		public bool GenerationFinished { get; private set; }
		public bool CreateObjects { get; private set; } = false;
		public RegionBuilder RegionParent { get; private set; }
		CoordinateMap _coordinateMap;
		public HashSet<ChunkData> AllChunks { get { return _chunks; } private set { } }

		public async void Initialize(RegionBuilder worldRegion, CoordinateMap coordinateMap)
		{
			await base.Initialize();
			RegionParent = worldRegion;
			_coordinateMap = coordinateMap;
			Initialized = true;
		}
		async Task CreateChunkMesh()
		{
			ChunkBuilder self = this; // Capture the instance of ChunkGeneration
			TaskBotConsole.Log(self, $"Creating {_coordinateMap.AllCoordinateValues.Count} Chunks");

			foreach (Vector2Int position in _coordinateMap.AllCoordinateValues)
			{
				TaskBotConsole.Log(self, $"\t>> Creating Chunk {position}");
				Coordinate coordinate = _coordinateMap.GetCoordinateAt(position);
				ChunkData newChunk = new ChunkData(self, coordinate); // Use the captured instance
				_chunks.Add(newChunk);
				_chunkMap[coordinate.ValueKey] = newChunk;
			}
			TaskBotConsole.Log(self, $">> Here they are! {_chunkMap.ToString()}");
			await Task.CompletedTask;
		}

		async Task CreateChunkObjs()
		{
			TaskBotConsole.Log(this, $"Creating {_chunks.Count} Meshes");
			foreach (ChunkData chunk in _chunks)
			{
				ChunkMesh newMesh = chunk.CreateChunkMesh();

				if (WorldBuilder.Instance != null) { continue; }
				GameObject newChunkObject = CreateMeshObject(chunk.Coordinate.ValueKey.ToString(), newMesh.Mesh, RegionParent.defaultMaterial);
				TaskBotConsole.Log(this, $"\tNewChunkMeshObject : {newChunkObject.name} : {newMesh.Mesh.vertexCount}");
			}
			await Task.CompletedTask;
		}


		public GameObject CreateMeshObject(string name, Mesh mesh, Material material)
		{

			Material defaultMaterial = RegionParent.defaultMaterial;
			if (RegionParent && RegionParent.defaultMaterial != null && material == null)
			{
				Debug.LogWarning($"{name} SetMaterial to GenerationParent default");
				material = RegionParent.defaultMaterial;
			}
			else if (mesh == null) { Debug.LogWarning($"{name} Mesh is null"); }

			GameObject worldObject = new GameObject(name);
			ChunkEditor chunkEditor = worldObject.AddComponent<ChunkEditor>();
			worldObject.transform.parent = transform;
			worldObject.transform.localPosition = Vector3.zero;

			MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();

			if (material == null)
			{
				worldObject.AddComponent<MeshRenderer>().material = defaultMaterial;
				worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
			}
			else
			{
				worldObject.AddComponent<MeshRenderer>().material = material;
				worldObject.AddComponent<MeshCollider>().sharedMesh = mesh;
			}
			return worldObject;
		}
		public async Task GenerationSequence()
		{
			/// STAGE 1 : [[ CREATE CHUNKS ]]
			TaskBot task1 = new TaskBot(this, "CreatingChunks [task1]", CreateChunkMesh());
			await Enqueue(task1);
			/// STAGE 1 : [[ CREATE CHUNKS ]]
			TaskBot task2 = new TaskBot(this, "CreatingChunkMeshObjs [task2]", CreateChunkObjs());
			await Enqueue(task2);

			await ExecuteAllTasks();
			TaskBotConsole.Log(this, "Initialization Sequence Completed");
			GenerationFinished = true;
		}

		public ChunkData GetChunkAt(Vector2Int position)
		{
			if (!Initialized || !_coordinateMap.AllCoordinateValues.Contains(position)) { return null; }
			return _chunkMap[position];
		}

		public ChunkData GetChunkAt(Coordinate worldCoord)
		{
			if (!Initialized || worldCoord == null) { return null; }
			return GetChunkAt(worldCoord.ValueKey);
		}

		public List<ChunkData> GetChunksAtCoordinateValues(List<Vector2Int> values)
		{
			if (!Initialized) { return new List<ChunkData>(); }

			List<ChunkData> chunks = new List<ChunkData>();
			foreach (Vector2Int value in values)
			{
				chunks.Add(GetChunkAt(value));
			}

			return chunks;
		}

		public void ResetAllChunkHeights()
		{
			foreach (ChunkData chunk in AllChunks)
			{
				chunk.SetGroundHeight(0);
			}
		}

		public void SetChunksToHeight(List<ChunkData> worldChunk, int chunkHeight)
		{
			foreach (ChunkData chunk in worldChunk)
			{
				chunk.SetGroundHeight(chunkHeight);
			}
		}

		public void SetChunksToHeightFromPositions(List<Vector2Int> positions, int chunkHeight)
		{
			foreach (Vector2Int pos in positions)
			{
				ChunkData chunk = GetChunkAt(pos);
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
				ChunkData currentChunk = GetChunkAt(path.AllPositions[i]);

				// Assign start/end chunk heights & CONTINUE
				if (i == 0) { currentChunk.SetGroundHeight(startHeight); continue; }
				else if (i == path.AllPositions.Count - 1) { currentChunk.SetGroundHeight(endHeight); continue; }
				else
				{
					// Determine heightOffset 
					int heightOffset = 0;

					// Determine the direction of the last & next chunk in path
					ChunkData previousChunk = GetChunkAt(path.AllPositions[i - 1]);
					ChunkData nextChunk = GetChunkAt(path.AllPositions[i + 1]);
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

		public override void Reset()
		{
			base.Reset();
			Initialized = false;
			_coordinateMap = null;
			_chunks.Clear();
			_chunkMap.Clear();
		}
	}
}