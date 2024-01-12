using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GremlinAnimator : MonoBehaviour
{

    GremlinAI ai;
    public Animator anim;
    public Canvas canvas;
    public void Start()
    {
        ai = GetComponent<GremlinAI>();
        canvas.worldCamera = Camera.main;
    }


    public void Update()
    {
        // set anim bools based on states

        anim.SetBool("idle", ai.state == GremlinState.IDLE );
        anim.SetBool("chase player", ai.state == GremlinState.CHASE_PLAYER);

    }

}
