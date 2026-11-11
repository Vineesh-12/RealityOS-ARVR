using System;
using System.Collections.Generic;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Workspace;

#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace RealityOS.AR
{
    public class ARWorkspaceController : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private ARPlacementReticle _placementReticle;
        [SerializeField] private ARTouchGestureHandler _gestureHandler;

#if UNITY_XR_ARFOUNDATION
        [SerializeField] private ARSession _arSession;
        [SerializeField] private ARPlaneManager _planeManager;
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private ARAnchorManager _anchorManager;
#endif

        [Header("UI Feedback")]
        [SerializeField] private TMPro.TextMeshProUGUI _guidanceText;
        [SerializeField] private GameObject _trackingWarningPanel;

        public event Action<TrackingState> OnTrackingStateChanged;
        public TrackingState CurrentTrackingState { get; private set; } = TrackingState.Unavailable;

        private PanelType _selectedPanelTypeToPlace = PanelType.Notes;

        private void Awake()
        {
#if UNITY_XR_ARFOUNDATION
            if (_arSession == null) _arSession = FindAnyObjectByType<ARSession>();
            if (_planeManager == null) _planeManager = FindAnyObjectByType<ARPlaneManager>();
            if (_raycastManager == null) _raycastManager = FindAnyObjectByType<ARRaycastManager>();
            if (_anchorManager == null) _anchorManager = FindAnyObjectByType<ARAnchorManager>();
#endif
            if (_placementReticle == null) _placementReticle = FindAnyObjectByType<ARPlacementReticle>();
            if (_gestureHandler == null) _gestureHandler = FindAnyObjectByType<ARTouchGestureHandler>();
        }

        private void Start()
        {
            UpdateGuidanceMessage("Move your device slowly to scan surfaces.");
            CheckDeviceCompatibility();
        }

        private void Update()
        {
            UpdateTrackingStatus();
        }

        private void CheckDeviceCompatibility()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_XR_ARFOUNDATION
                if (ARSession.state == ARSessionState.Unsupported)
                {
                    UpdateGuidanceMessage("ARCore is not supported on this device.");
                    ShowTrackingWarning(true);
                    return;
                }
#endif
            }
            else
            {
                UpdateGuidanceMessage("Running on Desktop. AR is simulated using virtual ground plane.");
            }
        }

        private void UpdateTrackingStatus()
        {
#if UNITY_XR_ARFOUNDATION
            if (_arSession != null)
            {
                ARSessionState state = ARSession.state;
                if (state == ARSessionState.SessionTracking)
                {
                    SetTrackingState(TrackingState.Tracking);
                    ShowTrackingWarning(false);
                    if (_placementReticle != null && _placementReticle.IsValidPlacement)
                    {
                        UpdateGuidanceMessage("Surface detected! Tap 'Place Panel' to anchor.");
                    }
                    else
                    {
                        UpdateGuidanceMessage("Scan environment to find horizontal or vertical surfaces.");
                    }
                }
                else if (state == ARSessionState.SessionInitializing)
                {
                    SetTrackingState(TrackingState.Limited);
                    UpdateGuidanceMessage("Initializing AR tracking...");
                    ShowTrackingWarning(false);
                }
                else
                {
                    SetTrackingState(TrackingState.Unavailable);
                    UpdateGuidanceMessage("Tracking temporarily unavailable. Keep device steady.");
                    ShowTrackingWarning(true);
                }
                return;
            }
#endif
            // Desktop fallback simulation
            SetTrackingState(TrackingState.Tracking);
            ShowTrackingWarning(false);
        }

        private void SetTrackingState(TrackingState state)
        {
            if (CurrentTrackingState != state)
            {
                CurrentTrackingState = state;
                OnTrackingStateChanged?.Invoke(state);
            }
        }

        public void SetSelectedPanelType(PanelType type)
        {
            _selectedPanelTypeToPlace = type;
        }

        public void PlaceSelectedPanelAtReticle()
        {
            if (_placementReticle == null || !_placementReticle.IsValidPlacement)
            {
                UpdateGuidanceMessage("Cannot place panel: point at a detected physical surface.");
                return;
            }

            Pose pose = _placementReticle.CurrentPose;
            Vector3 spawnPos = pose.position + Vector3.up * 0.15f; // Slight elevation above plane

            // Orient panel facing camera
            Camera cam = Camera.main;
            Quaternion rot = cam != null
                ? Quaternion.LookRotation(cam.transform.forward, Vector3.up)
                : pose.rotation;

            if (WorkspaceManager.Instance != null)
            {
                GameObject panelObj = WorkspaceManager.Instance.SpawnPanel(_selectedPanelTypeToPlace, spawnPos);
                if (panelObj != null)
                {
                    panelObj.transform.rotation = rot;

#if UNITY_XR_ARFOUNDATION
                    // Try to attach AR anchor to keep panel locked to real world coordinates
                    if (_anchorManager != null)
                    {
                        ARAnchor anchor = panelObj.AddComponent<ARAnchor>();
                        Debug.Log($"[RealityOS.AR] Attached ARAnchor to panel {panelObj.name}");
                    }
#endif
                }
            }
            UpdateGuidanceMessage($"Placed {_selectedPanelTypeToPlace} panel.");
        }

        public void TogglePlaneVisualization(bool show)
        {
#if UNITY_XR_ARFOUNDATION
            if (_planeManager != null)
            {
                foreach (var plane in _planeManager.trackables)
                {
                    plane.gameObject.SetActive(show);
                }
                _planeManager.enabled = show;
            }
#endif
        }

        public void ClearAllARPanels()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.ClearPanelsInScene();
                UpdateGuidanceMessage("Cleared all placed panels.");
            }
        }

        private void UpdateGuidanceMessage(string msg)
        {
            if (_guidanceText != null)
            {
                _guidanceText.text = msg;
            }
        }

        private void ShowTrackingWarning(bool show)
        {
            if (_trackingWarningPanel != null)
            {
                _trackingWarningPanel.SetActive(show);
            }
        }
    }
}
