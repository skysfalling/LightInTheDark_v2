using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UniversalInputManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction pointerPosition;
    public InputAction primarySelect;
    public InputAction secondarySelect;
    public InputAction cameraZoom;
    public InputAction cameraOrbit;


    private void OnEnable()
    {
        pointerPosition.Enable();
        primarySelect.Enable();
        secondarySelect.Enable();
        cameraZoom.Enable();
        cameraOrbit.Enable();
    }

    private void OnDisable()
    {
        pointerPosition.Disable();
        primarySelect.Disable();
        secondarySelect.Disable();
        cameraZoom.Disable();
        cameraOrbit.Disable();
    }

    private void Awake()
    {

    }

    /*
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Touch started"); 

        // Handle touch start
        Vector2 touchPosition = pointerPosition.ReadValue<Vector2>();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10)); // Adjust Z as needed
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        Debug.Log("Touch ended");
        selectAction.Reset();
    }
    */
}
