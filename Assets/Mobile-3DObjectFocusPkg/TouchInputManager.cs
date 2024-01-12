using UnityEngine;
using UnityEngine.Events;

public class TouchInputManager : MonoBehaviour
{
    // Define the data structure to hold tap details
    public class TapEventData
    {
        public Vector2 ScreenPosition;
        public Vector3 WorldPosition;
        public bool IsSingleTap;

        public TapEventData(Vector2 screenPosition, Vector3 worldPosition, bool isSingleTap)
        {
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
            IsSingleTap = isSingleTap;
        }
    }

    // Custom UnityEvent that uses TapEventData
    [System.Serializable]
    public class TapEvent : UnityEvent<TapEventData> { }

    private Vector2 touchStart;
    private Vector2 touchEnd;
    private bool swipeDetected = false;

    public float minimumSwipeDistance = 10f; // Minimum distance for a swipe to be registered

    private int tapCount; // Count the number of taps
    private float tapTimer; // Timer to measure time between taps
    public float doubleTapTime = 0.2f; // Time interval to consider for double tap

    // Unity Events
    public TapEvent onSingleTapInput;
    public TapEvent onDoubleTapInput;


    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    swipeDetected = false;
                    tapCount++;
                    break;

                case TouchPhase.Moved:
                    Vector2 touchMovement = touch.deltaPosition;

                    // Handle touch movement action
                    CameraManager cameraManager = FindObjectOfType<CameraManager>();
                    if (cameraManager != null)
                    {
                        cameraManager.HandleTouchMovement(touchMovement);
                    }
                    break;

                case TouchPhase.Ended:
                    touchEnd = touch.position;

                    // [[ DETECT TAP ]]
                    if (Vector2.Distance(touchStart, touchEnd) < minimumSwipeDistance)
                    {
                        DetectTap();
                    }
                    break;
            }
        }


        // << WAIT FOR SECOND TAP >>
        if (tapCount == 1)
        {
            tapTimer += Time.deltaTime;
            if (tapTimer > doubleTapTime)
            {
                tapTimer = 0;
                tapCount = 0;
            }
        }
        
    }


    void DetectTap()
    {
        Debug.Log($"Detect Tap {tapCount}");

        // [[ REGISTER SINGLE TAP ]]
        if (tapCount == 1)
        {
            tapTimer = 0;
            Invoke(nameof(SingleTap), doubleTapTime);
        }

        // [[ REGISTER DOUBLE TAP ]]
        else if (tapCount == 2)
        {
            CancelInvoke(nameof(SingleTap));
            DoubleTap();
            tapCount = 0; // Reset tap count
        }
    }

    void DetectSwipe()
    {
        if (Vector2.Distance(touchStart, touchEnd) >= minimumSwipeDistance && !swipeDetected)
        {
            Vector2 direction = touchEnd - touchStart;

            // Handle swipe action
            CameraManager cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager != null)
            {
                cameraManager.HandleTouchMovement(direction);
            }

            swipeDetected = true;
        }
    }

    void SingleTap()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchStart.x, touchStart.y, Camera.main.nearClipPlane));
        onSingleTapInput.Invoke(new TapEventData(touchStart, worldPosition, true));
        tapCount = 0;
    }

    void DoubleTap()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchStart.x, touchStart.y, Camera.main.nearClipPlane));
        onDoubleTapInput.Invoke(new TapEventData(touchStart, worldPosition, false));
    }

    public int GetTapCount()
    {
        return tapCount;
    }
}
