using UnityEngine;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Workspace;

namespace RealityOS.XR
{
    public class XRSimulatedController : MonoBehaviour
    {
        public enum HandType { Left, Right }

        [Header("Hand Setup")]
        [SerializeField] private HandType _hand = HandType.Right;
        [SerializeField] private LineRenderer _rayRenderer;
        [SerializeField] private Transform _reticleVisual;
        [SerializeField] private float _maxRayDistance = 12f;
        [SerializeField] private LayerMask _raycastLayers = ~0;

        public bool IsGrabbing { get; private set; }
        public WorkspacePanelBase GrabbedPanel { get; private set; }

        private float _grabDistance = 2.0f;
        private WorkspacePanelBase _hoveredPanel;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            EnsureLineRenderer();
            EnsureReticle();
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            AlignWithCameraAndMouse();
            UpdateRaycast();
            HandleInputs();
        }

        private void AlignWithCameraAndMouse()
        {
            if (_mainCamera == null) return;

            // Anchor hand relative to camera
            float lateralOffset = (_hand == HandType.Right) ? 0.24f : -0.24f;
            Vector3 handPos = _mainCamera.transform.position
                + _mainCamera.transform.right * lateralOffset
                + _mainCamera.transform.up * -0.18f
                + _mainCamera.transform.forward * 0.38f;

            transform.position = handPos;

            // Aim controller along mouse screen ray
            Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 aimTarget = mouseRay.origin + mouseRay.direction * _maxRayDistance;
            transform.LookAt(aimTarget);
        }

        private void UpdateRaycast()
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            Vector3 endPoint = origin + direction * _maxRayDistance;

            if (!IsGrabbing)
            {
                if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxRayDistance, _raycastLayers))
                {
                    endPoint = hit.point;

                    WorkspacePanelBase panel = hit.collider.GetComponentInParent<WorkspacePanelBase>();
                    if (panel != null)
                    {
                        if (_hoveredPanel != panel)
                        {
                            ClearHover();
                            _hoveredPanel = panel;
                            _hoveredPanel.OnHoverEnter();
                        }
                    }
                    else
                    {
                        ClearHover();
                    }

                    if (_reticleVisual != null)
                    {
                        _reticleVisual.gameObject.SetActive(true);
                        _reticleVisual.position = hit.point;
                        _reticleVisual.rotation = Quaternion.LookRotation(hit.normal);
                    }
                }
                else
                {
                    ClearHover();
                    if (_reticleVisual != null)
                    {
                        _reticleVisual.gameObject.SetActive(false);
                    }
                }
            }
            else if (GrabbedPanel != null)
            {
                // While grabbing, keep panel at target distance in front of controller
                endPoint = origin + direction * _grabDistance;
                GrabbedPanel.MoveTo(endPoint);

                if (_reticleVisual != null)
                {
                    _reticleVisual.gameObject.SetActive(true);
                    _reticleVisual.position = endPoint;
                }
            }

            if (_rayRenderer != null)
            {
                _rayRenderer.SetPosition(0, origin);
                _rayRenderer.SetPosition(1, endPoint);
            }
        }

        private void ClearHover()
        {
            if (_hoveredPanel != null)
            {
                _hoveredPanel.OnHoverExit();
                _hoveredPanel = null;
            }
        }

        private void HandleInputs()
        {
            bool triggerDown = false;
            bool gripDown = false;
            bool gripUp = false;

            if (_hand == HandType.Right)
            {
                triggerDown = Input.GetMouseButtonDown(0);
                gripDown = Input.GetKeyDown(KeyCode.G);
                gripUp = Input.GetKeyUp(KeyCode.G);
            }
            else
            {
                triggerDown = Input.GetKeyDown(KeyCode.T);
                gripDown = Input.GetKeyDown(KeyCode.H);
                gripUp = Input.GetKeyUp(KeyCode.H);
            }

            // Select panel
            if (triggerDown && _hoveredPanel != null && !IsGrabbing)
            {
                if (WorkspaceManager.Instance != null)
                {
                    WorkspaceManager.Instance.SelectPanel(_hoveredPanel);
                }
            }

            // Grab panel
            if (gripDown && _hoveredPanel != null && !IsGrabbing)
            {
                IsGrabbing = true;
                GrabbedPanel = _hoveredPanel;
                GrabbedPanel.BeginManipulation(ManipulationMode.Moving);
                _grabDistance = Vector3.Distance(transform.position, GrabbedPanel.transform.position);
                _grabDistance = Mathf.Clamp(_grabDistance, 0.8f, 6.0f);
            }

            // Release grab
            if (gripUp && IsGrabbing)
            {
                if (GrabbedPanel != null)
                {
                    GrabbedPanel.EndManipulation();
                    GrabbedPanel = null;
                }
                IsGrabbing = false;
            }
        }

        private void EnsureLineRenderer()
        {
            if (_rayRenderer == null)
            {
                _rayRenderer = GetComponent<LineRenderer>();
                if (_rayRenderer == null)
                {
                    _rayRenderer = gameObject.AddComponent<LineRenderer>();
                    _rayRenderer.startWidth = 0.008f;
                    _rayRenderer.endWidth = 0.002f;
                    _rayRenderer.positionCount = 2;

                    Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                    Material rayMat = new Material(s);
                    rayMat.color = new Color(0.0f, 0.95f, 0.9f, 0.8f);
                    _rayRenderer.material = rayMat;
                }
            }
        }

        private void EnsureReticle()
        {
            if (_reticleVisual == null)
            {
                GameObject ret = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ret.name = "RayReticle";
                ret.transform.localScale = Vector3.one * 0.035f;
                Collider col = ret.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var r = ret.GetComponent<Renderer>();
                if (r != null)
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                    Material m = new Material(s);
                    m.color = Color.cyan;
                    r.material = m;
                }
                ret.SetActive(false);
                _reticleVisual = ret.transform;
            }
        }
    }
}
