using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEntityManager : MonoBehaviour
{
    public static WorldEntityManager Instance;
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public float tickSpeed = 1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
