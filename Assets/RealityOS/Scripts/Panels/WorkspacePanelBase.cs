using System;
using UnityEngine;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;
using RealityOS.Workspace;

namespace RealityOS.Panels
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class WorkspacePanelBase : MonoBehaviour, IWorkspacePanel
    {
        [Header("Panel Identity")]
        [SerializeField] protected string _panelId = Guid.NewGuid().ToString();
        [SerializeField] protected PanelType _panelType = PanelType.Notes;
        [SerializeField] protected string _title = "Spatial Panel";

        public string PanelId => _panelId;
        public PanelType PanelType => _panelType;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                UpdateTitleDisplay();
            }
        }

        [Header("UI Controls")]
        [SerializeField] protected TextMeshProUGUI _titleText;
        [SerializeField] protected GameObject _selectionHighlight;
        [SerializeField] protected Renderer _panelBackingRenderer;
        
        [Header("Manipulation & Visuals")]
        [SerializeField] protected float _minScale = 0.4f;
        [SerializeField] protected float _maxScale = 3.0f;
        [SerializeField] protected Color _defaultOutlineColor = new Color(0.2f, 0.25f, 0.32f, 1f);
        [SerializeField] protected Color _selectedOutlineColor = new Color(0.0f, 0.82f, 0.77f, 1f); // Teal Accent
        [SerializeField] protected Color _hoveredOutlineColor = new Color(0.4f, 0.6f, 0.9f, 1f);

        public bool IsSelected { get; protected set; }
        public bool IsHovered { get; protected set; }
        public Transform TransformTarget => transform;
        public ManipulationMode CurrentMode { get; protected set; } = ManipulationMode.None;

        protected Vector3 _targetPosition;
        protected Quaternion _targetRotation;
        protected Vector3 _targetScale;
        protected bool _useInterpolation = true;
        protected float _lerpSpeed = 15f;

        protected virtual void Awake()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _targetScale = transform.localScale;

            EnsureCollider();
            UpdateVisualState();
        }

        protected virtual void Start()
        {
            UpdateTitleDisplay();
        }

        protected virtual void Update()
        {
            if (_useInterpolation)
            {
                transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _lerpSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _lerpSpeed);
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * _lerpSpeed);
            }
        }

        public virtual void OnSelect()
        {
            IsSelected = true;
            UpdateVisualState();
        }

        public virtual void OnDeselect()
        {
            IsSelected = false;
            UpdateVisualState();
        }

        public virtual void OnHoverEnter()
        {
            IsHovered = true;
            UpdateVisualState();
        }

        public virtual void OnHoverExit()
        {
            IsHovered = false;
            UpdateVisualState();
        }

        public virtual void BeginManipulation(ManipulationMode mode)
        {
            CurrentMode = mode;
        }

        public virtual void EndManipulation()
        {
            CurrentMode = ManipulationMode.None;
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _targetScale = transform.localScale;

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.MarkDirty();
            }
        }

        public virtual void MoveTo(Vector3 worldPosition)
        {
            _targetPosition = worldPosition;
            if (!_useInterpolation) transform.position = worldPosition;
        }

        public virtual void TranslateDelta(Vector3 deltaMovement)
        {
            _targetPosition += deltaMovement;
            if (!_useInterpolation) transform.position = _targetPosition;
        }

        public virtual void RotateBy(Vector3 deltaEuler)
        {
            _targetRotation = Quaternion.Euler(deltaEuler) * _targetRotation;
            if (!_useInterpolation) transform.rotation = _targetRotation;
        }

        public virtual void ScaleBy(float scaleMultiplier)
        {
            Vector3 newScale = _targetScale * scaleMultiplier;
            float clampedX = Mathf.Clamp(newScale.x, _minScale, _maxScale);
            float factor = clampedX / _targetScale.x;
            _targetScale *= factor;
            if (!_useInterpolation) transform.localScale = _targetScale;
        }

        public virtual void SetScale(Vector3 newScale)
        {
            _targetScale = new Vector3(
                Mathf.Clamp(newScale.x, _minScale, _maxScale),
                Mathf.Clamp(newScale.y, _minScale, _maxScale),
                Mathf.Clamp(newScale.z, _minScale, _maxScale)
            );
            if (!_useInterpolation) transform.localScale = _targetScale;
        }

        public virtual void Duplicate()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.DuplicatePanel(this);
            }
        }

        public virtual void ClosePanel()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.DeletePanel(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public virtual void FocusPanel()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 targetCamPos = transform.position - transform.forward * 1.5f;
                cam.transform.position = targetCamPos;
                cam.transform.LookAt(transform.position);
            }
        }

        public virtual void ResetOrientation()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                // Face towards camera directly
                Vector3 toCam = (cam.transform.position - transform.position).normalized;
                _targetRotation = Quaternion.LookRotation(-toCam, Vector3.up);
            }
            else
            {
                _targetRotation = Quaternion.identity;
            }
            _targetScale = Vector3.one;
            if (!_useInterpolation)
            {
                transform.rotation = _targetRotation;
                transform.localScale = _targetScale;
            }
        }

        public virtual void BringToForeground()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 dir = (transform.position - cam.transform.position).normalized;
                _targetPosition = cam.transform.position + dir * 1.2f;
            }
        }

        public virtual PanelData ExportData()
        {
            PanelData data = new PanelData(_panelType, _title, transform.position);
            data.panelId = _panelId;
            data.rotation = new SerializableQuaternion(transform.rotation);
            data.scale = new SerializableVector3(transform.localScale);
            data.isVisible = gameObject.activeSelf;
            return data;
        }

        public virtual void ImportData(PanelData data)
        {
            if (data == null) return;

            _panelId = data.panelId;
            _panelType = data.panelType;
            _title = data.title;
            transform.position = data.position.ToVector3();
            transform.rotation = data.rotation.ToQuaternion();
            transform.localScale = data.scale.ToVector3();

            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _targetScale = transform.localScale;

            gameObject.SetActive(data.isVisible);
            UpdateTitleDisplay();
            UpdateVisualState();
        }

        protected virtual void UpdateTitleDisplay()
        {
            if (_titleText != null)
            {
                _titleText.text = _title;
            }
        }

        protected virtual void UpdateVisualState()
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(IsSelected || IsHovered);
            }

            if (_panelBackingRenderer != null && _panelBackingRenderer.material != null)
            {
                Color targetBorder = IsSelected ? _selectedOutlineColor : (IsHovered ? _hoveredOutlineColor : _defaultOutlineColor);
                if (_panelBackingRenderer.material.HasProperty("_BorderColor"))
                {
                    _panelBackingRenderer.material.SetColor("_BorderColor", targetBorder);
                }
            }
        }

        private void EnsureCollider()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(0.8f, 0.6f, 0.05f);
            }
        }
    }
}
