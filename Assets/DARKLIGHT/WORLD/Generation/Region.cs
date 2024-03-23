using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darklight.Bot;
using Darklight.World.Map;
using UnityEngine;
using Darklight.World.Settings;
using Darklight.World.Builder;
using Darklight.UnityExt;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Generation
{
	using WorldGenSystem = WorldGenerationSystem;

	public class Region : IGridMapData<Region>
	{
		public bool Initialized { get; private set; }
		public static string Prefix => "[ REGION ]";
		public GridMap2D<Region> ParentGridMap { get; set; }
		public Vector2Int PositionKey { get; set; }
		public GridMap2D.Coordinate CoordinateValue { get; set; }
		public GridMap2D<Chunk> ChunkGridMap { get; set; }
		public Vector3 CenterPosition => CoordinateValue.GetPositionInScene();
		public Vector3 OriginPosition
		{
			get
			{
				GenerationSettings generationSettings = WorldGenSystem.Instance.Settings;
				return CenterPosition - (new Vector3(0.5f, 0, 0.5f) * generationSettings.RegionWidth_inGameUnits) + (new Vector3(0.5f, 0, 0.5f) * generationSettings.ChunkWidth_inGameUnits);
			}
		}
		public Region() { }
		public Region(GridMap2D<Region> parent, Vector2Int positionKey)
		{
			Initialize(parent, positionKey);
		}
		public Task Initialize(GridMap2D<Region> gridParent, Vector2Int positionKey)
		{
			this.ParentGridMap = gridParent;
			this.PositionKey = positionKey;
			this.CoordinateValue = ParentGridMap.GetCoordinateAt(positionKey);
			Initialized = true;
			return Task.CompletedTask;
		}


	}


	// ======= MonoBehaviour Operator ======================================== //
	public class RegionMonoOperator : MonoBehaviour, IUnityEditorListener
	{
		public WorldGenSystem WorldGen => WorldGenSystem.Instance;
		public Region Region { get; private set; }
		public ChunkBuilder ChunkBuilder { get; set; }

		public void OnEditorReloaded()
		{
			WorldGenSystem.DestroyInEditorContext(this.gameObject);
		}

		public async Task Initialize(Region region, GenerationSettings generationSettings)
		{
			Region = region;

			// Construct CHUNK GridMap2D  
			Region.ChunkGridMap = new GridMap2D<Chunk>(transform, region.OriginPosition, generationSettings, UnitSpace.REGION, UnitSpace.CHUNK);
			Region.ChunkGridMap.Initialize();

			// Create Chunk Builder Object as child 
			GameObject chunkBuilderObject = await WorldGenSystem.Instance.CreateGameObjectAt($"ChunkBuilder_{region.PositionKey}", Region.CoordinateValue);
			chunkBuilderObject.transform.parent = transform;

			ChunkBuilder = chunkBuilderObject.AddComponent<ChunkBuilder>();
			ChunkBuilder.Initialize(region);

			// Create Temp Mesh
			CreateTempMesh();
			await Task.CompletedTask;
		}

		public void CreateTempMesh()
		{
			if (Region != null)
			{
				int regionWidth = WorldGen.Settings.RegionWidth_inGameUnits;
				int chunkWidth = WorldGen.Settings.ChunkWidth_inGameUnits;
				int chunkDepth = WorldGen.Settings.ChunkDepth_inGameUnits;

				GameObject tempMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
				tempMesh.name = $"TempRegionMesh";
				tempMesh.transform.parent = this.transform;
				tempMesh.transform.position = Region.CenterPosition;
				tempMesh.transform.localScale = new Vector3(1, 0, 1) * regionWidth + (Vector3.up * chunkDepth);
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(RegionMonoOperator))]
	public class RegionMonoOperatorEditor : Editor
	{
		private GridMap2DEditor.View view = GridMap2DEditor.View.COORD_FLAG;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			RegionMonoOperator regionOperator = (RegionMonoOperator)target;
			Region region = regionOperator.Region;
		}

		public void OnSceneGUI()
		{
			RegionMonoOperator regionOperator = (RegionMonoOperator)target;
			Region region = regionOperator.Region;
			GridMap2D<Chunk> chunkMap = region.ChunkGridMap;
			GridMap2DEditor.DrawGridMap2D_SceneGUI(chunkMap, view, (GridMap2D.Coordinate coordinate) => { });

			// Draw World Outline
			CustomGizmos.DrawWireSquare(region.ParentGridMap.CenterPosition, WorldGenSystem.Instance.Settings.WorldWidth_inGameUnits, Color.grey, Vector3.up);
		}
	}



#endif




	/*
			/// <summary>
			/// Generates necessary exits for the region.
			/// </summary>
			/// <param name="createExits">Indicates whether to create exits if they don't exist.</param>
			async Task GenerateExits(bool createExits)
			{
				Dictionary<EdgeDirection, Vector2Int> neighborDirectionMap = GridMap2D.GetEdgeDirectionMap(PositionKey);

				// Iterate directly over the keys of the map
				foreach (EdgeDirection edgeDirection in neighborDirectionMap.Keys)
				{
					// << BASE CASE >> 
					Vector2Int neighborPositionKey = neighborDirectionMap[edgeDirection];
					GridMap2D.Coordinate neighborCoordinate = GridMapParent.GetCoordinateAt(neighborPositionKey);
					if (neighborCoordinate == null)
					{
						GridMapParent.CloseMapBorder(edgeDirection);
						continue;
					}

					#TODO : Create GridMap2D on Regions
					// >> get region neighbor
					Region neighborRegion = GridMapParent.GetDataAt(neighborPositionKey);
					// >> get matching border direction
					EdgeDirection matchingBorderOnNeighbor = (EdgeDirection)GridMap2D.GetOppositeEdge(edgeDirection);
					// >> get exits on neighbor region
					List<Vector2Int> neighborBorderExits =  

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

				// Clean up inactive corners once after all border processing is done.
				CoordinateMap.SetInactiveCornersToType(Coordinate.TYPE.BORDER);
				await Task.CompletedTask;
			}
			*/

	/*



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
				foreach (Chunk chunk in _chunkBuilder.AllChunks)
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
			*/




}

