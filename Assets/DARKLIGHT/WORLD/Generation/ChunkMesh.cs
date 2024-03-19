using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Darklight.World.Generation
{
	using FaceDirection = Chunk.FaceDirection;
	using Builder;
	using System.Linq;

	/// <summary>
	/// This Quad manipulates and stores the chunk mesh values
	/// </summary> 
	public class Quad
	{
		public Chunk chunkParent;
		public int startVertexIndex;
		public FaceDirection faceDirection;
		public Vector2Int faceCoord;
		public List<int> verticeIndexes = new List<int>(4);
		public List<Vector2> uvs = new List<Vector2>
		{
			new Vector2(1, 0), // bottomRight
			new Vector2(1, 1), // topRight
			new Vector2(0, 1),  // topLeft
			new Vector2(0, 0), // bottomLeft
		};
		public List<int> triangles = new List<int> { 0, 1, 2, 2, 3, 0 };
		//triangles = new List<int> { 0, 2, 1, 0, 3, 2 };

		public Quad(Chunk parent, List<int> vertices, FaceDirection faceDir, Vector2Int faceCoord)
		{
			this.chunkParent = parent;
			this.verticeIndexes = vertices;
			this.faceDirection = faceDir;
			this.faceCoord = faceCoord;

			if (vertices.Count != 4)
			{
				Debug.LogError("Quad must have 4 vertices");
			}

		}

		/*
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
				*/
	}

	public class ChunkMesh
	{
		Vector3Int _defaultDimensions;
		Vector3Int _currentDimensions;
		Chunk _chunkParent;
		int _groundHeight;
		Vector3 _positionInScene;
		Mesh _mesh;


		List<Vector3> _globalVertices = new(); // global vertices dictionary 
		Dictionary<FaceDirection, Dictionary<Vector2Int, Quad>> _quadData = new();
		List<FaceDirection> _visibleFaces = new List<FaceDirection>()
		{
			FaceDirection.FRONT , FaceDirection.BACK ,
			FaceDirection.LEFT, FaceDirection.RIGHT,
			FaceDirection.TOP, FaceDirection.BOTTOM
		};

		public Mesh Mesh => _mesh;

		public ChunkMesh(Chunk chunkParent)
		{
			this._chunkParent = chunkParent;
			this._groundHeight = chunkParent.GroundHeight;
			this._positionInScene = chunkParent.GroundPosition;
			GenerateMeshData();
			this._mesh = CreateMeshFromGeneratedData();
		}

		public Mesh Recalculate()
		{
			this._mesh = CreateMeshFromGeneratedData();
			return this._mesh;
		}

		public void AdjustQuadByHeight(FaceDirection faceType, Vector2Int faceCoord, float heightAdjustment)
		{
			//_quadData[faceType][faceCoord].AdjustHeight(heightAdjustment);
		}

		public Mesh CreateMeshFromGeneratedData()
		{
			List<Vector3> allVertices = new List<Vector3>();
			List<int> allTriangles = new List<int>();
			List<Vector2> allUVs = new List<Vector2>();
			int currentIndex = 0;

			// This offset is necessary because each quad has its own set of vertices starting from 0,
			// but when combined into one mesh, we need to adjust the triangle indices accordingly.
			foreach (FaceDirection faceDir in _quadData.Keys)
			{
				foreach (Quad quad in _quadData[faceDir].Values)
				{
					// Get the stored global vertex for each index
					foreach (int index in quad.verticeIndexes)
					{
						// get the global vertice at index
						Vector3 vertice = _globalVertices[index];
						allVertices.Add(vertice);
					}

					// Set Triangles for each index
					foreach (int triangle in quad.triangles)
					{
						allTriangles.Add(triangle + currentIndex);
					}

					// Store all UVs
					allUVs.AddRange(quad.uvs);

					currentIndex += 4;
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
			List<FaceDirection> facesToGenerate = _visibleFaces;
			int cellSize = WorldBuilder.Settings.CellSize_inGameUnits;
			int currentVertexIndex = 0;

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
			foreach (FaceDirection faceDir in facesToGenerate)
			{
				(Vector3 u, Vector3 v) = GetFaceUVDirectionalVectors(faceDir);
				(int uDivisions, int vDivisions) = GetFaceUVDivisions(faceDir);
				Vector3 startVertex = GetFaceStartVertex(faceDir, vDivisions);
				_quadData[faceDir] = new Dictionary<Vector2Int, Quad>(); // << RESET MESH DATA

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
						List<Vector3> quadVertices = new List<Vector3> { bottomLeft, bottomRight, topRight, topLeft };
						List<int> verticeIndexes = new List<int>();
						for (int q = 0; q < quadVertices.Count; q++)
						{
							quadVertices[q] += _positionInScene; // Apply offset

							// Create new Vertex
							if (!_globalVertices.Contains(quadVertices[q]))
							{
								_globalVertices.Add(quadVertices[q]); // Store in global list
								verticeIndexes.Add(currentVertexIndex);
								currentVertexIndex++; // Update Vertex Count
							}
							// Link the Quad to an old Vertex
							else
							{
								verticeIndexes.Add(_globalVertices.IndexOf(quadVertices[q]));
							}
						}

						// Create and store the MeshQuad
						Vector2Int faceCoordinate = new Vector2Int(i, j);

						Quad quad = new Quad(_chunkParent, verticeIndexes, faceDir, faceCoordinate);
						_quadData[faceDir][faceCoordinate] = quad; // Store in the nested dictionary
					}
				}
			}
		}

		(int, int) GetFaceUVDivisions(FaceDirection faceType)
		{
			// [[ GET VISIBLE DIVISIONS ]]
			// based on neighbors
			int GetVisibleVDivisions(FaceDirection type)
			{
				int faceHeight = _currentDimensions.y; // Get current height
				Chunk neighborChunk = null;

				switch (type)
				{
					case FaceDirection.FRONT:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.NORTH];
						break;
					case FaceDirection.BACK:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.SOUTH];
						break;
					case FaceDirection.LEFT:
						neighborChunk = _chunkParent.GetNaturalNeighborMap()[WorldDirection.WEST];
						break;
					case FaceDirection.RIGHT:
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
				case FaceDirection.FRONT:
				case FaceDirection.BACK:
					uDivisions = _currentDimensions.x;
					vDivisions = GetVisibleVDivisions(faceType);
					break;
				// Side Faces ZY plane
				case FaceDirection.LEFT:
				case FaceDirection.RIGHT:
					uDivisions = _currentDimensions.z;
					vDivisions = GetVisibleVDivisions(faceType);
					break;
				// Top Faces XZ plane
				case FaceDirection.TOP:
				case FaceDirection.BOTTOM:
					uDivisions = _currentDimensions.x;
					vDivisions = _currentDimensions.z;
					break;

			}

			return (uDivisions, vDivisions);
		}

		// note** :: starts the faces at -visibleDimensions.y so that top of chunk is at y=0
		// -- the chunks will be treated as a 'Generated Ground' to build upon
		Vector3 GetFaceStartVertex(FaceDirection faceType, int vDivisions)
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
				case FaceDirection.FRONT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, 1));
				case FaceDirection.BACK:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, -1));
				case FaceDirection.LEFT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, 1));
				case FaceDirection.RIGHT:
					return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, -1));
				case FaceDirection.TOP:
					return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(-1, 0, -1));
				case FaceDirection.BOTTOM:
					return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(1, 1, -1));
				default:
					return Vector3.zero;
			}

		}

		Vector3 GetFaceNormal(FaceDirection faceType)
		{
			switch (faceType)
			{
				case FaceDirection.FRONT: return Vector3.forward;
				case FaceDirection.BACK: return Vector3.back;
				case FaceDirection.LEFT: return Vector3.left;
				case FaceDirection.RIGHT: return Vector3.right;
				case FaceDirection.TOP: return Vector3.up;
				case FaceDirection.BOTTOM: return Vector3.down;
				default: return Vector3.zero;
			}
		}


		(Vector3, Vector3) GetFaceUVDirectionalVectors(FaceDirection faceType)
		{
			Vector3 u = Vector3.zero;
			Vector3 v = Vector3.zero;

			switch (faceType)
			{
				case FaceDirection.FRONT:
					u = Vector3.left;
					v = Vector3.up;
					break;
				case FaceDirection.BACK:
					u = Vector3.right;
					v = Vector3.up;
					break;
				case FaceDirection.LEFT:
					u = Vector3.back;
					v = Vector3.up;
					break;
				case FaceDirection.RIGHT:
					u = Vector3.forward;
					v = Vector3.up;
					break;
				case FaceDirection.TOP:
					u = Vector3.right;
					v = Vector3.forward;
					break;
				case FaceDirection.BOTTOM:
					u = Vector3.left;
					v = Vector3.forward;
					break;
			}
			return (u, v);
		}


	}
}



