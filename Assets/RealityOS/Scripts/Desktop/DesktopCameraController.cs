using UnityEngine;
using RealityOS.Core;
using RealityOS.Workspace;

namespace RealityOS.Desktop
{
    public class DesktopCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _normalSpeed = 3.5f;
        [SerializeField] private float _sprintMultiplier = 2.0f;
        [SerializeField] private float _damping = 10.0f;

        [Header("Look Settings")]
        [SerializeField] private float _lookSensitivity = 2.5f;
        [SerializeField] private float _minPitch = -80f;
        [SerializeField] private float _maxPitch = 80f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSpeed = 4.0f;
        [SerializeField] private float _minDistance = 0.5f;
        [SerializeField] private float _maxDistance = 15.0f;

        [Header("Environment Boundaries")]
        [SerializeField] private Vector3 _boundsMin = new Vector3(-8f, 0.2f, -8f);
        [SerializeField] private Vector3 _boundsMax = new Vector3(8f, 6.0f, 8f);

        private Vector3 _targetPosition;
        private float _yaw = 0f;
        private float _pitch = 0f;
        private bool _isLooking = false;

        private void Start()
        {
            _targetPosition = transform.position;
            Vector3 euler = transform.rotation.eulerAngles;
            _pitch = euler.x;
            _yaw = euler.y;

            if (SettingsManager.Instance != null)
            {
                _normalSpeed = SettingsManager.Instance.Current.cameraMoveSpeed;
                _lookSensitivity = SettingsManager.Instance.Current.cameraLookSensitivity;
            }
        }

        private void Update()
        {
            HandleKeyboardShortcuts();
            HandleLookInput();
            HandleMovementInput();
            HandleZoomInput();

            // Smooth position update
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _damping);
        }

        private void HandleMovementInput()
        {
            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S
            float up = 0f;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) up += 1f;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.C)) up -= 1f;

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f || Mathf.Abs(up) > 0.01f)
            {
                float speed = _normalSpeed * (Input.GetKey(KeyCode.LeftShift) ? _sprintMultiplier : 1.0f);
                Vector3 moveDir = transform.forward * v + transform.right * h + Vector3.up * up;
                _targetPosition += moveDir.normalized * (speed * Time.deltaTime);

                // Clamp to boundary
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, _boundsMin.x, _boundsMax.x);
                _targetPosition.y = Mathf.Clamp(_targetPosition.y, _boundsMin.y, _boundsMax.y);
                _targetPosition.z = Mathf.Clamp(_targetPosition.z, _boundsMin.z, _boundsMax.z);
            }
        }

        private void HandleLookInput()
        {
            // Right mouse button to look around
            if (Input.GetMouseButtonDown(1))
            {
                _isLooking = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _isLooking = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_isLooking)
            {
                float mouseX = Input.GetAxis("Mouse X") * _lookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * _lookSensitivity;

                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
        }

        private void HandleZoomInput()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetPosition += transform.forward * (scroll * _zoomSpeed);
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, _boundsMin.x, _boundsMax.x);
                _targetPosition.y = Mathf.Clamp(_targetPosition.y, _boundsMin.y, _boundsMax.y);
                _targetPosition.z = Mathf.Clamp(_targetPosition.z, _boundsMin.z, _boundsMax.z);
            }
        }

        private void HandleKeyboardShortcuts()
        {
            // Focus on selected panel
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusOnSelectedPanel();
            }

            // Reset selected object's rotation
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
                {
                    WorkspaceManager.Instance.SelectedPanel.ResetOrientation();
                }
            }

            // Duplicate selected panel
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            {
                if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
                {
                    WorkspaceManager.Instance.DuplicatePanel(WorkspaceManager.Instance.SelectedPanel);
                }
            }

            // Save Workspace (Ctrl + S)
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                if (WorkspaceManager.Instance != null)
                {
                    WorkspaceManager.Instance.SaveActiveWorkspace();
                }
            }

            // Delete selected panel
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
                {
                    WorkspaceManager.Instance.DeletePanel(WorkspaceManager.Instance.SelectedPanel);
                }
            }
        }

        public void FocusOnSelectedPanel()
        {
            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
            {
                Transform target = WorkspaceManager.Instance.SelectedPanel.TransformTarget;
                _targetPosition = target.position - target.forward * 1.5f;
                transform.LookAt(target.position);
                Vector3 euler = transform.rotation.eulerAngles;
                _pitch = euler.x;
                _yaw = euler.y;
            }
        }

        public void ResetCamera()
        {
            _targetPosition = new Vector3(0f, 1.4f, 0f);
            _pitch = 0f;
            _yaw = 0f;
            transform.rotation = Quaternion.identity;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _isLooking = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

