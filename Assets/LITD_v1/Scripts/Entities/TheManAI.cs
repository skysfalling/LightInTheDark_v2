using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum TheManState { IDLE , FOLLOW, CHASE , RETREAT , GRABBED_PLAYER , PLAYER_CAPTURED }

public class TheManAI : MonoBehaviour
{
    PlayerInventory player;
    [HideInInspector]
    public PlayerMovement playerMovement;
    Rigidbody2D rb;
    public SpriteRenderer spriteParent;
    public GameObject deathEffect;

    [Space(10)]
    public TheManState state = TheManState.IDLE;
    private Vector2 startPosition;

    [Space(10)]
    public bool playerInOuterTrigger;
    public float outerTriggerSize = 25f;
    [Space(5)]
    public bool playerInInnerTrigger;
    public float innerTriggerSize = 10f;

    [Header("Move")]
    public float distToPlayer;

    [Tooltip("Influence the direction of the ai move. Multiplies the axisWeight with the target move position.")]
    public Vector2 movingAxisWeight = new Vector2(1, 1.5f);
    public float chaseSpeed = 0.2f;
    public float followSpeed = 0.3f;
    public float retreatSpeed = 0.4f;
    public float grabSpeed = 0.6f;

    [Header("Grab")]
    public Transform grabPoint;
    public float timeToGrab = 2;
    private float timeToGrabTimer;
    public float grabDelay = 2; // delay next grab
    public bool grabStarted;
    [Space(5)]
    public float timeToCapture = 5;
    private float timeToCaptureTimer;
    [Space(5)]
    public int breakFree_struggleCount;

    [Header("Light Decay")] // if the player gets close, the man slowly starts to "melt" from the light
    public Light2D light;
    public Color startColor = Color.white;
    public Color endColor = Color.grey;
    public Vector2 intensityRange = new Vector2(0, 10f);
    
    [Space(5)]
    public int cur_health = 5;
    public int max_health = 5;
    private bool lifeCoroutineStarted;
    private bool lifeRestoreStarted;

    [Space(5)]
    int decayAmount = 1;
    public int retreatLightOrbCount = 3; // light orbs needed in player inventory to start decay
    public float lifeDecayDelay = 1;



    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        playerMovement = player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        state = TheManState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        playerInOuterTrigger = IsPlayerInTrigger(outerTriggerSize);
        playerInInnerTrigger = IsPlayerInTrigger(innerTriggerSize);

        // << UPDATE LIFE VALUES BASED ON PLAYER DISTANCE AND INVENTORY COUNT >>
        if (!lifeCoroutineStarted)
        {
            if (player.inventory.Count >= retreatLightOrbCount )
            {
                // inner trigger decay
                if (playerInInnerTrigger) { StartCoroutine(DrainLife(lifeDecayDelay)); }
            }
            else if (player.inventory.Count >= retreatLightOrbCount * 2)
            {
                // inner trigger decays twice as fast
                if (playerInInnerTrigger) { StartCoroutine(DrainLife(lifeDecayDelay * 0.5f)); }
            }
            // default health restore
            else if (cur_health < max_health) { StartCoroutine(RestoreLife()); }
        }

        // update dist To Player
        distToPlayer = Vector2.Distance(transform.position, player.transform.position);



