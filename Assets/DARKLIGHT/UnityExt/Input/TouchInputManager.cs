namespace Darklight.UnityExt.Input
{
	using UnityEngine;
	using UnityEngine.Events;
	using Darklight.Camera;

	public class TouchInputManager : MonoBehaviour
	{
		// Define the data structure to hold tap details
		public class TapEventData
		{
			public Vector2 ScreenPosition;
			public Vector3 WorldPosition;
			public bool IsSingleTap;

			public TapEventData(Vector2 screenPosition, Vector3 worldPosition, bool isSingleTap)
			{
				ScreenPosition = screenPosition;
				WorldPosition = worldPosition;
				IsSingleTap = isSingleTap;
			}
		}

		// Custom UnityEvent that uses TapEventData
		[System.Serializable]
		public class TapEvent : UnityEvent<TapEventData> { }

		[Header("Camera")]
		public OrbitCamera orbitCamera;

		[Header("Tap Touch Input")]
		private Vector2 touchZeroStart;
		private Vector2 touchZeroEnd;
		private int tapCount; // Count the number of taps
		private float tapTimer; // Timer to measure time between taps
		public float doubleTapMaxDelay = 0.2f; // Time interval to consider for double tap
		public float minimumSwipeDistance = 10f; // Minimum distance for a swipe to be registered

		// [Header("Double Touch Input")]
		private float _lastPinchDistance;
		private bool _wasPinching;

		[Header("Input Events")]
		public TapEvent onSingleTapInput;
		public TapEvent onDoubleTapInput;

		private void Start()
		{
			orbitCamera = FindAnyObjectByType<OrbitCamera>();
		}

		void Update()
		{
			if (Input.touchCount == 1)
			{
				HandleSingleTouch(Input.GetTouch(0));
			}
			else if (Input.touchCount == 2)
			{
				HandleDoubleTouch(Input.GetTouch(0), Input.GetTouch(1));
			}
			else
			{
				orbitCamera.DisableOrbitInput();

				// << DOUBLE TAP DELAY >>
				if (tapCount == 1)
				{
					// [[ WAIT FOR SECOND TAP ]]
					tapTimer += Time.deltaTime;
					if (tapTimer > doubleTapMaxDelay)
					{
						// reset if too much time has passed
						tapTimer = 0;
						tapCount = 0;
					}
				}

				// << RESET PINCH >>
				if (_wasPinching)
				{
					// Reset the last pinch distance when the touch ends
					_lastPinchDistance = 0;
					_wasPinching = false;
				}
			}
		}

		#region =========================== [[ UPDATE ==> SINGLE TOUCH TRACKING ]] ===========================================================
		void HandleSingleTouch(Touch touch)
		{
			switch (touch.phase)
			{
				case TouchPhase.Began:
					touchZeroStart = touch.position;
					tapCount++;
					break;

				case TouchPhase.Moved:
					Vector2 touchMovement = touch.deltaPosition;

					// Handle touch movement action
					if (orbitCamera != null)
					{
						orbitCamera.HandleOrbitInput(touchMovement);
					}
					break;

				case TouchPhase.Ended:
					touchZeroEnd = touch.position;

					// [[ DETECT TAP ]]
					if (Vector2.Distance(touchZeroStart, touchZeroEnd) < minimumSwipeDistance)
					{
						DetectTap();
					}
					break;
			}
		}

		void DetectTap()
		{
			Debug.Log($"Detect Tap {tapCount}");

			// [[ REGISTER SINGLE TAP ]]
			if (tapCount == 1)
			{
				tapTimer = 0;
				Invoke(nameof(SingleTap), doubleTapMaxDelay);
			}

			// [[ REGISTER DOUBLE TAP ]]
			else if (tapCount == 2)
			{
				CancelInvoke(nameof(SingleTap));
				DoubleTap();
				tapCount = 0; // Reset tap count
			}
		}

		void SingleTap()
		{
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchZeroStart.x, touchZeroStart.y, Camera.main.nearClipPlane));
			onSingleTapInput.Invoke(new TapEventData(touchZeroStart, worldPosition, true));
			tapCount = 0;
		}

		void DoubleTap()
		{
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchZeroStart.x, touchZeroStart.y, Camera.main.nearClipPlane));
			onDoubleTapInput.Invoke(new TapEventData(touchZeroStart, worldPosition, false));
			tapCount = 0;
		}
		#endregion

		#region =========================== [[ UPDATE ==> DOUBLE TOUCH TRACKING ]] ===========================================================

		void HandleDoubleTouch(Touch touchZero, Touch touchOne)
		{
			// Get the previous positions of the touches
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Calculate the previous and current distances between the touch points
			float prevDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float currentDistance = (touchZero.position - touchOne.position).magnitude;

			// Calculate the difference in distances between the current and previous frame
			float distanceDelta = currentDistance - prevDistance;

			// Pass the distanceDelta to the CameraManager's zoom handling method
			orbitCamera.HandleCameraZoom(distanceDelta);

			_wasPinching = true;
			_lastPinchDistance = currentDistance;
		}

		#endregion
	}

}