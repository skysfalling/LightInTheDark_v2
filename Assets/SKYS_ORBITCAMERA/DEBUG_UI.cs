using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Device;

public class DEBUG_UI : MonoBehaviour
{
    public TextMeshProUGUI debugTMP;
    public Image touchPointImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if there is at least one touch happening.
        if (Input.touchCount > 0)
        {
            // Get the first touch.
            Touch touch = Input.GetTouch(0);

            // Construct a message based on the touch phase.
            string inputType = "";

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    inputType = "TouchPhase.Began";
                    break;
                case TouchPhase.Moved:
                    inputType = "TouchPhase.Moved";
                    break;
                case TouchPhase.Stationary:
                    inputType = "TouchPhase.Stationary";
                    break;
                case TouchPhase.Ended:
                    inputType = "TouchPhase.Ended";
                    break;
                case TouchPhase.Canceled:
                    inputType = "TouchPhase.Canceled";
                    break;
                default:
                    inputType = "Input Type Null";
                    break;
            }

            debugTMP.text = inputType;
        }
    }

    /*
    public void VisualizeTapData(TouchInputManager.TapEventData eventData)
    {
        // Use eventData here
        //Debug.Log($"Tap received at screen position: {eventData.ScreenPosition}");
        //Debug.Log($"World position: {eventData.WorldPosition}");
        

        Debug.Log($"Tap Event Data : IsSingleTap {eventData.IsSingleTap} , ScreenPosition: {eventData.ScreenPosition}");


        RectTransform canvasRect = touchPointImage.canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.ScreenPosition, touchPointImage.canvas.worldCamera, out localPoint);

        touchPointImage.rectTransform.anchoredPosition = localPoint;

        if (eventData.IsSingleTap) { touchPointImage.color = Color.red; }
        else { touchPointImage.color = Color.blue; }
    }
        */

}