        StateMachine();
    }

    void StateMachine()
    {
        // move differently depending on state
        switch (state)
        {
            case TheManState.IDLE:
                // move back to startPosition
                rb.MovePosition(Vector2.MoveTowards(transform.position, startPosition, chaseSpeed * Time.deltaTime));
                
                // follow player
                if (playerInOuterTrigger) { state = TheManState.FOLLOW; }
                
                break;

            case TheManState.FOLLOW:


                // change state
                if (distToPlayer < outerTriggerSize * 0.75f)
                {
                    // determine retreat or chase
                    if (player.inventory.Count >= retreatLightOrbCount)
                    {
                        state = TheManState.RETREAT;
                        break;
                    }
                    else
                    {
                        state = TheManState.CHASE;
                        break;
                    }
                }

                // Calculate the direction vector towards the player
                Vector2 direction = (player.transform.position - transform.position).normalized;

                // Apply movingAxisWeight to the direction vector
                direction.x *= movingAxisWeight.x;
                direction.y *= movingAxisWeight.y;

                // Calculate the new position using Vector2.MoveTowards()
                Vector2 newPosition = Vector2.MoveTowards(transform.position, transform.position + (Vector3)direction, followSpeed * Time.deltaTime);

                // Move towards the player using the modified speed values
                rb.MovePosition(newPosition);

                break;

            case TheManState.CHASE:
                // chase player
                rb.MovePosition(Vector2.MoveTowards(transform.position, player.transform.position, chaseSpeed * Time.deltaTime));

                // << IF PLAYER IN INNER TRIGGER, START GRAB SEQUENCE >>
                if (playerInInnerTrigger)
                {
                    if (!grabStarted)
                    {
                        timeToGrabTimer += Time.deltaTime;

                        if (timeToGrabTimer >= timeToGrab)
                        {
                            state = TheManState.GRABBED_PLAYER;
                            StartCoroutine(GrabPlayer());
                            timeToGrabTimer = 0;
                        }
                    }
                }
                else { timeToGrabTimer = 0; }

                break;

            case TheManState.RETREAT:
                // run away from player
                Vector3 oppositeDirection = (transform.position - player.transform.position) * -1f;
                rb.MovePosition(Vector2.MoveTowards(transform.position, transform.position - oppositeDirection, retreatSpeed * Time.deltaTime));

                // change state
                if (distToPlayer > outerTriggerSize * 0.75f)
                {
                    state = TheManState.FOLLOW;
                    break;
                }


                break;
        }
    }

    IEnumerator DrainLife(float decayDelay)
    {
        lifeCoroutineStarted = true;

        cur_health--; // drain life
        
        // check for death
        if (cur_health <= 0) { Death(); yield return null; }

        yield return new WaitForSeconds(decayDelay); // wait for speed

        lifeCoroutineStarted = false;
    }

    IEnumerator RestoreLife()
    {
        lifeCoroutineStarted = true;

        cur_health++; // restore life

        yield return new WaitForSeconds(lifeDecayDelay); // wait for speed

        lifeCoroutineStarted = false;
    }

    IEnumerator GrabPlayer()
    {
        grabStarted = true;

        playerMovement.state = PlayerState.GRABBED;
        state = TheManState.GRABBED_PLAYER;
        timeToCaptureTimer = 0; // set capture timer

        // [[ while player hasn't broken free ]]
        while (playerMovement.struggleCount < breakFree_struggleCount && 
            state == TheManState.GRABBED_PLAYER && playerMovement.state == PlayerState.GRABBED)
        {
            // HOLD PLAYER
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            player.GetComponentInChildren<Rigidbody2D>().velocity = Vector3.zero;
            player.transform.position = grabPoint.position;

            // PLAYER CAPTURE
            timeToCaptureTimer += Time.deltaTime;
            if (timeToCaptureTimer >= timeToCapture)
            {
                state = TheManState.PLAYER_CAPTURED;
                timeToCaptureTimer = 0;

                grabStarted = false;
                
            }

            yield return null;
        }

       //[[ PLAYER BROKE FREE ]]
        if (state != TheManState.PLAYER_CAPTURED)
        {
            player.transform.parent = null;
            playerMovement.state = PlayerState.IDLE;

            state = TheManState.RETREAT;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;


            yield return new WaitForSeconds(grabDelay);

            grabStarted = false;
        }


    }

    void UpdateVisualDecay()
    {
        spriteParent.color = Color.Lerp(endColor, startColor, (float)cur_health / (float)max_health);

    }


    public bool IsPlayerInTrigger(float size)
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, size);
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

    public void Death()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, outerTriggerSize);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, innerTriggerSize);
    }
}
