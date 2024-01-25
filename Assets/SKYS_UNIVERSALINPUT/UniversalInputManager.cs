using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UniversalInputManager : MonoBehaviour
{
    string prefix = ":: UNIVERSAL INPUT MANAGER >> ";

    [Header("Input Actions")]
    public InputActionAsset UniversalBasicInputActions;
    InputActionMap BasicTouchActionMap;
    InputActionMap BasicMouseActionMap;
    InputActionMap BasicControllerActionMap;

    private InputAction pointerPosition;
    private InputAction primaryInteract;
    private InputAction secondaryInteract;

    private bool isAwaitingSecondTap = false;
    private float tapDelay = 0.3f; // Delay in seconds to wait for a second tap
    private Coroutine primarySelectCoroutine;

    private Vector2 startSwipePosition;
    private Vector2 endSwipePosition;
    private bool isSwiping = false;

    private void OnEnable()
    {
        UniversalBasicInputActions.Enable();
    }

    private void OnDisable()
    {
        UniversalBasicInputActions.Disable();
    }

    private void Awake()
    {
        BasicTouchActionMap = UniversalBasicInputActions.FindActionMap("BasicTouch");
        BasicMouseActionMap = UniversalBasicInputActions.FindActionMap("BasicMouse");
        BasicControllerActionMap = UniversalBasicInputActions.FindActionMap("BasicController");

        bool deviceFound = DetectAndEnableInputDevice();
        if (deviceFound)
        {
            primaryInteract.performed += context => Debug.Log($"Primary Interact : {primaryInteract}");
            secondaryInteract.performed += context => Debug.Log($"Secondary Interact : {secondaryInteract}");
        }
    }

    private void Update()
    {
        
    }

    private bool DetectAndEnableInputDevice()
    {
        // Disable all action maps initially
        BasicTouchActionMap.Disable();
        BasicMouseActionMap.Disable();
        BasicControllerActionMap.Disable();

        // Enable the appropriate action map based on the current input device
        if (Touchscreen.current != null)
        {
            BasicTouchActionMap.Enable();
            pointerPosition = BasicTouchActionMap.FindAction("PointerPosition");
            primaryInteract = BasicTouchActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicTouchActionMap.FindAction("SecondaryInteract");
            Debug.Log(prefix + $" BasicTouchActionMap Enabled");

        }
        else if (Mouse.current != null)
        {
            BasicMouseActionMap.Enable();
            pointerPosition = BasicMouseActionMap.FindAction("PointerPosition");
            primaryInteract = BasicMouseActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicMouseActionMap.FindAction("SecondaryInteract");

            Debug.Log(prefix + $" BasicMouseActionMap Enabled");
        }
        else if (Gamepad.current != null)
        {
            BasicControllerActionMap.Enable();
            pointerPosition = BasicControllerActionMap.FindAction("PointerPosition");
            primaryInteract = BasicControllerActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicControllerActionMap.FindAction("SecondaryInteract");

            Debug.Log(prefix + $" BasicControllerActionMap Enabled");
        }
        else
        {
            Debug.LogError(prefix + "Could not find Input Type");
            return false;
        }

        return true;
    }



    #region == PRIMARY / SECONDARY SELECT ====================
    void HandlePrimaryInteract()
    {
        if (isAwaitingSecondTap)
        {
            // If we are already waiting for a second tap, ignore this tap.
            return;
        }

        isAwaitingSecondTap = true;
        primarySelectCoroutine = StartCoroutine(WaitAndPerformPrimaryInteract());
    }

    IEnumerator WaitAndPerformPrimaryInteract()
    {
        yield return new WaitForSeconds(tapDelay);

        if (isAwaitingSecondTap) // If a second tap hasn't happened
        {
            Debug.Log($"Primary Select");
        }

        // Reset for the next tap
        isAwaitingSecondTap = false;
    }

    void HandleSecondaryInteract()
    {
        if (isAwaitingSecondTap)
        {
            if (primarySelectCoroutine != null)
            {
                StopCoroutine(primarySelectCoroutine);
            }
            isAwaitingSecondTap = false;
            Debug.Log($"Secondary Select");
        }
    }
    #endregion

    #region == SWIPE INPUT ==================
    private void StartSwipe()
    {
        // Capture the start position of the swipe
        startSwipePosition = pointerPosition.ReadValue<Vector2>();
        isSwiping = true;
    }

    private void EndSwipe()
    {
        if (isSwiping)
        {
            // Capture the end position of the swipe
            endSwipePosition = pointerPosition.ReadValue<Vector2>();
            Vector2 swipeDirection = (endSwipePosition - startSwipePosition).normalized;
            isSwiping = false;

            // Perform action based on swipe direction
            Debug.Log($"Swipe Direction: {swipeDirection}");
        }
    }

    #endregion
}
