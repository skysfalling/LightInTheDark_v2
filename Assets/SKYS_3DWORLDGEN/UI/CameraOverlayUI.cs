using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraOverlayUI : MonoBehaviour
{
    WorldGenerationStats _worldGenerationStats;

    public TextMeshProUGUI worldStatsTMP;

    // Start is called before the first frame update
    void Start()
    {
        _worldGenerationStats = FindObjectOfType<WorldGenerationStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_worldGenerationStats != null && worldStatsTMP != null)
        { 
            worldStatsTMP.text = _worldGenerationStats.GetWorldStats(); 
        }
    }
}
