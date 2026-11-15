using UnityEngine;
using UnityEngine.EventSystems;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Workspace;

namespace RealityOS.Desktop
{
    public class DesktopInteractionRaycaster : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask _interactableLayer = ~0;
        [SerializeField] private float _maxRayDistance = 30f;
        [SerializeField] private float _doubleClickThreshold = 0.3f;

        private Camera _camera;
        private WorkspacePanelBase _hoveredPanel;
        private WorkspacePanelBase _draggingPanel;
        private Plane _dragPlane;
        private Vector3 _dragOffset;
        private float _lastClickTime = 0f;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>() ?? Camera.main;
                if (_camera == null) return;
            }

            // Do not raycast if pointer is interacting with screen-space UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearHover();
                return;
            }

            HandleHover();
            HandleSelectionAndDrag();

            // Forward mouse wheel scrolling to hovered 3D browser viewport
            if (_hoveredPanel != null)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    var bridge = _hoveredPanel.GetComponentInChildren<RealityOS.LaptopIntegration.BrowserInputBridge>();
                    if (bridge != null)
                    {
                        bridge.RaycastScroll(scroll);
                    }
                }
            }
        }

        private void HandleHover()
        {
            if (_draggingPanel != null) return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _interactableLayer))
            {
                WorkspacePanelBase panel = hit.collider.GetComponentInParent<WorkspacePanelBase>();
                if (panel != null)
                {
                    if (_hoveredPanel != panel)
                    {
                        ClearHover();
                        _hoveredPanel = panel;
                        _hoveredPanel.OnHoverEnter();
                    }
                    return;
                }
            }
            ClearHover();
        }

        private void ClearHover()
        {
            if (_hoveredPanel != null)
            {
                _hoveredPanel.OnHoverExit();
                _hoveredPanel = null;
            }
        }

        private void HandleSelectionAndDrag()
        {
            // Left click down
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _interactableLayer))
                {
                    WorkspacePanelBase panel = hit.collider.GetComponentInParent<WorkspacePanelBase>();
                    if (panel != null)
                    {
                        // Check double click
                        float timeSinceLast = Time.time - _lastClickTime;
                        _lastClickTime = Time.time;

                        if (timeSinceLast < _doubleClickThreshold)
                        {
                            panel.FocusPanel();
                        }
                        else
                        {
                            if (WorkspaceManager.Instance != null)
                            {
                                WorkspaceManager.Instance.SelectPanel(panel);
                            }

                            var bridge = hit.collider.GetComponentInParent<RealityOS.LaptopIntegration.BrowserInputBridge>();
                            if (bridge != null)
                            {
                                bridge.RaycastClick(hit.point);
                            }
                        }

                        // Begin drag on plane perpendicular to camera
                        _draggingPanel = panel;
                        _draggingPanel.BeginManipulation(ManipulationMode.Moving);
                        _dragPlane = new Plane(-_camera.transform.forward, panel.transform.position);

                        if (_dragPlane.Raycast(ray, out float enter))
                        {
                            _dragOffset = panel.transform.position - ray.GetPoint(enter);
                        }
                        return;
                    }
                }
                else
                {
                    // Clicked empty space: deselect
                    if (WorkspaceManager.Instance != null)
                    {
                        WorkspaceManager.Instance.SelectPanel(null);
                    }
                }
            }

            // Dragging
            if (Input.GetMouseButton(0) && _draggingPanel != null)
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (_dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 point = ray.GetPoint(enter);
                    _draggingPanel.MoveTo(point + _dragOffset);
                }
            }

            // Release drag
            if (Input.GetMouseButtonUp(0) && _draggingPanel != null)
            {
                _draggingPanel.EndManipulation();
                _draggingPanel = null;
            }
        }
    }
}

