using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeviathanAI : MonoBehaviour
{
    PlayerMovement player;
    public List<Transform> pathObjects = new List<Transform>();

    public float moveSpeed;

    private float rotationOffset = 180f;

    [Space(10)]
    public bool playerInTrigger;
    public float triggerSize = 2f;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        // set leviathan to start at first node
        transform.position = pathObjects[0].position;

        StartCoroutine(PathMove(0));
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        if (playerInTrigger)
        {
            player.Slowed(5);
        }
    }

    IEnumerator PathMove(int index)
    {
        Transform target = pathObjects[index];


        while (Vector2.Distance(transform.position, target.position) > 5)
        {
            RotateTowardsTarget(target);

            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        int nextIndex = index + 1;
        if (index == pathObjects.Count - 1)
        {
            nextIndex = 0;
        }

        StartCoroutine(PathMove(nextIndex));
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
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

    void RotateTowardsTarget(Transform target)
    {
        // rotate parent and UI towards throw point
        Vector2 direction = target.position - transform.position;
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, rotation + rotationOffset)), Time.deltaTime);
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerSize);


        // << DRAWN PATH LINE >>
        if (pathObjects.Count < 2)
        {
            return;
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < pathObjects.Count - 1; i++)
        {
            Transform startTransform = pathObjects[i];
            Transform endTransform = pathObjects[i + 1];
            Gizmos.DrawLine(startTransform.position, endTransform.position);
        }


    }
}
