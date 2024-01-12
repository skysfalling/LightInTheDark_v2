using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CleansingCrystal : SubmitItemObject
{
    [Header("Rift Color Change")]
    public SpriteRenderer riftSprite;
    private Color riftOriginalColor;
    public Color riftColorChange = Color.white;
    public float riftColorChangeSpeed = 0.1f;

    [Header("Item Color Change")]
    public Color itemColorChange = Color.white;
    public float itemColorChangeSpeed = 0.5f;

    [Header("Conversion")]
    public GameObject convertItem;
    public float conversionDelay = 2;
    public Transform spawnTarget;
    public float spawnTargetRadius = 5;
    [HideInInspector]
    public bool itemConverted;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        riftOriginalColor = riftSprite.color;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();


        // << RIFT COLOR CHANGE >>
        if (playerInTrigger)
        {
            riftSprite.color = Color.Lerp(riftSprite.color, riftColorChange, riftColorChangeSpeed * Time.deltaTime);
        }
        else
        {
            riftSprite.color = Color.Lerp(riftSprite.color, riftOriginalColor, riftColorChangeSpeed * Time.deltaTime);

        }

    }

    public override IEnumerator SubmitItem()
    {
        canSubmit = false;

        // remove from inventory
        GameObject itemObject = submissionOverflow[0];
        player.inventory.Remove(itemObject);
        itemObject.transform.parent = transform; // set parent
        itemObject.GetComponent<Item>().state = ItemState.SUBMITTED;

        // change color
        SpriteRenderer sprite = itemObject.GetComponent<SpriteRenderer>();
        Color startColor = sprite.color;
        Light2D light = itemObject.GetComponent<Light2D>();

        // << MOVE ITEM TO CENTER >>
        while ( Vector2.Distance(itemObject.transform.position, transform.position) > 5 && playerInTrigger )
        {
            itemObject.transform.position = Vector3.MoveTowards(itemObject.transform.position, transform.position, submitSpeed * Time.deltaTime);

            sprite.color = Color.Lerp(sprite.color, itemColorChange, itemColorChangeSpeed * Time.deltaTime);

            if (light) { light.color = sprite.color; }

            yield return null;
        }

        // if player not in trigger, add back to inventory
        if (!playerInTrigger && Vector2.Distance(itemObject.transform.position, transform.position) > 5)
        {
            player.inventory.Add(itemObject);
            itemObject.transform.parent = player.transform;
            sprite.color = startColor;

            submissionOverflow.Clear();

            canSubmit = true;
            yield return null;
        }
        else
        {
            Debug.Log("Submit Item", itemObject);

            // << SUBMIT ITEM >>
            submissionOverflow.Remove(itemObject);

            // destroy item
            itemObject.GetComponent<Item>().Destroy();
            canSubmit = true;

            itemConverted = true;

            // << CONVERSION >>
            yield return new WaitForSeconds(conversionDelay);

            // create new item
            GameObject newItem = Instantiate(convertItem, transform.position, Quaternion.identity);

            // << SPAWN EFFECT >>
            submitEffect.GetComponent<ParticleSystem>().startColor = riftColorChange;
            GameObject effect = Instantiate(submitEffect, transform);
            Destroy(effect, 5);


            // get random point
            Vector3 randomPoint = spawnTarget.position + (Vector3)Random.insideUnitCircle * spawnTargetRadius;

            // << MOVE NEW ITEM TO CENTER >>
            while (newItem != null && newItem.transform.position != randomPoint && newItem.GetComponent<Item>().state != ItemState.PLAYER_INVENTORY)
            {
                newItem.transform.position = Vector3.MoveTowards(newItem.transform.position, randomPoint, submitSpeed * Time.deltaTime);
                yield return null;
            }
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnTarget.position, spawnTargetRadius);

    }
}
