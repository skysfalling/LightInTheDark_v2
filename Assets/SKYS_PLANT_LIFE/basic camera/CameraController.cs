using UnityEngine;
public class CameraController : MonoBehaviour
{
    public Transform target;        // the point around which the camera will rotate and focus
    public float distance = 5.0f;   // the distance between the camera and the target
    public float sensitivityX = 4.0f;   // horizontal rotation speed
    public float sensitivityY = 1.0f;   // vertical rotation speed
    public float zoomSpeed = 2.0f;   // zoom speed
    public float minDistance = 2.0f;   // minimum distance between the camera and the target
    public float maxDistance = 10.0f;  // maximum distance between the camera and the target
    public float minYAngle = -20.0f;   // minimum vertical angle of the camera
    public float maxYAngle = 80.0f;   // maximum vertical angle of the camera
    public float zoomSmoothTime = 0.2f;   // time it takes to smoothly zoom to target distance

    private float currentDistance;   // the current distance between the camera and the target
    private float targetDistance;    // the target distance for zooming
    private float mouseX = 0.0f;   // horizontal rotation value
    private float mouseY = 0.0f;   // vertical rotation value
    private Vector3 smoothDampVelocity;   // velocity for smooth damping

    void Start()
    {
        currentDistance = distance;
        targetDistance = distance;
        mouseX = transform.eulerAngles.y;
        mouseY = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        // handle mouse input
        if (Input.GetMouseButton(0))
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivityX;
            mouseY -= Input.GetAxis("Mouse Y") * sensitivityY;

            // clamp vertical angle
            mouseY = Mathf.Clamp(mouseY, minYAngle, maxYAngle);
        }

        // handle zoom input
        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // smoothly interpolate current distance to target distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime / zoomSmoothTime);

        // calculate camera position based on target and rotation values
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0.0f);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance) + target.position;

        // set camera position and rotation
        transform.rotation = rotation;
        transform.position = position;
    }
}
