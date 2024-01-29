using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralTrigger : MonoBehaviour
{
    public bool playerInTrigger;
    public Vector2 triggerSize = new Vector2(50, 50);

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapBoxAll(transform.position, triggerSize, 0);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, triggerSize);
    }
}
