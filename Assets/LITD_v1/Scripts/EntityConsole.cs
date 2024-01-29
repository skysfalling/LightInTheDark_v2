using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntityConsole : MonoBehaviour
{
    public GameObject messageParent;
    public TextMeshProUGUI textMesh;

    [Header(" === Message Values === ")]
    public Color defaultColor = Color.white;
    public float messageDelay = 2;
    private float msgFadeInDuration = 1;
    private float msgStayDuration = 1;
    private float msgFadeOutDuration = 5;

    [Header(" === Commands === ")]
    public char flowerCommand = 'f';
    public Color flowerColor = Color.magenta;

    [Space(10)]
    public char lightCommand = 'l';
    public Color lightColor = Color.cyan;

    [Space(10)]
    public char darkCommand = 'd';
    public Color darkColor = Color.grey;

    [Space(10)]
    public char goldCommand = 'g';
    public Color goldColor = Color.yellow;

    // Start is called before the first frame update
    public void Start()
    {
        messageParent.SetActive(false);

        SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay

    }

    public void NewMessage(string input_text)
    {
        // set colors
        textMesh.color = defaultColor;
        textMesh.text = DecodeColorString(input_text);

        // fade out ui
        StartCoroutine(FadeText(textMesh, msgFadeInDuration, msgStayDuration, msgFadeOutDuration));
    }

    public void NewMessage(string input_text, Color color)
    {
        textMesh.color = color;
        textMesh.text = DecodeColorString(input_text);

        StartCoroutine(FadeText(textMesh, msgFadeInDuration, msgStayDuration, msgFadeOutDuration));
    }

    public void NewMessage(string input_text, float delay)
    {
        StartCoroutine(MessageDelay(input_text, Color.white, delay));
    }

    public void MessageList(List<string> list, float delay)
    {
        StartCoroutine(MessageListCoroutine(list, defaultColor, delay));
    }

    public void MessageList(List<string> list, Color color, float delay)
    {
        StartCoroutine(MessageListCoroutine(list, color, delay));
    }

    public void NewRandomMessageFromList(List<string> list)
    {
        NewMessage(list[Random.Range(0, list.Count)]);
    }

    IEnumerator MessageListCoroutine(List<string> list, Color color, float delay)
    {
        foreach (string msg in list)
        {
            NewMessage(msg, color);

            yield return new WaitForSeconds(delay);
        }
    }

    public MessageEventListener EventMessage(float baseValue, EventValCompare comparison, float checkValue, string message)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<EntityConsole>(), baseValue, comparison, checkValue, message, defaultColor);
        return eventListener;
    }

    public MessageEventListener EventMessage(float baseValue, EventValCompare comparison, float checkValue, string message, Color color)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<EntityConsole>(), baseValue, comparison, checkValue, message, color);
        return eventListener;
    }

    public MessageEventListener EventMessage(bool checkBool, bool bool_switch, string message)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<EntityConsole>(), checkBool, bool_switch, message, defaultColor);
        return eventListener;
    }

    public MessageEventListener EventMessage(bool checkBool, bool bool_switch, string message, Color color)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<EntityConsole>(), checkBool, bool_switch, message, color);
        return eventListener;
    }


    #region HELPER FUNCTIONS =========================
    IEnumerator MessageDelay(string input_text, Color color, float delay)
    {
        yield return new WaitForSeconds(delay);

        NewMessage(input_text, color);
    }

    public void SetFullFadeDuration(float duration)
    {
        msgFadeInDuration = duration * 0.25f;
        msgStayDuration = duration * 0.25f;
        msgFadeOutDuration = duration * 0.5f;
    }

    IEnumerator FadeText(TextMeshProUGUI text, float fadeInDuration, float stayDuration, float fadeOutDuration)
    {
        messageParent.SetActive(true);

        // Get the initial color of the text
        Color initialColor = text.color;

        // start color as transparent
        Color startColor = initialColor;
        startColor.a = 0;
        text.color = startColor;

        // Calculate the amount to fade per frame
        float fadeInAmount = Time.deltaTime / fadeInDuration;

        // Fade the text in
        while (text.color.a < 1)
        {
            startColor.a += fadeInAmount;
            text.color = startColor;

            yield return null;
        }

        yield return new WaitForSeconds(msgStayDuration);

        // Calculate the amount to fade per frame
        float fadeOutAmount = Time.deltaTime / fadeOutDuration;

        // Fade the text out
        while (text.color.a > 0)
        {
            initialColor.a -= fadeOutAmount;
            text.color = initialColor;
            yield return null;
        }

        // Disable the text game object once it's completely faded out
        messageParent.SetActive(false);
    }

    // decodes color commands by checking the character before '(' => y( this turns yellow )
    public string DecodeColorString(string input_text)
    {
        string out_text = "";

        int startIndex = input_text.IndexOf('(');
        while (startIndex != -1)
        {
            char openingCommand = input_text[startIndex - 1];
            int endIndex = input_text.IndexOf(")", startIndex + 1);
            if (endIndex != -1)
            {
                string textBeforeCommand = input_text.Substring(0, startIndex - 1);
                string commandText = input_text.Substring(startIndex + 1, endIndex - startIndex - 1);
                string textAfterCommand = input_text.Substring(endIndex + 1);

                string colorTag;
                if (openingCommand == goldCommand)
                {
                    colorTag = "<color=#" + ColorToHex(goldColor) + ">";
                }
                else if (openingCommand == lightCommand)
                {
                    colorTag = "<color=#" + ColorToHex(lightColor) + ">";
                }
                else if (openingCommand == darkCommand)
                {
                    colorTag = "<color=#" + ColorToHex(darkColor) + ">";
                }
                else if (openingCommand == flowerCommand)
                {
                    colorTag = "<color=#" + ColorToHex(flowerColor) + ">";
                }
                else if (openingCommand == 'w')
                {
                    colorTag = "<color=white>";
                }
                else
                {
                    colorTag = "";
                }

                out_text += textBeforeCommand + colorTag + commandText + "</color>";
                input_text = textAfterCommand;

                startIndex = input_text.IndexOf('(');
            }
            else
            {
                // If an opening command is found but no closing command, stop the loop
                out_text += input_text;
                break;
            }
        }

        out_text += input_text; // Add any remaining text after the last closing command

        return out_text;

    }

    public string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    #endregion
}



