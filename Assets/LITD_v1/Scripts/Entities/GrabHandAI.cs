using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandState { IDLE, TRACKING, ATTACK, GRAB, GRAB_BROKEN, TRAVEL, PLAYER_CAPTURED }

public class GrabHandAI : MonoBehaviour
{
    PlayerMovement player;
    public HandState state = HandState.IDLE;

    public Transform handHome;

    [Header("Trigger")]
    public Transform triggerParent;
    public bool playerInTrigger;
    public float triggerSize = 5;
    public LayerMask playerLayer;

    [Header("Tracking")]
    public bool x_axis;
    public bool y_axis;
    public bool trackingStarted;
    [Space(5)]
    public float trackingSpeed;
    public float trackingTime;

    [Header("Attack")]
    public bool canAttack = true;
    public bool attackStarted;
    public float attackSpeed = 10;
    public float attackDelay = 1.5f;
    [Space(5)]
    public Vector2 attackPoint;
    public float attackPointRange = 5;

    [Header("Grab")]
    public bool grabStarted;
    public float grab_pullBackSpeed;
    public int breakFree_struggleCount = 4;

    [Header("Player Captured")]
    public bool captureStarted;
    public Transform capturePoint;

    [Header("Travel Hand")]
    public bool isTravelHand;
    public bool travelStarted;
    public Transform travelDestination;


