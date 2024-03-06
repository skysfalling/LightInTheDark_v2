using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.ThirdDimensional.World
{
    using FaceType = Chunk.FaceType;

    public class CellMap
    {
        Chunk _chunk;
        ChunkMesh _chunkMesh;
        HashSet<Cell> _cells = new();
        Dictionary<FaceType, HashSet<MeshQuad>> _quads = new();
        Dictionary<FaceType, HashSet<Cell>> _faceMap = new();

        public Chunk ChunkParent { get; private set; }
        public List<Cell> AllCells => _cells.ToList();
        public Dictionary<FaceType, HashSet<Cell>> FaceMap => _faceMap;

        public CellMap(Chunk chunk, ChunkMesh chunkMesh)
        {
            _chunk = chunk;
            _chunkMesh = chunkMesh;
            _quads = _chunkMesh.MeshQuads;

            // Create a new Cell for each quad
            foreach (FaceType faceType in _quads.Keys)
            {
                foreach (MeshQuad quad in _quads[faceType])
                {
                    // >> create new cell
                    Cell newCell = new Cell(_chunk, quad);
                    _cells.Add(newCell);

                    // >> add to face map
                    if (!_faceMap.ContainsKey(quad.faceType)) { _faceMap[faceType] = new(); }
                    _faceMap[faceType].Add(newCell);
                }
            }
        }
    }
}
