using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraOverlayUI : MonoBehaviour
{
    WorldStatTracker _worldGenerationStats;
    WorldChunkDebug _worldChunkDebug;

    public TextMeshProUGUI worldStatsTMP;
    public TextMeshProUGUI chunkStatsTMP;

    // Start is called before the first frame update
    void Start()
    {
        _worldGenerationStats = FindObjectOfType<WorldStatTracker>();
        _worldChunkDebug = FindObjectOfType<WorldChunkDebug>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_worldGenerationStats != null && worldStatsTMP != null)
        { 
            worldStatsTMP.text = _worldGenerationStats.GetWorldStats(); 
        }

        WorldChunk selectedChunk = _worldChunkDebug.selected_worldChunk;
        if (_worldChunkDebug != null && chunkStatsTMP != null &&
            selectedChunk != null && selectedChunk.initialized)
        {
            chunkStatsTMP.text = _worldChunkDebug.GetChunkStats(selectedChunk);
        }
    }
}
