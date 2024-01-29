using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EventValCompare { IS_EQUAL, IS_GREATER, IS_LESS, IS_GREATER_EQUAL, IS_LESS_EQUAL}
public class GameConsole : MonoBehaviour
{
    public Transform spawnParent;
    public GameObject messagePrefab;

    [Space(10)]
    public bool testMessage;
    public string test = "testing the messages";

    [Space(10)]
    public Color defaultColor = Color.grey;

    [Space(10)]
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

    [Space(10)]
    public char purpleCommand = 'p';
    public Color purpleColor = Color.magenta;

    [Space(10)]
    public int maxMessages = 4;
    public float line_spacing = 10;
    public float messageMoveSpeed = 1;
    public float messageFadeDuration = 5;

    [Space(20)]
    private List<GameObject> messages = new List<GameObject>();
    public List<string> message_list = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        if (testMessage)
        {
            NewMessage(test, Color.magenta);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMessages();
    }

    public void UpdateMessages()
    {
        // remove last message is at max messages
        if (messages.Count > maxMessages)
        {
            Destroy(messages[messages.Count - 1]);
            messages.RemoveAt(messages.Count - 1);
        }

        // << UPDATE MESSAGE LIST >>
        message_list.Clear();

        foreach (GameObject message in messages)
        {
            message_list.Add(message.GetComponent<TextMeshProUGUI>().text);
        }

        // << ADJUST POSITIONS >>
        for (int i = 0; i < messages.Count; i++)
        {
            // slowly move the messages to their position
            messages[i].transform.position = Vector3.Lerp(messages[i].transform.position, spawnParent.transform.position + new Vector3(0, (i + 1) * line_spacing), messageMoveSpeed * Time.deltaTime);
        }

    }

    public void NewMessage(string input_text, float delay)
    {
        StartCoroutine(MessageDelay(input_text, defaultColor, delay));
    }

    public void NewMessage(string input_text)
    {
        GameObject newMessage = Instantiate(messagePrefab, spawnParent);
        newMessage.transform.position = spawnParent.transform.position;

        TextMeshProUGUI textUI = newMessage.GetComponent<TextMeshProUGUI>();

        // set colors
        textUI.color = defaultColor;
        textUI.text = DecodeColorString(input_text);

        // fade out ui
        StartCoroutine(FadeOutText(textUI, messageFadeDuration));

        // insert into list
        messages.Insert(0, newMessage);
    }

    public void NewMessage(string input_text, Color base_color)
    {
        GameObject newMessage = Instantiate(messagePrefab, spawnParent);
        newMessage.transform.position = spawnParent.transform.position;

        TextMeshProUGUI textUI = newMessage.GetComponent<TextMeshProUGUI>();

        textUI.color = base_color;
        textUI.text = DecodeColorString(input_text);

        StartCoroutine(FadeOutText(textUI, messageFadeDuration));

        messages.Insert(0, newMessage);
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
                else if (openingCommand == purpleCommand)
                {
                    colorTag = "<color=#" + ColorToHex(purpleColor) + ">";
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

    public MessageEventListener EventMessage(float baseValue, EventValCompare comparison, float checkValue, string message)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<GameConsole>(), baseValue, comparison, checkValue, message, Color.white);
        return eventListener;
    }

