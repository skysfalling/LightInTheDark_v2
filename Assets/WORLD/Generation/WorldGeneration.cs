using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


public class WorldGeneration : MonoBehaviour
{
    class Cell
    {
        Chunk chunkParent;
        int chunkIndex;
        public Vector3[] vertices; // Corners of the cell
        public Vector3 center;     // Center of the cell
        // Add other cell properties here

        public Cell(Chunk chunkParent, int index, Vector3[] vertices)
        {
            this.vertices = vertices;
            this.chunkIndex = index;
            this.chunkParent = chunkParent;
            center = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;

            /*
            GameObject cell_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell_object.transform.position = center;
            cell_object.name = "ChunkCell " + chunkIndex;
            */

            string output = "NEW CELL " + chunkIndex + " :: ";
            foreach (Vector3 vector in vertices)
            {
                output += vector.ToString() + "\n"; // Adding a newline for readability
            }
            Debug.Log(output);
        }
    }

    class Chunk
    {
        public Mesh mesh;
        public Vector3 position;

        List<Cell> cells = new List<Cell>();
        int cellSize = 4; // Size of each subdivision cell
        int chunkWidthCellCount = 3; // Length of width in cells
        int chunkHeightCellCount = 3; // Length of height in cells

        public Chunk (Mesh mesh, Vector3 position)
        {
            this.mesh = mesh;
            this.position = position;

            OffsetMesh(position);
            CreateCells();
        }

        void OffsetMesh(Vector3 position)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += position;
            }
            mesh.vertices = vertices;
        }

        public void CreateCells()
        {
            int cell_index = 0;

            // Get topface vertices
            List<Vector3> topfacevertices = new List<Vector3>();
            foreach (Vector3 vertice in mesh.vertices)
            {
                if (vertice.y >= 0) 
                {
                    topfacevertices.Add(vertice);
                }
            }

            // Sort topface vertices
            List<Vector3> uniquevertices = topfacevertices.Distinct().ToList();
            List<Vector3> sortedVertices = uniquevertices.OrderBy(v => v.z).ThenBy(v => v.x).ToList();

            // Group sorted vertices into squares
            // Iterate through rows
            for (int rowStartIndex = 0; rowStartIndex < sortedVertices.Count; rowStartIndex += chunkWidthCellCount + 1)
            {
                // Iterate through columns
                for (int colIndex = rowStartIndex; colIndex < rowStartIndex + chunkWidthCellCount; colIndex += 1)
                {
                    // Check for invalid indexes
                    if (colIndex + (chunkWidthCellCount + 2) < sortedVertices.Count)
                    {
                        Vector3 bottomLeft = sortedVertices[colIndex];
                        Vector3 bottomRight = sortedVertices[colIndex + 1];
                        Vector3 topLeft = sortedVertices[colIndex + (chunkWidthCellCount + 1)];
                        Vector3 topRight = sortedVertices[colIndex + (chunkWidthCellCount + 2)];

                        // Create a square (as a cube) at each set of vertices
                        cells.Add(new Cell(this, cell_index, new Vector3[] { bottomLeft, bottomRight, topLeft, topRight }));
                        cell_index++;
                    }
                }
            }
        }
    }

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

    /// <summary>
    /// Generate a combined mesh object by generating random positions, 
    /// creating chunk meshes at those positions, offsetting them, and then combining them into a single mesh.
    /// </summary>
    [EasyButtons.Button]
    void Generate()
    {
        // Generate Random Path
        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> positions = GenerateRandomWalkPositions(steps);

        foreach (Vector3 position in positions)
        {
            Chunk newChunk = CreateChunkMesh(position);

            meshes.Add(newChunk.mesh);
        }

        Mesh combinedMesh = CombineMeshes(meshes);
        CreateCombinedMeshObject(combinedMesh);
    }

    /// <summary>
    /// Creates a chunk mesh with specified dimensions, vertices, triangles, and UVs. 
    /// Constructs faces of the chunk and computes triangles for each face.
    /// </summary>
    /// <returns>A Mesh object representing a chunk of terrain or object.</returns>
    Chunk CreateChunkMesh(Vector3 position)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Initialize UV list

        // Helper method to add vertices for a face
        // 'start' is the starting point of the face, 'u' and 'v' are the directions of the grid
        List<Vector3> AddFace(Vector3 start, Vector3 u, Vector3 v, int uDivisions, int vDivisions, Vector3 faceNormal)
        {
            List<Vector3> faceVertices = new List<Vector3>();

            for (int i = 0; i <= vDivisions; i++)
            {
                for (int j = 0; j <= uDivisions; j++)
                {

                    Vector3 newVertex = start + (i * cellSize * v) + (j * cellSize * u);
                    vertices.Add(newVertex);
                    faceVertices.Add(newVertex);

                    // Standard UV rectangle for each face
                    float uCoord = 1 - (j / (float)uDivisions); // Flipped horizontally
                    float vCoord = i / (float)vDivisions;
                    uvs.Add(new Vector2(uCoord, vCoord));
                }
            }

            return faceVertices;
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

        // Bottom face
        Vector3 bottomFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(-1, 1, -1));
        Vector3 bottomFaceNormal = Vector3.down; // Normal for bottom face
        AddFace(bottomFaceStartVertex, Vector3.right, Vector3.forward, chunkDimensions.x, chunkDimensions.z, bottomFaceNormal);

        // Top face
        Vector3 topFaceStartVertex = MultiplyVectors(newFaceStartOffset, new Vector3(1, 0, -1));
        Vector3 topFaceNormal = Vector3.up; // Normal for top face
        List<Vector3> topfacevertices = AddFace(topFaceStartVertex, Vector3.left, Vector3.forward, chunkDimensions.x, chunkDimensions.z, topFaceNormal);



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

        // << CREATE CHUNK >>
        Chunk chunk = new Chunk(mesh, position);
        return chunk;
    }

    /// <summary>
    /// Combines multiple Mesh objects into a single mesh. This is useful for optimizing rendering by reducing draw calls.
    /// </summary>
    /// <param name="meshes">A List of Mesh objects to be combined.</param>
    /// <returns>A single combined Mesh object.</returns>
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

    /// <summary>
    /// Creates a GameObject to represent the combined mesh in the scene, attaching necessary components like MeshFilter and MeshRenderer.
    /// </summary>
    /// <param name="combinedMesh">The combined Mesh to be represented.</param>
    void CreateCombinedMeshObject(Mesh combinedMesh)
    {
        combinedMeshObject = new GameObject("CombinedChunk");
        combinedMeshObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        combinedMeshObject.AddComponent<MeshRenderer>().material = chunkMaterial;
    }

    /// <summary>
    /// Generates a list of Vector3 positions based on a random walk algorithm.
    /// </summary>
    /// <param name="steps">Number of steps in the random walk.</param>
    /// <returns>A List of Vector3 positions representing the path of the random walk.</returns>
    List<Vector3> GenerateRandomWalkPositions(int steps)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 currentPos = Vector3.zero; // Starting position
        if (steps <= 1) { positions.Add(currentPos); return positions; }


        for (int i = 0; i < steps; i++)
        {
            currentPos += RandomDirectionOnXZ() * fullSized_chunkWidth;
            positions.Add(currentPos);
        }

        return positions;
    }

    /// <summary>
    /// Generates a random direction on the XZ plane.
    /// </summary>
    /// <returns>A Vector3 representing a random direction on the XZ plane.</returns>
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

