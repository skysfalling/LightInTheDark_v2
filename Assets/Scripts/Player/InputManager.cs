using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public bool selectInput;

    [Header("Input Actions")]
    public InputAction selectAction;
    public InputAction touchAction; // New InputAction for touch

    [Header("Interact Particles")]
    public GameObject interactParticles;

    public Vector2 selectedScreenPosition;
    public Vector3 moveDirection = Vector3.zero;

    private void OnEnable()
    {
        selectAction.Enable();
        touchAction.Enable();
    }

    private void OnDisable()
    {
        selectAction.Disable();
        touchAction.Disable();
    }

    private void Awake()
    {
        selectAction.started += context => selectInput = true;
        selectAction.performed += context => selectInput = false;

        touchAction.started += OnTouchStarted;
        touchAction.canceled += OnTouchEnded;

    }


    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Touch started"); 

        // Handle touch start
        /*
        Vector2 touchPosition = context.ReadValue<Vector2>();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10)); // Adjust Z as needed

        if (interactParticles)
            Instantiate(interactParticles, worldPosition, Quaternion.identity);
        */
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        Debug.Log("Touch ended");
        touchAction.Reset();
        // Handle touch end if needed
    }
}
