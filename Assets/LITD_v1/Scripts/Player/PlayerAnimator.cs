using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAnimator : MonoBehaviour
{

    GameManager gameManager;
    PlayerMovement movement;
    PlayerInventory inv_script;
    public Animator anim;
    public GameObject spriteParent;

    [Header("Animation")]
    public Transform parent;

    public GameObject closedEyes;
    public GameObject halfClosedEyes;
    public GameObject openEyes;
    public GameObject xEyes;
    public GameObject winceEyes;
    public GameObject deathEyes;


    [Space(10)]
    public Transform bodySprite;
    public float bodyLeanAngle = 10;
    public float leanSpeed = 2;

    [Space(10)]
    public float rotation; // "aim direction" rotation

    [Header("Effects")]
    public GameObject stunEffect;
    public GameObject panicEffect;
    public GameObject slowEffect;
    public GameObject dashEffect;

    [Header("Lighting")]
    public Light2D playerLight; // player light
    public Light2D outerLight; // outer light

    private Color currLightColor;
    private float currLightIntensity;
    private float currLightRadius;

    [Header("-- Player Light")]
    public float player_defaultLightIntensity = 0.7f;
    public float player_inventoryIntensityAddition = 0.1f; // how much light intensity is added per inventory item


    [Header("-- Outer Light")]
    public Color outer_lightColor;
    public float outer_defaultLightIntensity = 0.7f;
    public float outer_defaultLightRange = 5;
    public float outer_inventoryIntensityAddition = 0.3f; // how much light intensity is added per inventory item
    public float outer_inventoryRangeAddition = 0.5f; // how much light intensity is added per inventory item

    [Space(10)]
    public Color outer_chargeColor;
    public float outer_chargingLightIntensity = 5f;
    public float outer_chargingLightRange = 2f;
    public float player_chargingLightIntensity = 3;



    // Start is called before the first frame update
    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        inv_script = GetComponent<PlayerInventory>();
        playerLight = GetComponent<Light2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        stunEffect.SetActive(false);
        panicEffect.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(BlinkCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        AnimationStateMachine();
        LightingStateMachine();

        // update body sprite
        LeanTowardsMoveDirection();
    }

    public void AnimationStateMachine()
    {
        // get angle value of aim direaction
        rotation = Mathf.Atan2(movement.aimDirection.y, movement.aimDirection.x) * Mathf.Rad2Deg;

        UpdateStateBasedEffect(movement.state);

        anim.SetBool("isStunned", movement.state == PlayerState.STUNNED);
        anim.SetBool("isHoldingObj", movement.throwObject != null);

        // enable / disable aim indicator
        movement.aimIndicator.gameObject.SetActive(movement.state == PlayerState.THROWING);

        // play throw anim
        if (movement.inThrow) { PlayAnimationIfNotPlaying("throw"); }




        // states
        switch (movement.state)
        {
            case PlayerState.IDLE:

                anim.SetBool("isMoving", false);
                break;

            case PlayerState.MOVING:

                anim.SetBool("isMoving", true);

                break;

            case PlayerState.STUNNED:
            case PlayerState.SLOWED:
            case PlayerState.PANIC:

                SetActiveEyes(openEyes);

                break;

            case PlayerState.THROWING:

                // rotate parent and UI towards throw point
                movement.aimIndicator.transform.eulerAngles = new Vector3(0, 0, rotation);
                SetActiveEyes(openEyes);

                break;

            case PlayerState.DASH:

                SetActiveEyes(xEyes);

                dashEffect.transform.eulerAngles = new Vector3(0, 0, rotation - 180f);
                anim.SetBool("isMoving", true);

                break;

            default:
                break;

        }
    }

    private void LeanTowardsMoveDirection()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        if (movement.moveDirection.x > 0.5f)
        {
            targetRotation = Quaternion.Euler(0f, 0f, -bodyLeanAngle);

            parent.rotation = Quaternion.Euler(0f, 0, 0);
        }
        else if (movement.moveDirection.x < -0.5f)
        {
            targetRotation = Quaternion.Euler(0f, 0f, bodyLeanAngle);

            parent.rotation = Quaternion.Euler(0f, 180, 0);

        }
        else
        {
            targetRotation = Quaternion.Euler(0f, 0f, 0);

        }

        bodySprite.rotation = Quaternion.Lerp(bodySprite.rotation, targetRotation, Time.deltaTime * leanSpeed);
    }

    public void UpdateStateBasedEffect(PlayerState state)
    {
        // stunned
        if (state == PlayerState.STUNNED) { stunEffect.SetActive(true); }
        else { stunEffect.SetActive(false); }

        // panic
        if (state == PlayerState.PANIC) { panicEffect.SetActive(true); }
        else { panicEffect.SetActive(false); }

        // slowed
        if (state == PlayerState.PANIC || state == PlayerState.SLOWED) 
        { 
            slowEffect.SetActive(true);
            gameManager.effectManager.EnablePanicShader();
            gameManager.camManager.Panic();

        }
        else { 

            slowEffect.SetActive(false);
            //gameManager.effectManager.DisablePanicShader();

        }

    }

    public void PlayDashEffect()
    {
        // gameManager.camManager.ShakeCamera(movement.dashDuration, 0.1f);

        // there are two particle effects that activate on awake so i did this cause im lazy
        dashEffect.SetActive(false);
        dashEffect.SetActive(true); 
    }


    // <<<< EYES >>>>
    private void SetActiveEyes(GameObject activeEyes)
    {
        closedEyes.SetActive(false);
        halfClosedEyes.SetActive(false);
        openEyes.SetActive(false);
        xEyes.SetActive(false);
        winceEyes.SetActive(false);
        deathEyes.SetActive(false);

        activeEyes.SetActive(true);
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            float blinkTime = Random.Range(0.5f, 1);
            yield return new WaitForSeconds(blinkTime);

            if ( movement.state == PlayerState.IDLE || movement.state == PlayerState.MOVING || movement.state == PlayerState.THROWING)
            {
                SetActiveEyes(closedEyes);
                yield return new WaitForSeconds(Random.Range(0.5f, 1));

                SetActiveEyes(halfClosedEyes);
            }
        }
    }

    #region <<<< LIGHTING >>>>
    public void LightingStateMachine()
    {
        switch(movement.state)
        {
            case PlayerState.IDLE:
            case PlayerState.MOVING:
                // raise light intensity with more items
                InventoryCountLightLerp();
                break;

            default:
                break;

        }
    }

    public void InventoryCountLightLerp()
    {
        float playerLightIntensity = (inv_script.inventory.Count * player_inventoryIntensityAddition) + player_defaultLightIntensity;
        playerLight.intensity = Mathf.Lerp(playerLight.intensity, playerLightIntensity, Time.deltaTime);


        outerLight.color = outer_lightColor;


        float outerLightIntensity = (inv_script.inventory.Count * outer_inventoryIntensityAddition) + outer_defaultLightIntensity;
        outerLight.intensity = Mathf.Lerp(outerLight.intensity, outerLightIntensity, Time.deltaTime);

        float outerLightRadius = (inv_script.inventory.Count * outer_inventoryRangeAddition) + outer_defaultLightRange;
        outerLight.pointLightOuterRadius = Mathf.Lerp(outerLight.pointLightOuterRadius, outerLightRadius, Time.deltaTime * 0.2f);


    }

    public void PlayAnimationIfNotPlaying(string animationName)
    {
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            anim.Play(animationName);
        }
    }
    #endregion

}
