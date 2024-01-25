using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class OrbitCamera : MonoBehaviour
{
    string _prefix = "SKYS_ORBIT_CAMERA >> ";
    bool _handleDragOrbit;
    Vector3 _rotationVelocity = Vector3.zero; // Current rotational velocity

    UniversalInputManager _universalInputManager;
    public InputActionAsset orbitCameraInteraction;
    InputActionMap actionMap;
    InputAction startDragOrbit;
    InputAction dragOrbitDelta;

    public Camera connectedCamera;

    [Space(5)]
    public Vector3 targetOrbitRotation = Vector3.zero;
    public float cameraRotationSpeed = 1;
    public float decelerationRate = 0.95f; // Deceleration factor

    Vector2 xAxisClamp = new Vector2(0, 90);
    Vector2 zAxisClamp = new Vector2(0, 0);


    public float zoomSpeed = 0.1f;
    [Range(10, 100)]
    public float minZoom = 5f;
    [Range(10, 100)]
    public float maxZoom = 60f;

    private void Start()
    {
        _universalInputManager = FindObjectOfType<UniversalInputManager>();
        orbitCameraInteraction.Enable();

        if (_universalInputManager.inputType == UniversalInputManager.InputType.TOUCH) { actionMap = orbitCameraInteraction.FindActionMap("Touch"); }
        else if (_universalInputManager.inputType == UniversalInputManager.InputType.MOUSE)
        {
            actionMap = orbitCameraInteraction.FindActionMap("Mouse");
        }

        startDragOrbit = actionMap.FindAction("StartDragOrbit");
        dragOrbitDelta = actionMap.FindAction("DragOrbitDelta");

        startDragOrbit.performed += context => HandleOrbitInput(dragOrbitDelta.ReadValue<Vector2>());
        startDragOrbit.canceled += context => DisableOrbitInput();


        _universalInputManager.primaryInteract.performed += context => Debug.Log(_prefix + "primaryInteract");


        targetOrbitRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (_handleDragOrbit)
        {
            HandleOrbitInput(dragOrbitDelta.ReadValue<Vector2>());
        }

        // Smoothly interpolate to the target rotation
        Quaternion targetQuaternion = Quaternion.Euler(targetOrbitRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, cameraRotationSpeed * Time.deltaTime);

    }

    public Vector3 ClampRotation(Vector3 rotation, Vector3 xAxisClamp, Vector3 zAxisClamp)
    {
        // Clamp each axis individually
        Vector3 clampedRotation = rotation;
        clampedRotation.x = ClampAngle(clampedRotation.x, xAxisClamp.x, xAxisClamp.y);
        clampedRotation.z = ClampAngle(clampedRotation.z, zAxisClamp.x, zAxisClamp.y);

        return clampedRotation;
    }

    public void SetPivotRotation(Vector3 rotationTarget)
    {
        targetOrbitRotation = rotationTarget;
    }

    public void HandleOrbitInput(Vector2 swipeDirection)
    {
        _handleDragOrbit = true;
        Debug.Log("DragOrbit");

        // Define the rotation sensitivity (degrees per swipe unit)
        float rotationSensitivity = 1f;

        // Translate swipe direction to rotation
        float horizontalRotation = swipeDirection.x * rotationSensitivity;
        float verticalRotation = -swipeDirection.y * rotationSensitivity;

        // Update target rotation
        targetOrbitRotation += new Vector3(verticalRotation, horizontalRotation, 0);

        if (targetOrbitRotation != transform.rotation.eulerAngles)
        {
            // Clamp Target Rotation
            targetOrbitRotation = ClampRotation(targetOrbitRotation, xAxisClamp, zAxisClamp);
        }
    }

    public void DisableOrbitInput()
    {
        _handleDragOrbit = false;
        Debug.Log("CancelDragOrbit");
    }

    public void PinchToZoom(float distanceDelta)
    {

        // Instead of using FOV, use local Z position
        Vector3 mobileCamLocalPos = connectedCamera.transform.localPosition;
        float destinationZ = Mathf.Abs(mobileCamLocalPos.z) + (distanceDelta * -0.1f);
        destinationZ = Mathf.Clamp(destinationZ, minZoom, maxZoom);

        // Set negative Z local position
        Vector3 destinationPosition = new Vector3(mobileCamLocalPos.x, mobileCamLocalPos.y, -destinationZ);
        connectedCamera.transform.localPosition = Vector3.Lerp(connectedCamera.transform.localPosition, destinationPosition, zoomSpeed * Time.deltaTime);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        //angle = NormalizeAngle(angle);
        return Mathf.Clamp(angle, min, max);
    }

}
