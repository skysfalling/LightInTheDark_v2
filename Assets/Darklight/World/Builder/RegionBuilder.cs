using System.Collections;
namespace Darklight.World.Builder
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEditor;
	using UnityEngine;

	using Bot;
	using Builder;
	using Generation;
	using Map;
	using Settings;

	public class RegionBuilder : TaskBotQueen, ITaskEntity
	{
		string _prefix = "[[ REGION BUILDER ]]";
		HashSet<GameObject> _regionObjects = new();
		GridMap2D<Chunk> _chunkGridMap = new GridMap2D<Chunk>();


		#region [[ GETTERS ]]
		public WorldGenerationSystem WorldGenerationSystem => WorldGenerationSystem.Instance;
		public GenerationSettings WorldSettings => WorldGenerationSystem.Instance.Settings;
		public bool GenerationFinished { get; private set; } = false;
		#endregion

		public override void Reset()
		{
			base.Reset();

			//_chunkBuilder.Reset();

			foreach (GameObject gameObject in _regionObjects)
			{
				WorldGenerationSystem.DestroyGameObject(gameObject);
			}

			TaskBotConsole.Reset();
		}

		// INITIALIZE ============================== /////
		public override async Task Initialize()
		{
			this.Name = "RegionAsyncTaskQueen";
			if (Initialized) return;

			await base.Initialize();

			/*

								// Generate Exits, Paths and Zones based on neighboring regions
								TaskBot RegionGenerationTask = new TaskBot(this, "RegionGenerationTask", async () =>
								{
									while (_coordinateMap.Initialized != true)
									{
										await Awaitable.WaitForSecondsAsync(0.1f);
									}

									await GenerateExits(true);
									await CoordinateMap.GeneratePathsBetweenExits();
									await CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.Shape> { Zone.Shape.SINGLE });

									//TaskBotConsole.Log(this, $"Region Generation Complete with {CoordinateMap.Exits.Count} Exits and {CoordinateMap.Zones.Count} Zones");
									Debug.Log($"Region Generation Complete [[ {CoordinateMap.Exits.Count} Exits ,, {CoordinateMap.Zones.Count} Zones ]");

									await Task.CompletedTask;
								});
								await Enqueue(RegionGenerationTask);

								// Create the chunk map for the region
								TaskBot ChunkGenerationTask = new TaskBot(this, "Initialize Chunk Generation", async () =>
								{
									// Initialize chunks
									_chunkBuilder.Initialize(this, this._coordinateMap);
									await _chunkBuilder.GenerationSequence();
								});
								await Enqueue(ChunkGenerationTask);


								// [[ STAGE 3 ]] COMBINE CHUNK MESH OBJECTS ---- >> 
								// it WorldBuilder is present, then combine the chunk mesh
								if (WorldBuilder.Instance != null)
								{
									TaskBot CombineMesh = new TaskBot(this, "Mesh Generation", async () =>
									{
										//TaskBotConsole.Log(this, "Starting mesh generation...");

										while (_chunkBuilder == null || _chunkBuilder.Initialized == false)
										{
											await Awaitable.WaitForSecondsAsync(0.1f);
										}

										// Asynchronously create and initialize combined chunk mesh
										await CombineChunkMesh();
										//TaskBotConsole.Log(this, "Mesh generation complete.");
									});
									await Enqueue(CombineMesh);
								}*/

			/*
	*/


			if (selfGenerate)
			{
				await GenerationSequence();
			}
		}

		public bool selfGenerate = false;
		/// <summary>
		/// The initialization sequence for the region builder.
		/// </summary>
		/// <returns>The task representing the initialization sequence.</returns>
		public async Task GenerationSequence()
		{
			//this._chunkBuilder = GetComponent<ChunkBuilder>();


			// Execute all tasks queued in AsyncTaskQueen
			//await ExecuteAllTasks();
			await Task.CompletedTask;
			GenerationFinished = true;
		}




	}
}