    public MessageEventListener EventMessage(float baseValue, EventValCompare comparison, float checkValue, string message, Color color)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<GameConsole>(), baseValue, comparison, checkValue, message, color);
        return eventListener;
    }

    public MessageEventListener EventMessage(bool checkBool, bool bool_switch, string message)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<GameConsole>(), checkBool, bool_switch, message, Color.white);
        return eventListener;
    }

    public MessageEventListener EventMessage(bool checkBool, bool bool_switch, string message, Color color)
    {
        MessageEventListener eventListener = new MessageEventListener(GetComponent<GameConsole>(), checkBool, bool_switch, message, color);
        return eventListener;
    }

    public string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public IEnumerator MessageDelay(string input_text, Color color, float delay)
    {
        yield return new WaitForSeconds(delay);

        NewMessage(input_text, color);
    }

    public void MessageList(List<string> list, Color color, float delay)
    {
        StartCoroutine(MessageListCoroutine(list, color, delay));
    }

    IEnumerator MessageListCoroutine(List<string> list, Color color, float delay)
    {
        foreach (string msg in list)
        {
            NewMessage(msg, color);

            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator FadeOutText(TextMeshProUGUI text, float fadeDuration)
    {
        // Get the initial color of the text
        Color initialColor = text.color;

        // Calculate the amount to fade per frame
        float fadeAmount = Time.deltaTime / fadeDuration;

        // Fade the text out
        while (text.color.a > 0)
        {
            initialColor.a -= fadeAmount;
            text.color = initialColor;
            yield return null;
        }

        // Destroy the text game object once it's completely faded out
        if (text != null)
        {
            messages.Remove(text.gameObject);
            Destroy(text.gameObject);
        }
    }

    public void Clear()
    {
        foreach (GameObject msg in messages)
        {
            Destroy(msg);
        }

        messages.Clear();
        message_list.Clear();
    }
}

public class MessageEventListener
{
    public GameConsole gameConsole;
    public EntityConsole entityConsole;
    public Color color;

    public float checkValue;
    public float baseValue;

    public bool checkBool;
    public bool bool_switch;

    public EventValCompare comparison;
    public string message;

    private bool messageSent = false;

    public MessageEventListener() { }
    public MessageEventListener(GameConsole console, float checkValue, EventValCompare comparison, float baseValue, string message, Color color)
    {
        this.gameConsole = console;
        this.color = color;
        this.checkValue = checkValue;
        this.baseValue = baseValue;
        this.comparison = comparison;
        this.message = message;
    }

    public MessageEventListener(GameConsole console, bool checkBool, bool bool_switch, string message, Color color)
    {
        this.gameConsole = console;
        this.color = color;
        this.checkBool = checkBool;
        this.bool_switch = bool_switch;
        this.message = message;
    }

    public MessageEventListener(EntityConsole console, float checkValue, EventValCompare comparison, float baseValue, string message, Color color)
    {
        this.entityConsole = console;
        this.color = color;
        this.checkValue = checkValue;
        this.baseValue = baseValue;
        this.comparison = comparison;
        this.message = message;
    }

    public MessageEventListener(EntityConsole console, bool checkBool, bool bool_switch, string message, Color color)
    {
        this.entityConsole = console;
        this.color = color;
        this.checkBool = checkBool;
        this.bool_switch = bool_switch;
        this.message = message;
    }

    public void EventUpdate(bool checkBool)
    {
        switch (bool_switch)
        {
            case true:
                if (checkBool && !messageSent)
                {
                    SendMessageToConsole(message, color);
                    messageSent = true;
                }
                else if (!checkBool) { messageSent = false; }
                break;
            case false:
                if (!checkBool && !messageSent)
                {
                    SendMessageToConsole(message, color);
                    messageSent = true;
                }
                else if (checkBool) { messageSent = false; }
                break;
        }
    }

    public void EventUpdate(float checkValue)
    {
        switch (comparison)
        {
            case EventValCompare.IS_EQUAL:
                if (Mathf.Approximately(checkValue, baseValue))
                {
                    if (!messageSent)
                    {
                        SendMessageToConsole(message, color);
                        messageSent = true;
                    }
                }
                else
                {
                    messageSent = false;
                }
                break;
            case EventValCompare.IS_GREATER:
                if (checkValue > baseValue)
                {
                    if (!messageSent)
                    {
                        SendMessageToConsole(message, color);
                        messageSent = true;
                    }
                }
                else
                {
                    messageSent = false;
                }
                break;
            case EventValCompare.IS_LESS:
                if (checkValue < baseValue)
                {
                    if (!messageSent)
                    {
                        SendMessageToConsole(message, color);
                        messageSent = true;
                    }
                }
                else
                {
                    messageSent = false;
                }
                break;
            case EventValCompare.IS_GREATER_EQUAL:
                if (checkValue >= baseValue)
                {
                    if (!messageSent)
                    {
                        SendMessageToConsole(message, color);
                        messageSent = true;
                    }
                }
                else
                {
                    messageSent = false;
                }
                break;
            case EventValCompare.IS_LESS_EQUAL:
                if (checkValue <= baseValue)
                {
                    if (!messageSent)
                    {
                        SendMessageToConsole(message, color);
                        messageSent = true;
                    }
                }
                else
                {
                    messageSent = false;
                }
                break;
            default:
                break;
        }
    }


    void SendMessageToConsole(string message, Color color)
    {
        if (gameConsole)
        {
            gameConsole.NewMessage(message, color);
        }
        
        if (entityConsole)
        {
            entityConsole.NewMessage(message, color);
        }
    }

}
