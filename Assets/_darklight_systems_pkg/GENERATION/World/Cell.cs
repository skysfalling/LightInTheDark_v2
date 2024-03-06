using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Represents the smallest unit of the game world, encapsulating terrain,
/// environmental, and gameplay-related data. Functions as a building block
/// for the larger world map, enabling detailed and scalable world design.
/// </summary>

namespace Darklight.ThirdDimensional.World
{
    public class Cell
    {
        public enum TYPE { EMPTY, EDGE, CORNER, OBSTACLE, SPAWN_POINT }

        // [[ PRIVATE DATA VARIABLES ]]
        MeshQuad _meshQuad;
        TYPE _type;

        // [[ PUBLIC REFERENCE VARIABLES ]]
        public Chunk ChunkParent { get; private set; }
        public MeshQuad MeshQuad => _meshQuad;
        public TYPE Type => _type;
        public Vector3 Position
        {
            get
            {
                return ChunkParent.CenterPosition + MeshQuad.GetCenterPosition();
            }
        }

        public Cell(Chunk chunkParent, MeshQuad meshQuad)
        {
            this.ChunkParent = chunkParent;
            this._meshQuad = meshQuad;
        }

        public void SetCellType(TYPE type)
        {
            this._type = type;
        }
    }
}


