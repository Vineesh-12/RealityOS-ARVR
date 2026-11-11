using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RealityOS.Core;

namespace RealityOS.Panels
{
    public class PanelControlBar : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Target Panel")]
        [SerializeField] private WorkspacePanelBase _targetPanel;

        [Header("Buttons")]
        [SerializeField] private Button _duplicateButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _focusButton;

        [Header("Handles")]
        [SerializeField] private GameObject _moveHandle;
        [SerializeField] private GameObject _rotateHandle;
        [SerializeField] private GameObject _resizeHandle;

        private Camera _mainCamera;
        private Vector3 _dragPlaneOffset;
        private Plane _dragPlane;
        private ManipulationMode _activeHandleMode = ManipulationMode.None;

        private void Awake()
        {
            if (_targetPanel == null)
            {
                _targetPanel = GetComponentInParent<WorkspacePanelBase>();
            }

            _mainCamera = Camera.main;

            if (_duplicateButton != null) _duplicateButton.onClick.AddListener(OnDuplicateClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
            if (_resetButton != null) _resetButton.onClick.AddListener(OnResetClicked);
            if (_focusButton != null) _focusButton.onClick.AddListener(OnFocusClicked);
        }

        private void OnDestroy()
        {
            if (_duplicateButton != null) _duplicateButton.onClick.RemoveListener(OnDuplicateClicked);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
            if (_resetButton != null) _resetButton.onClick.RemoveListener(OnResetClicked);
            if (_focusButton != null) _focusButton.onClick.RemoveListener(OnFocusClicked);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_targetPanel == null) return;

            if (_mainCamera == null) _mainCamera = Camera.main;

            // Determine if clicking move, rotate or resize
            _activeHandleMode = ManipulationMode.Moving;
            _targetPanel.BeginManipulation(_activeHandleMode);

            _dragPlane = new Plane(-_mainCamera.transform.forward, _targetPanel.transform.position);
            Ray ray = _mainCamera.ScreenPointToRay(eventData.position);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                _dragPlaneOffset = _targetPanel.transform.position - ray.GetPoint(enter);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_targetPanel == null || _activeHandleMode == ManipulationMode.None) return;

            if (_mainCamera == null) _mainCamera = Camera.main;

            Ray ray = _mainCamera.ScreenPointToRay(eventData.position);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                _targetPanel.MoveTo(hitPoint + _dragPlaneOffset);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_targetPanel != null)
            {
                _targetPanel.EndManipulation();
            }
            _activeHandleMode = ManipulationMode.None;
        }

        private void OnDuplicateClicked()
        {
            if (_targetPanel != null) _targetPanel.Duplicate();
        }

        private void OnCloseClicked()
        {
            if (_targetPanel != null) _targetPanel.ClosePanel();
        }

        private void OnResetClicked()
        {
            if (_targetPanel != null) _targetPanel.ResetOrientation();
        }

        private void OnFocusClicked()
        {
            if (_targetPanel != null) _targetPanel.FocusPanel();
        }
    }
}

