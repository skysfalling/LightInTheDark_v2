using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardHolder : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject rewardPrefab;
    GameObject spawnedObject;

    [Space(10)]
    public bool locked;
    public bool rewardCollected;

    [Space(10)]
    public bool playerInTrigger;
    public float triggerSize = 5f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        if (!playerInTrigger)
        {
            SpawnReward();
        }
        else if (playerInTrigger && locked)
        {
            DestroyReward();
        }
        else if (spawnedObject == null || spawnedObject.GetComponent<Item>().state == ItemState.PLAYER_INVENTORY)
        {
            rewardCollected = true;
        }
    }

    void SpawnReward()
    {
        // Check if the object has been spawned already
        if (spawnedObject == null)
        {
            // Spawn the object
            spawnedObject = Instantiate(rewardPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    void DestroyReward()
    {
        // Check if the object has been spawned already
        if (spawnedObject != null)
        {
            // Spawn the object
            Destroy(spawnedObject);
        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(spawnPoint.position, triggerSize);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            if (col.tag == "Player")
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(spawnPoint.position, triggerSize);
    }
}
