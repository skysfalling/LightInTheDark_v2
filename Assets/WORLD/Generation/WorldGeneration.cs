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

        // Vertices of the cuboid
        Vector3[] vertices = new Vector3[]
        {
        // Bottom vertices
        new Vector3(-6, -4, -6),
        new Vector3(6, -4, -6),
        new Vector3(6, -4, 6),
        new Vector3(-6, -4, 6),

        // Top vertices
        new Vector3(-6, 4, -6),
        new Vector3(6, 4, -6),
        new Vector3(6, 4, 6),
        new Vector3(-6, 4, 6)
        };

        // Triangles (two triangles per face, six faces total) with inverted winding order
        int[] triangles = new int[]
        {
        // Bottom (inverted)
        0, 1, 2, 0, 2, 3,
        // Top (inverted)
        4, 6, 5, 4, 7, 6,
        // Front (inverted)
        4, 1, 0, 4, 5, 1,
        // Back (inverted)
        3, 6, 7, 3, 2, 6,
        // Left (inverted)
        4, 3, 7, 4, 0, 3,
        // Right (inverted)
        1, 5, 6, 1, 6, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Recalculate normals for proper lighting

        // UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        // Bottom UVs
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);

        // Top UVs
        uvs[4] = new Vector2(0, 0);
        uvs[5] = new Vector2(1, 0);
        uvs[6] = new Vector2(1, 1);
        uvs[7] = new Vector2(0, 1);

        // Apply the vertices, triangles, and UVs to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals(); // Recalculate normals for proper lighting

        return mesh;
    }

}

