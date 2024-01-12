using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MobileInputManager : MonoBehaviour
{
    // InputAction: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputAction.html

    [Header("Interact Particles")]
    public GameObject interactParticles;

    [Header("Movement")]
    public Vector2 moveDirection = Vector2.zero;
    public InputAction directionAction;

    [Header("A Button")]
    public bool aInput;
    public InputAction aAction;

    [Header("Y Attack")]
    public bool bInput;
    public InputAction bAction;

    private void OnEnable()
    {
        directionAction.Enable();
        aAction.Enable();
        bAction.Enable();
    }

    private void OnDisable()
    {
        directionAction.Disable();
        aAction.Disable();
        bAction.Disable();
    }

    private void Update()
    { 
        moveDirection = directionAction.ReadValue<Vector2>();

        aAction.started += context => aInput = true;
        aAction.canceled += context => aInput = false;

        bAction.started += context => bInput = true;
        bAction.canceled += context => bInput = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                // Convert touch position to world space
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10)); // Adjust Z as needed

                // Instantiate the GameObject
                Instantiate(interactParticles, touchPosition, Quaternion.identity);
            }
        }
    }

}
