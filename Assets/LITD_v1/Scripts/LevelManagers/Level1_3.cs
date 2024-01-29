using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_3 : LevelManager
{

    [Space(10)]
    public bool introComplete;



    [Header("Room 1")]
    public List<Totem> room1Totems;

    [Header("Room 2")]
    public Transform room2SavePoint;
    public float room2TimeCountdown = 120;

    [Header("Script Lines")]
    public float messageDelay = 2;
    public string flowerExclamation = "OW!";
    public List<string> flower_lines1;
    public List<string> flower_lines2;
    public List<string> flower_lines3;

    public void Start()
    {
        currLifeFlower = lifeFlowers[0];
        currLifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay
    }

    public override void StartLevelFromPoint(LevelState state)
    {
        Debug.Log(">>> Start Level from Save Point " + state);

        if (state == LevelState.ROOM2) { Room2SavePoint(); }
        else { StartCoroutine(Intro()); }
    }

    void Room2SavePoint()
    {
        // spawn player at rift
        player.transform.parent = null;
        player.transform.position = room2SavePoint.position;

        // start transition
        uiManager.StartTransitionFadeOut();

        StartCoroutine(Room2());
    }

    IEnumerator Intro()
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
        gameConsole.NewMessage("Level 1.3.1");
        state = LevelState.ROOM1;
        player.state = PlayerState.INACTIVE;

        gameManager.levelSavePoint = LevelState.ROOM1;

        // zoom in on totem
        camManager.NewZoomInTarget(room1Totems[0].transform);

        // play Witness line introducing totem and throwing mechanic
        NewDialogue(dialogueManager.witness_totem_introduction);
        yield return new WaitUntil(() => !uiManager.inDialogue);
        yield return new WaitForSeconds(1);

        camManager.NewZoomOutTarget(room1Totems[0].transform);
        yield return new WaitForSeconds(2);

        // play Witness line introducing totem and throwing mechanic
        NewDialogue(dialogueManager.witness_throwing_introduction);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        player.Idle();
        camManager.state = CameraState.ROOM_BASED;

        // throw orb at the totem to unlock door
        // wait until totem is activated
        yield return new WaitUntil(() => room1Totems[0].overflowing);

        player.Inactive();

        // zoom in on opening door
        camManager.NewZoomInTarget(room1Totems[0].transform);
        yield return new WaitForSeconds(2);

        // zoom in on opening door
        camManager.NewZoomInTarget(room1Totems[0].unlockDoors[0].transform);
        yield return new WaitForSeconds(2);

        // play Witness line demonstrating door unlocks
        NewDialogue(dialogueManager.witness_totem_door_introduction);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        player.Idle();
        camManager.state = CameraState.ROOM_BASED;

        // move to second part
        // wait for player to throw orbs at corresponding totems
        yield return new WaitUntil(() => room1Totems[1].overflowing && room1Totems[2].overflowing);

        // end of Room 1
        StartCoroutine(Room2());
    }

    IEnumerator Room2()
    {
        state = LevelState.ROOM2;
        player.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;

        gameManager.levelSavePoint = LevelState.ROOM2;


        while (Vector2.Distance(player.transform.position, currLifeFlower.transform.position) > 25)
        {
            yield return null;
        }

        player.state = PlayerState.INACTIVE;

        // focus on flower
        camManager.NewZoomInTarget(currLifeFlower.transform);

        // play Witness line
        NewDialogue(dialogueManager.witness_flower_introduction);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        player.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;
        StartFlowerDecay(currLifeFlower, 0.95f);

        // begin encounter and timer countdown from 120
        StartCountdown(room2TimeCountdown);

        // wait until countdown == 0 to win, if flower death restart from last save
        yield return new WaitUntil(() => (countdownTimer <= 0 || currLifeFlower.IsDead()));

        // if dead , exit routine
        if (currLifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

        // else continue on
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

        // new dialogue
        NewDialogue(dialogueManager.witness_end_1_2_2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        camManager.state = CameraState.ROOM_BASED;
        player.Idle();

        Debug.Log("Finished Level 1.3");

        uiManager.StartTransitionFadeIn();

        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitForSeconds(1);

    }

}
