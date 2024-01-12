using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//                                     100 >    100-75, 74-50, 49-25,  25-0,     < -1 
public enum FlowerState { HEALED, OVERFLOWING, HEALTHY, OKAY, SICK, NEAR_DEATH, DEAD}
public class LifeFlower : SubmitItemObject
{
    [HideInInspector]
    public LifeFlowerConsole console;
    [HideInInspector]
    public LifeFlowerAnimator anim;

    [Header("Flower Values")]
    public FlowerState state;

    [Space(10)]
    public int lifeForce = 60;
    public int maxLifeForce = 60;
    public int deathAmount = -10;

    [Space(10)]
    public bool decayActive;
    public float decay_speed = 1;

    [Space(10)]
    public float darklightPanicTime = 5;

    [Header("Flower Lines")]
    public List<string> damageReactions;

    public void Awake()
    {
        console = GetComponent<LifeFlowerConsole>();
        anim = GetComponent<LifeFlowerAnimator>();
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();



        StartCoroutine(Decay());

    }

    // Update is called once per frame 
    new void Update()
    {
        base.Update();

        FlowerStateMachine();

        SubmissionManager();

    }

    void FlowerStateMachine()
    {
        if (state == FlowerState.HEALED) { return; }

        // << SET STATES BASED ON HEALTH >>
        // 100 >
        if (lifeForce > maxLifeForce) { state = FlowerState.OVERFLOWING; }
        // 100 - 75
        else if (lifeForce > maxLifeForce * 0.75f) { state = FlowerState.HEALTHY; }
        // 74 - 50
        else if (lifeForce > maxLifeForce * 0.5f) { state = FlowerState.OKAY; }
        // 49 - 25
        else if (lifeForce > maxLifeForce * 0.25f) { state = FlowerState.SICK; }
        // 24 - 0
        else if (lifeForce > 0) { state = FlowerState.NEAR_DEATH; }
        // < 0
        else { state = FlowerState.DEAD; }
    }

    public override void SubmissionManager()
    {
        // << DRAIN PLAYERS ENTIRE INVENTORY >>
        if (playerInTrigger && player.inventory.Count > 0)
        {
            List<GameObject> inventory = player.inventory;

            RemoveNullValues(player.inventory);

            for (int i = 0; i < inventory.Count; i++)
            {
                // if item type is allowed and not already in overflow
                if (submissionTypes.Contains(inventory[i].GetComponent<Item>().type) &&
                    !submissionOverflow.Contains(inventory[i]))
                {
                    // add to overflow
                    submissionOverflow.Add(inventory[i]);

                    player.RemoveItem(inventory[i]);
                }
            }
        }

        // << SUBMISSION OVERFLOW MANAGER >>
        if (submissionOverflow.Count > 0)
        {
            // circle overflow items
            CircleAroundTransform(submissionOverflow);

            // remove null values in the list
            RemoveNullValues(submissionOverflow);

            if (canSubmit && state != FlowerState.OVERFLOWING)
            {
                StartCoroutine(SubmitItem());
            }
        }
    }

    public override IEnumerator SubmitItem()
    {

        if (submissionOverflow.Count == 0) { yield return null; }
        if (submissionOverflow[0] == null) { yield return null; }

        canSubmit = false;

        // get item
        Item item = submissionOverflow[0].GetComponent<Item>();
        submissionOverflow.RemoveAt(0);

        item.transform.parent = transform; // set parent

        // << MOVE ITEM TO CENTER >>
        while (Vector2.Distance(item.transform.position, transform.position) > 5f)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, submitSpeed * Time.deltaTime);
        }

        // add to life force
        lifeForce += item.lifeForce;

        if (item.lifeForce < 0) { DamageReaction(); }

        // << SPAWN EFFECT >>
        submitEffect.GetComponent<ParticleSystem>().startColor = item.GetComponent<SpriteRenderer>().color;
        GameObject effect = Instantiate(submitEffect, transform);
        Destroy(effect, 5);

        // destroy item
        item.Destroy();

        yield return new WaitForSeconds(1);

        canSubmit = true;
    }

    public IEnumerator Decay()
    {
        if (decay_speed <= 0) { yield return null; }

        yield return new WaitForSeconds(decay_speed);

        if (decayActive)
        {
            lifeForce--;
        }

        if (!levelManager.IsEndOfLevel())
        {
            StartCoroutine(Decay());
        }

    }

    public void DamageReaction()
    {
        anim.SpawnAggressiveBurstEffect();
        levelManager.camManager.ShakeCamera();
        console.NewMessage(GetRandomLine(damageReactions));

        levelManager.player.Panic(darklightPanicTime);
    }

    public string GetRandomLine(List<string> lines)
    {
        if (lines.Count == 0) { return ""; }

        return lines[Random.Range(0, lines.Count)];
    }

    public bool IsOverflowing()
    {
        return state == FlowerState.OVERFLOWING;
    }

    public bool IsDead()
    {
        return state == FlowerState.DEAD;
    }

    private void RemoveNullValues(List<GameObject> list)
    {
        list.RemoveAll(item => item == null);
    }
}
