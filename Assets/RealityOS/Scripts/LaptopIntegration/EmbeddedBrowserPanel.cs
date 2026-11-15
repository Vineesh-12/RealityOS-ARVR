using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Persistence;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Interactive 3D World-Space Browser Panel for RealityOS.
    /// Embeds web browsing, Google search, bookmarks, and link clicking directly inside the 3D workspace.
    /// </summary>
    public class EmbeddedBrowserPanel : WorkspacePanelBase
    {
        [Header("Browser Engine")]
        [SerializeField] private EmbeddedBrowserController _controller;
        [SerializeField] private BrowserInputBridge _inputBridge;

        [Header("UI Controls")]
        [SerializeField] private TMP_InputField _urlInputField;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _forwardButton;
        [SerializeField] private Button _reloadButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _goButton;
        [SerializeField] private Button _openInChromeButton;
        [SerializeField] private RawImage _viewportImage;
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private TextMeshProUGUI _statusBarText;

        private Canvas _worldCanvas;

        protected override void Awake()
        {
            _panelType = PanelType.Browser;
            _title = "Spatial Web Browser";
            base.Awake();

            EnsureBrowserEngine();
            EnsureWorldCanvasUI();
            BindControls();
        }

        protected override void Start()
        {
            base.Start();
            UpdateTitleDisplay();
        }

        private void EnsureBrowserEngine()
        {
            if (_controller == null)
            {
                _controller = GetComponent<EmbeddedBrowserController>() ?? gameObject.AddComponent<EmbeddedBrowserController>();
            }
            if (_inputBridge == null)
            {
                _inputBridge = GetComponent<BrowserInputBridge>() ?? gameObject.AddComponent<BrowserInputBridge>();
            }
        }

        private void EnsureWorldCanvasUI()
        {
            // If already configured via prefab, return
            if (_viewportImage != null && _urlInputField != null) return;

            Transform canvasTrans = transform.Find("BrowserWorldCanvas");
            GameObject canvasObj;
            if (canvasTrans == null)
            {
                canvasObj = new GameObject("BrowserWorldCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);
                canvasObj.transform.localPosition = new Vector3(0f, 0f, -0.025f);
                canvasObj.transform.localRotation = Quaternion.identity;
                canvasObj.transform.localScale = Vector3.one * 0.001f; // 1 unit = 1000 px
            }
            else
            {
                canvasObj = canvasTrans.gameObject;
            }

            _worldCanvas = canvasObj.GetComponent<Canvas>();
            if (_worldCanvas == null) _worldCanvas = canvasObj.AddComponent<Canvas>();
            _worldCanvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
            if (canvasRt != null) canvasRt.sizeDelta = new Vector2(960, 680);

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.dynamicPixelsPerUnit = 2;

            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Viewport RawImage
            Transform vpTrans = canvasObj.transform.Find("ViewportImage");
            GameObject vpObj = vpTrans != null ? vpTrans.gameObject : new GameObject("ViewportImage");
            vpObj.transform.SetParent(canvasObj.transform, false);
            _viewportImage = vpObj.GetComponent<RawImage>() ?? vpObj.AddComponent<RawImage>();

            RectTransform vpRt = vpObj.GetComponent<RectTransform>();
            vpRt.anchorMin = new Vector2(0f, 0f);
            vpRt.anchorMax = new Vector2(1f, 1f);
            vpRt.offsetMin = new Vector2(15, 15);
            vpRt.offsetMax = new Vector2(-15, -95); // Leave top 95px for toolbar

            if (_controller != null && _controller.BrowserTexture != null)
            {
                _viewportImage.texture = _controller.BrowserTexture;
            }

            if (_inputBridge != null && _viewportImage != null)
            {
                _inputBridge.SetViewportRect(_viewportImage.rectTransform);
            }
        }

        private void BindControls()
        {
            if (_controller != null)
            {
                _controller.OnTextureUpdated += tex =>
                {
                    if (_viewportImage != null) _viewportImage.texture = tex;
                };

                _controller.OnLoadingChanged += loading =>
                {
                    if (_loadingIndicator != null) _loadingIndicator.SetActive(loading);
                    if (_statusBarText != null)
                    {
                        _statusBarText.text = loading ? "Loading..." : _controller.Session.CurrentUrl;
                    }
                };

                _controller.OnPageLoaded += (url, pageTitle) =>
                {
                    if (_urlInputField != null) _urlInputField.text = url;
                    Title = string.IsNullOrEmpty(pageTitle) ? "Spatial Browser" : pageTitle;
                    if (_statusBarText != null) _statusBarText.text = url;
                };

                if (_controller.Session != null)
                {
                    _controller.Session.OnHistoryStateChanged += (canBack, canForward) =>
                    {
                        if (_backButton != null) _backButton.interactable = canBack;
                        if (_forwardButton != null) _forwardButton.interactable = canForward;
                    };
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(() => _controller?.GoBack());
            if (_forwardButton != null) _forwardButton.onClick.AddListener(() => _controller?.GoForward());
            if (_reloadButton != null) _reloadButton.onClick.AddListener(() => _controller?.Reload());
            if (_homeButton != null) _homeButton.onClick.AddListener(() => _controller?.NavigateTo("https://www.google.com"));
            if (_goButton != null) _goButton.onClick.AddListener(OnGoClicked);
            if (_urlInputField != null) _urlInputField.onSubmit.AddListener(text => OnGoClicked());
            if (_openInChromeButton != null) _openInChromeButton.onClick.AddListener(OnOpenInChromeClicked);
        }

        private void OnGoClicked()
        {
            if (_urlInputField != null && !string.IsNullOrWhiteSpace(_urlInputField.text))
            {
                _controller?.NavigateTo(_urlInputField.text);
            }
        }

        private void OnOpenInChromeClicked()
        {
            string url = _controller?.Session?.CurrentUrl ?? "https://www.google.com";
            BrowserLauncher.OpenUrl(url);
        }

        public void Navigate(string urlOrQuery)
        {
            _controller?.NavigateTo(urlOrQuery);
        }

        public override void OnHoverEnter()
        {
            base.OnHoverEnter();
            _inputBridge?.SetHovered(true);
        }

        public override void OnHoverExit()
        {
            base.OnHoverExit();
            _inputBridge?.SetHovered(false);
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.notesContent = _controller?.Session?.CurrentUrl ?? "https://www.google.com";
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null && !string.IsNullOrEmpty(data.notesContent))
            {
                _controller?.NavigateTo(data.notesContent);
            }
        }
    }
}
