using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Darklight.World.Generation;
using Darklight.World.Generation.System;

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// SKYS_3DWORLDGEN : Created by skysfalling @ darklightinteractive 2024
/// 
/// Represents the smallest unit of the game world, encapsulating terrain,
/// environmental, and gameplay-related data. Functions as a building block
/// for the larger world map, enabling detailed and scalable world design.
/// </summary>

namespace Darklight.World.Generation.Unit
{
	public class Cell
	{
		public enum TYPE { EMPTY, EDGE, CORNER, OBSTACLE, SPAWN_POINT }

		// [[ PRIVATE DATA VARIABLES ]]
		Quad _meshQuad;
		TYPE _type;

		// [[ PUBLIC REFERENCE VARIABLES ]]
		public Chunk ChunkParent { get; private set; }
		//public Coordinate Coordinate => ChunkParent.GetCoordinateAtCell(this);
		public Quad MeshQuad => _meshQuad;
		public TYPE Type => _type;
		public Color TypeColor { get; private set; } = Color.white;
		public Vector3 Position
		{
			get
			{
				return Vector3.zero;
				//return ChunkParent.GroundPosition;
			}
		}

		public Vector2Int FaceCoord => _meshQuad.faceCoord;
		public Chunk.FaceDirection FaceType => _meshQuad.faceDirection;
		//public Vector3 Normal => _meshQuad.faceNormal;
		public int Size = WorldGenerationSystem.Instance.Settings.CellSize_inGameUnits;


		public Cell(Chunk chunkParent, Quad meshQuad)
		{
			this.ChunkParent = chunkParent;
			this._meshQuad = meshQuad;
		}

		public void CreateCellMeshObject()
		{
			GameObject cellObject = new GameObject("Cell");
			//Mesh mesh = _meshQuad.GenerateMesh();

			cellObject.transform.position = Position;
			//cellObject.AddComponent<MeshFilter>().mesh = mesh;
			cellObject.AddComponent<MeshRenderer>().material = WorldGenerationSystem.Instance.defaultMaterial;
		}

		public void SetCellType(TYPE newType)
		{
			this._type = newType;
			switch (newType)
			{
				case TYPE.EMPTY: TypeColor = Color.white; break;
				case TYPE.EDGE: TypeColor = Color.red; break;
				case TYPE.CORNER: TypeColor = Color.yellow; break;
				case TYPE.OBSTACLE: TypeColor = Color.black; break;
				case TYPE.SPAWN_POINT: TypeColor = Color.yellow; break;
			}
		}
	}
}


