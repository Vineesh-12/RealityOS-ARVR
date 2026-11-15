using System;
using UnityEngine;
using UnityEngine.EventSystems;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Workspace;

namespace RealityOS.AR
{
    public class ARTouchGestureHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _dragSensitivity = 1.0f;
        [SerializeField] private float _pinchSensitivity = 0.005f;
        [SerializeField] private float _rotateSensitivity = 0.5f;
        [SerializeField] private float _longPressDuration = 0.6f;
        [SerializeField] private float _doubleTapMaxInterval = 0.3f;

        [Header("Dependencies")]
        [SerializeField] private Camera _arCamera;
        [SerializeField] private ARPlacementReticle _reticle;

        public event Action<WorkspacePanelBase> OnLongPress;
        public event Action<WorkspacePanelBase> OnDoubleTap;

        private WorkspacePanelBase _targetPanel;
        private float _touchStartTime = 0f;
        private float _lastTapTime = 0f;
        private bool _isLongPressTriggered = false;
        private Vector2 _touchStartPos;
        private Plane _touchDragPlane;
        private Vector3 _dragOffset;

        private void Start()
        {
            if (_arCamera == null) _arCamera = Camera.main;
            if (_reticle == null) _reticle = FindAnyObjectByType<ARPlacementReticle>();
        }

        private void Update()
        {
            if (Input.touchCount == 0)
            {
                // Also support mouse simulation in Editor
                HandleMouseFallback();
                return;
            }

            // Check if touch is over screen UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                return;
            }

            if (Input.touchCount == 1)
            {
                HandleSingleTouch(Input.GetTouch(0));
            }
            else if (Input.touchCount == 2)
            {
                HandleTwoFingers(Input.GetTouch(0), Input.GetTouch(1));
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStartTime = Time.time;
                    _touchStartPos = touch.position;
                    _isLongPressTriggered = false;

                    Ray ray = _arCamera.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                    {
                        WorkspacePanelBase panel = hit.collider.GetComponentInParent<WorkspacePanelBase>();
                        if (panel != null)
                        {
                            _targetPanel = panel;
                            _targetPanel.BeginManipulation(ManipulationMode.Moving);
                            if (WorkspaceManager.Instance != null)
                            {
                                WorkspaceManager.Instance.SelectPanel(panel);
                            }

                            _touchDragPlane = new Plane(-_arCamera.transform.forward, panel.transform.position);
                            if (_touchDragPlane.Raycast(ray, out float enter))
                            {
                                _dragOffset = panel.transform.position - ray.GetPoint(enter);
                            }
                        }
                    }
                    else
                    {
                        _targetPanel = null;
                    }
                    break;

                case TouchPhase.Moved:
                    if (_targetPanel != null)
                    {
                        Ray dragRay = _arCamera.ScreenPointToRay(touch.position);
                        if (_touchDragPlane.Raycast(dragRay, out float enter))
                        {
                            Vector3 hitPoint = dragRay.GetPoint(enter);
                            _targetPanel.MoveTo(hitPoint + (_dragOffset * _dragSensitivity));
                        }
                    }
                    break;

                case TouchPhase.Stationary:
                    if (_targetPanel != null && !_isLongPressTriggered)
                    {
                        if (Time.time - _touchStartTime > _longPressDuration && Vector2.Distance(touch.position, _touchStartPos) < 15f)
                        {
                            _isLongPressTriggered = true;
                            OnLongPress?.Invoke(_targetPanel);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    if (_targetPanel != null)
                    {
                        _targetPanel.EndManipulation();

                        // Check double tap
                        if (!_isLongPressTriggered && Vector2.Distance(touch.position, _touchStartPos) < 20f)
                        {
                            float interval = Time.time - _lastTapTime;
                            if (interval < _doubleTapMaxInterval)
                            {
                                OnDoubleTap?.Invoke(_targetPanel);
                                _targetPanel.FocusPanel();
                            }
                            _lastTapTime = Time.time;
                        }
                        _targetPanel = null;
                    }
                    else
                    {
                        // Tap on empty space with valid reticle: can trigger placement
                        if (Vector2.Distance(touch.position, _touchStartPos) < 15f && _reticle != null && _reticle.IsValidPlacement)
                        {
                            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel == null)
                            {
                                // WorkspaceManager can spawn panel here
                            }
                        }
                    }
                    break;
            }
        }

        private void HandleTwoFingers(Touch touchZero, Touch touchOne)
        {
            if (_targetPanel == null)
            {
                if (WorkspaceManager.Instance != null)
                {
                    _targetPanel = WorkspaceManager.Instance.SelectedPanel as WorkspacePanelBase;
                }
                if (_targetPanel == null) return;
            }

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // 1. Pinch for uniform scale
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            float scaleFactor = 1.0f + (deltaMagnitudeDiff * _pinchSensitivity);
            _targetPanel.ScaleBy(scaleFactor);

            // 2. Twist for rotation
            Vector2 prevDir = touchOnePrevPos - touchZeroPrevPos;
            Vector2 currentDir = touchOne.position - touchZero.position;
            float angle = Vector2.SignedAngle(prevDir, currentDir);

            _targetPanel.RotateBy(new Vector3(0f, -angle * _rotateSensitivity, 0f));
        }

        private void HandleMouseFallback()
        {
            if (Application.isMobilePlatform) return;

            // Mouse wheel for scaling selected panel in Editor
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f && WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
            {
                WorkspaceManager.Instance.SelectedPanel.ScaleBy(1f + scroll * 1.5f);
            }
        }
    }
}

