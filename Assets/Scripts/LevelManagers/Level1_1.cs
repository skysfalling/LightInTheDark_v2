using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_1 : LevelManager
{

    public List<Spawner> levelSpawners;

    [TextArea(1, 10)]
    [Header("Script Lines")]
    public List<string> witnessComment1;
    [TextArea(1, 10)]
    public List<string> witnessComment2;
    [TextArea(1, 10)]
    public List<string> witnessComment3;

    public override IEnumerator Intro()
    {
        state = LevelState.INTRO;
        player.Inactive();

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
        state = LevelState.ROOM1;

        player.Inactive();
        camManager.NewZoomInTarget(player.transform);

        // wait until spawned
        yield return new WaitUntil(() => playerSpawn.playerSpawned);

        // focus on flower
        camManager.NewZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // [[ LINES 1 ]]
        NewDialogue(witnessComment1);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // let player move
        player.Idle();
        camManager.Player();
        gameConsole.NewMessage("use WASD to move");

        // wait until player moves
        yield return new WaitUntil(() => gameManager.inputManager.moveDirection != Vector2.zero);
        yield return new WaitForSeconds(4);

        // focus on new light orbs
        player.Inactive();
        camManager.NewZoomInTarget(levelSpawners[0].transform);
        yield return new WaitForSeconds(1);
        EnableSpawners(levelSpawners);
        gameConsole.NewMessage("pickup the l(light orb) by moving into it");
        yield return new WaitForSeconds(2);

        // allow player to move
        player.Idle();
        camManager.Player();

        // wait for player to pick up light orb
        yield return new WaitUntil(() => playerInventory.GetTypeCount(ItemType.LIGHT) > 0);
        gameConsole.NewMessage("now bring the l(light orb) to the f(flower)");

        // wait for submission to flower
        yield return new WaitUntil(() => currLifeFlower.submissionOverflow.Count > 0);

        // [[ LINES 2 ]]
        NewDialogue(witnessComment2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // wait for flower to be overflowing
        yield return new WaitUntil(() => currLifeFlower.state == FlowerState.OVERFLOWING);

        // level is complete
        state = LevelState.COMPLETE;
        currLifeFlower.state = FlowerState.HEALED;
        player.Inactive();
        camManager.NewZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        Debug.Log("Finished Level 1.1");

        // [[ LINES 3 ]]
        NewDialogue(witnessComment3);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        endGrabHand.canAttack = true;
        yield return new WaitUntil(() => endGrabHand.state == HandState.PLAYER_CAPTURED);
        uiManager.StartTransitionFadeIn();

        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitForSeconds(1);

        gameManager.LoadScene(gameManager.level_1_2);
    }
}
