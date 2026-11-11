using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;
using RealityOS.Persistence;

namespace RealityOS.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Mode Selection Buttons")]
        [SerializeField] private Button _desktopModeButton;
        [SerializeField] private Button _mobileArModeButton;
        [SerializeField] private Button _simulatedXrModeButton;

        [Header("Top / Footer Navigation")]
        [SerializeField] private Button _openWorkspacesButton;
        [SerializeField] private Button _openSettingsButton;
        [SerializeField] private Button _openHelpButton;
        [SerializeField] private Button _exitAppButton;

        [Header("Workspace Management Modal")]
        [SerializeField] private GameObject _workspacesModal;
        [SerializeField] private Transform _workspaceListContainer;
        [SerializeField] private GameObject _workspaceItemPrefab;
        [SerializeField] private TMP_InputField _newWorkspaceNameInput;
        [SerializeField] private Button _createNewWorkspaceButton;
        [SerializeField] private Button _closeWorkspacesButton;

        [Header("Settings Modal")]
        [SerializeField] private GameObject _settingsModal;
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private Slider _cameraSpeedSlider;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private TMP_Dropdown _graphicsQualityDropdown;
        [SerializeField] private Toggle _snapTurnToggle;
        [SerializeField] private Button _resetSettingsButton;
        [SerializeField] private Button _saveSettingsButton;
        [SerializeField] private Button _closeSettingsButton;

        [Header("Help Modal")]
        [SerializeField] private GameObject _helpModal;
        [SerializeField] private Button _closeHelpButton;

        private void Start()
        {
            BindButtons();
            RefreshWorkspaceList();
            InitSettingsUI();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void BindButtons()
        {
            if (_desktopModeButton != null) _desktopModeButton.onClick.AddListener(() => LaunchMode(InteractionMode.DesktopSpatial));
            if (_mobileArModeButton != null) _mobileArModeButton.onClick.AddListener(() => LaunchMode(InteractionMode.MobileAR));
            if (_simulatedXrModeButton != null) _simulatedXrModeButton.onClick.AddListener(() => LaunchMode(InteractionMode.SimulatedXR));

            if (_openWorkspacesButton != null) _openWorkspacesButton.onClick.AddListener(OpenWorkspacesModal);
            if (_closeWorkspacesButton != null) _closeWorkspacesButton.onClick.AddListener(CloseWorkspacesModal);
            if (_createNewWorkspaceButton != null) _createNewWorkspaceButton.onClick.AddListener(OnCreateWorkspaceClicked);

            if (_openSettingsButton != null) _openSettingsButton.onClick.AddListener(OpenSettingsModal);
            if (_closeSettingsButton != null) _closeSettingsButton.onClick.AddListener(CloseSettingsModal);
            if (_resetSettingsButton != null) _resetSettingsButton.onClick.AddListener(OnResetSettingsClicked);
            if (_saveSettingsButton != null) _saveSettingsButton.onClick.AddListener(OnSaveSettingsClicked);

            if (_openHelpButton != null) _openHelpButton.onClick.AddListener(OpenHelpModal);
            if (_closeHelpButton != null) _closeHelpButton.onClick.AddListener(CloseHelpModal);

            if (_exitAppButton != null) _exitAppButton.onClick.AddListener(OnExitAppClicked);
        }

        private void UnbindButtons()
        {
            if (_desktopModeButton != null) _desktopModeButton.onClick.RemoveAllListeners();
            if (_mobileArModeButton != null) _mobileArModeButton.onClick.RemoveAllListeners();
            if (_simulatedXrModeButton != null) _simulatedXrModeButton.onClick.RemoveAllListeners();

            if (_openWorkspacesButton != null) _openWorkspacesButton.onClick.RemoveAllListeners();
            if (_closeWorkspacesButton != null) _closeWorkspacesButton.onClick.RemoveAllListeners();
            if (_createNewWorkspaceButton != null) _createNewWorkspaceButton.onClick.RemoveAllListeners();

            if (_openSettingsButton != null) _openSettingsButton.onClick.RemoveAllListeners();
            if (_closeSettingsButton != null) _closeSettingsButton.onClick.RemoveAllListeners();
            if (_resetSettingsButton != null) _resetSettingsButton.onClick.RemoveAllListeners();
            if (_saveSettingsButton != null) _saveSettingsButton.onClick.RemoveAllListeners();

            if (_openHelpButton != null) _openHelpButton.onClick.RemoveAllListeners();
            if (_closeHelpButton != null) _closeHelpButton.onClick.RemoveAllListeners();

            if (_exitAppButton != null) _exitAppButton.onClick.RemoveAllListeners();
        }

        private void LaunchMode(InteractionMode mode)
        {
            if (AppModeManager.Instance != null)
            {
                AppModeManager.Instance.SetMode(mode);
            }
            if (SceneNavigationManager.Instance != null)
            {
                SceneNavigationManager.Instance.LoadModeScene(mode);
            }
        }

        public void OpenWorkspacesModal()
        {
            RefreshWorkspaceList();
            if (_workspacesModal != null) _workspacesModal.SetActive(true);
        }

        public void CloseWorkspacesModal()
        {
            if (_workspacesModal != null) _workspacesModal.SetActive(false);
        }

        private void RefreshWorkspaceList()
        {
            if (_workspaceListContainer == null || WorkspaceManager.Instance == null) return;

            for (int i = _workspaceListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_workspaceListContainer.GetChild(i).gameObject);
            }

            List<WorkspaceData> all = WorkspaceManager.Instance.Persistence.GetAllWorkspaces();
            foreach (var ws in all)
            {
                CreateWorkspaceEntryUI(ws, _workspaceListContainer);
            }
        }

        private void CreateWorkspaceEntryUI(WorkspaceData ws, Transform parent)
        {
            GameObject item = new GameObject($"Workspace_{ws.workspaceId}");
            item.transform.SetParent(parent, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480f, 44f);

            var img = item.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);

            // Label
            GameObject textObj = new GameObject("Title");
            textObj.transform.SetParent(item.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(0.65f, 1f);
            textRt.offsetMin = new Vector2(10f, 0f);
            textRt.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{ws.workspaceName} ({ws.panels.Count} panels)";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Load Button
            GameObject loadBtnObj = new GameObject("LoadBtn");
            loadBtnObj.transform.SetParent(item.transform, false);
            var loadRt = loadBtnObj.AddComponent<RectTransform>();
            loadRt.anchorMin = new Vector2(0.68f, 0.15f);
            loadRt.anchorMax = new Vector2(0.82f, 0.85f);
            loadRt.offsetMin = Vector2.zero;
            loadRt.offsetMax = Vector2.zero;
            var loadImg = loadBtnObj.AddComponent<Image>();
            loadImg.color = new Color(0f, 0.65f, 0.6f);
            var loadBtn = loadBtnObj.AddComponent<Button>();
            string idCopy = ws.workspaceId;
            loadBtn.onClick.AddListener(() =>
            {
                WorkspaceManager.Instance.LoadWorkspaceById(idCopy);
                CloseWorkspacesModal();
            });

            GameObject loadTxt = new GameObject("Text");
            loadTxt.transform.SetParent(loadBtnObj.transform, false);
            var loadTxtRt = loadTxt.AddComponent<RectTransform>();
            loadTxtRt.anchorMin = Vector2.zero;
            loadTxtRt.anchorMax = Vector2.one;
            var loadTmp = loadTxt.AddComponent<TextMeshProUGUI>();
            loadTmp.text = "Load";
            loadTmp.fontSize = 12;
            loadTmp.alignment = TextAlignmentOptions.Center;

            // Delete Button
            GameObject delBtnObj = new GameObject("DelBtn");
            delBtnObj.transform.SetParent(item.transform, false);
            var delRt = delBtnObj.AddComponent<RectTransform>();
            delRt.anchorMin = new Vector2(0.84f, 0.15f);
            delRt.anchorMax = new Vector2(0.98f, 0.85f);
            delRt.offsetMin = Vector2.zero;
            delRt.offsetMax = Vector2.zero;
            var delImg = delBtnObj.AddComponent<Image>();
            delImg.color = new Color(0.8f, 0.25f, 0.25f);
            var delBtn = delBtnObj.AddComponent<Button>();
            delBtn.onClick.AddListener(() =>
            {
                WorkspaceManager.Instance.DeleteWorkspace(idCopy);
                RefreshWorkspaceList();
            });

            GameObject delTxt = new GameObject("Text");
            delTxt.transform.SetParent(delBtnObj.transform, false);
            var delTxtRt = delTxt.AddComponent<RectTransform>();
            delTxtRt.anchorMin = Vector2.zero;
            delTxtRt.anchorMax = Vector2.one;
            var delTmp = delTxt.AddComponent<TextMeshProUGUI>();
            delTmp.text = "Del";
            delTmp.fontSize = 12;
            delTmp.alignment = TextAlignmentOptions.Center;
        }

        private void OnCreateWorkspaceClicked()
        {
            if (_newWorkspaceNameInput != null && !string.IsNullOrWhiteSpace(_newWorkspaceNameInput.text))
            {
                WorkspaceManager.Instance.CreateNewWorkspace(_newWorkspaceNameInput.text);
                _newWorkspaceNameInput.text = "";
                RefreshWorkspaceList();
            }
        }

        public void OpenSettingsModal()
        {
            InitSettingsUI();
            if (_settingsModal != null) _settingsModal.SetActive(true);
        }

        public void CloseSettingsModal()
        {
            if (_settingsModal != null) _settingsModal.SetActive(false);
        }

        private void InitSettingsUI()
        {
            if (SettingsManager.Instance == null) return;
            var cur = SettingsManager.Instance.Current;

            if (_mouseSensitivitySlider != null) _mouseSensitivitySlider.value = cur.mouseSensitivity;
            if (_cameraSpeedSlider != null) _cameraSpeedSlider.value = cur.cameraMoveSpeed;
            if (_volumeSlider != null) _volumeSlider.value = cur.masterVolume;
            if (_graphicsQualityDropdown != null) _graphicsQualityDropdown.value = cur.graphicsQualityLevel;
            if (_snapTurnToggle != null) _snapTurnToggle.isOn = cur.comfortSnapTurn;
        }

        private void OnSaveSettingsClicked()
        {
            if (SettingsManager.Instance == null) return;

            if (_mouseSensitivitySlider != null) SettingsManager.Instance.SetMouseSensitivity(_mouseSensitivitySlider.value);
            if (_cameraSpeedSlider != null) SettingsManager.Instance.SetCameraMoveSpeed(_cameraSpeedSlider.value);
            if (_volumeSlider != null) SettingsManager.Instance.SetMasterVolume(_volumeSlider.value);
            if (_graphicsQualityDropdown != null) SettingsManager.Instance.SetGraphicsQuality(_graphicsQualityDropdown.value);
            if (_snapTurnToggle != null) SettingsManager.Instance.SetComfortSnapTurn(_snapTurnToggle.isOn);

            SettingsManager.Instance.SaveSettings();
            CloseSettingsModal();
        }

        private void OnResetSettingsClicked()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ResetToDefaults();
                InitSettingsUI();
            }
        }

        public void OpenHelpModal()
        {
            if (_helpModal != null) _helpModal.SetActive(true);
        }

        public void CloseHelpModal()
        {
            if (_helpModal != null) _helpModal.SetActive(false);
        }

        private void OnExitAppClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
