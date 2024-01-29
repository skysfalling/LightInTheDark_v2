using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    GameManager gameManager;
    CameraManager camManager;
    private bool camReset;
    public Vector2 zoneSize;
    public bool playerInTrigger = false;

    [Header("Active Focus")]
    public int customZoom;
    

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        camManager = gameManager.camManager;
    }

    void Update()
    {
        // << PLAYER IN TRIGGER >>
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, zoneSize, 0f);
        playerInTrigger = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                playerInTrigger = true;
                break;
            }
        }


        if (playerInTrigger)
        {
            camReset = true;
            camManager.NewActiveFocus(transform, customZoom);
        }
        else if (camReset)
        {
            camReset = false;
            camManager.Player();
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, zoneSize);
    }
}
