using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraOverlayUI : MonoBehaviour
{
    WorldStatTracker _worldGenerationStats;
    WorldChunkMap _worldChunkMap;

    public TextMeshProUGUI worldStatsTMP;
    public TextMeshProUGUI chunkStatsTMP;

    // Start is called before the first frame update
    void Start()
    {
        _worldGenerationStats = FindObjectOfType<WorldStatTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_worldGenerationStats != null && worldStatsTMP != null)
        { 
            worldStatsTMP.text = _worldGenerationStats.GetWorldStats(); 
        }

        /*
        WorldChunk selectedChunk = _worldChunkMap.selected_worldChunk;
        if (_worldChunkMap != null && chunkStatsTMP != null &&
            selectedChunk != null && selectedChunk.initialized)
        {
            chunkStatsTMP.text = _worldChunkMap.GetChunkStats(selectedChunk);
        }
        */
    }
}
