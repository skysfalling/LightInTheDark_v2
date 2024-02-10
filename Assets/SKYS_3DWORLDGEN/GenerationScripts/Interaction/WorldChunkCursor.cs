using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkCursor : MonoBehaviour
{
    WorldChunk _activeChunk;
    Dictionary<WorldChunk, GameObject> _activeChunkCursors = new();

    public GameObject cursorPrefab;
    

    #region == Mouse Input ======================================= ///////
    public void MouseHoverInput(Vector3 worldPosition)
    {
        WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        //UpdateHoverCursorCell(closestCell);
    }

    public void MouseSelectInput(Vector3 worldPosition)
    {
        WorldCell closestCell = WorldCellMap.Instance.FindClosestCellTo(worldPosition);
        SelectChunk(closestCell.GetChunk());
    }
    #endregion

    void SelectChunk(WorldChunk chunk)
    {
        if (chunk == null) { return; }
        if (_activeChunk != null && chunk == _activeChunk) { return; }

        if (_activeChunk != null)
        {
            // Remove old active chunk
            RemoveCursorAt(_activeChunk);
            WorldCellMap.Instance.Debug_DestroyChunkLocalCells(_activeChunk);
        }

        // New active chunk
        _activeChunk = chunk;
        transform.position = _activeChunk.worldCoord.WorldPosition;
        CreateCursorAt(_activeChunk);
        WorldCellMap.Instance.Debug_ShowChunkLocalCells(_activeChunk);
    }

    void CreateCursorAt(WorldChunk chunk)
    {
        GameObject cursor = null;

        if (_activeChunkCursors.ContainsKey(chunk) && chunk != _activeChunk)
        {
            RemoveCursorAt(chunk);
        }

        // Create New Cursor
        cursor = Instantiate(cursorPrefab, chunk.worldCoord.WorldPosition, Quaternion.identity);

        int chunkCursorWidth = (WorldGeneration.ChunkArea.x + 1) * WorldGeneration.CellSize;
        cursor.transform.localScale = new Vector3(chunkCursorWidth, 1, chunkCursorWidth);

        cursor.name = $"{cursorPrefab.name} :: Chunk {chunk.worldCoord}";
        _activeChunkCursors[chunk] = cursor;

    }

    void RemoveCursorAt(WorldChunk cell)
    {
        if (cell != null && _activeChunkCursors.ContainsKey(cell))
        {
            Destroy(_activeChunkCursors[cell]);
            _activeChunkCursors.Remove(cell);
        }
    }
}
