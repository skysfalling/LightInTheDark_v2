namespace Darklight.Camera
{
	using UnityEngine;
	using UnityEngine.InputSystem;
	using Darklight.UniversalInput;
	public class OrbitCamera : MonoBehaviour
	{
		string _prefix = "SKYS_ORBIT_CAMERA >> ";
		bool _handleDragOrbit;
		bool _handlePinchZoom;
		Vector3 _rotationVelocity = Vector3.zero; // Current rotational velocity
		Vector3 _targetOrbitRotation = Vector3.zero;
		Vector2 _xAxisClamp = new Vector2(0, 90);
		Vector2 _zAxisClamp = new Vector2(0, 0);
		float _dollyDelta;
		Vector3 _targetZoomPosition;
		Vector2 _prevTouch0Position;
		Vector2 _prevTouch1Position;

		UniversalInputManager _universalInputManager;

		public InputActionAsset orbitCameraInteraction;
		InputActionMap actionMap;
		InputAction startDragOrbit;
		InputAction dragOrbitDelta;
		InputAction scrollZoom;

		[Header("Camera Settings")]
		public Camera connectedCamera;

		[Space(10), Header("Focus Target")]
		public Transform focusTarget;
		[Range(0.1f, 10f)] public float focusSpeed = 2;
		public void SetFocusTarget(Transform focusTarget) { this.focusTarget = focusTarget; }

		[Space(10), Header("Follow Position Target")]
		public Transform positionTarget;
		[Range(0.1f, 10f)] public float followSpeed = 2;
		public void SetPositionTarget(Transform followTarget) { this.positionTarget = followTarget; }

		[Space(10), Header("Orbit")]
		[Range(0.1f, 1f)] public float orbitSensitivity = 0.1f;
		[Range(0.1f, 10f)] public float orbitSpeed = 1;

		[Space(10), Header("Zoom")]
		[Range(0.1f, 1f)] public float zoomSensitivity = 0.1f;
		[Range(0.1f, 10f)] public float zoomSpeed = 2f;
		public float minZ_camZoom = 5f;
		public float maxZ_camZoom = 60f;

		private void Start()
		{
			_universalInputManager = GameObject.FindFirstObjectByType<UniversalInputManager>();
			orbitCameraInteraction.Enable();

			// << DETERMINE ACTION MAP >>
			if (_universalInputManager.inputType == UniversalInputManager.InputType.TOUCH)
			{
				actionMap = orbitCameraInteraction.FindActionMap("Touch");

				// Initialize Values
				_prevTouch0Position = actionMap.FindAction("Touch0Position").ReadValue<Vector2>();
				_prevTouch1Position = actionMap.FindAction("Touch1Position").ReadValue<Vector2>();

				// Initialize Events
				actionMap.FindAction("PinchStart").performed += context => _handlePinchZoom = true;
				actionMap.FindAction("PinchStart").canceled += context => _handlePinchZoom = false;
			}
			else if (_universalInputManager.inputType == UniversalInputManager.InputType.MOUSE_ONLY
				|| _universalInputManager.inputType == UniversalInputManager.InputType.MOUSE_AND_KEYBOARD)
			{
				actionMap = orbitCameraInteraction.FindActionMap("Mouse");
				InputAction scrollZoom = actionMap.FindAction("ScrollZoom");
				scrollZoom.started += context => HandleCameraZoom(scrollZoom.ReadValue<Vector2>().y);
			}

			// Store actions
			startDragOrbit = actionMap.FindAction("StartDragOrbit");
			dragOrbitDelta = actionMap.FindAction("DragOrbitDelta");

			// Connect input events
			startDragOrbit.performed += context => HandleOrbitInput(dragOrbitDelta.ReadValue<Vector2>());
			startDragOrbit.canceled += context => DisableOrbitInput();

			// Initialize Values
			_targetOrbitRotation = transform.rotation.eulerAngles;
			_targetZoomPosition = connectedCamera.transform.localPosition;
		}

		private void FixedUpdate()
		{
			if (_handleDragOrbit)
			{
				HandleOrbitInput(dragOrbitDelta.ReadValue<Vector2>());
			}

			if (_universalInputManager.inputType == UniversalInputManager.InputType.TOUCH && _handlePinchZoom)
			{
				Vector2 touch0Position = actionMap.FindAction("Touch0Position").ReadValue<Vector2>();
				Vector2 touch1Position = actionMap.FindAction("Touch1Position").ReadValue<Vector2>();
				HandlePinch(touch0Position, touch1Position);
			}
			else if (_universalInputManager.inputType == UniversalInputManager.InputType.MOUSE_ONLY)
			{

			}

			// << UPDATE CAM POSITION >>
			transform.position = Vector3.Slerp(transform.position, positionTarget.position, followSpeed * Time.deltaTime);

			// << UPDATE CAM ROTATION TO FOCUSTARGET >>
			Quaternion targetQuaternion = Quaternion.Euler(_targetOrbitRotation);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, focusSpeed * Time.deltaTime);

			// Smoothly interpolate the connected camera to the target zoom position
			Vector3 camLocalPos = connectedCamera.transform.localPosition;
			connectedCamera.transform.localPosition = Vector3.Slerp(camLocalPos, _targetZoomPosition, zoomSpeed * Time.deltaTime);

			connectedCamera.transform.LookAt(focusTarget);

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
			_targetOrbitRotation = rotationTarget;
		}

		private float ClampAngle(float angle, float min, float max)
		{
			//angle = NormalizeAngle(angle);
			return Mathf.Clamp(angle, min, max);
		}


		#region == ORBIT INPUT =============================================================
		public void HandleOrbitInput(Vector2 delta)
		{
			_handleDragOrbit = true;

			// Translate swipe direction to rotation
			float horizontalRotation = delta.x * orbitSensitivity;
			float verticalRotation = -delta.y * orbitSensitivity;

			// Update target rotation
			_targetOrbitRotation += new Vector3(verticalRotation, horizontalRotation, 0);

			if (_targetOrbitRotation != transform.rotation.eulerAngles)
			{
				// Clamp Target Rotation
				_targetOrbitRotation = ClampRotation(_targetOrbitRotation, _xAxisClamp, _zAxisClamp);
			}
		}

		public void DisableOrbitInput()
		{
			_handleDragOrbit = false;
		}
		#endregion

		#region == ZOOM INPUT ==========================================================

		void HandlePinch(Vector2 touch0Pos, Vector2 touch1Pos)
		{
			// Calculate the previous and current distances between the touch points
			float prevDistance = (_prevTouch0Position - _prevTouch1Position).magnitude;
			float currentDistance = (touch0Pos - touch1Pos).magnitude;

			// Calculate the difference in distances between the current and previous frame
			float distanceDelta = currentDistance - prevDistance;

			// Pass the distanceDelta to the CameraManager's zoom handling method
			HandleCameraZoom(distanceDelta);

			// Update previous positions of the touches
			_prevTouch0Position = touch0Pos;
			_prevTouch1Position = touch1Pos;
		}

		public void HandleCameraZoom(float distanceDelta)
		{
			// Instead of using FOV, use local Z position
			Vector3 camLocalPos = connectedCamera.transform.localPosition;
			float destinationZ = Mathf.Abs(camLocalPos.z) + (distanceDelta * -zoomSensitivity);
			destinationZ = Mathf.Clamp(destinationZ, minZ_camZoom, maxZ_camZoom);

			// Set negative Z local position
			_targetZoomPosition = new Vector3(camLocalPos.x, camLocalPos.y, -destinationZ);
		}
		#endregion


	}

}