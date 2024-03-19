using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Darklight.World.Generation
{
	using FaceType = Chunk.FaceType;
	using Builder;
	using System.Linq;

	public class MeshQuad
	{
		public Chunk chunkParent;
		public FaceType faceType;
		public Vector2Int faceCoord;
		public Vector3 faceNormal;
		public List<Vector3> vertices = new();
		public List<int> triangles = new();
		public List<Vector2> uvs = new();

		public MeshQuad(Chunk parent, FaceType faceType, Vector2Int faceCoord, Vector3 faceNormal, List<Vector3> vertices, List<Vector2> uvs)
		{
			this.chunkParent = parent;
			this.faceType = faceType;
			this.faceCoord = faceCoord;
			this.faceNormal = faceNormal;

			for (int i = 0; i < vertices.Count; i++)
			{
				vertices[i] += parent.GroundPosition;
			}
			this.vertices = vertices;

			/*
			List<Vector2> quadUVs = new List<Vector2>
			{

				new Vector2(1, 0), // bottomRight
				new Vector2(1, 1), // topRight
				new Vector2(0, 1),  // topLeft
				new Vector2(0, 0), // bottomLeft
			};
			*/
			this.uvs = uvs;

			triangles = new List<int> { 0, 1, 2, 2, 3, 0 };
			//triangles = new List<int> { 0, 2, 1, 0, 3, 2 };
		}

		public Vector3 GetCenterPosition()
		{
			return (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
		}

		public void AdjustHeight(float heightAdjustment)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 adjustedVertex = new Vector3(vertices[i].x, vertices[i].y + heightAdjustment, vertices[i].z);
				vertices[i] = adjustedVertex;
			}
		}

		public Mesh GenerateMesh()
		{
			Mesh mesh = new Mesh
			{
				vertices = vertices.ToArray(),
				uv = uvs.ToArray(),
				triangles = triangles.ToArray()
			};
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}
	}

	public class ChunkMesh
	{
		Vector3Int _defaultDimensions;
		Vector3Int _currentDimensions;
		Chunk _chunkParent;
		int _groundHeight;
		Vector3 _positionInScene;
		Mesh _mesh;
		// Change the storage to a nested dictionary for easier access by faceType and coordinates
		Dictionary<FaceType, Dictionary<Vector2Int, MeshQuad>> _meshData = new();
		List<FaceType> _visibleFaces = new List<FaceType>()
		{
			FaceType.FRONT , FaceType.BACK ,
			FaceType.LEFT, FaceType.RIGHT,
			FaceType.TOP, FaceType.BOTTOM
		};

		public Mesh Mesh => _mesh;

		public ChunkMesh(Chunk chunkParent)
		{
			this._chunkParent = chunkParent;
			this._groundHeight = chunkParent.GroundHeight;
			this._positionInScene = chunkParent.GroundPosition;
			GenerateMeshData();
		}

		public Mesh Recalculate(Chunk chunkParent)
		{
			this._chunkParent = chunkParent;
			this._groundHeight = chunkParent.GroundHeight;
			this._positionInScene = chunkParent.GroundPosition;

			GenerateMeshData();
			return this._mesh;
		}

		public Mesh CreateMeshFromGeneratedData()
		{
			List<Vector3> allVertices = new List<Vector3>();
			List<int> allTriangles = new List<int>();
			List<Vector2> allUVs = new List<Vector2>();

			// This offset is necessary because each quad has its own set of vertices starting from 0,
			// but when combined into one mesh, we need to adjust the triangle indices accordingly.
			int vertexOffset = 0;

			foreach (var faceType in _meshData.Keys)
			{
				foreach (var quad in _meshData[faceType].Values)
				{
					// Add the current quad's vertices and UVs directly to the lists
					allVertices.AddRange(quad.vertices);
					allUVs.AddRange(quad.uvs);

					// Adjust the triangle indices for the current offset and add them to the list
					foreach (var tri in quad.triangles)
					{
						allTriangles.Add(tri + vertexOffset);
					}

					// Update the vertexOffset for the next quad
					vertexOffset += quad.vertices.Count;
				}
			}

			// Now that we have all the data consolidated, let's create the mesh
			Mesh fullChunkMesh = new Mesh
			{
				vertices = allVertices.ToArray(),
				triangles = allTriangles.ToArray(),
				uv = allUVs.ToArray()
			};

			// Optional: Optimize the mesh for better performance
			fullChunkMesh.Optimize();

			// Recalculate normals for correct lighting
			fullChunkMesh.RecalculateNormals();
			fullChunkMesh.RecalculateBounds();

			// Finally, assign this newly created mesh to the _mesh field
			_mesh = fullChunkMesh;
			return _mesh;
			// If you have a MeshFilter component attached to your chunk GameObject, you can update it like this:
			// this.GetComponent<MeshFilter>().mesh = _mesh;
		}

		void GenerateMeshData()
		{
			int groundHeight = _groundHeight;
			List<FaceType> facesToGenerate = _visibleFaces;
			int cellSize = WorldBuilder.Settings.CellSize_inGameUnits;
			Mesh newMesh = new Mesh();
			List<Vector3> vertices = new();
			List<Vector2> uvs = new();
			List<int> triangles = new();
			int currentVertexIndex = 0;
			int quadIndex = 0; // Keep track of the current quad index for UV mapping

			// << GET SETTINGS >>
			if (WorldBuilder.Settings != null)
			{
				cellSize = WorldBuilder.Settings.CellSize_inGameUnits;
				_defaultDimensions = WorldBuilder.Settings.ChunkVec3Dimensions_inCellUnits;
			}
			else if (RegionBuilder.Settings != null)
			{
				cellSize = RegionBuilder.Settings.CellSize_inGameUnits;
				_defaultDimensions = RegionBuilder.Settings.ChunkVec3Dimensions_inCellUnits;
			}
			else
			{
				Debug.LogError("Settings not found");
			}

			// << UPDATE DIMENSIONS >>
			_currentDimensions = _defaultDimensions + (Vector3Int.up * groundHeight); // Add ground height to default dimensions

			// << CREATE MESH FACES >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
			// Updates meshVertices dictionary with < FaceType , List<Vector3> vertices >
			// 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid
			foreach (FaceType faceType in facesToGenerate)
			{
				(Vector3 u, Vector3 v) = GetFaceUVDirectionalVectors(faceType);
				(int uDivisions, int vDivisions) = GetFaceUVDivisions(faceType);
				Vector3 startVertex = GetFaceStartVertex(faceType, vDivisions);
				_meshData[faceType] = new Dictionary<Vector2Int, MeshQuad>(); // << RESET MESH DATA

				for (int i = 0; i < uDivisions; i++)
				{
					for (int j = 0; j < vDivisions; j++)
					{
						// Calculate vertices for the current quad
						Vector3 bottomLeft = startVertex + (i * cellSize * u) + (j * cellSize * v);
						Vector3 bottomRight = bottomLeft + (cellSize * v);
						Vector3 topLeft = bottomLeft + (cellSize * u);
						Vector3 topRight = topLeft + (cellSize * v);

						// Adjust the order here if necessary to ensure correct winding
						//vertices.AddRange(new Vector3[] { bottomLeft, bottomRight, topRight, topLeft });
						vertices.AddRange(new Vector3[] { bottomLeft, topLeft, topRight, bottomRight });

						// Set UVs
						List<Vector2> quadUVs = new List<Vector2>
						{
							new Vector2(1, 0), // bottomRight
							new Vector2(1, 1), // topRight
							new Vector2(0, 1),  // topLeft
							new Vector2(0, 0), // bottomLeft
						};
						uvs.AddRange(quadUVs);

						// Create and store the MeshQuad
						Vector2Int faceCoordinate = new Vector2Int(i, j);
						List<Vector3> quadVertices = new List<Vector3> { bottomLeft, bottomRight, topRight, topLeft };


						// Add triangles for the current quad
						int baseIndex = quadIndex * 4;
						List<int> quadTriangles = new List<int> { baseIndex, baseIndex + 2, baseIndex + 1, baseIndex, baseIndex + 3, baseIndex + 2 };
						triangles.AddRange(quadTriangles);
						quadIndex++;

						MeshQuad quad = new MeshQuad(_chunkParent, faceType, faceCoordinate, GetFaceNormal(faceType), quadVertices, quadUVs);
						_meshData[faceType][faceCoordinate] = quad; // Store in the nested dictionary
					}
				}
				// UPDATE VERTEX COUNT
				switch (faceType)
				{
					// Side Faces XY plane
					case FaceType.FRONT:
					case FaceType.BACK:
					case FaceType.LEFT:
					case FaceType.RIGHT:
						currentVertexIndex += (_currentDimensions.x + 1) * (vDivisions + 1);
						break;
					// Top Faces XZ plane
					case FaceType.TOP:
					case FaceType.BOTTOM:
						currentVertexIndex += (_currentDimensions.x + 1) * (_currentDimensions.z + 1);
						break;
				}
			}
		}

		(int, int) GetFaceUVDivisions(FaceType faceType)
		{
			// [[ GET VISIBLE DIVISIONS ]]
			// based on neighbors
			int GetVisibleVDivisions(FaceType type)
			{
				int faceHeight = _currentDimensions.y; // Get current height
				Chunk neighborChunk = null;

				switch (type)
				{
					case FaceType.FRONT:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.NORTH];
						break;
					case FaceType.BACK:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.SOUTH];
						break;
					case FaceType.LEFT:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.WEST];
						break;
					case FaceType.RIGHT:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.EAST];
						break;
				}

				if (neighborChunk != null)
				{
					faceHeight -= neighborChunk.GroundHeight; // subtract based on neighbor height
					faceHeight -= _defaultDimensions.y; // subtract 'underground' amount
					faceHeight = Mathf.Max(faceHeight, 0); // set to 0 as minimum
				}

				return faceHeight;
			}

			int uDivisions = 0;
			int vDivisions = 0;
			switch (faceType)
			{
				// Side Faces XY plane
				case FaceType.FRONT:
				case FaceType.BACK:
					uDivisions = _currentDimensions.x;
					vDivisions = GetVisibleVDivisions(faceType);
					break;
				// Side Faces ZY plane
				case FaceType.LEFT:
				case FaceType.RIGHT:
					uDivisions = _currentDimensions.z;
					vDivisions = GetVisibleVDivisions(faceType);
					break;
				// Top Faces XZ plane
				case FaceType.TOP:
				case FaceType.BOTTOM:
					uDivisions = _currentDimensions.x;
					vDivisions = _currentDimensions.z;
					break;

			}

			return (uDivisions, vDivisions);
		}

		// note** :: starts the faces at -visibleDimensions.y so that top of chunk is at y=0
		// -- the chunks will be treated as a 'Generated Ground' to build upon
		Vector3 GetFaceStartVertex(FaceType faceType, int vDivisions)
		{
			Vector3 MultiplyVectors(Vector3 a, Vector3 b)
			{
				return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
			}

			int cellSize = WorldBuilder.Settings.CellSize_inGameUnits;

			// Get starting vertex of visible vDivisions
			Vector3 visibleSideFaceStartVertex = new Vector3(_currentDimensions.x, -vDivisions, _currentDimensions.z) * cellSize;
			Vector3 newSideFaceStartOffset = new Vector3(visibleSideFaceStartVertex.x * 0.5f, visibleSideFaceStartVertex.y, visibleSideFaceStartVertex.z * 0.5f);

			// Use current chunk mesh height for bottom and top faces
			Vector3 verticalFaceStartVertex = new Vector3(_currentDimensions.x, -_currentDimensions.y, _currentDimensions.z) * cellSize;
			Vector3 newVerticalFaceStartOffset = new Vector3(verticalFaceStartVertex.x * 0.5f, verticalFaceStartVertex.y, verticalFaceStartVertex.z * 0.5f);

			switch (faceType)
			{
				case FaceType.FRONT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, 1));
				case FaceType.BACK:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, -1));
				case FaceType.LEFT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, 1));
				case FaceType.RIGHT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, -1));
				case FaceType.TOP:
					return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(-1, 0, -1));
				case FaceType.BOTTOM:
					return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(1, 1, -1));
				default:
					return Vector3.zero;
			}

		}

		Vector3 GetFaceNormal(FaceType faceType)
		{
			switch (faceType)
			{
				case FaceType.FRONT: return Vector3.forward;
				case FaceType.BACK: return Vector3.back;
				case FaceType.LEFT: return Vector3.left;
				case FaceType.RIGHT: return Vector3.right;
				case FaceType.TOP: return Vector3.up;
				case FaceType.BOTTOM: return Vector3.down;
				default: return Vector3.zero;
			}
		}


		(Vector3, Vector3) GetFaceUVDirectionalVectors(FaceType faceType)
		{
			Vector3 u = Vector3.zero;
			Vector3 v = Vector3.zero;

			switch (faceType)
			{
				case FaceType.FRONT:
					u = Vector3.left;
					v = Vector3.up;
					break;
				case FaceType.BACK:
					u = Vector3.right;
					v = Vector3.up;
					break;
				case FaceType.LEFT:
					u = Vector3.back;
					v = Vector3.up;
					break;
				case FaceType.RIGHT:
					u = Vector3.forward;
					v = Vector3.up;
					break;
				case FaceType.TOP:
					u = Vector3.right;
					v = Vector3.forward;
					break;
				case FaceType.BOTTOM:
					u = Vector3.left;
					v = Vector3.forward;
					break;
			}
			return (u, v);
		}


	}
}



