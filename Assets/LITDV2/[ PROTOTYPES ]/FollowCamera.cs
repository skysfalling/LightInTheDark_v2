using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour
{
	#region [[ PRIVATE ]]
	private Transform _defaultLookTarget;
	private float _defaultMoveSpeed;
	private float _defaultRotationSpeed;

	void Awake()
	{
		_defaultLookTarget = lookTarget;
		_defaultMoveSpeed = cameraMoveSpeed;
		_defaultRotationSpeed = cameraRotationSpeed;
	}

	#endregion

	public Transform orbitCenter;
	public Transform orbitHandle;
	public Transform positionTarget;
	public Transform lookTarget;
	public Camera camera3D;

	[Range(1, 100)]
	public float orbitRadius = 10f;

	[Range(0, 360)]
	public float currOrbitOffset = 180f;

	[Range(-50, 50)]
	public float camHeightOffset = 10f;

	[Header("CAMERA SETTINGS")]
	[Range(1, 50)]
	public float cameraMoveSpeed = 10f;

	[Range(1, 50)]
	public float cameraRotationSpeed = 20f;

	[Header("OPERATOR STATES")]
	public bool overrideState = false;

	void FixedUpdate()
	{
		// The player is moving on FixedUpdate so the camera must as well
		UpdateOrbitHandler(Time.deltaTime);
		UpdateCamera(Time.deltaTime);
	}

	void UpdateOrbitHandler(float delta)
	{
		if (orbitCenter != null)
		{
			Vector3 nodePosition = orbitCenter.position;
			Vector3 targetPosition = new Vector3(
				nodePosition.x,
				orbitHandle.position.y,
				nodePosition.z
			);
			orbitHandle.position = targetPosition;

			// Update handle to rotate with the center
			Vector3 targetRotation = orbitCenter.eulerAngles + (Vector3.up * currOrbitOffset);
			orbitHandle.eulerAngles = targetRotation;

			// Clamp yaw (orbit_offset)
			currOrbitOffset = currOrbitOffset % 360;
			if (currOrbitOffset < 0)
			{
				currOrbitOffset += 360;
			}
		}

		// Update orbit radius [position target local position]
		positionTarget.localPosition = Vector3.forward * orbitRadius;
	}

	void UpdateCamera(float delta)
	{
		// Move to target position
		Vector3 targetPosition = positionTarget.position;
		Vector3 newPosition = new Vector3(targetPosition.x, camHeightOffset, targetPosition.z);
		camera3D.transform.position = Vector3.Lerp(
			camera3D.transform.position,
			newPosition,
			cameraMoveSpeed * delta
		);
	}

	public void SetOrbitOffset(float angle)
	{
		currOrbitOffset = angle;
	}

	public void AdjustOrbitOffset(float angle)
	{
		currOrbitOffset += angle;
	}

	public void SetLookTarget(Transform target)
	{
		lookTarget = target;
		Debug.Log("set look target : " + target.name);
	}

	void OnDrawGizmos()
	{
		if (orbitHandle != null && positionTarget != null)
		{
			// Draw the orbit radius
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(orbitHandle.position, orbitRadius);

			// Draw a line from the orbit handle to the position target to visualize the orbit radius in direction
			Gizmos.DrawLine(orbitHandle.position, positionTarget.position);

			// Visualizing the camera height offset
			if (camera3D != null)
			{
				Vector3 cameraHeightPoint = new Vector3(
					camera3D.transform.position.x,
					orbitHandle.position.y + camHeightOffset,
					camera3D.transform.position.z
				);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(camera3D.transform.position, cameraHeightPoint);
				Gizmos.DrawSphere(cameraHeightPoint, 0.5f); // Small sphere to mark the height offset point
			}

			// If you want to draw a line towards the defaultLookTarget
			if (_defaultLookTarget != null)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(camera3D.transform.position, _defaultLookTarget.position);
			}
		}
	}
}