    [Header("Animation Gameobjects")]
    public GameObject idleSprite;
    public GameObject trackingSprite;
    public GameObject attackSprite;
    public GameObject grabSprite;
    public GameObject grab_brokenSprite;
    public GameObject player_capturedSprite;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        StateMachine();
        AnimationStateMachine();
    }

    #region AI STATES =======================================
    void StateMachine()
    {

        switch (state)
        {
            case HandState.IDLE:

                if (playerInTrigger) { state = HandState.TRACKING; }
                else
                {
                    // move to home position
                    transform.position = Vector3.Lerp(transform.position, handHome.position, trackingSpeed * Time.deltaTime);
                }

                break;

            case HandState.TRACKING:

                if (!playerInTrigger) { state = HandState.IDLE; trackingStarted = false; }
                else if (!trackingStarted)
                {
                    StartCoroutine(Tracking());
                }

                break;

            case HandState.ATTACK:

                if (!attackStarted)
                {
                    StartCoroutine(Attacking(attackPoint, attackSpeed, attackPointRange));
                }

                break;

            case HandState.GRAB:

                if (!grabStarted)
                {
                    StartCoroutine(Grabbing());
                }

                break;

            case HandState.PLAYER_CAPTURED:

                if (!captureStarted)
                {
                    StartCoroutine(PlayerCapture());
                }
                break;

            case HandState.TRAVEL:
                if (!travelStarted)
                {
                    StartCoroutine(TravelRoutine());
                }
                break;


            default:
                break;
        }
    }

    IEnumerator Tracking()
    {
        float trackingTimer = 0f;
        trackingStarted = true;

        // continue following the player for the specified amount of time
        while (trackingTimer < trackingTime && playerInTrigger)
        {
            if (x_axis)
            {
                Vector3 targetPosition = new Vector3(player.transform.position.x, handHome.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, trackingSpeed * Time.deltaTime);
            }
            else if (y_axis)
            {
                Vector3 targetPosition = new Vector3(handHome.position.x, player.transform.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, trackingSpeed * Time.deltaTime);
            }

            trackingTimer += Time.deltaTime;

            attackPoint = player.transform.position;
            
            yield return null;

        }

        if (canAttack)
        {
            // ATTACK if player still in trigger
            state = HandState.ATTACK;
        }

        trackingStarted = false;

    }

    IEnumerator Attacking(Vector2 attackPoint, float attackSpeed, float attackPointRange)
    {
        attackStarted = true;

        // while hand is not at player position
        while (Vector2.Distance(transform.position, attackPoint) > 2)
        {
            transform.position = Vector3.Lerp(transform.position, attackPoint, attackSpeed * Time.deltaTime);
            yield return null;
        }

        // if player in attack point range , grab
        if (Vector2.Distance(player.transform.position, attackPoint) < attackPointRange)
        {
            state = HandState.GRAB;
            player.state = PlayerState.GRABBED;
        }
        else
        {
            state = HandState.IDLE;
        }

        // grab player
        attackStarted = false;
    }

    IEnumerator Grabbing()
    {
        grabStarted = true;
        player.transform.parent = transform;
        player.transform.position = transform.position;

        // move hand back to home position AND player hasn't broken free
        while (Vector2.Distance(transform.position, handHome.position) > 5 && player.struggleCount < breakFree_struggleCount)
        {

            transform.position = Vector3.Lerp(transform.position, handHome.position, grab_pullBackSpeed * Time.deltaTime);
            
            player.transform.localPosition = Vector3.zero;


            yield return null;
        }

        // if broken free, release player
        if (player.struggleCount >= breakFree_struggleCount)
        {
            player.transform.parent = null;
            player.state = PlayerState.IDLE;

            state = HandState.IDLE;
        }
        else if (isTravelHand)
        {
            state = HandState.TRAVEL;
            player.state = PlayerState.INACTIVE;

        }
        else
        {
            state = HandState.PLAYER_CAPTURED;
            player.state = PlayerState.INACTIVE;

        }


        grabStarted = false;


    }

    IEnumerator PlayerCapture()
    {
        captureStarted = true;
        player.transform.parent = transform;
        player.transform.position = transform.position;

        // move hand back to home position AND player hasn't broken free
        while (Vector2.Distance(transform.position, capturePoint.position) > 10)
        {
            transform.position = Vector3.Lerp(transform.position, capturePoint.position, attackSpeed * Time.deltaTime);

            yield return null;
        }

        captureStarted = false;

    }

    IEnumerator TravelRoutine()
    {
        travelStarted = true;

        player.transform.parent = transform;
        player.transform.position = transform.position;

        // << MOVE HAND TO TRAVEL DESTINATION >>
        while (Vector2.Distance(transform.position, travelDestination.position) > 10)
        {
            transform.position = Vector3.Lerp(transform.position, travelDestination.position, trackingSpeed * Time.deltaTime);

            yield return null;
        }

        // let go of player
        player.transform.parent = null;
        player.GetComponent<PlayerMovement>().moveTarget = player.transform.position;
        player.GetComponent<PlayerMovement>().state = PlayerState.IDLE;


        // << MOVE HAND BACK TO START >>
        state = HandState.IDLE;

        travelStarted = false;

        yield return null;

    }


    public void OverrideAttackPlayer(float attackSpeed, float attackRange)
    {
        attackStarted = true;

        // ATTACK if player still in trigger
        state = HandState.ATTACK;
        trackingStarted = false;

        StartCoroutine(Attacking(player.transform.position, attackSpeed, attackRange));
    }

    public void DoomAttackPlayer()
    {
        attackStarted = true;

        // ATTACK if player still in trigger
        state = HandState.ATTACK;
        trackingStarted = false;

        StartCoroutine(Attacking(player.transform.position, 10, 100));

        grab_pullBackSpeed *= 2;
        breakFree_struggleCount = 9999;
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(triggerParent.position, triggerSize, playerLayer);
        Debug.Log("playerInTrigger " + playerCol);
        if (playerCol) { return true; }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (triggerParent != null)
        {
            Gizmos.DrawWireSphere(triggerParent.position, triggerSize);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint, attackPointRange);

    }

    #endregion

    #region ANIMATION STATES =======================================

    void AnimationStateMachine()
    {
        switch (state)
        {
            case HandState.IDLE:
                SetHandState(HandState.IDLE);
                break;
            case HandState.TRACKING:
                SetHandState(HandState.TRACKING);
                break;
            case HandState.ATTACK:
                SetHandState(HandState.ATTACK);
                break;
            case HandState.GRAB:
                SetHandState(HandState.GRAB);
                break;
            case HandState.GRAB_BROKEN:
                SetHandState(HandState.GRAB_BROKEN);
                break;
            case HandState.PLAYER_CAPTURED:
                SetHandState(HandState.PLAYER_CAPTURED);
                break;
            default:
                break;
        }
    }


    void SetHandState(HandState newState)
    {
        // Disable all game objects
        idleSprite.SetActive(false);
        trackingSprite.SetActive(false);
        attackSprite.SetActive(false);
        grabSprite.SetActive(false);
        grab_brokenSprite.SetActive(false);
        player_capturedSprite.SetActive(false);

        // Enable the corresponding game object based on the newState parameter
        switch (newState)
        {
            case HandState.IDLE:
                idleSprite.SetActive(true);
                break;
            case HandState.TRACKING:
                trackingSprite.SetActive(true);
                break;
            case HandState.ATTACK:
                attackSprite.SetActive(true);
                break;
            case HandState.GRAB:
                grabSprite.SetActive(true);
                break;
            case HandState.GRAB_BROKEN:
                grab_brokenSprite.SetActive(true);
                break;
            case HandState.PLAYER_CAPTURED:
                player_capturedSprite.SetActive(true);
                break;
            default:
                break;
        }
    }


    #endregion
}
