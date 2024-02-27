using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WorldChunk;
using FaceType = WorldChunk.FaceType;

public class MeshQuad
{
    public FaceType faceType;
    public Vector2Int faceCoord;
    public Vector3 faceNormal;
    List<Vector3> vertices = new();

    public MeshQuad(FaceType faceType, Vector2Int faceCoord, Vector3 faceNormal, List<Vector3> vertices)
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

public class WorldChunkMesh
{
    int _cellSize = WorldGeneration.CellWidth_inWorldSpace;
    Vector3Int default_chunkMeshDimensions = WorldGeneration.GetChunkVec3Dimensions();
    Vector3Int current_chunkMeshDimensions;
    WorldChunk chunk;
    WorldCoordinate worldCoordinate;
    Dictionary<FaceType, List<Vector3>> _meshVertices = new();
    Dictionary<FaceType, List<Vector2>> _meshUVs = new();
    public List<MeshQuad> meshQuads = new();

    public Mesh mesh;

    public WorldChunkMesh(WorldChunk chunk, int groundHeight, Vector3 groundPosition)
    {
        this.chunk = chunk;
        this.worldCoordinate = chunk.worldCoordinate;

        List<FaceType> facesToGenerate = new List<FaceType>()
        {
            FaceType.Front , FaceType.Back ,
            FaceType.Left, FaceType.Right, 
            FaceType.Top, FaceType.Bottom
        };

        this.mesh = CreateMesh(groundHeight, facesToGenerate);
        OffsetMesh(groundPosition);
    }

    Mesh CreateMesh(int groundHeight, List<FaceType> facesToGenerate)
    {
        int cellSize = WorldGeneration.CellWidth_inWorldSpace;
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
                FaceType faceType = facesToGenerate[faceIndex];

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
                FaceType faceType = facesToGenerate[faceIndex];

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
                        Vector2Int faceCoordinate = new Vector2Int(j, i);
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
                    case FaceType.Front:
                    case FaceType.Back:
                    case FaceType.Left:
                    case FaceType.Right:
                        currentVertexIndex += (current_chunkMeshDimensions.x + 1) * (vDivisions + 1);
                        break;
                    // Top Faces XZ plane
                    case FaceType.Top:
                    case FaceType.Bottom:
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

    (int, int) GetFaceUVDivisions(FaceType faceType)
    {
        // [[ GET VISIBLE DIVISIONS ]]
        // based on neighbors
        int GetVisibleVDivisions(FaceType type)
        {
            int faceHeight = current_chunkMeshDimensions.y; // Get current height
            WorldCoordinate neighborCoord = null;

            switch (type)
            {
                case FaceType.Front:
                    neighborCoord = worldCoordinate.GetNeighborInDirection(WorldDirection.NORTH);
                    break;
                case FaceType.Back:
                    neighborCoord = worldCoordinate.GetNeighborInDirection(WorldDirection.SOUTH);
                    break;
                case FaceType.Left:
                    neighborCoord = worldCoordinate.GetNeighborInDirection(WorldDirection.WEST);
                    break;
                case FaceType.Right:
                    neighborCoord = worldCoordinate.GetNeighborInDirection(WorldDirection.EAST);
                    break;
                case FaceType.Top:
                case FaceType.Bottom:
                    break;
            }

            if (neighborCoord != null)
            {
                WorldChunk neighborChunk = WorldChunkMap.GetChunkAt(neighborCoord);
                faceHeight -= neighborChunk.groundHeight; // subtract based on neighbor height
                faceHeight -= default_chunkMeshDimensions.y; // subtract 'underground' amount
                faceHeight = Mathf.Max(faceHeight, 0); // set to 0 as minimum
            }

            return faceHeight;
        }

        int uDivisions = 0;
        int vDivisions = 0;
        switch (faceType)
        {
            // Side Faces XY plane
            case FaceType.Front:
            case FaceType.Back:
                uDivisions = current_chunkMeshDimensions.x;
                vDivisions = GetVisibleVDivisions(faceType);
                break;
            // Side Faces ZY plane
            case FaceType.Left:
            case FaceType.Right:
                uDivisions = current_chunkMeshDimensions.z;
                vDivisions = GetVisibleVDivisions(faceType);
                break;
            // Top Faces XZ plane
            case FaceType.Top:
            case FaceType.Bottom:
                uDivisions = current_chunkMeshDimensions.x;
                vDivisions = current_chunkMeshDimensions.z;
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

        // Get starting vertex of visible vDivisions
        Vector3 visibleSideFaceStartVertex = new Vector3(current_chunkMeshDimensions.x, -vDivisions, current_chunkMeshDimensions.z ) * _cellSize;
        Vector3 newSideFaceStartOffset = new Vector3(visibleSideFaceStartVertex.x * 0.5f, visibleSideFaceStartVertex.y, visibleSideFaceStartVertex.z * 0.5f);

        // Use current chunk mesh height for bottom and top faces
        Vector3 verticalFaceStartVertex = new Vector3(current_chunkMeshDimensions.x, -current_chunkMeshDimensions.y, current_chunkMeshDimensions.z) * _cellSize;
        Vector3 newVerticalFaceStartOffset = new Vector3(verticalFaceStartVertex.x * 0.5f, verticalFaceStartVertex.y, verticalFaceStartVertex.z * 0.5f);

        switch (faceType)
        {
            case FaceType.Front:
                return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, 1));
            case FaceType.Back:
                return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, -1));
            case FaceType.Left:
                return MultiplyVectors(newSideFaceStartOffset, new Vector3(-1, 1, -1));
            case FaceType.Right:
                return MultiplyVectors(newSideFaceStartOffset, new Vector3(1, 1, 1));
            case FaceType.Top:
                return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(1, 0, -1));
            case FaceType.Bottom:
                return MultiplyVectors(newVerticalFaceStartOffset, new Vector3(-1, 1, -1));
            default:
                return Vector3.zero;
        }

    }

    Vector3 GetFaceNormal(FaceType faceType)
    {
        switch (faceType)
        {
            case FaceType.Front: return Vector3.forward;
            case FaceType.Back: return Vector3.back;
            case FaceType.Left: return Vector3.left;
            case FaceType.Right: return Vector3.right;
            case FaceType.Top: return Vector3.up;
            case FaceType.Bottom: return Vector3.down;
            default: return Vector3.zero;
        }
    }


    (Vector3, Vector3) GetFaceUVDirectionalVectors(FaceType faceType)
    {
        Vector3 u = Vector3.zero;
        Vector3 v = Vector3.zero;

        switch (faceType)
        {
            case FaceType.Front:
                u = Vector3.right;
                v = Vector3.up;
                break;
            case FaceType.Back:
                u = Vector3.left;
                v = Vector3.up;
                break;
            case FaceType.Left:
                u = Vector3.forward;
                v = Vector3.up;
                break;
            case FaceType.Right:
                u = Vector3.back;
                v = Vector3.up;
                break;
            case FaceType.Top:
                u = Vector3.left;
                v = Vector3.forward;
                break;
            case FaceType.Bottom:
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

