using System.Collections;
using System.Collections.Generic;
using Darklight.ThirdDimensional.Generation;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    WorldGeneration _worldGeneration = WorldGeneration.Instance;

    // Start is called before the first frame update
    void Start()
    {
        if (_worldGeneration != null && _worldGeneration.Initialized)
        {
            _worldGeneration.ResetGeneration();
        }


    }

    public void StartScene(){



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
