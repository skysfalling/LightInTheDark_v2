namespace Darklight.Game.CameraController
{
    using UnityEngine;

    public class ThirdPersonCamera : MonoBehaviour
    {
        #region [[ PRIVATE VARIABLES ]]
        private Vector3 _cameraPosOffset => new Vector3(_xPosOffset, _yPosOffset, _zPosOffset);
        private Vector3 _cameraRotOffset => new Vector3(_xRotOffset, _yRotOffset, _zRotOffset);
        private Transform _pivotHandle; // Set to self transform
        private Camera _camera;
        #endregion

        #region [[ PUBLIC ACCESSORS ]]
        public bool Initialized { get; private set; } = false;
        #endregion

        #region [[ PUBLIC INSPECTOR VARIABLES ]]
        [Header("References")]
        [SerializeField]
        private GameObject cameraPrefab;

        [SerializeField]
        private Transform playerTarget;

        [Header("CameraPositionOffset"), SerializeField, Range(-50, 50)]
        private int _xPosOffset = 0;

        [SerializeField, Range(0, 100)]
        private int _yPosOffset = 50;

        [SerializeField, Range(-100, 0)]
        private int _zPosOffset = -50;

        [Header("CameraRotationOffset"), SerializeField, Range(-180, 180)]
        private int _xRotOffset = -45;

        [SerializeField, Range(-90, 90)]
        private int _yRotOffset = 0;

        [SerializeField, Range(-45, 45)]
        private int _zRotOffset = -50;

        [Header("PivotHandleRotation"), SerializeField, Range(-180, 180)]
        public int pivotHandleRotation = 0;

        [Header("Speeds"), SerializeField, Range(0, 10)]
        private int followSpeed = 2;

        [SerializeField, Range(0, 10)]
        private int rotateSpeed = 2;

        #endregion


        // ======================================================================
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!_camera || !_pivotHandle)
            {
                ResetCamera();
                AssignCamera();
            }

            SetToEditorValues();
            Initialized = true;
        }

        public void ResetCamera()
        {
            if (_pivotHandle != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_pivotHandle.gameObject);
                }
                else
                {
                    DestroyImmediate(_pivotHandle.gameObject);
                }
            }
            _pivotHandle = null;
            _camera = null;

            Initialized = false;
        }

        void AssignCamera()
        {
            _camera = GetComponentInChildren<Camera>();
            if (_camera == null)
            {
                // Destroy existing pivot handle
                if (_pivotHandle != null)
                {
                    Destroy(_pivotHandle.gameObject);
                    _pivotHandle = null;
                }

                // Create pivot handle at the root of the [SelectionBase]
                _pivotHandle = new GameObject("PivotHandle").transform;
                _pivotHandle.transform.SetParent(transform);
                _pivotHandle.localPosition = Vector3.zero;

                // Prefab is set and no camera is found
                if (cameraPrefab != null)
                {
                    _camera = Instantiate(cameraPrefab, _pivotHandle).GetComponent<Camera>(); // Create camera that is child of pivot
                    _camera.transform.localPosition = Vector3.zero;
                    _camera.transform.localRotation = Quaternion.identity;
                }
                else if (cameraPrefab == null)
                {
                    Debug.Log("Camera Prefab is not Assigned");
                    _camera = null;
                }
            }
        }

        public void FixedUpdate()
        {
            if (!playerTarget || !_camera || !_pivotHandle)
                return;
            // Calculate and Lerp position
            Vector3 targetPosition = GetCameraFollowPosition(
                playerTarget.position,
                _cameraPosOffset
            );
            _camera.transform.localPosition = Vector3.Lerp(
                _camera.transform.localPosition,
                targetPosition,
                followSpeed * Time.deltaTime
            );

            // Calculate and Slerp rotation
            Quaternion targetRotation = GetCameraLookRotation(
                playerTarget.position,
                _cameraRotOffset
            );
            _camera.transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            // Update Pivot Rotation
            _pivotHandle.rotation = GetPivotRotation();
        }

        #region << GETTER FUNCTIONS >>

        Vector3 GetCameraFollowPosition(Vector3 followTargetPosition, Vector3 positionOffset)
        {
            return followTargetPosition + positionOffset;
        }

        Quaternion GetCameraLookRotation(Vector3 focusTargetPosition, Vector3 focusPositionOffset)
        {
            Vector3 offset = focusTargetPosition + focusPositionOffset;
            Vector3 direction = (offset - _camera.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            return lookRotation;
        }

        public Quaternion GetPivotRotation()
        {
            Vector3 yAxisRotation = Vector3.up * pivotHandleRotation;
            return Quaternion.Euler(yAxisRotation);
        }

        #endregion

        public void SetToEditorValues()
        {
            if (!playerTarget || !_camera || !_pivotHandle)
                return;

            // Calculate pivot rotation
            _pivotHandle.rotation = GetPivotRotation();

            // Calculate and override position
            _camera.transform.localPosition = GetCameraFollowPosition(
                playerTarget.position,
                _cameraPosOffset
            );

            // Calculate and override rotation
            _camera.transform.rotation = GetCameraLookRotation(
                playerTarget.position,
                _cameraRotOffset
            );
        }
    }
}