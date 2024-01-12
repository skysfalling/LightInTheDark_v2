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
        if (parentEntity == null) { return; }

        // Enemy Hit Player
        if (parentEntity.GetComponent<EnemyAI>() != null && other.GetComponent<PlayerController>() != null)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.Hit();
        }

        // Player Hit Enemy
        else if (parentEntity.GetComponent<PlayerController>() != null && other.GetComponent<EnemyAI>() != null)
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            enemy.Hit();
        }

        GameObject particles = Instantiate(hitParticles, this.transform.position, Quaternion.identity);
        Destroy(particles, 2);
        Destroy(gameObject);
    }
}
