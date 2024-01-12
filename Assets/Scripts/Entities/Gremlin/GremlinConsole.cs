using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GremlinConsole : EntityConsole
{
    private GremlinAI ai;

    [Header("CHASE")]
    public bool saidChaseQuip;
    public List<string> chase_quips = new List<string>();

    [Header("STUN")]
    public bool saidStunQuip;
    public List<string> stun_quips = new List<string>();

    [Header("TARGET ITEM")]
    public bool saidTargetItemQuip;
    public List<string> targetItem_quips = new List<string>();

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        ai = GetComponent<GremlinAI>();
    }

    // Update is called once per frame
    void Update()
    {
        ConsoleStateMachine();
    }

    void ConsoleStateMachine()
    {
        switch (ai.state)
        {
            case GremlinState.CHASE_PLAYER:
                if (!saidChaseQuip)
                {
                    NewRandomMessageFromList(chase_quips);
                    saidChaseQuip = true;
                }
                break;
            case GremlinState.STUN_PLAYER:
                if (!saidStunQuip)
                {
                    NewRandomMessageFromList(stun_quips);
                    saidStunQuip = true;
                }
                break;
            case GremlinState.TARGET_ITEM:
                if (!saidTargetItemQuip)
                {
                    NewRandomMessageFromList(targetItem_quips);
                    saidTargetItemQuip = true;
                }
                break;
            default:
                ResetQuipBools();
                break;
        }
    }

    public void ResetQuipBools()
    {
        saidChaseQuip = false;
        saidStunQuip = false;
        saidTargetItemQuip = false;
    }
}
