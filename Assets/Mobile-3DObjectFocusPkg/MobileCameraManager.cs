using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MobileCameraManager : MonoBehaviour
{
    bool _handleTouchMovement;
    Vector3 _rotationVelocity = Vector3.zero; // Current rotational velocity

    public Vector3 targetPivotRotation = Vector3.zero;
    public float cameraRotationSpeed = 1;
    public float rotationDeceleration = 0.95f; // Deceleration factor



    Vector2 xAxisClamp = new Vector2(0, 90);
    Vector2 yAxisClamp = new Vector2(0, 360);
    Vector2 zAxisClamp = new Vector2(0, 0);


    private void Start()
    {
        targetPivotRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (_handleTouchMovement)
        {
            if (targetPivotRotation != transform.rotation.eulerAngles)
            {
                Vector3 desiredRotation = targetPivotRotation;

                // Clamp each axis individually
                desiredRotation.x = ClampAngle(desiredRotation.x, xAxisClamp.x, xAxisClamp.y);
                //desiredRotation.y = ClampAngle(desiredRotation.y, yAxisClamp.x, yAxisClamp.y);
                desiredRotation.z = ClampAngle(desiredRotation.z, zAxisClamp.x, zAxisClamp.y);

                // Calculate the rotational step
                Quaternion targetQuaternion = Quaternion.Euler(desiredRotation);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, cameraRotationSpeed * Time.deltaTime);

                // Update the rotational velocity
                _rotationVelocity = (targetQuaternion.eulerAngles - transform.rotation.eulerAngles) / Time.deltaTime;
            }
        }
        else
        {
            // Apply deceleration to the rotational velocity
            _rotationVelocity *= rotationDeceleration;

            // Apply the decelerated rotation
            transform.Rotate(_rotationVelocity * Time.deltaTime, Space.World);

            // Optional: Stop the rotation if it's slow enough
            if (_rotationVelocity.magnitude < 0.01f)
            {
                _rotationVelocity = Vector3.zero;
            }
        }

    }

    public void SetPivotRotation(Vector3 rotationTarget)
    {
        targetPivotRotation = rotationTarget;
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
        targetPivotRotation += new Vector3(verticalRotation, horizontalRotation, 0);
    }

    public void DisableTouchMovement()
    {
        _handleTouchMovement = false;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        //angle = NormalizeAngle(angle);
        return Mathf.Clamp(angle, min, max);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }
}
