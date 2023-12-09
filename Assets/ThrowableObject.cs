using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    public GameObject hitParticles;
    [HideInInspector] public GameObject parentEntity;

    private void Start()
    {
        Destroy(gameObject, 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == parentEntity) { return; }

        // Hit Player
        if (other.GetComponent<PlayerController>() != null)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.Hit();
        }

        // Hit Enemy
        else if (other.GetComponent<EnemyAI>() != null)
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            enemy.Hit();
        }

        Destroy(gameObject);
    }
}
