using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
    public class ObjectViewerPanel : WorkspacePanelBase
    {
        [Header("3D Object Viewer References")]
        [SerializeField] private Transform _modelAnchor;
        [SerializeField] private GameObject _currentModelInstance;
        [SerializeField] private Slider _zoomSlider;
        [SerializeField] private Toggle _autoSpinToggle;
        [SerializeField] private TextMeshProUGUI _modelLabel;

        [Header("State")]
        [SerializeField] private string _modelName = "GeometricPrimitive";
        [SerializeField] private float _rotationY = 0f;
        [SerializeField] private float _zoom = 1f;
        [SerializeField] private bool _autoSpin = true;
        [SerializeField] private float _spinSpeed = 35f;

        protected override void Awake()
        {
            _panelType = PanelType.ObjectViewer;
            base.Awake();

            if (_zoomSlider != null)
            {
                _zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
            }
            if (_autoSpinToggle != null)
            {
                _autoSpinToggle.onValueChanged.AddListener(OnAutoSpinChanged);
            }
        }

        protected override void Start()
        {
            base.Start();
            EnsureModelAnchor();
            LoadModel(_modelName);
        }

        protected override void Update()
        {
            base.Update();

            if (_modelAnchor != null)
            {
                if (_autoSpin)
                {
                    _rotationY = (_rotationY + _spinSpeed * Time.deltaTime) % 360f;
                }
                _modelAnchor.localRotation = Quaternion.Euler(15f, _rotationY, 0f);
                _modelAnchor.localScale = Vector3.one * _zoom;
            }
        }

        private void EnsureModelAnchor()
        {
            if (_modelAnchor == null)
            {
                GameObject anchorObj = new GameObject("ModelAnchor");
                anchorObj.transform.SetParent(transform, false);
                anchorObj.transform.localPosition = new Vector3(0f, 0f, -0.15f);
                _modelAnchor = anchorObj.transform;
            }
        }

        public void LoadModel(string modelName)
        {
            EnsureModelAnchor();
            _modelName = modelName;

            if (_currentModelInstance != null)
            {
                Destroy(_currentModelInstance);
            }

            // Create procedural 3D geometric showcase object
            _currentModelInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _currentModelInstance.transform.SetParent(_modelAnchor, false);
            _currentModelInstance.transform.localPosition = Vector3.zero;
            _currentModelInstance.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);

            // Give it an appealing spatial material
            var renderer = _currentModelInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.1f, 0.7f, 0.9f);
                mat.SetFloat("_Metallic", 0.6f);
                mat.SetFloat("_Glossiness", 0.8f);
                renderer.material = mat;
            }

            // Remove primitive collider so it doesn't block panel drag
            var col = _currentModelInstance.GetComponent<Collider>();
            if (col != null) Destroy(col);

            if (_modelLabel != null)
            {
                _modelLabel.text = _modelName;
            }
        }

        public void RotateObject(float deltaY)
        {
            _rotationY = (_rotationY + deltaY) % 360f;
        }

        public void SetZoom(float zoomFactor)
        {
            _zoom = Mathf.Clamp(zoomFactor, 0.5f, 2.5f);
            if (_zoomSlider != null) _zoomSlider.value = _zoom;
        }

        private void OnZoomSliderChanged(float val)
        {
            _zoom = val;
        }

        private void OnAutoSpinChanged(bool spin)
        {
            _autoSpin = spin;
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.viewerModelName = _modelName;
            data.viewerRotationY = _rotationY;
            data.viewerZoom = _zoom;
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null)
            {
                _modelName = string.IsNullOrEmpty(data.viewerModelName) ? "GeometricPrimitive" : data.viewerModelName;
                _rotationY = data.viewerRotationY;
                SetZoom(data.viewerZoom > 0f ? data.viewerZoom : 1f);
                LoadModel(_modelName);
            }
        }
    }
}

