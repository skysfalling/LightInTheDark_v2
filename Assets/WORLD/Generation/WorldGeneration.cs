using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public Material chunkMaterial; // Assign a material in the inspector
    public int steps = 10; // Number of steps in the random walk
    public int chunkSize = 12; // Distance between each chunk

    void Start()
    {
        List<Mesh> meshes = new List<Mesh>();
        //List<Vector3> positions = GenerateRandomWalkPositions(steps);
        List<Vector3> positions = new List<Vector3> { Vector3.zero };

        foreach (Vector3 position in positions)
        {
            Mesh chunkMesh = CreateChunkMesh();
            chunkMesh = OffsetMesh(chunkMesh, position);
            meshes.Add(chunkMesh);
            CreateCombinedMeshObject(chunkMesh);

        }

        //Mesh combinedMesh = CombineMeshes(meshes);
        //CreateCombinedMeshObject(combinedMesh);
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
        float mergeThreshold = 0.01f;
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        foreach (Mesh mesh in meshes)
        {
            int[] originalTriangles = mesh.triangles;
            Vector3[] originalVertices = mesh.vertices;

            Dictionary<int, int> vertexMap = new Dictionary<int, int>();

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                bool isMerged = false;

                for (int j = 0; j < newVertices.Count; j++)
                {
                    if (Vector3.Distance(vertex, newVertices[j]) < mergeThreshold)
                    {
                        isMerged = true;
                        vertexMap[i] = j; // Map original vertex index to new merged index
                        break;
                    }
                }

                if (!isMerged)
                {
                    newVertices.Add(vertex);
                    vertexMap[i] = newVertices.Count - 1;
                }
            }

            foreach (int triangleIndex in originalTriangles)
            {
                if (vertexMap.ContainsKey(triangleIndex))
                {
                    newTriangles.Add(vertexMap[triangleIndex]); // Remap triangle indices
                }
                else
                {
                    Debug.LogError("Triangle index out of bounds: " + triangleIndex);
                    return null; // Early exit if there's an error
                }
            }
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = newVertices.ToArray();
        combinedMesh.triangles = newTriangles.ToArray();

        // Recalculate UVs based on the new vertices
        Vector2[] newUVs = new Vector2[newVertices.Count];
        for (int i = 0; i < newVertices.Count; i++)
        {
            Vector3 vertex = newVertices[i];
            newUVs[i] = new Vector2(vertex.x, vertex.z);
        }
        combinedMesh.uv = newUVs;

        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();

        return combinedMesh;
    }

    void CreateCombinedMeshObject(Mesh combinedMesh)
    {
        GameObject combinedObject = new GameObject("CombinedChunk");
        combinedObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        combinedObject.AddComponent<MeshRenderer>().material = chunkMaterial;
    }

    List<Vector3> GenerateRandomWalkPositions(int steps)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 currentPos = Vector3.zero; // Starting position

        for (int i = 0; i < steps; i++)
        {
            currentPos += RandomDirectionOnXZ() * chunkSize;
            positions.Add(currentPos);
        }

        return positions;
    }

    Vector3 RandomDirectionOnXZ()
    {
        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector3.forward;
            case 1: return Vector3.back;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            default: return Vector3.forward;
        }
    }

    void GenerateChunkAtPosition(Vector3 position)
    {
        Mesh chunkMesh = CreateChunkMesh();

        GameObject chunk = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer));
        chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
        chunk.GetComponent<MeshRenderer>().material = chunkMaterial;

        // Position the chunk at the specified location
        chunk.transform.position = new Vector3(position.x, position.y + 4, position.z); // Adjust y to be half of the height
    }

    Mesh CreateChunkMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Size of each subdivision cell
        int cellSize = 4;
        // Default Chunk Dimensions [ in CellsSize Units! as set above ^^ ]
        Vector3Int chunkDimensions = new Vector3Int(3, 2, 3);
        Vector3 fullsized_chunkDimensions = chunkDimensions * cellSize;

        // Helper method to add vertices for a face
        // 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid
        void AddFace(Vector3 start, Vector3 u, Vector3 v, int uDivisions, int vDivisions, float size)
        {
            string vertices_debug = "new face vertices ";

            // Loop over each subdivision in the vertical direction
            for (int i = 0; i <= vDivisions; i++)
            {
                // Calculate the current position along the vertical direction 'v'
                Vector3 vPos = start + (i * size * v);

                // Loop over each subdivision in the horizontal direction
                for (int j = 0; j <= uDivisions; j++)
                {
                    // Calculate the current position along the horizontal direction 'u'
                    Vector3 uPos = j * size * u;

                    // Add the vertex at the current position (vPos + uPos)
                    // This represents a point on the face grid
                    vertices.Add(vPos + uPos);
                    vertices_debug += (vPos + uPos) + "\n";
                }
            }

            print(vertices_debug);
        }

        Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        // << SIDEFACES >>
        // note** :: starts the face at -fullsized_chunkDimensions so that top of chunk is at 0
                // -- the chunks will be treated as a 'Generated Ground' to build upon
        Vector3 newFaceStartOffset = new Vector3(fullsized_chunkDimensions.x * 0.5f, -fullsized_chunkDimensions.y, fullsized_chunkDimensions.z * 0.5f);

        // Forward face
        Vector3 forwardFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, 1));
        AddFace(forwardFaceStartVertex, Vector3.right, Vector3.up, chunkDimensions.x, chunkDimensions.y, cellSize);

        // Back face
        Vector3 backFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, -1));
        AddFace(backFaceStartVertex, Vector3.left, Vector3.up, chunkDimensions.x, chunkDimensions.y, cellSize);

        // Right face
        Vector3 rightFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        AddFace(rightFaceStartVertex, Vector3.forward, Vector3.up, chunkDimensions.z, chunkDimensions.y, cellSize);

        // Left face
        Vector3 leftFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 1, 1));
        AddFace(leftFaceStartVertex, Vector3.back, Vector3.up, chunkDimensions.z, chunkDimensions.y, cellSize);

        // << VERTICAL FACES >> 
        // Top face
        Vector3 topFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 0, -1));
        AddFace(topFaceStartVertex, Vector3.left, Vector3.forward, chunkDimensions.x, chunkDimensions.z, cellSize);

        // Bottom face
        Vector3 bottomFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        AddFace(bottomFaceStartVertex, Vector3.right, Vector3.forward, chunkDimensions.x, chunkDimensions.z, cellSize);

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

                    string debug = "newsquare : ";
                    for (int k = 0; k < newSquareMesh.Count; k++)
                    {
                        debug += newSquareMesh[k];
                    }
                    print(debug);
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

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();

        // UV mapping can be updated as required

        return mesh;
    }
}

