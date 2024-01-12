using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool spawnerActive = true;
    public float spawnDelay = 1f; // The delay before spawning a new object

    [Space(10)]
    public GameObject prefab; // The prefab to spawn
    public GameObject spawnedObject; // The object that has been spawned

    [Space(10)]
    public bool spawnOnStart;
    public bool infiniteSpawn;

    [Space(10)]
    public bool destroyOverTime;
    public float destroyDelay = 5;

    [Header("Shooting Spawner")]
    public bool shootingSpawner;
    public Transform fireTarget;
    public float fireForce = 50;
    public Vector2 randomShootDelay = new Vector2(0, 2);


    private void Start()
    {
        if (spawnOnStart) { StartSpawn(); }
    }

    public void StartSpawn()
    {
        spawnerActive = true;
        StartCoroutine(SpawnObjectWithDelay());
    }

    IEnumerator SpawnObjectWithDelay()
    {
        // Check if the object has been spawned already
        if (infiniteSpawn || (!spawnedObject && spawnerActive))
        {
            // random shoot delay
            if (shootingSpawner)
            {
                float randomDelay = Random.Range(randomShootDelay.x, randomShootDelay.y);
                yield return new WaitForSeconds(randomDelay);
            }

            // Spawn the object
            spawnedObject = Instantiate(prefab, transform.position, transform.rotation);

            // shoot object
            if (shootingSpawner) { AddForceToSpawnItem(); }

            // destroy over time
            if (destroyOverTime) { spawnedObject.GetComponent<Item>().Destroy(destroyDelay); }
        }
        else if (spawnedObject)
        {
            // check if been picked up
            Item item = spawnedObject.GetComponent<Item>();
            if (item && item.state != ItemState.FREE) { spawnedObject = null; }
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(spawnDelay);

        // Start the coroutine again to spawn another object with delay
        StartCoroutine(SpawnObjectWithDelay());
    }

    public void DestroySpawnedObject()
    {
        spawnedObject.GetComponent<Item>().Destroy();
        spawnedObject = null;
        spawnerActive = false;
    }

    public void AddForceToSpawnItem()
    {
        if (spawnedObject != null)
        {
            Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (fireTarget.position - spawnedObject.transform.position).normalized;
                rb.AddForce(direction * fireForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (shootingSpawner && fireTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, fireTarget.position);
        }
    }
}
