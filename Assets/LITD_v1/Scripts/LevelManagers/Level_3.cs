using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_3 : LevelManager
{
    [Space(10)]
    public bool introComplete;

    [Header("Room")]
    public Transform roomCenter;

    public void Start()
    {


    }

    public override void StartLevelFromPoint(LevelState state)
    {
        StartCoroutine(Intro());
    }

    public override IEnumerator Intro()
    {
        state = LevelState.INTRO;
        player.state = PlayerState.INACTIVE;

        uiManager.StartTransitionFadeOut(); // start transition

        playerSpawn.StartSpawnRoutine();

        // wait until spawned
        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitUntil(() => playerSpawn.playerSpawned);


        // continue to next room
        StartCoroutine(Room1());
    }

    IEnumerator Room1()
    {
        gameConsole.NewMessage("Level 3");
        gameConsole.NewMessage("[[ The Abyss ]]");

        state = LevelState.ROOM1;
        player.state = PlayerState.IDLE;
        camManager.state = CameraState.PLAYER;

        while (Vector2.Distance(player.transform.position, currLifeFlower.transform.position) > activateRange)
        {
            yield return null;
        }

        // focus on life flower
        player.state = PlayerState.INACTIVE;
        camManager.NewZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // << START FLOWER DECAY >>
        StartFlowerDecay(currLifeFlower, 0.75f);
        currLifeFlower.decayActive = false;

        NewDialogue(dialogueManager.witness_start_1_2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        camManager.NewZoomInTarget(player.transform);
        yield return new WaitForSeconds(1);

        uiManager.NewEncounterAnnouncement("keep the flower alive");

        currLifeFlower.decayActive = true;

        camManager.state = CameraState.PLAYER;
        player.state = PlayerState.IDLE;

        // wait until flower is overflowing 
        yield return new WaitUntil(() => (currLifeFlower.IsOverflowing() || currLifeFlower.IsDead()));

        // if dead , exit routine
        if (currLifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

    }

    IEnumerator FailedLevelRoutine()
    {
        gameConsole.NewMessage("Level Failed");
        state = LevelState.FAIL;
        player.Inactive();

        camManager.NewZoomInTarget(currLifeFlower.transform);
        NewRandomDialogue(dialogueManager.witness_onFail);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        endGrabHand.canAttack = true;
        camManager.state = CameraState.ROOM_BASED;
        yield return new WaitUntil(() => endGrabHand.state == HandState.GRAB);

        uiManager.StartTransitionFadeIn();

        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitForSeconds(1);

        gameManager.RestartLevelFromSavePoint();
    }

    IEnumerator CompletedLeveRoutine()
    {
        gameConsole.NewMessage("Level Completed");
        player.Inactive();
        state = LevelState.COMPLETE;

        currLifeFlower.canSubmit = false;
        currLifeFlower.state = FlowerState.HEALED;

        // zoom in to flower
        camManager.NewZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // zoom out of flower
        camManager.NewZoomOutTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // destroy items
        playerInventory.Destroy();
        yield return new WaitForSeconds(1);

        // new dialogue
        NewDialogue(dialogueManager.witness_end_1_2_2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        camManager.state = CameraState.PLAYER;
        player.Idle();

        Debug.Log("Finished Level 3");

        uiManager.StartTransitionFadeIn();

        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitForSeconds(1);

    }

    private void OnDrawGizmos()
    {
        if (currLifeFlower != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(currLifeFlower.transform.position, activateRange);
        }
    }
}
