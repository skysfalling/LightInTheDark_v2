using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmitItemObject : MonoBehaviour
{
    [HideInInspector]
    public LevelManager levelManager;
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public PlayerInventory player;

    public Transform triggerParent;
    public LayerMask playerLayer;
    public bool playerInTrigger = true;
    public float triggerSize = 2f;


    [Header("Submission")]
    public List<ItemType> submissionTypes;
    [Space(10)]
    public List<GameObject> submissionOverflow = new List<GameObject>();
    [Space(10)]
    public bool canSubmit;
    public float submitSpeed = 10; // how fast the submitted item moves
    public GameObject submitEffect;

    [Header("Circle Object")]
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 10f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle


    // Start is called before the first frame update
    public void Start()
    {
        // << INIT VALUES >>
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        gameConsole = levelManager.gameConsole;

        canSubmit = true;

        if (triggerParent == null) { triggerParent = transform; }
    }

    // Update is called once per frame
    public void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        CollectFreeItemsInTrigger();

        RemoveNullValues(submissionOverflow);

        // submit
        SubmissionManager();
    }



    public virtual void SubmissionManager()
    {
        // << REFERENCE PLAYER INVENTORY >> doesnt remove from inventory, focus on submitting one at a time
        if (playerInTrigger && player.inventory.Count > 0)
        {
            List<GameObject> inventory = player.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                Item item = inventory[i].GetComponent<Item>();

                // if item type is allowed
                if (submissionTypes.Contains(item.type) && !submissionOverflow.Contains(item.gameObject))
                {
                    // add to overflow
                    submissionOverflow.Add(inventory[i]);

                    player.RemoveItem(item.gameObject);
                }
            }
        }

        if (submissionOverflow.Count > 0 && canSubmit)
        {
            StartCoroutine(SubmitItem());
        }
    }

    public virtual IEnumerator SubmitItem()
    {
        canSubmit = false;

        // remove from inventory
        GameObject item = submissionOverflow[0];

        item.GetComponent<Item>().state = ItemState.SUBMITTED;

        // << MOVE ITEM TO CENTER >>
        while (item.transform.position != transform.position)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, submitSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Submit Item", item);

        // << SPAWN EFFECT >>
        GameObject effect = Instantiate(submitEffect, transform);
        //submitEffect.GetComponent<ParticleSystem>().startColor = item.GetComponent<SpriteRenderer>().color;
        Destroy(effect, 5);

        // << SUBMIT ITEM >>
        submissionOverflow.Remove(item);

        // destroy item
        player.inventory.Remove(item);
        Destroy(item.gameObject);
        canSubmit = true;
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(triggerParent.position, triggerSize, playerLayer);
        Debug.Log("playerInTrigger " + playerCol);
        if (playerCol) { return true; }
        return false;
    }

    public void CollectFreeItemsInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(triggerParent.position, triggerSize);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            // if free item

            if (col.tag == "Item" && col.GetComponent<Item>())
            {
                Item item = col.GetComponent<Item>();

                // if not in submission overflow
                if (!submissionOverflow.Contains(col.gameObject) &&
                    (col.GetComponent<Item>().state == ItemState.FREE || col.GetComponent<Item>().state == ItemState.THROWN))
                {
                    // add to overflow
                    submissionOverflow.Add(col.gameObject);
                    col.GetComponent<Item>().state = ItemState.FREE;
                }
            }
        }
    }

    public void CircleAroundTransform(List<GameObject> items)
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = triggerParent.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) { continue; }
            items[i].transform.parent = triggerParent;

            float angleRadians = (currCircleAngle + (360f / items.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            items[i].GetComponent<Item>().rb.position = Vector3.Lerp(items[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp
        }
    }

    private void RemoveNullValues(List<GameObject> list)
    {
        list.RemoveAll(item => item == null);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (triggerParent != null) 
        {
            Gizmos.DrawWireSphere(triggerParent.position, triggerSize);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, triggerSize);
        }


    }
}
