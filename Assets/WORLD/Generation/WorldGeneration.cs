using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public Material chunkMaterial; // Assign a material in the inspector
    public int steps = 10; // Number of steps in the random walk


    int cellSize = 4; // Size of each subdivision cell
    int chunkWidthCellCount = 3; // Length of width in cells
    int chunkHeightCellCount = 3; // Length of height in cells

    Vector3Int chunkDimensions; // default Chunk Dimensions [ size units == cellSize ]
    Vector3 fullsized_chunkDimensions; // fullsize Chunk Dimensions [ size units == meters ] >> calculated by chunkDimensions * cellSize
    int fullSized_chunkWidth;
    int fullSized_chunkHeight;
    GameObject combinedMeshObject;

    private void Awake()
    {
        chunkDimensions = new Vector3Int(chunkWidthCellCount, chunkHeightCellCount, chunkWidthCellCount);
        fullsized_chunkDimensions = chunkDimensions * cellSize;
        fullSized_chunkWidth = chunkWidthCellCount * cellSize;
        fullSized_chunkHeight = chunkHeightCellCount * cellSize;  
    }

    [EasyButtons.Button]
    void Generate()
    {
        // Generate Random Path
        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> positions = GenerateRandomWalkPositions(steps);

        foreach (Vector3 position in positions)
        {
            Mesh chunkMesh = CreateChunkMesh();
            chunkMesh = OffsetMesh(chunkMesh, position);
            meshes.Add(chunkMesh);
        }

        Mesh combinedMesh = CombineMeshes(meshes);
        CreateCombinedMeshObject(combinedMesh);
    }

    Mesh CreateChunkMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Initialize UV list

        // Helper method to add vertices for a face
        // 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid

        void AddFace(Vector3 start, Vector3 u, Vector3 v, int uDivisions, int vDivisions, Vector3 faceNormal)
        {
            for (int i = 0; i <= vDivisions; i++)
            {
                for (int j = 0; j <= uDivisions; j++)
                {
                    vertices.Add(start + (i * cellSize * v) + (j * cellSize * u));

                    // Standard UV rectangle for each face
                    float uCoord = 1 - (j / (float)uDivisions); // Flipped horizontally
                    float vCoord = i / (float)vDivisions;
                    uvs.Add(new Vector2(uCoord, vCoord));
                }
            }
        }

        Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        // << FACES >>
        // note** :: starts the face at -fullsized_chunkDimensions so that top of chunk is at 0
        // -- the chunks will be treated as a 'Generated Ground' to build upon
        Vector3 newFaceStartOffset = new Vector3(fullsized_chunkDimensions.x * 0.5f, -fullsized_chunkDimensions.y, fullsized_chunkDimensions.z * 0.5f);

        // Forward face
        Vector3 forwardFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, 1));
        Vector3 forwardFaceNormal = Vector3.forward; // Normal for forward face
        AddFace(forwardFaceStartVertex, Vector3.right, Vector3.up, chunkDimensions.x, chunkDimensions.y, forwardFaceNormal);

        // Back face
        Vector3 backFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, -1));
        Vector3 backFaceNormal = Vector3.back; // Normal for back face
        AddFace(backFaceStartVertex, Vector3.left, Vector3.up, chunkDimensions.x, chunkDimensions.y, backFaceNormal);

        // Right face
        Vector3 rightFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        Vector3 rightFaceNormal = Vector3.right; // Normal for right face
        AddFace(rightFaceStartVertex, Vector3.forward, Vector3.up, chunkDimensions.z, chunkDimensions.y, rightFaceNormal);

        // Left face
        Vector3 leftFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, 1));
        Vector3 leftFaceNormal = Vector3.left; // Normal for left face
        AddFace(leftFaceStartVertex, Vector3.back, Vector3.up, chunkDimensions.z, chunkDimensions.y, leftFaceNormal);

        // Top face
        Vector3 topFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 0, -1));
        Vector3 topFaceNormal = Vector3.up; // Normal for top face
        AddFace(topFaceStartVertex, Vector3.left, Vector3.forward, chunkDimensions.x, chunkDimensions.z, topFaceNormal);

        // Bottom face
        Vector3 bottomFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        Vector3 bottomFaceNormal = Vector3.down; // Normal for bottom face
        AddFace(bottomFaceStartVertex, Vector3.right, Vector3.forward, chunkDimensions.x, chunkDimensions.z, bottomFaceNormal);


        // Helper method to dynamically generate triangles for a face
        void AddFaceTriangles(int faceStartIndex, int uDivisions, int vDivisions)
        {
            for (int i = 0; i < vDivisions; i++)
            {
                for (int j = 0; j < uDivisions; j++)
                {
                    int rowStart = faceStartIndex + i * (uDivisions + 1);
                    int nextRowStart = faceStartIndex + (i + 1) * (uDivisions + 1);

                    int bottomLeft = rowStart + j;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = nextRowStart + j;
                    int topRight = topLeft + 1;

                    // Add two triangles for each square
                    List<int> newSquareMesh = new List<int>() { bottomLeft, topRight, topLeft, topRight, bottomLeft, bottomRight };
                    triangles.AddRange(newSquareMesh);
                }
            }
        }

        // >> SIDEFACE FACE TRIANGLES [ XY plane ]
        int side_vertexCountPerFace = (chunkDimensions.x + 1) * (chunkDimensions.y + 1);

        // >> VERTICAL FACE TRIANGLES [ XZ plane ]
        int vert_vertexCountPerFace = (chunkDimensions.x + 1) * (chunkDimensions.z + 1);

        // ITERATE through 6 faces
        // Triangles generation
        int vertexCount = 0;
        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            if (faceIndex < 4) // Side faces (XY plane)
            {
                AddFaceTriangles(vertexCount, chunkDimensions.x, chunkDimensions.y);
                vertexCount += (chunkDimensions.x + 1) * (chunkDimensions.y + 1);
            }
            else // Vertical faces (XZ plane)
            {
                AddFaceTriangles(vertexCount, chunkDimensions.x, chunkDimensions.z);
                vertexCount += (chunkDimensions.x + 1) * (chunkDimensions.z + 1);
            }
        }

        // Apply the vertices and triangles to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();


        void OnDrawGizmos()
        {
            if (mesh == null) return;

            Gizmos.color = Color.red;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 vertex = transform.TransformPoint(mesh.vertices[i]);
                Vector2 uv = mesh.uv[i];

                // Draw a small line or dot at the vertex position
                // Use the UV coordinates to offset the line or dot, so you can visualize the direction
                Gizmos.DrawRay(vertex, new Vector3(uv.x, uv.y, 0) * 0.1f);
            }
        }


        return mesh;
    }

    Mesh OffsetMesh(Mesh mesh, Vector3 position)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += position;
        }
        mesh.vertices = vertices;
        return mesh;
    }

    Mesh CombineMeshes(List<Mesh> meshes)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>(); // Add a list for the new UVs

        int vertexOffset = 0; // Keep track of the vertex offset

        foreach (Mesh mesh in meshes)
        {
            newVertices.AddRange(mesh.vertices); // Add all vertices

            // Add all UVs from the current mesh
            newUVs.AddRange(mesh.uv);

            // Add the triangles, adjusted by the current vertex offset
            foreach (var tri in mesh.triangles)
            {
                newTriangles.Add(tri + vertexOffset);
            }

            // Update the vertex offset for the next mesh
            vertexOffset += mesh.vertexCount;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = newVertices.ToArray();
        combinedMesh.triangles = newTriangles.ToArray();
        combinedMesh.uv = newUVs.ToArray(); // Set the combined UVs

        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();

        return combinedMesh;
    }

    void CreateCombinedMeshObject(Mesh combinedMesh)
    {
        if (combinedMeshObject != null) { Destroy(combinedMeshObject); }

        combinedMeshObject = new GameObject("CombinedChunk");
        combinedMeshObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        combinedMeshObject.AddComponent<MeshRenderer>().material = chunkMaterial;
    }

    List<Vector3> GenerateRandomWalkPositions(int steps)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 currentPos = Vector3.zero; // Starting position

        for (int i = 0; i < steps; i++)
        {
            currentPos += RandomDirectionOnXZ() * fullSized_chunkWidth;
            positions.Add(currentPos);
        }

        return positions;
    }

    Vector3 RandomDirectionOnXZ()
    {
        int choice = UnityEngine.Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector3.forward;
            case 1: return Vector3.back;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            default: return Vector3.forward;
        }
    }

}

