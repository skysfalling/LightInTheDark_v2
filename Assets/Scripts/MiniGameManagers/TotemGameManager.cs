using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemGameManager : MonoBehaviour
{
    public bool playerInGameArea;
    public bool playerParticipatedInGame;
    public Transform triggerParent;
    public Vector2 triggerSize;

    [Space(10)]
    public GrabHandAI judgementHand;

    [Header("GameRules")]
    public bool winConditionMet;
    public Totem submissionTotem;
    public List<Totem> allTotems;

    [Space(10)]
    public bool playerJudged;
    public RewardHolder rewardHolder;

    // Start is called before the first frame update
    void Start()
    {
        rewardHolder.locked = true;
    }

    // Update is called once per frame
    void Update()
    {
        playerInGameArea = IsPlayerInTrigger();

        if (playerInGameArea)
        {
            winConditionMet = GetWinCondition();
            playerParticipatedInGame = GetParticipation();
        }
        else if (!playerInGameArea && playerParticipatedInGame && !playerJudged)
        {
            JudgePlayer();
        }

    }

    public bool GetParticipation()
    {
        // if any wrong totems have orbs , not winning
        foreach (Totem totem in allTotems)
        {
            if (totem.submissionOverflow.Count > 0)
            {
                return true;
            }
        }

        return false;
    }


    public bool GetWinCondition()
    {
        // if any wrong totems have orbs , not winning
        foreach (Totem totem in allTotems)
        {
            if (totem != submissionTotem && totem.submissionOverflow.Count > 0)
            {
                return false;
            }
        }

        // if only submission totem has orbs, win condition met
        if (submissionTotem.submissionOverflow.Count > 0)
        {
            return true;
        }

        return false;
    }

    public void JudgePlayer()
    {
        playerJudged = true;

        if (winConditionMet)
        {
            rewardHolder.locked = false;
        }
        else
        {
            judgementHand.DoomAttackPlayer();
        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapBoxAll(triggerParent.position, triggerSize, 0);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            if (col.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (triggerParent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(triggerParent.position, triggerSize);
        }
    }
}
