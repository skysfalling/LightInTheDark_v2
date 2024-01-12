 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraLevel_2 : LevelManager
{
    [Space(10)]
    public bool introComplete;

    [Header("Room")]
    public float roomTimeCountdown = 180;
    public Totem doorTotem;
    private MessageEventListener openDoorEvent;

    [Header("Script Lines")]
    public float messageDelay = 2;
    public string flowerExclamation = "OW!";

    public void Start()
    {
        currLifeFlower = lifeFlowers[0];
        currLifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay


    }

    public override void Update()
    {
        base.Update();


        // create open door event
        try
        {
            openDoorEvent.EventUpdate(doorTotem.overflowing);
        }
        catch
        {   // send game console message when the door is opened
            openDoorEvent = gameConsole.EventMessage(doorTotem.overflowing, true, "the way is open ...", gameConsole.lightColor);
        }
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
        StartCoroutine(Room());
    }

    IEnumerator Room()
    {
        gameConsole.NewMessage("Level 3");
        state = LevelState.ROOM1;

        player.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;

        // wait until the player gets close
        while (Vector2.Distance(player.transform.position, currLifeFlower.transform.position) > 40)
        {
            yield return null;
        }

        player.state = PlayerState.INACTIVE;
        camManager.NewZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // << START FLOWER DECAY >>
        StartFlowerDecay(currLifeFlower, 0.5f);
        currLifeFlower.decayActive = false;
        yield return new WaitForSeconds(2);

        NewDialogue(dialogueManager.witness_start_1_2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // << START GAMEPLAY >>
        StartCountdown(roomTimeCountdown);
        countdownStarted = false;

        camManager.NewZoomInTarget(player.transform);
        player.state = PlayerState.IDLE;

        yield return new WaitForSeconds(1);

        currLifeFlower.decayActive = true;
        countdownStarted = true;

        camManager.state = CameraState.ROOM_BASED;
        player.state = PlayerState.IDLE;

        // wait until flower is overflowing 
        yield return new WaitUntil(() => (currLifeFlower.IsOverflowing() || CountdownOver() || currLifeFlower.IsDead()) );

        // if dead , exit routine
        if (currLifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }
        else { StartCoroutine(CompletedLeveRoutine()); }
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

        Debug.Log("Finished Level 3");

        uiManager.StartTransitionFadeIn();

        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitForSeconds(1);

    }

}
