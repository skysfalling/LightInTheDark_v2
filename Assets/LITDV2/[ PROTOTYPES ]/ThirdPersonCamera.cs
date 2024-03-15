using System;
using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
	private Camera _cameraComponent => GetComponent<Camera>();
	private Vector3 _cameraOffset => new Vector3(_xPosOffset, _yPosOffset, _zPosOffset);

	#region [[ PUBLIC INSPECTOR VARIABLES ]]
	[Header("Transforms")]
	public Transform playerTarget; // #TODO Default to player object

	[Header("PositionOffset"), SerializeField, Range(-50, 50)]
	private int _xPosOffset = 0;

	[SerializeField, Range(0, 100)]
	private int _yPosOffset = 50;

	[SerializeField, Range(-100, 0)]
	private int _zPosOffset = -50;
	
	[Header("RotationOffset"), SerializeField, Range(-180, 180)]
	private int _xRotOffset = -45;
		[SerializeField, Range(-90, 90)]
	private int _yRotOffset = 0;

	[SerializeField, Range(-45, 45)]
	private int _zPotOffset = -50;

	[Header("Speeds"), SerializeField, Range(0, 10)]
	private int followSpeed = 2;

	[SerializeField, Range(0, 10)]
	private int rotateSpeed = 2;

	#endregion

	public void Start()
	{
		// Ensure the camera maintains a fixed offset from the player at start
		if (playerTarget)
		{
			transform.position = playerTarget.position + _cameraOffset;
		}
	}

	public void FixedUpdate()
	{
		HandleCameraMovement();
		HandleCameraRotation();
	}

	public void SetToEditorValues()
	{
		_cameraComponent.transform.position = playerTarget.position + _cameraOffset;

		Vector3 direction = (
			playerTarget.position - _cameraComponent.transform.position
		);
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		_cameraComponent.transform.rotation = Quaternion.Slerp(
			transform.rotation,
			lookRotation,
			rotateSpeed * Time.deltaTime
		);
	}

	public void HandleCameraMovement()
	{
		// Get the player position
		Vector3 playerPosition = playerTarget.position;

		//Apply the offsets & Lerp the position
		Vector3 camOffsetPosition = playerPosition + new Vector3(_xPosOffset, _yPosOffset, _zPosOffset);
		_cameraComponent.transform.position = Vector3.Lerp(
			_cameraComponent.transform.position,
			camOffsetPosition,
			followSpeed * Time.deltaTime
		);
	}

	public void HandleCameraRotation()
	{
		Vector3 direction = (playerTarget.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));
		transform.rotation = Quaternion.Slerp(
			transform.rotation,
			lookRotation,
			rotateSpeed * Time.deltaTime
		);
	}

	void OnDrawGizmos()
	{
		Vector3 playerPosition = playerTarget.position;

		// Draw z Offset
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(playerPosition, playerPosition + (Vector3.forward * _zPosOffset));

		Gizmos.color = Color.red;
		Gizmos.DrawLine(playerPosition, playerPosition + (Vector3.right * _xPosOffset));

		Gizmos.color = Color.green;
		Gizmos.DrawLine(playerPosition, playerPosition + (Vector3.up * _yPosOffset));
	}
}
