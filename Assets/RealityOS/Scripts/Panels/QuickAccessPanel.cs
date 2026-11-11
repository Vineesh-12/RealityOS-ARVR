using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;
using RealityOS.Workspace;

namespace RealityOS.Panels
{
    public class QuickAccessPanel : WorkspacePanelBase
    {
        [Header("Quick Access UI")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private TMP_InputField _newLinkInput;
        [SerializeField] private Button _addLinkButton;

        [Header("State")]
        [SerializeField] private List<string> _quickLinks = new List<string>();

        protected override void Awake()
        {
            _panelType = PanelType.QuickAccess;
            base.Awake();

            if (_addLinkButton != null)
            {
                _addLinkButton.onClick.AddListener(OnAddLinkClick);
            }
        }

        private void OnDestroy()
        {
            if (_addLinkButton != null)
            {
                _addLinkButton.onClick.RemoveListener(OnAddLinkClick);
            }
        }

        public void AddLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) return;

            _quickLinks.Add(link.Trim());
            RefreshUI();

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.MarkDirty();
            }
        }

        public void RemoveLink(string link)
        {
            _quickLinks.Remove(link);
            RefreshUI();

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.MarkDirty();
            }
        }

        private void OnAddLinkClick()
        {
            if (_newLinkInput != null && !string.IsNullOrWhiteSpace(_newLinkInput.text))
            {
                AddLink(_newLinkInput.text);
                _newLinkInput.text = "";
            }
        }

        public void RefreshUI()
        {
            if (_itemsContainer == null) return;

            for (int i = _itemsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemsContainer.GetChild(i).gameObject);
            }

            foreach (var link in _quickLinks)
            {
                GameObject item = CreateProceduralLinkItem(link, _itemsContainer);
            }
        }

        private GameObject CreateProceduralLinkItem(string title, Transform parent)
        {
            GameObject go = new GameObject($"Link_{title}");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280f, 32f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.25f, 0.35f, 0.9f);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => OnShortcutClicked(title));

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 2f);
            textRt.offsetMax = new Vector2(-10f, -2f);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"> {title}";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            return go;
        }

        private void OnShortcutClicked(string title)
        {
            Debug.Log($"[RealityOS.QuickAccess] Shortcut triggered: {title}");
            if (title.Contains("MainMenu") || title.Contains("Main Menu"))
            {
                SceneNavigationManager.Instance.ReturnToMainMenu();
            }
            else if (title.Contains("AR"))
            {
                SceneNavigationManager.Instance.LoadModeScene(InteractionMode.MobileAR);
            }
            else if (title.Contains("XR"))
            {
                SceneNavigationManager.Instance.LoadModeScene(InteractionMode.SimulatedXR);
            }
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.quickLinks = new List<string>(_quickLinks);
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null && data.quickLinks != null)
            {
                _quickLinks = new List<string>(data.quickLinks);
                RefreshUI();
            }
        }
    }
}
