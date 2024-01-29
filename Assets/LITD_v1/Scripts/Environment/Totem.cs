using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Totem : MonoBehaviour
{
    [HideInInspector]
    public PlayerInventory player;

    public Transform triggerParent;
    public bool playerInTrigger;
    public bool playerIsCollecting;
    public float triggerSize = 2f;

    [Space(10)]
    public bool playerInCenter;
    public float playerCenterRange = 2;

    [Header("Life Force")]
    public bool overflowing;
    public int lifeForce = 60;
    public int maxLifeForce = 60;
    public int deathAmount = -10;

    [Space(10)]
    public int lifeForceSubmitAmount = 1;

    [Space(10)]
    public bool decayActive;
    public float decay_speed = 1;
    public int decayAmount = 1;

    [Header("Submission")]
    public List<ItemType> submissionTypes;
    [Space(10)]
    public List<GameObject> submissionOverflow = new List<GameObject>();
    public bool canSubmit;

    [Header("Sorting Layers")]
    public string blockPlayer = "Block Player";
    public string envFront = "Env Front";
    public SpriteRenderer totemSpriteRenderer;
    public List<SpriteRenderer> glowSprites = new List<SpriteRenderer>();
    // A dictionary to store the original sorting layers of each sprite renderer
    private Dictionary<SpriteRenderer, string> originalSortingLayers = new Dictionary<SpriteRenderer, string>();

    [Header("Lights")]
    public float lightAdjustSpeed = 3;
    [Space(10)]
    public Light2D mainLight;
    public Vector2 mainLightIntensity;
    public Vector2 mainLightRange;
    [Space(10)]
    public Light2D glowLight;
    public Vector2 glowLightIntensity;
    public Vector2 glowLightRange;

    [Header("Particles")]
    public ParticleSystem overflowParticles;
    public ParticleSystem smallParticles;
    public ParticleSystem smallParticles1;
    public ParticleSystem submissionParticles;

    [Header("Door Unlock")]
    public float doorLockPercentage = 0.5f;
    public List<Door> unlockDoors;

    [Header("Circle Object")]
    public float circleSpeed = 10f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle
    private float currCircleAngle = 0f; // Current angle of rotation


    private void Start()
    {
        // << INIT VALUES >>
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        canSubmit = true;

        StartCoroutine(Decay());

    }


    // Update is called once per frame 
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        // if player is in center, collect all items
        if (Vector2.Distance(player.transform.position, triggerParent.position) < playerCenterRange)
        {
            playerIsCollecting = true;

            // move all submission objects to player
            foreach (GameObject obj in submissionOverflow)
            {
                obj.transform.parent = null;

                player.AddItemToInventory(obj);
            }

            submissionOverflow.Clear();
        }
        else
        {
            SubmissionManager();
        }

        // Update overflow
        overflowing = lifeForce >= maxLifeForce;
        PlayOrStopParticles(overflowParticles, overflowing);
        PlayOrStopParticles(smallParticles, lifeForce > (maxLifeForce * 0.2f) && lifeForce <= (maxLifeForce * 0.5f));
        PlayOrStopParticles(smallParticles1, lifeForce > (maxLifeForce * 0.5f));
 
        // lock door
        LockDoors(lifeForce <= maxLifeForce * doorLockPercentage);


        // << GLOW LIGHT >>
        // target percentage
        float targetIntensity = Mathf.Lerp(glowLightIntensity.x, glowLightIntensity.y, (float)lifeForce / (float)maxLifeForce);
        glowLight.intensity = Mathf.Lerp(glowLight.intensity, targetIntensity, Time.deltaTime);

        float targetRange = Mathf.Lerp(glowLightRange.x, glowLightRange.y, (float)lifeForce / (float)maxLifeForce);
        glowLight.pointLightOuterRadius = Mathf.Lerp(glowLight.pointLightOuterRadius, targetRange, lightAdjustSpeed * Time.deltaTime);


        // << MAIN LIGHT >>
        // target percentage
        float mainTargetIntensity = Mathf.Lerp(mainLightIntensity.x, mainLightIntensity.y, (float)lifeForce / (float)maxLifeForce);
        mainLight.intensity = Mathf.Lerp(mainLight.intensity, mainTargetIntensity, Time.deltaTime);

        float mainTargetRange = Mathf.Lerp(mainLightRange.x, mainLightRange.y, (float)lifeForce / (float)maxLifeForce);
        mainLight.pointLightOuterRadius = Mathf.Lerp(mainLight.pointLightOuterRadius, mainTargetRange, lightAdjustSpeed * Time.deltaTime);
    }

    private void PlayOrStopParticles(ParticleSystem particles, bool shouldPlay)
    {
        if (shouldPlay)
        {
            if (!particles.isPlaying)
            {
                particles.Play();
            }
        }
        else
        {
            if (particles.isPlaying)
            {
                particles.Stop();
            }
        }
    }

    public void SubmissionManager()
    {
        // << DRAIN PLAYERS ENTIRE INVENTORY >>
        if (playerInTrigger && player.inventory.Count > 0 && !playerIsCollecting)
        {
            List<GameObject> inventory = player.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                // if item type is allowed
                if (submissionTypes.Contains(inventory[i].GetComponent<Item>().type) && !submissionOverflow.Contains(inventory[i]))
                {
                    // add to overflow
                    submissionOverflow.Add(inventory[i]);

                    // change item state
                    inventory[i].GetComponent<Item>().state = ItemState.FREE;

                    player.RemoveItem(inventory[i]);
                }
            }
        }
        else if (playerInTrigger && player.inventory.Count <= 0)
        {
            playerIsCollecting = true;
        }
        else if (!playerInTrigger)
        {
            playerIsCollecting = false;
        }

        // << REMOVE NOT FREE STATE ITEMS >>
        for (int i = 0; i < submissionOverflow.Count; i++)
        {
            if (submissionOverflow[i].GetComponent<Item>().state != ItemState.FREE)
            {
                submissionOverflow.Remove(submissionOverflow[i]);
            }
        }

        // collect all free items in trigger
        CollectFreeItemsInTrigger();

        // << SUBMISSION OVERFLOW MANAGER >>
        if (submissionOverflow.Count > 0)
        {
            // circle overflow items
            CircleAroundTransform(submissionOverflow);

            if (canSubmit )
            {
                StartCoroutine(SubmitItem());
            }
        }
    }

    public IEnumerator Decay()
    {
        if (decay_speed <= 0) { yield return null; }

        yield return new WaitForSeconds(decay_speed);

        if (lifeForce > deathAmount)
        {
            lifeForce -= decayAmount;
        }

        StartCoroutine(Decay());
    }

    public IEnumerator SubmitItem()
    {

        if (submissionOverflow.Count == 0) { yield return null; }

        canSubmit = false;

        // get item
        Item item = submissionOverflow[0].GetComponent<Item>();

        item.transform.parent = transform; // set parent

        // << MOVE ITEM TO CENTER >>
        while (Vector2.Distance(item.transform.position, transform.position) > 1f)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, Time.deltaTime);
        }

        submissionParticles.GetComponent<ParticleSystem>().Play();
        submissionOverflow.Remove(item.gameObject);

        // add to life force
        lifeForce += lifeForceSubmitAmount;
        //Debug.Log("Totem Submission");

        // destroy item
        player.inventory.Remove(item.gameObject);
        item.Destroy();

        yield return new WaitForSeconds(1);

        canSubmit = true;
    }

    public void LockDoors(bool enabled)
    {
        Debug.Log("Door lock" + enabled);

        foreach (Door door in unlockDoors)
        {
            if (door == null) { continue; }
            door.locked = enabled;
        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(triggerParent.position, triggerSize);
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

    public void CollectFreeItemsInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(triggerParent.position, triggerSize);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            // if free item

            if (col.tag == "Item" && col.GetComponent<Item>() )
            {
                Item item = col.GetComponent<Item>();

                // if not in submission overflow
                if (!submissionOverflow.Contains(col.gameObject) &&
                    (col.GetComponent<Item>().state == ItemState.FREE || col.GetComponent<Item>().state == ItemState.THROWN))
                {
                    // add to overflow
                    submissionOverflow.Add(col.gameObject);
                    col.GetComponent<Item>().state = ItemState.FREE;

                    if (player.inventory.Contains(col.gameObject)) { player.inventory.Remove(col.gameObject); }
                }
            }
        }
    }

    public float GetLifeForcePercentage()
    {
        return (float)lifeForce / (float)maxLifeForce;
    }

    public void CircleAroundTransform(List<GameObject> items)
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = transform.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < items.Count; i++)
        {

            items[i].transform.parent = triggerParent;

            float angleRadians = (currCircleAngle + (360f / items.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            items[i].GetComponent<Item>().rb.position = Vector3.Lerp(items[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {


        // Check if the other collider has a SpriteRenderer component
        if (glowSprites != null && glowSprites.Count > 0)
        {


            // Save the original sorting layers of each sprite renderer
            foreach (SpriteRenderer spriteRenderer in glowSprites)
            {
                if (!originalSortingLayers.ContainsKey(spriteRenderer))
                {
                    originalSortingLayers.Add(spriteRenderer, spriteRenderer.sortingLayerName);
                }
            }

            // Call the NewSortingLayer function with the spriteRenderers and newSortingLayerName parameters
            NewSortingLayer(glowSprites, envFront);

            NewSortingLayer(totemSpriteRenderer, blockPlayer);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the other collider has a SpriteRenderer component
        if (glowSprites != null && glowSprites.Count > 0)
        {
            // Call the ResetSortingLayers function with the spriteRenderers parameter
            ResetSortingLayers(glowSprites);
        }
    }

    private void NewSortingLayer(SpriteRenderer spriteRenderer, string newSortingLayerName)
    {
        // Save the original sorting layer of the sprite renderer
        if (!originalSortingLayers.ContainsKey(spriteRenderer))
        {
            originalSortingLayers.Add(spriteRenderer, spriteRenderer.sortingLayerName);
        }

        spriteRenderer.sortingLayerName = newSortingLayerName;
    }

    private void NewSortingLayer(List<SpriteRenderer> spriteRenderers, string newSortingLayerName)
    {
        // Loop through each sprite renderer in the list
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            NewSortingLayer(spriteRenderer, newSortingLayerName);
        }
    }

    private void ResetSortingLayers(List<SpriteRenderer> spriteRenderers)
    {

        if (originalSortingLayers.ContainsKey(totemSpriteRenderer))
        {
            // Set the sprite renderer's sorting layer to its original value
            totemSpriteRenderer.sortingLayerName = originalSortingLayers[totemSpriteRenderer];

            // Remove the sprite renderer from the original sorting layers dictionary
            originalSortingLayers.Remove(totemSpriteRenderer);
        }

        // Loop through each sprite renderer in the list
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            // Check if the sprite renderer has an original sorting layer saved
            if (originalSortingLayers.ContainsKey(spriteRenderer))
            {
                // Set the sprite renderer's sorting layer to its original value
                spriteRenderer.sortingLayerName = originalSortingLayers[spriteRenderer];

                // Remove the sprite renderer from the original sorting layers dictionary
                originalSortingLayers.Remove(spriteRenderer);
            }
        }
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

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(triggerParent.position, playerCenterRange);

    }
}
