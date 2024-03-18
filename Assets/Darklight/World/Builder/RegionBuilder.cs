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
	using Darklight.World.Editor;

	[RequireComponent(typeof(ChunkBuilder))]
	public class RegionBuilder : TaskQueen, ITaskEntity
	{
		#region [[ PRIVATE VARIABLES ]] 
		string _prefix = "[[ REGION BUILDER ]]";
		WorldBuilder _generationParent;
		Coordinate _coordinate;
		CoordinateMap _coordinateMap;
		ChunkBuilder _chunkBuilder;
		GameObject _combinedMeshObject;
		HashSet<GameObject> _regionObjects = new();
		#endregion
		#region [[ GETTERS ]]
		/// <summary>
		/// Gets a value indicating whether the region builder has been initialized.
		/// </summary>
		public bool Initialized { get; private set; }

		/// <summary>
		/// Indicated if the generation of the region has finished.
		/// </summary>
		public bool GenerationFinished { get; private set; }

		/// <summary>
		/// Gets the parent WorldBuilder.
		/// </summary>
		public WorldBuilder GenerationParent => _generationParent;

		/// <summary>
		/// Gets the coordinate of the region.
		/// </summary>
		public Coordinate Coordinate => _coordinate;

		/// <summary>
		/// Gets the coordinate map for the region.
		/// </summary>
		public CoordinateMap CoordinateMap => _coordinateMap;

		/// <summary>
		/// Gets the chunk generation component.
		/// </summary>
		public ChunkBuilder ChunkBuilder => GetComponent<ChunkBuilder>();

		/// <summary>
		/// Gets the center position of the region.
		/// </summary>
		public Vector3 CenterPosition
		{
			get
			{
				if (Coordinate == null) { return Vector3.zero; }
				else { return Coordinate.ScenePosition; }
			}
		}

		/// <summary>
		/// Gets the origin position of the region.
		/// </summary>
		public Vector3 OriginPosition
		{
			get
			{
				Vector3 origin = CenterPosition;
				origin -= RegionBuilder.Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				origin += RegionBuilder.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				return origin;
			}
		}
		#endregion
		#region [[ INSPECTOR SETTINGS ]]	
		public Material defaultMaterial;
		static GenerationSettings _regionSettings = new();
		public CustomGenerationSettings customRegionSettings; // ScriptableObject
		public static GenerationSettings Settings => _regionSettings;

		/// <summary>
		/// Override the default generation settings.
		/// </summary>
		/// <param name="customSettings">The custom generation settings.</param>
		public void OverrideSettings(CustomGenerationSettings customSettings)
		{
			if (customSettings == null) { _regionSettings = new GenerationSettings(); return; }
			_regionSettings = new GenerationSettings(customSettings);
			customRegionSettings = customSettings;
		}
		#endregion
		#region [[ RANDOM SEED ]] 

		/// <summary>
		/// Gets the seed used for random generation.
		/// </summary>
		public static string Seed { get { return Settings.Seed; } }

		/// <summary>
		/// Initializes the random seed.
		/// </summary>
		public static void InitializeSeedRandom()
		{
			UnityEngine.Random.InitState(Settings.Seed.GetHashCode());
		}

		#endregion

		/// <summary>
		/// Indicates whether the region should be initialized on start.
		/// </summary>
		public bool initializeOnStart;
		/// <summary>
		/// Assigns the region to a parent <see cref="WorldBuilder"/>.
		/// This method should be called by the world builder to assign the region to itself.
		/// </summary>
		/// <param name="parent">The parent WorldBuilder.</param>
		/// <param name="coordinate">The coordinate of the region.</param>
		/// <param name="taskQueenName">The name of the task queen.</param>
		public void AssignToWorldParent(WorldBuilder parent, Coordinate coordinate, string taskQueenName = "Region Task Queen")
		{
			this._generationParent = parent;
			this._coordinate = coordinate;

			if (parent.customWorldGenSettings != null)
			{
				OverrideSettings(parent.customWorldGenSettings);
			}
			this.GenerationParent.TaskBotConsole.Log(this, $"\t Child Region Assigned to World Coordinate {coordinate}");
		}

		public async void Start()
		{
			if (initializeOnStart == true)
			{
				if (WorldBuilder.Instance == null)
				{
					WorldBuilder.OverrideSettings(customRegionSettings);
					this._coordinate = new Coordinate(Vector2Int.zero, UnitSpace.REGION);
				}

				Debug.Log($"{_prefix} Initialize On Start");
				await this.Initialize();
			}
		}
		public override void Reset()
		{
			base.Reset();

			Initialized = false;
			_coordinateMap = null;

			foreach (GameObject gameObject in _regionObjects)
			{
				DestroyGameObject(gameObject);
			}

			TaskBotConsole.Reset();

		}

		// INITIALIZE ============================== /////
		public override async Task Initialize()
		{
			this.Name = "RegionAsyncTaskQueen";
			if (Initialized) return;
			this._coordinateMap = new CoordinateMap(this);
			await _coordinateMap.InitializeDefaultMap();
			await base.Initialize();
			Initialized = true;

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
			this._chunkBuilder = GetComponent<ChunkBuilder>();

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

				TaskBotConsole.Log(this, $"Region Generation Complete with {CoordinateMap.Exits.Count} Exits and {CoordinateMap.Zones.Count} Zones");
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
					TaskBotConsole.Log(this, "Starting mesh generation...");

					while (_chunkBuilder == null || _chunkBuilder.Initialized == false)
					{
						await Awaitable.WaitForSecondsAsync(0.1f);
					}

					// Asynchronously create and initialize combined chunk mesh
					await CombineChunkMesh();
					TaskBotConsole.Log(this, "Mesh generation complete.");
				});
				await Enqueue(CombineMesh);
			}

			// Execute all tasks queued in AsyncTaskQueen
			await ExecuteAllTasks();
			GenerationFinished = true;
		}

		/// <summary>
		/// Generates necessary exits for the region.
		/// </summary>
		/// <param name="createExits">Indicates whether to create exits if they don't exist.</param>
		async Task GenerateExits(bool createExits)
		{
			if (this.Coordinate == null)
			{
				Debug.LogError("RegionBuilder: CoordinateParent is null, abort GenerateExitsTask");
				await Task.CompletedTask;
			}
			else
			{
				Dictionary<WorldDirection, Vector2Int> neighborDirectionMap = this.Coordinate.NeighborDirectionMap;

				// Iterate directly over the keys of the map
				foreach (WorldDirection neighborDirection in neighborDirectionMap.Keys)
				{
					Vector2Int neighborCoordinateValue = neighborDirectionMap[neighborDirection];
					BorderDirection? currentBorderWithNeighbor = CoordinateMap.GetBorderDirection(neighborDirection);

					// Skip iteration if no border direction is found.
					if (!currentBorderWithNeighbor.HasValue) continue;

					// Check if the neighbor exists; close the border if it doesn't.
					if (this.GenerationParent.CoordinateMap.GetCoordinateAt(neighborCoordinateValue) == null)
					{
						// Close borders on chunks if neighbor not found.
						this.CoordinateMap.CloseMapBorder(currentBorderWithNeighbor.Value);
					}
					else
					{
						// Proceed with exit handling if the neighbor exists.
						BorderDirection borderInThisRegion = (BorderDirection)currentBorderWithNeighbor; // >> convert border direction to non-nullable type
																										 // >> get reference to neighbor region
						RegionBuilder neighborRegion = this.GenerationParent.RegionMap[neighborCoordinateValue];
						// >> get matching border direction
						BorderDirection matchingBorderOnNeighbor = (BorderDirection)CoordinateMap.GetOppositeBorder(borderInThisRegion);
						// >> get exits on neighbor region
						HashSet<Vector2Int> neighborBorderExits = neighborRegion.CoordinateMap.GetExitsOnBorder(matchingBorderOnNeighbor);

						// If neighbor has exits, create matching exits.
						if (neighborBorderExits != null && neighborBorderExits.Count > 0)
						{
							foreach (Vector2Int exit in neighborBorderExits)
							{
								this.CoordinateMap.CreateMatchingExit(matchingBorderOnNeighbor, exit);
							}
						}
						// If neighbor has no exits and exits are to be created, generate them randomly.
						else if (createExits)
						{
							this.CoordinateMap.GenerateRandomExitOnBorder(borderInThisRegion);
						}
					}
				}

				// Clean up inactive corners once after all border processing is done.
				CoordinateMap.SetInactiveCornersToType(Coordinate.TYPE.BORDER);
				await Task.CompletedTask;
			}
		}

		/// <summary>
		/// Destroys the region builder.
		/// </summary>
		public void Destroy()
		{
			DestroyGameObject(this.gameObject);
		}

		// == MESH GENERATION ============================================== >>>>
		/// <summary>
		/// Creates a GameObject with the given name, mesh, and material.
		/// </summary>
		/// <param name="name">The name of the GameObject.</param>
		/// <param name="mesh">The mesh for the GameObject.</param>
		/// <param name="material">The material for the GameObject.</param>
		/// <returns>The created GameObject.</returns>
		public GameObject CreateMeshObject(string name, Mesh mesh, Material material = null)
		{
			if (GenerationParent && GenerationParent.defaultMaterial != null)
			{
				Debug.LogWarning($"{name} Material is null -> setMaterial to GenerationParent default");
				defaultMaterial = material;
			}
			else if (material == null) { Debug.LogWarning($"{name} Material is null"); }
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

			_regionObjects.Add(worldObject);

			return worldObject;
		}

		/// <summary>
		/// Asynchronously creates a combined chunk mesh and initializes the mesh object.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task CombineChunkMesh()
		{
			Mesh combinedMesh = CombineChunks();
			while (combinedMesh == null)
			{
				combinedMesh = CombineChunks();
			}

			// Check if combinedMesh creation was successful
			if (combinedMesh != null)
			{
				try
				{
					// Proceed with creating the GameObject based on the combined mesh
					_combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, WorldBuilder.Instance.defaultMaterial);

					if (_combinedMeshObject != null)
					{
						// Additional setup for the combined mesh object as needed
						// Debug.Log($"Successfully created combined mesh object: {_combinedMeshObject.name}");
						_combinedMeshObject.transform.parent = this.transform;
						MeshCollider collider = _combinedMeshObject.AddComponent<MeshCollider>();
						collider.sharedMesh = combinedMesh;
					}
					else
					{
						Debug.LogError("Failed to create combined mesh object.");
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"Error while setting up the combined mesh object: {ex.Message}");
				}
			}
			else
			{
				Debug.LogError("CombinedMesh is null after combining chunks.");
			}

			await Task.CompletedTask;
		}

		/// <summary>
		/// Combines multiple Mesh objects into a single mesh in an async-compatible manner.
		/// This method is intended to be run on a background thread to avoid blocking the main thread.
		/// </summary>
		/// <returns>A single combined Mesh object.</returns>
		private Mesh CombineChunks()
		{
			List<Mesh> meshes = new List<Mesh>();
			foreach (ChunkData chunk in _chunkBuilder.AllChunks)
			{
				if (chunk?.ChunkMesh?.Mesh != null)
				{
					meshes.Add(chunk.ChunkMesh.Mesh);
				}
				else
				{
					Debug.LogWarning("Invalid chunk or mesh found while combining chunks.");
				}
			}

			if (meshes.Count == 0) return null;

			int totalVertexCount = meshes.Sum(m => m.vertexCount);
			List<Vector3> newVertices = new List<Vector3>(totalVertexCount);
			List<int> newTriangles = new List<int>(totalVertexCount); // Note: This might need adjusting based on your meshes
			List<Vector2> newUVs = new List<Vector2>(totalVertexCount);

			int vertexOffset = 0;

			foreach (Mesh mesh in meshes)
			{
				newVertices.AddRange(mesh.vertices);
				newUVs.AddRange(mesh.uv);

				foreach (int triangle in mesh.triangles)
				{
					newTriangles.Add(triangle + vertexOffset);
				}

				vertexOffset += mesh.vertexCount;
			}

			Mesh combinedMesh = new Mesh();
			if (totalVertexCount > 65535)
			{
				combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support for more than 65535 vertices
			}
			combinedMesh.vertices = newVertices.ToArray();
			combinedMesh.triangles = newTriangles.ToArray();
			combinedMesh.uv = newUVs.ToArray();

			combinedMesh.RecalculateBounds();
			combinedMesh.RecalculateNormals();

			return combinedMesh;
		}

		/// <summary> Destroy GameObject in Play andEdit mode </summary>
		public static void DestroyGameObject(GameObject gameObject)
		{
			// Check if we are running in the Unity Editor
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
			{
				// Use DestroyImmediate if in edit mode and not playing
				DestroyImmediate(gameObject);
				return;
			}
			else
#endif
			{
				// Use Destroy in play mode or in a build
				Destroy(gameObject);
			}
		}
	}
}