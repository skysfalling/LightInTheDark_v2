using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum LevelState { INTRO, ROOM1, ROOM2, FAIL, COMPLETE }
public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public GameManager gameManager;
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public SoundManager soundManager;
    [HideInInspector]
    public PlayerMovement player;
    [HideInInspector]
    public PlayerInventory playerInventory;
    [HideInInspector]
    public PlayerAnimator playerAnim;
    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public CameraManager camManager;
    [HideInInspector]
    public DialogueManager dialogueManager;
    [HideInInspector]
    public EffectManager effectManager;

    [Header("Game Values")]
    public LevelState state = LevelState.INTRO;

    [Header("Life Flower")]
    public float activateRange = 50f;
    public LifeFlower currLifeFlower;
    public List<LifeFlower> lifeFlowers;

    [Header("Timer")]
    public float gameClock;
    float startTime;

    [Header("Countdown Timer")]
    public bool countdownStarted;
    public float countdownTimer;

    [Header("Spawn")]
    public Transform camStart;
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;


    public void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        uiManager = gameManager.GetComponentInChildren<UIManager>();
        camManager = gameManager.GetComponentInChildren<CameraManager>();
        gameConsole = gameManager.gameConsole;
        soundManager = gameManager.soundManager;
        dialogueManager = gameManager.dialogueManager;
        effectManager = gameManager.effectManager;

        try
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            playerInventory = player.gameObject.GetComponent<PlayerInventory>();
            playerAnim = player.gameObject.GetComponent<PlayerAnimator>();
        }
        catch { Debug.Log("Level Manager could not find player"); }


        startTime = Time.time;

        if (currLifeFlower == null)
        {
            currLifeFlower = lifeFlowers[0];
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        LevelStateMachine();

        if (state != LevelState.INTRO) { UpdateGameClock(); }

        if (countdownStarted && countdownTimer > 0) { UpdateCountdown(); }
    }

    public virtual void StartLevelFromPoint(LevelState state)
    {
        StartCoroutine(Intro());
    }

    public virtual IEnumerator Intro()
    {
        state = LevelState.INTRO;
        if (player)
        {
            player.state = PlayerState.INACTIVE;
        }

        uiManager.StartTransitionFadeOut(); // start transition

        playerSpawn.StartSpawnRoutine();

        // wait until spawned
        yield return new WaitUntil(() => uiManager.transitionFinished);
        yield return new WaitUntil(() => playerSpawn.playerSpawned);

        camManager.state = CameraState.PLAYER;
        uiManager.NewEncounterAnnouncement();
    }

    public virtual void LevelStateMachine()
    {
        


    }

    #region << DIALOGUE >>
    public void NewDialogue(string dialogue)
    {
        uiManager.NewDialogue(dialogue);
    }

    public void NewDialogue(List<string> dialogue)
    {
        uiManager.NewDialogue(dialogue);
    }

    public void NewRandomDialogue(List<string> dialogue)
    {
        uiManager.NewDialogue(dialogue[Random.Range(0, dialogue.Count)]);
    }

    public void NewTimedDialogue(List<string> dialogue, float sentenceDelay)
    {
        uiManager.TimedDialogue(dialogue, sentenceDelay);
    }

    public void NewDialogue(string dialogue, GameObject focusObject)
    {
        camManager.NewGameTipTarget(focusObject.transform);
        uiManager.NewDialogue(dialogue);
    }

    public void NewDialogue(List<string> dialogue, GameObject focusObject)
    {
        camManager.NewGameTipTarget(focusObject.transform);
        uiManager.NewDialogue(dialogue);
    }

    public void NewRandomDialogue(List<string> dialogue, GameObject focusObject)
    {
        uiManager.NewDialogue(dialogue[Random.Range(0, dialogue.Count)]);
        camManager.NewGameTipTarget(focusObject.transform);
    }
    #endregion


    public virtual IEnumerator WitnessDarklightReaction(float time)
    {
        /*
        // witness dialogue
        NewTimedDialogue(dialogueManager.witness_darklightSubmit, 2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // start panic
        player.Panic(10);
        NewTimedDialogue(dialogueManager.witness_startSoulPanic, 2);
        yield return new WaitUntil(() => !uiManager.inDialogue);
        */

        yield return null;

    }

    public void StartFlowerDecay(LifeFlower lifeFlower, float healthPercent = 0.5f)
    {
        lifeFlower.decayActive = true; // start decay
        lifeFlower.lifeForce = Mathf.FloorToInt(lifeFlower.maxLifeForce * healthPercent);
        lifeFlower.DamageReaction();
    }

    public void EnableSpawners(List<Spawner> spawners)
    {
        foreach (Spawner spawner in spawners)
        {
            if (spawner == null) { continue; }
            spawner.StartSpawn();
        }
    }

    public void DestroySpawners(List<Spawner> spawners)
    {
        foreach (Spawner spawner in spawners)
        {
            if (spawner != null & spawner.spawnedObject)
            {
                spawner.DestroySpawnedObject();
            }
        }
    }

    public void UpdateGameClock()
    {
        float timePassed = Time.time - startTime;
        gameClock =  Mathf.Round(timePassed * 10) / 10f;
    }

    #region <<<< COUNTDOWN >>>>
    public void StartCountdown(float count)
    {
        countdownTimer = count;
        countdownStarted = true;
    }

    public void UpdateCountdown()
    {
        countdownTimer -= Time.deltaTime;
    }
    public void StopCountdown()
    {
        countdownStarted = false;
    }

    public float GetCurrentCountdown()
    {
        return  Mathf.Round(countdownTimer * 10) / 10f;
    }

    public bool CountdownOver()
    {
        return countdownTimer <= 0;
    }
    #endregion

    public bool IsEndOfLevel()
    {
        if (state == LevelState.FAIL || state == LevelState.COMPLETE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
