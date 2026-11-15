using UnityEngine;
using RealityOS.Core;

namespace RealityOS.XR
{
    public class XRLocomotionProvider : MonoBehaviour
    {
        [Header("XR Rig Reference")]
        [SerializeField] private Transform _xrOrigin;

        [Header("Snap Turn Settings")]
        [SerializeField] private bool _enableSnapTurn = true;
        [SerializeField] private float _snapTurnAngle = 45f;
        [SerializeField] private float _turnCooldown = 0.25f;

        [Header("Teleportation Settings")]
        [SerializeField] private bool _enableTeleport = true;
        [SerializeField] private LayerMask _teleportLayer = ~0;
        [SerializeField] private float _maxTeleportDistance = 15f;
        [SerializeField] private LineRenderer _teleportLineRenderer;
        [SerializeField] private GameObject _teleportMarker;

        private float _lastTurnTime = 0f;
        private bool _isTeleportAiming = false;
        private Vector3 _teleportTargetPosition;

        private void Start()
        {
            if (_xrOrigin == null)
            {
                _xrOrigin = transform;
            }

            if (SettingsManager.Instance != null)
            {
                _enableSnapTurn = SettingsManager.Instance.Current.comfortSnapTurn;
                _snapTurnAngle = SettingsManager.Instance.Current.snapTurnAngle;
            }

            if (_teleportMarker != null) _teleportMarker.SetActive(false);
            if (_teleportLineRenderer != null) _teleportLineRenderer.enabled = false;
        }

        private void Update()
        {
            HandleSnapTurn();
            HandleTeleport();
        }

        private void HandleSnapTurn()
        {
            if (!_enableSnapTurn) return;

            if (Time.time - _lastTurnTime < _turnCooldown) return;

            // Snap turn with Q (turn left) or E (turn right)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _xrOrigin.Rotate(Vector3.up, -_snapTurnAngle, Space.World);
                _lastTurnTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _xrOrigin.Rotate(Vector3.up, _snapTurnAngle, Space.World);
                _lastTurnTime = Time.time;
            }
        }

        private void HandleTeleport()
        {
            if (!_enableTeleport) return;

            // Hold 'T' to aim teleport
            if (Input.GetKey(KeyCode.T))
            {
                _isTeleportAiming = true;
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                    bool hitValid = false;

                    // 1. Raycast against physical colliders and floor meshes
                    if (Physics.Raycast(ray, out RaycastHit hit, _maxTeleportDistance, _teleportLayer))
                    {
                        if (Vector3.Dot(hit.normal, Vector3.up) > 0.6f) // Horizontal floor surface
                        {
                            _teleportTargetPosition = hit.point;
                            hitValid = true;
                        }
                    }

                    // 2. Fallback to reference floor plane at y=0
                    if (!hitValid)
                    {
                        Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
                        if (floorPlane.Raycast(ray, out float enter) && enter <= _maxTeleportDistance)
                        {
                            _teleportTargetPosition = ray.GetPoint(enter);
                            hitValid = true;
                        }
                    }

                    if (hitValid)
                    {
                        if (_teleportMarker != null)
                        {
                            _teleportMarker.SetActive(true);
                            _teleportMarker.transform.position = _teleportTargetPosition;
                        }

                        if (_teleportLineRenderer != null)
                        {
                            _teleportLineRenderer.enabled = true;
                            _teleportLineRenderer.SetPosition(0, cam.transform.position + Vector3.down * 0.2f);
                            _teleportLineRenderer.SetPosition(1, _teleportTargetPosition);
                        }
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.T) && _isTeleportAiming)
            {
                // Execute teleport
                _xrOrigin.position = new Vector3(_teleportTargetPosition.x, _xrOrigin.position.y, _teleportTargetPosition.z);

                _isTeleportAiming = false;
                if (_teleportMarker != null) _teleportMarker.SetActive(false);
                if (_teleportLineRenderer != null) _teleportLineRenderer.enabled = false;
            }
        }
    }
}

