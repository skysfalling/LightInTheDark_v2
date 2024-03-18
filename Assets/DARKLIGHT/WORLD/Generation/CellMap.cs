using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.World.Generation
{
    using FaceType = Chunk.FaceType;

    public class CellMap
    {
        Chunk _chunk;
        ChunkMesh _chunkMesh;
        HashSet<Cell> _cells = new();
        Dictionary<FaceType, List<MeshQuad>> _quads = new();
        Dictionary<FaceType, HashSet<Cell>> _faceMap = new();

        public Chunk ChunkParent { get; private set; }
        public List<Cell> AllCells => _cells.ToList();
        public Dictionary<FaceType, HashSet<Cell>> ChunkFaceMap => _faceMap;

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

        public Cell GetCellAtCoordinate(Coordinate coordinate)
        {
            List<Cell> topFaceCells = _faceMap[FaceType.TOP].ToList();
            foreach (Cell cell in topFaceCells)
            {
                Vector3 cellXZ = new Vector3(cell.Position.x, 0, cell.Position.z);
                Vector3 coordXZ = new Vector3(coordinate.ScenePosition.x, 0, coordinate.ScenePosition.z);
                if (cellXZ == coordXZ)
                {
                    return cell;
                }
            }
            return null;
        }
    }
}
