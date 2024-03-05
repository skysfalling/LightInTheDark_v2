using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    using WorldGen = Generation;
    using Face = Chunk.Face;

    [System.Serializable]
    public class MeshQuad
    {
        public Face faceType;
        public Vector2Int faceCoord;
        public Vector3 faceNormal;
        List<Vector3> vertices = new();

        public MeshQuad(Face faceType, Vector2Int faceCoord, Vector3 faceNormal, List<Vector3> vertices)
        {
            this.faceType = faceType;
            this.faceCoord = faceCoord;
            this.faceNormal = faceNormal;
            this.vertices = vertices;
        }

        public Vector3 GetCenterPosition()
        {
            return (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;
        }
    }

    [System.Serializable]
    public class ChunkMesh
    {
        Vector3Int default_chunkMeshDimensions = WorldGen.Settings.ChunkVec3Dimensions_inCellUnits;
        Vector3Int current_chunkMeshDimensions;
        Chunk _chunk;
        Coordinate _worldCoordinate;
        ChunkMap _worldChunkMap;
        Dictionary<Face, List<Vector3>> _meshVertices = new();
        Dictionary<Face, List<Vector2>> _meshUVs = new();
        public List<MeshQuad> meshQuads = new();

        public Mesh mesh;

        public ChunkMesh(Chunk chunk, int groundHeight, Vector3 groundPosition)
        {
            this._chunk = chunk;
            this._worldCoordinate = chunk.Coordinate;
            this._worldChunkMap = chunk.ChunkMapParent;

            List<Face> facesToGenerate = new List<Face>()
        {
            Face.Front , Face.Back ,
            Face.Left, Face.Right,
            Face.Top, Face.Bottom
        };

            this.mesh = CreateMesh(groundHeight, facesToGenerate);
            OffsetMesh(groundPosition);
        }

        Mesh CreateMesh(int groundHeight, List<Face> facesToGenerate)
        {
            int cellSize = WorldGen.Settings.CellSize_inGameUnits;
            Mesh newMesh = new Mesh();
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();


            // << UPDATE DIMENSIONS >>
            current_chunkMeshDimensions = default_chunkMeshDimensions + (Vector3Int.up * groundHeight); // Add ground height to default dimensions

            // << CREATE MESH FACES >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // Updates meshVertices dictionary with < FaceType , List<Vector3> vertices >
            // 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid
            void CreateMeshFaces()
            {
                for (int faceIndex = 0; faceIndex < facesToGenerate.Count; faceIndex++)
                {
                    // Determine face plane values
                    Face faceType = facesToGenerate[faceIndex];

                    (Vector3 u, Vector3 v) = GetFaceUVDirectionalVectors(faceType); // Directional Vectors
                    (int uDivisions, int vDivisions) = GetFaceUVDivisions(faceType); // Divisions of Plane
                    Vector3 startVertex = GetFaceStartVertex(faceType, vDivisions); // Start Vertex

                    // Create Vertices & paired UVs
                    List<Vector3> faceVerticesList = new List<Vector3>();
                    List<Vector2> faceUVsList = new List<Vector2>();
                    for (int i = 0; i <= vDivisions; i++)
                    {
                        for (int j = 0; j <= uDivisions; j++)
                        {
                            // Create new Vertex
                            Vector3 newVertex = startVertex + (i * cellSize * v) + (j * cellSize * u);
                            vertices.Add(newVertex);
                            faceVerticesList.Add(newVertex);

                            // Standard UV rectangle for each face
                            float uCoord = 1 - (j / (float)uDivisions); // Flipped horizontally
                            float vCoord = i / (float)vDivisions;
                            Vector2 newUV = new Vector2(uCoord, vCoord);
                            uvs.Add(newUV);
                            faceUVsList.Add(newUV); // UV mapping
                        }
                    }

                    // Update the dictionary with vertices of the current face
                    _meshVertices[faceType] = faceVerticesList;
                    _meshUVs[faceType] = faceUVsList;
                }
            }

            // << CREATE MESH TRIANGLES >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // Updated AddFaceTriangles to include FaceType and to track quads
            void CreateMeshTriangles()
            {
                // Store overall vertex count
                int currentVertexIndex = 0;

                for (int faceIndex = 0; faceIndex < facesToGenerate.Count; faceIndex++)
                {
                    Face faceType = facesToGenerate[faceIndex];

                    // SET U & V DIVISIONS
                    (int uDivisions, int vDivisions) = GetFaceUVDivisions(faceType);

                    /*
                    Debug.Log($"Chunk Mesh {worldCoordinate.Coordinate} : {faceType}" +
                        $"\n\t chunkMeshDimensions {current_chunkMeshDimensions}" +
                        $"\n\t uDivisions {uDivisions} vDivisions {vDivisions}");
                    */

                    // ADD FACE TRIANGLES
                    for (int i = 0; i < vDivisions; i++)
                    {
                        for (int j = 0; j < uDivisions; j++)
                        {
                            // Get rows
                            int rowStart = currentVertexIndex + i * (uDivisions + 1);
                            int nextRowStart = currentVertexIndex + (i + 1) * (uDivisions + 1);

                            // Get vertices
                            int bottomLeft = rowStart + j;
                            int bottomRight = bottomLeft + 1;
                            int topLeft = nextRowStart + j;
                            int topRight = topLeft + 1;

                            // Track the quad
                            Vector2Int faceCoordinate = new Vector2Int(i, j);
                            List<Vector3> quadVertices = new();
                            try
                            {
                                quadVertices = new List<Vector3>
                            {
                                vertices[bottomLeft],
                                vertices[bottomRight],
                                vertices[topRight],
                                vertices[topLeft]
                            };
                            }
                            catch
                            {
                                Debug.LogError($"Quad Creation Failed. " +
                                    $"\n\tCurrent Vertice count {vertices.Count}" +
                                    $"\n\tBottomLeft index {bottomLeft} || BottomRight index {topRight}" +
                                    $"\n\tTopLeft index {topLeft} || TopRight index {topRight}");
                            }


                            MeshQuad quad = new MeshQuad(faceType, faceCoordinate, GetFaceNormal(faceType), quadVertices);
                            meshQuads.Add(quad);

                            // Add two triangles for each square
                            List<int> newSquareMesh = new List<int>() { bottomLeft, topRight, topLeft, topRight, bottomLeft, bottomRight };
                            triangles.AddRange(newSquareMesh);
                        }
                    }

                    // UPDATE VERTEX COUNT
                    switch (faceType)
                    {
                        // Side Faces XY plane
                        case Face.Front:
                        case Face.Back:
                        case Face.Left:
                        case Face.Right:
                            currentVertexIndex += (current_chunkMeshDimensions.x + 1) * (vDivisions + 1);
                            break;
                        // Top Faces XZ plane
                        case Face.Top:
                        case Face.Bottom:
                            currentVertexIndex += (current_chunkMeshDimensions.x + 1) * (current_chunkMeshDimensions.z + 1);
                            break;
                    }
                }
            }

            // Mesh generation
            CreateMeshFaces();

            // Triangles generation
            CreateMeshTriangles();

            // << SET MESH VALUES >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // Apply the vertices and triangles to the mesh
            newMesh.vertices = vertices.ToArray();
            newMesh.uv = uvs.ToArray();
            newMesh.triangles = triangles.ToArray();

            // Recalculate normals for proper lighting
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            return newMesh;
        }

        // [[[[ FACES ]]]] 

        (int, int) GetFaceUVDivisions(Face faceType)
        {
            // [[ GET VISIBLE DIVISIONS ]]
            // based on neighbors
            int GetVisibleVDivisions(Face type)
            {
                int faceHeight = current_chunkMeshDimensions.y; // Get current height
                Coordinate neighborCoord = null;

                switch (type)
                {
                    case Face.Front:
                        neighborCoord = _worldCoordinate.GetNeighborInDirection(WorldDirection.NORTH);
                        break;
                    case Face.Back:
                        neighborCoord = _worldCoordinate.GetNeighborInDirection(WorldDirection.SOUTH);
                        break;
                    case Face.Left:
                        neighborCoord = _worldCoordinate.GetNeighborInDirection(WorldDirection.WEST);
                        break;
                    case Face.Right:
                        neighborCoord = _worldCoordinate.GetNeighborInDirection(WorldDirection.EAST);
                        break;
                    case Face.Top:
                    case Face.Bottom:
                        break;
                }

                if (neighborCoord != null)
                {
                    Chunk neighborChunk = _worldChunkMap.GetChunkAt(neighborCoord);
                    if (neighborChunk != null)
                    {
                        faceHeight -= neighborChunk.GroundHeight; // subtract based on neighbor height
                        faceHeight -= default_chunkMeshDimensions.y; // subtract 'underground' amount
                        faceHeight = Mathf.Max(faceHeight, 0); // set to 0 as minimum
                    }

                }

                return faceHeight;
            }

            int uDivisions = 0;
            int vDivisions = 0;
            switch (faceType)
            {
                // Side Faces XY plane
                case Face.Front:
                case Face.Back:
                    uDivisions = current_chunkMeshDimensions.x;
                    vDivisions = GetVisibleVDivisions(faceType);
                    break;
                // Side Faces ZY plane
                case Face.Left:
                case Face.Right:
                    uDivisions = current_chunkMeshDimensions.z;
                    vDivisions = GetVisibleVDivisions(faceType);
                    break;
                // Top Faces XZ plane
                case Face.Top:
                case Face.Bottom:
                    uDivisions = current_chunkMeshDimensions.x;
                    vDivisions = current_chunkMeshDimensions.z;
                    break;

            }

            return (uDivisions, vDivisions);
        }

        // note** :: starts the faces at -visibleDimensions.y so that top of chunk is at y=0
        // -- the chunks will be treated as a 'Generated Ground' to build upon
        Vector3 GetFaceStartVertex(Face faceType, int vDivisions)
        {
            Vector3 MultiplyVectors(Vector3 a, Vector3 b)
            {
                return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
            }

            int cellSize = WorldGen.Settings.CellSize_inGameUnits;

            // Get starting vertex of visible vDivisions
            Vector3 visibleSideFaceStartVertex = new Vector3(current_chunkMeshDimensions.x, -vDivisions, current_chunkMeshDimensions.z) * cellSize;
            Vector3 newSideFaceStartOffset = new Vector3(visibleSideFaceStartVertex.x * 0.5f, visibleSideFaceStartVertex.y, visibleSideFaceStartVertex.z * 0.5f);

            // Use current chunk mesh height for bottom and top faces
            Vector3 verticalFaceStartVertex = new Vector3(current_chunkMeshDimensions.x, -current_chunkMeshDimensions.y, current_chunkMeshDimensions.z) * cellSize;
            Vector3 newVerticalFaceStartOffset = new Vector3(verticalFaceStartVertex.x * 0.5f, verticalFaceStartVertex.y, verticalFaceStartVertex.z * 0.5f);

            switch (faceType)
            {
                case Face.Front:
                    return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, 1));
                case Face.Back:
                    return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, -1));
                case Face.Left:
                    return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, -1));
                case Face.Right:
                    return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, 1));
                case Face.Top:
                    return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(1, 0, -1));
                case Face.Bottom:
                    return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(-1, 1, -1));
                default:
                    return Vector3.zero;
            }

        }

        Vector3 GetFaceNormal(Face faceType)
        {
            switch (faceType)
            {
                case Face.Front: return Vector3.forward;
                case Face.Back: return Vector3.back;
                case Face.Left: return Vector3.left;
                case Face.Right: return Vector3.right;
                case Face.Top: return Vector3.up;
                case Face.Bottom: return Vector3.down;
                default: return Vector3.zero;
            }
        }


        (Vector3, Vector3) GetFaceUVDirectionalVectors(Face faceType)
        {
            Vector3 u = Vector3.zero;
            Vector3 v = Vector3.zero;

            switch (faceType)
            {
                case Face.Front:
                    u = Vector3.right;
                    v = Vector3.up;
                    break;
                case Face.Back:
                    u = Vector3.left;
                    v = Vector3.up;
                    break;
                case Face.Left:
                    u = Vector3.forward;
                    v = Vector3.up;
                    break;
                case Face.Right:
                    u = Vector3.back;
                    v = Vector3.up;
                    break;
                case Face.Top:
                    u = Vector3.left;
                    v = Vector3.forward;
                    break;
                case Face.Bottom:
                    u = Vector3.right;
                    v = Vector3.forward;
                    break;
            }
            return (u, v);
        }


        void OffsetMesh(Vector3 chunkWorldPosition)
        {
            Vector3[] vertices = this.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += chunkWorldPosition;
            }
            mesh.vertices = vertices;
        }

    }
}



