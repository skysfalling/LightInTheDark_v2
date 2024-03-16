namespace Darklight.UniversalInput
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class UniversalInputManager : MonoBehaviour
    {
        public static UniversalInputManager Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        string prefix = ":: UNIVERSAL INPUT MANAGER >> ";

        public enum InputType
        {
            NULL,
            TOUCH,
            MOUSE_ONLY,
            MOUSE_AND_KEYBOARD,
            CONTROLLER
        }

        [HideInInspector]
        public InputType inputType = InputType.NULL;

        [Header("Input Action Map")]
        public InputActionAsset UniversalBasicInputActions;
        InputActionMap BasicTouchActionMap;
        InputActionMap BasicMouseActionMap;
        InputActionMap BasicControllerActionMap;
        InputActionMap KeyboardMovementActionMap;

        // Input Actions
        [HideInInspector]
        public InputAction pointerScreenPosition;

        [HideInInspector]
        public InputAction primaryInteract;

        [HideInInspector]
        public InputAction secondaryInteract;

        [HideInInspector]
        public InputAction moveInput;

        [System.Serializable]
        public class WorldPointerEvent : UnityEvent<Vector3> { }

        public WorldPointerEvent activePointerPositionEvent;
        public WorldPointerEvent primaryInteractionEvent;
        public WorldPointerEvent secondaryInteractionEvent;

        [System.Serializable]
        public class MoveInputEvent : UnityEvent<Vector2> { }

        public MoveInputEvent moveInputEvent;

        private void OnEnable()
        {
            UniversalBasicInputActions.Enable();
        }

        private void OnDisable()
        {
            UniversalBasicInputActions.Disable();
        }

        private void Start()
        {
            BasicTouchActionMap = UniversalBasicInputActions.FindActionMap("BasicTouch");
            BasicMouseActionMap = UniversalBasicInputActions.FindActionMap("BasicMouse");
            BasicControllerActionMap = UniversalBasicInputActions.FindActionMap("BasicController");
            KeyboardMovementActionMap = UniversalBasicInputActions.FindActionMap(
                "KeyboardMovement"
            );

            bool deviceFound = DetectAndEnableInputDevice();
            if (deviceFound)
            {
                pointerScreenPosition.performed += context =>
                //InvokeActivePointerPositionEvent(pointerScreenPosition.ReadValue<Vector2>());
                primaryInteract.performed += context =>
                //InvokePrimaryInteractionEvent(pointerScreenPosition.ReadValue<Vector2>());
                secondaryInteract.performed += context =>
                //InvokeSecondaryInteractionEvent(pointerScreenPosition.ReadValue<Vector2>());
                moveInput.performed += context =>
                    InvokeMoveInteractionEvent(moveInput.ReadValue<Vector2>()); // Sent out move input value
                moveInput.canceled += context => InvokeMoveInteractionEvent(Vector2.zero); // Reset input value
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

                Debug.Log(prefix + $" BasicMouseActionMap Enabled");

                if (Keyboard.current != null)
                {
                    inputType = InputType.MOUSE_AND_KEYBOARD;
                    KeyboardMovementActionMap.Enable();
                    Debug.Log(prefix + $" Keyboard Movement Enabled");

                    moveInput = KeyboardMovementActionMap.FindAction("MoveInput");
                }
                else
                {
                    inputType = InputType.MOUSE_ONLY;
                }
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

        void InvokeActivePointerPositionEvent(Vector2 pointerScreenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 worldPointerPosition = hit.point + new Vector3(0, 0.5f, 0); // Adjust the Y offset as needed
                activePointerPositionEvent.Invoke(worldPointerPosition);
                //Debug.Log(prefix + $" Invoke PrimaryInteractionEvent {worldPointerPosition})");
            }
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

        void InvokeMoveInteractionEvent(Vector2 moveInput)
        {
            moveInputEvent.Invoke(moveInput);
        }
    }
}
