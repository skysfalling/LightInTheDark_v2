using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UniversalInputManager : MonoBehaviour
{
    string prefix = ":: UNIVERSAL INPUT MANAGER >> ";
    public enum InputType { NULL, TOUCH, MOUSE, CONTROLLER }
    [HideInInspector] public InputType inputType = InputType.NULL;

    [Header("Input Actions")]
    public InputActionAsset UniversalBasicInputActions;
    InputActionMap BasicTouchActionMap;
    InputActionMap BasicMouseActionMap;
    InputActionMap BasicControllerActionMap;

    [HideInInspector] public InputAction pointerScreenPosition;
    [HideInInspector]public InputAction primaryInteract;
    [HideInInspector] public InputAction secondaryInteract;

    [System.Serializable]
    public class WorldPointerEvent : UnityEvent<Vector3> { }
    public WorldPointerEvent primaryInteractionEvent;
    public WorldPointerEvent secondaryInteractionEvent;


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
            primaryInteract.performed += context => InvokePrimaryInteractionEvent(pointerScreenPosition.ReadValue<Vector2>());
            secondaryInteract.performed += context => InvokeSecondaryInteractionEvent(pointerScreenPosition.ReadValue<Vector2>());
        }
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
            pointerScreenPosition = BasicTouchActionMap.FindAction("PointerPosition");
            primaryInteract = BasicTouchActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicTouchActionMap.FindAction("SecondaryInteract");

            inputType = InputType.TOUCH;
            Debug.Log(prefix + $" BasicTouchActionMap Enabled");

        }
        else if (Mouse.current != null)
        {
            BasicMouseActionMap.Enable();
            pointerScreenPosition = BasicMouseActionMap.FindAction("PointerPosition");
            primaryInteract = BasicMouseActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicMouseActionMap.FindAction("SecondaryInteract");

            inputType = InputType.MOUSE;
            Debug.Log(prefix + $" BasicMouseActionMap Enabled");
        }
        else if (Gamepad.current != null)
        {
            BasicControllerActionMap.Enable();
            pointerScreenPosition = BasicControllerActionMap.FindAction("PointerPosition");
            primaryInteract = BasicControllerActionMap.FindAction("PrimaryInteract");
            secondaryInteract = BasicControllerActionMap.FindAction("SecondaryInteract");

            inputType = InputType.CONTROLLER;
            Debug.Log(prefix + $" BasicControllerActionMap Enabled");
        }
        else
        {
            Debug.LogError(prefix + "Could not find Input Type");
            return false;
        }

        return true;
    }


    void InvokePrimaryInteractionEvent(Vector2 pointerScreenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPointerPosition = hit.point + new Vector3(0, 0.5f, 0); // Adjust the Y offset as needed
            primaryInteractionEvent.Invoke(worldPointerPosition);
            //Debug.Log(prefix + $" Invoke PrimaryInteractionEvent {worldPointerPosition})");
        }
    }

    void InvokeSecondaryInteractionEvent(Vector2 pointerScreenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPointerPosition = hit.point + new Vector3(0, 0.5f, 0); // Adjust the Y offset as needed
            secondaryInteractionEvent.Invoke(worldPointerPosition);
            //Debug.Log(prefix + $" Invoke SecondaryInteractionEvent {worldPointerPosition})");
        }
    }
}
