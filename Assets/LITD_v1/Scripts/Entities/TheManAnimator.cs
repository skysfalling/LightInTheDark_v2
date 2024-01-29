using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TheManAnimator : MonoBehaviour
{
    PlayerMovement playerMovement;
    TheManAI ai;
    public Animator anim;
    public Canvas canvas;
    public Transform spriteParent;

    [Header("Faces")]
    public GameObject lightFace;
    public GameObject darkFace;

    [Header("Particles")]
    public GameObject suckLifeParticles;

    [Header("UI")]
    public TextMeshProUGUI struggleText;

    [Header("Light")]
    public Light2D light;

    [Header("Fog")]
    public Material fogMaterial;
    public Vector2 fogOpacityRange = new Vector2(0, 1.2f);

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<TheManAI>();
        playerMovement = ai.playerMovement;
        light = GetComponent<Light2D>();
        canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement == null)
        {
            playerMovement = ai.playerMovement;
        }

        // fog opacity
        if (fogMaterial != null)
        {
            float fogOpacity = Mathf.Lerp(fogOpacityRange.x, fogOpacityRange.y, ai.distToPlayer / ai.outerTriggerSize);

            fogMaterial.SetFloat("_Opacity", fogOpacity);
        }

        // << ANIM VALUES >>
        anim.SetBool("Idle", ai.state == TheManState.IDLE || ai.state == TheManState.RETREAT || ai.state == TheManState.FOLLOW);
        anim.SetBool("Grab", ai.state == TheManState.GRABBED_PLAYER || ai.state == TheManState.PLAYER_CAPTURED);
        anim.SetBool("Chase", ai.state == TheManState.CHASE);


        switch (ai.state)
        {
            case TheManState.GRABBED_PLAYER:
            case TheManState.PLAYER_CAPTURED:

                lightFace.SetActive(true);
                darkFace.SetActive(false);
                suckLifeParticles.SetActive(true);

                // enable vortex particles

                // show struggle count down
                if (ai.state == TheManState.PLAYER_CAPTURED) { struggleText.text = "help"; }
                else
                {
                    // << STRUGGLE >>
                    struggleText.text = "" + (ai.breakFree_struggleCount - ai.playerMovement.struggleCount);
                }
                struggleText.gameObject.SetActive(true);
                break;

            case TheManState.CHASE:
            case TheManState.RETREAT:
            case TheManState.IDLE:

                darkFace.SetActive(true);
                lightFace.SetActive(false);
                suckLifeParticles.SetActive(false);

                // struggle reset
                struggleText.gameObject.SetActive(false);

                // filp towards player
                if (ai.state != TheManState.IDLE)
                {
                    FlipTowardsPlayer(playerMovement.transform);
                }
                break;

        }



    }


    public void FlipTowardsPlayer(Transform target)
    {
        // Set the rotation speed and the offset from the player
        float rotationSpeed = 20f;
        float flipOffset = 50;

        // player is to the left
        if (target.position.x > transform.position.x + flipOffset)
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 180f, 0f); // rotate 180 degrees on the y-axis

            spriteParent.rotation = Quaternion.Lerp(transform.rotation, flipRotation, Time.deltaTime * rotationSpeed);
        }
        // player is to the right
        else if (target.position.x < transform.position.x - flipOffset) 
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 0f, 0f); // rotate back to original rotation

            spriteParent.rotation = Quaternion.Lerp(transform.rotation, flipRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
