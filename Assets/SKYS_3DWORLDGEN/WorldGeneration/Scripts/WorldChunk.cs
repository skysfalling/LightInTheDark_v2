using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class WorldChunk
{
    public Mesh mesh;
    public Vector3 position;

    public List<WorldCell> cells = new List<WorldCell>();
    int cellSize = 4; // Size of each subdivision cell
    public int width_in_cells = 3; // Length of width in cells
    public int height_in_cells = 3; // Length of height in cells



    //*** CREATION ============================================================================== >>>>>
    #region
    public WorldChunk(Mesh mesh, Vector3 position, int width = 3, int height = 3, int cellSize = 4)
    {
        this.mesh = mesh;
        this.position = position;
        this.cellSize = cellSize;
        this.width_in_cells = width;
        this.height_in_cells = height;


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

        // Determine the number of vertices per row
        int verticesPerRow = 1;
        for (int i = 1; i < sortedVertices.Count; i++)
        {
            if (sortedVertices[i].z != sortedVertices[0].z)
            {
                break;
            }
            verticesPerRow++;
        }

        // Group sorted vertices into squares
        for (int rowStartIndex = 0; rowStartIndex < sortedVertices.Count - verticesPerRow; rowStartIndex += verticesPerRow)
        {
            for (int colIndex = rowStartIndex; colIndex < rowStartIndex + verticesPerRow - 1; colIndex++)
            {
                // Check for invalid indexes
                if (colIndex + verticesPerRow < sortedVertices.Count)
                {
                    Vector3 bottomLeft = sortedVertices[colIndex];
                    Vector3 bottomRight = sortedVertices[colIndex + 1];
                    Vector3 topLeft = sortedVertices[colIndex + verticesPerRow];
                    Vector3 topRight = sortedVertices[colIndex + verticesPerRow + 1];

                    // Create a square (as a cube) at each set of vertices
                    cells.Add(new WorldCell(this, cell_index, new Vector3[] { bottomLeft, bottomRight, topLeft, topRight }));
                    cell_index++;
                }
            }
        }
    }
    #endregion





}


