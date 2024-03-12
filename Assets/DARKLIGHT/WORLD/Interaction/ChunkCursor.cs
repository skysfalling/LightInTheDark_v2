using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight.World.Interaction
{
	using Generation;
	using Builder;

	public class WorldChunkCursor : MonoBehaviour
	{
		Chunk _activeChunk;
		Dictionary<Chunk, GameObject> _activeChunkCursors = new();

		public GameObject cursorPrefab;


		#region == Mouse Input ======================================= ///////
		public void MouseHoverInput(Vector3 worldPosition)
		{
			//WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
			//UpdateHoverCursorCell(closestCell);
		}

		public void MouseSelectInput(Vector3 worldPosition)
		{
			//WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
			//SelectChunk(closestCell.GetChunk());
		}
		#endregion

		void SelectChunk(Chunk chunk)
		{
			if (chunk == null) { return; }
			if (_activeChunk != null && chunk == _activeChunk) { return; }

			if (_activeChunk != null)
			{
				// Remove old active chunk
				RemoveCursorAt(_activeChunk);
			}

			// New active chunk
			_activeChunk = chunk;
			transform.position = _activeChunk.GroundPosition;
			CreateCursorAt(_activeChunk);
		}

		void CreateCursorAt(Chunk chunk)
		{
			GameObject cursor = null;

			if (_activeChunkCursors.ContainsKey(chunk) && chunk != _activeChunk)
			{
				RemoveCursorAt(chunk);
			}

			// Create New Cursor
			cursor = Instantiate(cursorPrefab, chunk.GroundPosition, Quaternion.identity);

			int chunkCursorWidth = (WorldBuilder.Settings.ChunkWidth_inCellUnits + 1) * WorldBuilder.Settings.CellSize_inGameUnits;
			cursor.transform.localScale = new Vector3(chunkCursorWidth, 1, chunkCursorWidth);

			cursor.name = $"{cursorPrefab.name} :: Chunk {chunk.Coordinate.ValueKey}";
			_activeChunkCursors[chunk] = cursor;

		}

		void RemoveCursorAt(Chunk cell)
		{
			if (cell != null && _activeChunkCursors.ContainsKey(cell))
			{
				Destroy(_activeChunkCursors[cell]);
				_activeChunkCursors.Remove(cell);
			}
		}
	}
}