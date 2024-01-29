using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    GameManager gameManager;
    Animator anim;
    public LevelManager levelManager;

    [Header("UI")]
    public TextMeshProUGUI flowerLifeForceTMP;
    public TextMeshProUGUI countdownTMP;
    public float UI_horzOffset = 100;
    public bool uiMoving;

    [Header("Transition")]
    public Image transition;
    public bool transitionFinished;
    public float gameDissolveAmount = 0.5f;
    public float transitionDelay = 1;
    public TextMeshProUGUI deathText;
    public float transitionSpeed = 5;

    [Header("Encounter Announcement")]
    public TextMeshProUGUI encounterAnnounceText;

    [Header("Dialogue")]
    public GameObject dialogueObject;
    public TextMeshProUGUI dialogueText;
    public GameObject contText;
    public float wordDelay = 0.05f;
    public bool inDialogue;


    private void Awake()
    {
        gameManager = GetComponentInParent<GameManager>();
        anim = GetComponent<Animator>();

        transition.material.SetFloat("_Dissolve", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        levelManager = gameManager.levelManager;
    }

    private void Update()
    {
        if (levelManager == null)
        {
            levelManager = gameManager.levelManager;
        }
        else
        {
            // << countdown text >>
            countdownTMP.text = "" + levelManager.GetCurrentCountdown();

            // << life force text >>
            if (levelManager.currLifeFlower != null) { flowerLifeForceTMP.text = "" + levelManager.currLifeFlower.lifeForce; }
        }

    }

    #region LevelTransition
    public void StartTransitionFadeOut()
    {
        StartCoroutine(TransitionFadeOut(transitionDelay));
    }

    public void StartTransitionFadeIn()
    {
        StartCoroutine(TransitionFadeIn(transitionDelay));
    }

    IEnumerator TransitionFadeOut(float delay)
    {
        transitionFinished = false;

        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = 0;
        while (transitionAmount < gameDissolveAmount)
        {
            transitionAmount = Mathf.MoveTowards(transitionAmount, gameDissolveAmount, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);

            yield return null;
        }

        transitionFinished = true;
    }

    IEnumerator TransitionFadeIn(float delay)
    {
        transitionFinished = false;

        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = gameDissolveAmount;
        while (transitionAmount > 0)
        {

            transitionAmount = Mathf.MoveTowards(transitionAmount, 0, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);
            yield return null;
        }

        transitionFinished = true;
    }
    #endregion

    #region EncounterAnnouncement
    public void NewEncounterAnnouncement(string text = "keep your flower alive")
    {
        StartCoroutine(IntenseGameDialogue(encounterAnnounceText, text, 1));

        anim.Play("StartEncounterAnnouncement");
    }
    #endregion

    #region Dialogue
    public void NewDialogue(string text)
    {
        dialogueObject.SetActive(true);
        string decodedText = gameManager.gameConsole.DecodeColorString(text);

        StartCoroutine(GameDialogueRoutine(dialogueText, decodedText, wordDelay));
            
    }

    public void SlowIntentseDialogue(string text, float slowDelay)
    {
        encounterAnnounceText.gameObject.SetActive(true);
        string decodedText = gameManager.gameConsole.DecodeColorString(text);

        StartCoroutine(IntenseGameDialogue(encounterAnnounceText, decodedText, slowDelay));
    }

    public void NewDialogue(List<string> text)
    {
        dialogueObject.SetActive(true);

        StartCoroutine(GameDialogueRoutine(dialogueText, text, wordDelay));

    }

    public void TimedDialogue(List<string> text, float sentenceDelay)
    {
        dialogueObject.SetActive(true);

        StartCoroutine(TimedGameDialogueRoutine(dialogueText, text, wordDelay, sentenceDelay));
    }

    public void DialoguePromptContinue()
    {
        contText.SetActive(true);
    }

    public void DisableDialogue()
    {
        contText.SetActive(false);
        dialogueObject.SetActive(false);
    }

    IEnumerator GameDialogueRoutine(TextMeshProUGUI textComponent, List<string> dialogue , float wordDelay)
    {
        inDialogue = true;

        int currentStringIndex = 0;
        string[] currentWords;

        while (currentStringIndex < dialogue.Count)
        {
            // Debug.Log("Dialogue string #" + currentStringIndex);

            // get string
            string decodedText = gameManager.gameConsole.DecodeColorString(dialogue[currentStringIndex]);
            currentWords = decodedText.Split(' ');
            textComponent.text = ""; // reset text

            // iterate through string with delay
            for (int i = 0; i < currentWords.Length; i++)
            {
                textComponent.text += currentWords[i] + " ";
                yield return new WaitForSeconds(wordDelay);
            }

            yield return new WaitForSeconds(0.25f);

            // after string is shown, wait for player input
            bool stringDisplayed = false;
            while (!stringDisplayed)
            {
                if (Input.anyKeyDown)
                {
                    stringDisplayed = true;
                }
                yield return null;
            }

            currentStringIndex++;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator GameDialogueRoutine(TextMeshProUGUI textComponent, string dialogue, float wordDelay)
    {
        inDialogue = true;

        string[] words = gameManager.gameConsole.DecodeColorString(dialogue).Split(' ');
        textComponent.text = "";

        for (int i = 0; i < words.Length; i++)
        {
            textComponent.text += words[i] + " ";
            yield return new WaitForSeconds(wordDelay);
        }

        // after string is shown, wait for player input
        bool stringDisplayed = false;
        while (!stringDisplayed)
        {
            if (Input.anyKeyDown)
            {
                stringDisplayed = true;
            }
            yield return null;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator IntenseGameDialogue(TextMeshProUGUI textComponent, string dialogue, float wordDelay)
    {
        inDialogue = true;

        string[] words = gameManager.gameConsole.DecodeColorString(dialogue).Split(' ');
        textComponent.text = "";

        for (int i = 0; i < words.Length; i++)
        {
            textComponent.text += words[i] + " ";
            yield return new WaitForSeconds(wordDelay);

            DialogueShake(textComponent);
        }

        // after string is shown, wait for player input
        bool stringDisplayed = false;
        while (!stringDisplayed)
        {
            if (Input.anyKeyDown)
            {
                stringDisplayed = true;
            }
            yield return null;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator TimedGameDialogueRoutine(TextMeshProUGUI textComponent, List<string> dialogue, float wordDelay, float sentenceDelay)
    {
        inDialogue = true;

        int currentStringIndex = 0;
        string[] currentWords;

        while (currentStringIndex < dialogue.Count)
        {
            // Debug.Log("Timed Dialogue string #" + currentStringIndex);

            // get string
            string decodedText = gameManager.gameConsole.DecodeColorString(dialogue[currentStringIndex]);
            currentWords = decodedText.Split(' ');
            textComponent.text = ""; // reset text

            // iterate through string with delay
            for (int i = 0; i < currentWords.Length; i++)
            {
                textComponent.text += currentWords[i] + " ";
                yield return new WaitForSeconds(wordDelay);
            }

            yield return new WaitForSeconds(sentenceDelay);

            currentStringIndex++;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator TimedGameDialogueRoutine(TextMeshProUGUI textComponent, string dialogue, float wordDelay, float endDelay)
    {
        inDialogue = true;

        string[] words = gameManager.gameConsole.DecodeColorString(dialogue).Split(' ');
        textComponent.text = "";

        for (int i = 0; i < words.Length; i++)
        {
            textComponent.text += words[i] + " ";
            yield return new WaitForSeconds(wordDelay);
        }

        yield return new WaitForSeconds(endDelay);

        inDialogue = false;
        DisableDialogue();
    }

    public void DialogueShake(TextMeshProUGUI textComponent)
    {
        StartCoroutine(DialogueShake(textComponent, 0.2f, 10, new Vector2(-1, -1)));
    }

    IEnumerator DialogueShake(TextMeshProUGUI textComponent, float duration, float magnitude, Vector2 dirInfluence)
    {
        float elapsed = 0.0f;

        Vector2 originalPos = textComponent.transform.localPosition;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Vector2 shakePos = new Vector2(x, y) * dirInfluence;

            textComponent.transform.localPosition = (Vector3)shakePos + textComponent.transform.localPosition;

            elapsed += Time.deltaTime;

            yield return null;
        }

        textComponent.transform.localPosition = originalPos;

    }

    #endregion

}
