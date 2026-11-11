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
        [SerializeField] private float _maxRayDistance = 10f;
        [SerializeField] private LayerMask _raycastLayers = ~0;

        [Header("Controller Visual")]
        [SerializeField] private GameObject _controllerModel;

        public bool IsGrabbing { get; private set; }
        public WorkspacePanelBase GrabbedPanel { get; private set; }

        private Vector3 _initialGrabLocalPos;
        private Quaternion _initialGrabLocalRot;
        private WorkspacePanelBase _hoveredPanel;

        private void Awake()
        {
            EnsureLineRenderer();
            EnsureReticle();
        }

        private void Update()
        {
            UpdateRaycast();
            HandleInputs();
        }

        private void UpdateRaycast()
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            Vector3 endPoint = origin + direction * _maxRayDistance;

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

            if (_rayRenderer != null)
            {
                _rayRenderer.SetPosition(0, origin);
                _rayRenderer.SetPosition(1, endPoint);
            }

            // If currently grabbing a panel, keep it attached to controller transform
            if (IsGrabbing && GrabbedPanel != null)
            {
                Vector3 targetWorldPos = transform.TransformPoint(_initialGrabLocalPos);
                GrabbedPanel.MoveTo(targetWorldPos);
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
            // Key triggers for simulation:
            // Right Hand: Left Mouse Click (Trigger/Select), 'G' (Grip/Grab)
            // Left Hand: 'T' (Teleport Ray / Grip)
            bool triggerDown = false;
            bool triggerUp = false;
            bool gripDown = false;
            bool gripUp = false;

            if (_hand == HandType.Right)
            {
                triggerDown = Input.GetMouseButtonDown(0);
                triggerUp = Input.GetMouseButtonUp(0);
                gripDown = Input.GetKeyDown(KeyCode.G);
                gripUp = Input.GetKeyUp(KeyCode.G);
            }
            else
            {
                triggerDown = Input.GetKeyDown(KeyCode.T);
                triggerUp = Input.GetKeyUp(KeyCode.T);
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
                _initialGrabLocalPos = transform.InverseTransformPoint(GrabbedPanel.transform.position);
                _initialGrabLocalRot = Quaternion.Inverse(transform.rotation) * GrabbedPanel.transform.rotation;
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

                    Material rayMat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Unlit"));
                    rayMat.color = new Color(0.0f, 0.82f, 0.77f, 0.7f);
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
                    Material m = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Unlit"));
                    m.color = Color.cyan;
                    r.material = m;
                }
                ret.SetActive(false);
                _reticleVisual = ret.transform;
            }
        }
    }
}

