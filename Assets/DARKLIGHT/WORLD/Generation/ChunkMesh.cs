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
		public FaceDirection faceDirection;
		public Vector2Int faceCoord;
		public List<int> verticeIndexes;
		public List<Vector2> uvs;
		public List<int> triangles;
		public Dictionary<int, List<Quad>> extrudedQuads { get; private set; }
		public int baseHeightOffset;
		public Quad(Chunk parent, List<int> verticeIndexes, FaceDirection faceDirection, Vector2Int faceCoord)
		{
			this.chunkParent = parent;
			this.faceDirection = faceDirection;
			this.faceCoord = faceCoord;

			this.verticeIndexes = verticeIndexes;
			this.uvs = new List<Vector2>
			{
				new Vector2(1, 0), // bottomRight
				new Vector2(1, 1), // topRight
				new Vector2(0, 1),  // topLeft
				new Vector2(0, 0), // bottomLeft
			};

			this.triangles = new List<int> { 0, 1, 2, 2, 3, 0 };

			extrudedQuads = new Dictionary<int, List<Quad>>();
			baseHeightOffset = 0;
		}

		/// <summary>
		/// Adds a list of quads that have been extruded to a certain height relative to this quad.
		/// </summary>
		/// <param name="quads">The quads extruded from this one.</param>
		public void AddExtrudedQuads(List<Quad> quads)
		{
			this.baseHeightOffset = this.baseHeightOffset + 1; // Add to base height
			extrudedQuads.Add(this.baseHeightOffset, new List<Quad>(quads));
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



		public List<Vector3> GetGlobalVertices(List<int> verticeIndexes, List<Vector3> globalVertices)
		{
			List<Vector3> positions = new List<Vector3>();
			foreach (int index in verticeIndexes)
			{
				positions.Add(globalVertices[index]);
			}
			return positions;
		}


		public Mesh CreateMeshFromGeneratedData()
		{
			List<Vector3> allVertices = new List<Vector3>();
			List<int> allTriangles = new List<int>();
			List<Vector2> allUVs = new List<Vector2>();
			int currentIndex = 0;

			List<Quad> allQuads = new List<Quad>();


			// This offset is necessary because each quad has its own set of vertices starting from 0,
			// but when combined into one mesh, we need to adjust the triangle indices accordingly.
			foreach (FaceDirection faceDir in _quadData.Keys)
			{
				foreach (Quad quad in _quadData[faceDir].Values)
				{
					allQuads.Add(quad);
					if (quad.extrudedQuads.Count > 0)
					{
						allQuads.AddRange(quad.extrudedQuads.Values.SelectMany(quads => quads));
					}
				}
			}

			// Store Quad Data
			foreach (Quad quad in allQuads)
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

		public void ExtrudeQuad(FaceDirection faceDir, Vector2Int faceCoord)
		{
			// Check if the specified quad exists
			if (!_quadData.ContainsKey(faceDir) || !_quadData[faceDir].ContainsKey(faceCoord))
			{
				Debug.LogError("Quad does not exist.");
				return;
			}

			Quad baseQuad = _quadData[faceDir][faceCoord];
			int newHeight = baseQuad.baseHeightOffset + 1; // Increase height by 1

			// Calculate new top vertices based on the height offset
			List<Vector3> baseQuadVertices = baseQuad.verticeIndexes.Select(index => _globalVertices[index]).ToList();
			List<Vector3> bottomVertices = baseQuadVertices.Select(v => v + Vector3.up * (newHeight - 1)).ToList();
			List<Vector3> topVertices = baseQuadVertices.Select(v => v + Vector3.up * newHeight).ToList();

			// Update global vertices with top vertices and get their indices
			List<int> topVerticeIndexes = new List<int>();
			foreach (Vector3 vertex in topVertices)
			{
				int index = _globalVertices.Count;  // Assume _globalVertices is a List<Vector3>
				_globalVertices.Add(vertex);  // Add new vertex to global list
				topVerticeIndexes.Add(index);  // Store new index
			}

			List<Quad> extrudedQuads = new List<Quad>();

			// Generate the new top quad and side quads
			List<int> newBottomVerticeIndexes = bottomVertices.Select(vertex => _globalVertices.IndexOf(vertex)).ToList();
			List<int> newTopVerticeIndexes = topVertices.Select(vertex => _globalVertices.IndexOf(vertex)).ToList();
			Quad newTopQuad = new Quad(_chunkParent, newTopVerticeIndexes, FaceDirection.TOP, faceCoord); // Adjust faceCoord for top quad
			extrudedQuads.Add(newTopQuad);

			// Generate side quads for each side
			for (int i = 0; i < 4; i++)
			{
				List<int> sideVerticeIndexes = new List<int>
				{
					newBottomVerticeIndexes[i],
					newBottomVerticeIndexes[(i + 1) % 4],
					newTopVerticeIndexes[(i + 1) % 4],
					newTopVerticeIndexes[i]
				};

				// Determine the side face direction based on i and faceDir
				FaceDirection sideFaceDirection = DetermineSideFaceDirection(i, faceDir);

				// Create and store the side quad
				Quad sideQuad = new Quad(_chunkParent, sideVerticeIndexes, sideFaceDirection, faceCoord);
				extrudedQuads.Add(sideQuad);
			}

			baseQuad.AddExtrudedQuads(extrudedQuads);
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

		FaceDirection DetermineSideFaceDirection(int sideIndex, FaceDirection baseFaceDirection)
		{
			// This switch determines the side face direction based on the base face direction and the side index.
			// The side index corresponds to the vertices of the base quad being extruded:
			// 0 -> the side between the first and second vertices, 1 -> between second and third, etc.
			switch (baseFaceDirection)
			{
				case FaceDirection.FRONT:
					// Assuming clockwise order of vertices
					switch (sideIndex)
					{
						case 0: return FaceDirection.LEFT;
						case 1: return FaceDirection.TOP;
						case 2: return FaceDirection.RIGHT;
						case 3: return FaceDirection.BOTTOM;
					}
					break;
				case FaceDirection.BACK:
					switch (sideIndex)
					{
						case 0: return FaceDirection.RIGHT;
						case 1: return FaceDirection.TOP;
						case 2: return FaceDirection.LEFT;
						case 3: return FaceDirection.BOTTOM;
					}
					break;
				case FaceDirection.LEFT:
					switch (sideIndex)
					{
						case 0: return FaceDirection.BACK;
						case 1: return FaceDirection.TOP;
						case 2: return FaceDirection.FRONT;
						case 3: return FaceDirection.BOTTOM;
					}
					break;
				case FaceDirection.RIGHT:
					switch (sideIndex)
					{
						case 0: return FaceDirection.FRONT;
						case 1: return FaceDirection.TOP;
						case 2: return FaceDirection.BACK;
						case 3: return FaceDirection.BOTTOM;
					}
					break;
				case FaceDirection.TOP:
					// For the top face, side quads are vertical faces, so their directions are based on their positions relative to the top
					switch (sideIndex)
					{
						case 0: return FaceDirection.LEFT;
						case 1: return FaceDirection.BACK;
						case 2: return FaceDirection.RIGHT;
						case 3: return FaceDirection.FRONT;
					}
					break;
				case FaceDirection.BOTTOM:
					switch (sideIndex)
					{
						case 0: return FaceDirection.LEFT;
						case 1: return FaceDirection.FRONT;
						case 2: return FaceDirection.RIGHT;
						case 3: return FaceDirection.BACK;
					}
					break;
			}

			Debug.LogError($"Invalid combination of face direction: {baseFaceDirection} and side index: {sideIndex}");
			return baseFaceDirection; // Default return to prevent compile error, logic should be verified to avoid reaching here.
		}

	}
}



