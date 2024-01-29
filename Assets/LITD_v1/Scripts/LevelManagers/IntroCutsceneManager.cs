using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class IntroCutsceneManager : LevelManager
{
    [TextArea(1, 10)]
    [Header("=== Introduction ===")]
    public List<string> unknownMonologue;

    [TextArea(1, 10)]
    [Space(10)]
    public string doctorLine = "";

    [Space(10)]
    [TextArea(1, 10)]
    public List<string> mrAftersMonologue;

    [Header("=== Welcome to the Mindscape ===")]
    [TextArea(1, 10)]
    public List<string> witnessWelcome1;
    [TextArea(1, 10)]
    public List<string> witnessWelcome2;
    [TextArea(1, 10)]
    public List<string> witnessWelcome3;
    [TextArea(1, 10)]
    public List<string> witnessWelcome4;
    public override void StartLevelFromPoint(LevelState state)
    {
        Debug.Log("Start " + SceneManager.GetActiveScene() + " from " + state);

        StartCoroutine(Cutscene());
    }

    public virtual IEnumerator Cutscene()
    {

        #region Introduction
        state = LevelState.INTRO;

        uiManager.StartTransitionFadeOut(); // start transition

        // <<< MAIN MENU LOOP >>>

        // <<< UNKOWN MONOLOGUE >>>
        yield return new WaitForSeconds(1);
        NewDialogue(unknownMonologue);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // TODO : hospital sounds
        yield return new WaitForSeconds(1);
        NewDialogue("*hospital sounds*");

        // << DOCTOR >>
        yield return new WaitForSeconds(1);
        NewDialogue(doctorLine);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // << MR AFTERS >>
        yield return new WaitForSeconds(1);
        NewDialogue(mrAftersMonologue);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // TODO : hospital sounds
        yield return new WaitForSeconds(1);
        NewDialogue("*hospital sounds*");

        // TODO : exit transition
        yield return new WaitForSeconds(1);
        NewDialogue("*exit transition*");

        #endregion

        #region Welcome to the Mindscape
        state = LevelState.ROOM1;

        // TODO : enter the mindspace
        yield return new WaitForSeconds(1);
        NewDialogue("*enter the mindspace*");


        // << WITNESS WELCOME >>
        yield return new WaitForSeconds(1);
        NewDialogue(witnessWelcome1);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // TODO : witness vaguely appears
        yield return new WaitForSeconds(1);
        NewDialogue("*the witness' eyes enter the chat*");
        yield return new WaitUntil(() => !uiManager.inDialogue);


        // << WITNESS WELCOME 2 >>
        yield return new WaitForSeconds(1);
        NewDialogue(witnessWelcome2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // << WITNESS WELCOME 3 >>
        yield return new WaitForSeconds(1);
        NewDialogue(witnessWelcome3);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // << WITNESS WELCOME 4 >>
        yield return new WaitForSeconds(1);
        NewDialogue(witnessWelcome4);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // TODO : exit transition
        yield return new WaitForSeconds(1);
        NewDialogue("*exit transition*");
        uiManager.StartTransitionFadeOut();

        #endregion

        state = LevelState.COMPLETE;
        yield return new WaitForSeconds(1);

        gameManager.LoadScene(gameManager.level_1_1);
    }

}
