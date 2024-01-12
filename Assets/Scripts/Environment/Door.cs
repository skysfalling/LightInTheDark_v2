using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool locked;
    public bool unlockedByPlayer;
    public bool lockBehindPlayer;

    [Space(10)]
    public bool playerPassedThrough;

    [Space(10)]
    public Transform doorTrigger;
    public Vector2 doorTriggerSize = new Vector2(50, 50);
    public bool playerInTrigger;

    [Space(10)]
    public Transform door;
    public Transform doorCloseTarget;
    public Transform doorOpenTarget;
    public float doorSpeed = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        // << PLAYER UNLOCK >>
        if (unlockedByPlayer)
        {
            locked = !IsPlayerInTrigger();
        }

        // << PLAYER PASS THROUGH >>
        if (!playerPassedThrough && playerInTrigger)
        {
            playerPassedThrough = true;
        }
        else if (playerPassedThrough && !playerInTrigger && lockBehindPlayer)
        {
            locked = true;
        }


        // << LOCK MECHANIC >>
        if (!locked)
        {
            //open door
            door.transform.position = Vector2.MoveTowards(door.transform.position, doorOpenTarget.transform.position, doorSpeed * Time.deltaTime);
        }
        else
        {
            // close door
            door.transform.position = Vector2.MoveTowards(door.transform.position, doorCloseTarget.transform.position, doorSpeed * Time.deltaTime);

        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapBoxAll(doorTrigger.position, doorTriggerSize, 0);
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

    private void OnDrawGizmosSelected()
    {
        if (doorTrigger != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(doorTrigger.position, doorTriggerSize);
        }
    }
}
