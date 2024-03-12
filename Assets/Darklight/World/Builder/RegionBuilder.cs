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



	[RequireComponent(typeof(ChunkGeneration))]
	public class RegionBuilder : TaskQueen
	{
		#region [[ PRIVATE VARIABLES ]] 
		string _prefix = "[[ REGION BUILDER ]]";
		WorldBuilder _generationParent;
		Coordinate _coordinate;
		CoordinateMap _coordinateMap;
		ChunkGeneration _chunkGeneration;
		GameObject _combinedMeshObject;
		HashSet<GameObject> _regionObjects = new();
		#endregion
		#region [[ GETTERS ]]
		/// <summary>
		/// Gets a value indicating whether the region builder has been initialized.
		/// </summary>
		public bool Initialized { get; private set; }

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
		public ChunkGeneration ChunkGeneration => GetComponent<ChunkGeneration>();

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
				origin -= WorldBuilder.Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				origin += WorldBuilder.Settings.ChunkWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
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
		#region [[ RANDOM SEED ]] -------------------------------------- >> 

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
		/// Assigns the region to the world.
		/// </summary>
		/// <param name="parent">The parent WorldBuilder.</param>
		/// <param name="coordinate">The coordinate of the region.</param>
		/// <param name="taskQueenName">The name of the task queen.</param>
		public void AssignToWorld(WorldBuilder parent, Coordinate coordinate, string taskQueenName = "Region Task Queen")
		{
			this._generationParent = parent;
			this._coordinate = coordinate;



			Debug.Log($"Region {Coordinate.ValueKey} created at {coordinate.ScenePosition}");


			if (parent.customWorldGenSettings != null)
			{
				OverrideSettings(parent.customWorldGenSettings);
			}

			Console.Log(this, "Assigned to World Builder with settings " + $"{_regionSettings.Seed}");
		}

		public void Start()
		{
			Debug.Log("Start");
			if (initializeOnStart == true)
			{
				Debug.Log($"{_prefix} Initialize On Start");
				this.Initialize();
			}
		}

		public override void Initialize(string name = "RegionAsyncTaskQueen")
		{
			if (Initialized) return;
			Initialized = true;
			base.Initialize(name);

			_ = InitializationSequence();
		}

		public void Reset()
		{
			Initialized = false;
			_coordinateMap = null;

			foreach (GameObject gameObject in _regionObjects)
			{
				DestroyGameObject(gameObject);
			}
		}

		/// <summary>
		/// The initialization sequence for the region builder.
		/// </summary>
		/// <returns>The task representing the initialization sequence.</returns>
		async Task InitializationSequence()
		{
			// Create the coordinate map
			Enqueue(new TaskBot("Initialize Coordinate Map", async () =>
			{
				Debug.Log("Initializing Coordinate Map");

				this._coordinateMap = new CoordinateMap(this.GenerationParent);
				while (this._coordinateMap.Initialized == false)
				{
					await Awaitable.WaitForSecondsAsync(1);
				}
			}));

			// Create the chunk map for the region
			TaskBot task2 = new TaskBot("Initialize Chunk Generation", async () =>
			{
				this._chunkGeneration = GetComponent<ChunkGeneration>();

				while (_coordinateMap == null || !_coordinateMap.Initialized)
				{
					await Awaitable.WaitForSecondsAsync(1);
				}

				// Conditionally initialize chunk generation based on the WorldBuilder instance
				_chunkGeneration.Initialize(this, _coordinateMap);
				await _chunkGeneration.GenerationSequence();
			});
			Enqueue(task2);

			if (WorldBuilder.Instance != null)
			{
				// Combine the chunk mesh if WorldBuilder exists
				TaskBot task3 = new TaskBot("Mesh Generation", async () =>
				{
					Console.Log(this, "Starting mesh generation...");

					while (_chunkGeneration == null || _chunkGeneration.Initialized == false)
					{
						await Awaitable.WaitForSecondsAsync(1);
					}

					// Asynchronously create and initialize combined chunk mesh
					await CreateAndInitializeCombinedChunkMeshAsync();
					Console.Log(this, "Mesh generation complete.");
				});
				Enqueue(task3);

			}

			await Awaitable.WaitForSecondsAsync(1);

			ExecuteAllTasks();
			Debug.Log("Region Builder Initialized");


			// Execute all tasks queued in AsyncTaskQueen
			Initialized = true;
		}

		/// <summary>
		/// Generates necessary exits for the region.
		/// </summary>
		/// <param name="createExits">Indicates whether to create exits if they don't exist.</param>
		public void GenerateNecessaryExits(bool createExits)
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
			if (mesh == null) { Debug.LogError("Mesh is null"); }

			GameObject worldObject = new GameObject(name);
			worldObject.transform.parent = transform;
			worldObject.transform.localPosition = Vector3.zero;

			MeshFilter meshFilter = worldObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();

			if (material == null)
			{
				Debug.LogError("Mesh material is null -> setMaterial to default");
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
		public async Task CreateAndInitializeCombinedChunkMeshAsync()
		{
			Mesh combinedMesh = CombineChunks();
			while (combinedMesh == null)
			{
				combinedMesh = CombineChunks();
				await Task.Delay(1000);
			}

			// Check if combinedMesh creation was successful
			if (combinedMesh != null)
			{
				try
				{
					// Proceed with creating the GameObject based on the combined mesh
					_combinedMeshObject = CreateMeshObject($"CombinedChunkMesh", combinedMesh, null);

					if (_combinedMeshObject != null)
					{
						// Additional setup for the combined mesh object as needed
						Debug.Log($"Successfully created combined mesh object: {_combinedMeshObject.name}");
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
		}

		/// <summary>
		/// Combines multiple Mesh objects into a single mesh in an async-compatible manner.
		/// This method is intended to be run on a background thread to avoid blocking the main thread.
		/// </summary>
		/// <returns>A single combined Mesh object.</returns>
		private Mesh CombineChunks()
		{
			List<Mesh> meshes = new List<Mesh>();
			foreach (Chunk chunk in _chunkGeneration.AllChunks)
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