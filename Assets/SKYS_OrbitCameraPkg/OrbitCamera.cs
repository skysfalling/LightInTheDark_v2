using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class OrbitCamera : MonoBehaviour
{
    bool _handleTouchMovement;
    Vector3 _rotationVelocity = Vector3.zero; // Current rotational velocity

    public Camera connectedCamera;
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
        if (connectedCamera == null) { GetComponentInChildren<Camera>(); }
        targetOrbitRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (_handleTouchMovement)
        {
            if (targetOrbitRotation != transform.rotation.eulerAngles)
            {
                // Clamp Target Rotation
                targetOrbitRotation = ClampRotation(targetOrbitRotation, xAxisClamp, zAxisClamp);

            }
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

    public void HandleTouchMovement(Vector2 swipeDirection)
    {
        _handleTouchMovement = true;

        // Define the rotation sensitivity (degrees per swipe unit)
        float rotationSensitivity = 0.1f;

        // Translate swipe direction to rotation
        float horizontalRotation = swipeDirection.x * rotationSensitivity;
        float verticalRotation = -swipeDirection.y * rotationSensitivity;

        // Update target rotation
        targetOrbitRotation += new Vector3(verticalRotation, horizontalRotation, 0);
    }

    public void DisableTouchMovement()
    {
        _handleTouchMovement = false;
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
